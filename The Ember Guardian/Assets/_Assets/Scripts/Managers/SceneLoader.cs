using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneType sceneType;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private GameObject blackBackground;
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private LevelSO defaultLevelSO;
    [SerializeField] private bool isDemoIntro;

    private bool isCrossfading;

    public static SceneLoader Instance;

    public event EventHandler<OnSceneFadeOutEventArgs> OnSceneFadeOut;
    public event EventHandler<OnSceneFadeOutEventArgs> OnSceneFadeIn;

    public class OnSceneFadeOutEventArgs : EventArgs {
        public float fadeOutTime;
        public bool fadeOutSound;
    }

    public enum SceneType {
        MainMenu,
        HUB,
        Level,
        Tutorial,
        AdjustGamma,
    }

    private void Awake() {
        Instance = this;
        isCrossfading = true;
        transitionAnimator.speed = .5f;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;

        StartCoroutine(RemoveBlackBackgroundAfterDelay(.1f));

        if (sceneType == SceneType.Level) {
            if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
                SavingManager_Level.Instance.OnLoadGameEnded += SavingManager_OnLoadGameEnded;
                return;
            }
        };

        transitionAnimator.SetTrigger("End");
        loadingIcon.gameObject.SetActive(false);
        StartCoroutine(SetCrossadeEndedAfterDelay(1.5f));
    }


    private void Start() {
        SetPlayerLeftFromLevel();

        if (sceneType != SceneType.Level) return;
        if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            loadingIcon.gameObject.SetActive(true);
        } else {
            loadingIcon.gameObject.SetActive(false);
        }
    }

    private void SavingManager_OnLoadGameEnded(object sender, EventArgs e) {
        transitionAnimator.SetTrigger("End");
        StartCoroutine(SetCrossadeEndedAfterDelay(1.5f));
    }


    public SceneType GetSceneType() {
        return sceneType;
    }

    public void LoadTutorial(float crossfadeDuration) {
        Debug.Log("LoadTutorial ");
        StartCoroutine(LoadSceneAfterCrossfade("Level0_Tutorial", crossfadeDuration));
    }

    public void LoadDemoIntro(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("DemoLevel_Intro", crossfadeDuration));
    }

    public void LoadMainMenu(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("MainMenu", crossfadeDuration));
    }

    public void LoadHub(float crossfadeDuration) {
        if(isDemoIntro || DemoMainLevelManager.Instance != null || (VersioningManager.Instance != null && VersioningManager.Instance.GetIsDemo())) {
            StartCoroutine(LoadSceneAfterCrossfade("HUB_Demo", crossfadeDuration));
        } else {
            StartCoroutine(LoadSceneAfterCrossfade("HUB_Main", crossfadeDuration));
        }
    }

    public void LoadLevel(LevelSO levelSO, float crossfadeDuration) {
        Debug.Log("LoadLevel " + levelSO);
        string sceneName = levelSO.linkedSceneName;
        StartCoroutine(LoadSceneAfterCrossfade(sceneName, crossfadeDuration));
    }

    public void LoadLastLevel(float crossfadeDuration) {
        string defaultLevelSOName = defaultLevelSO.linkedSceneName;
        string lastLevelSOName = ES3.Load("lastLevel", defaultValue:defaultLevelSOName);

        Debug.Log("LoadLastLevel " + lastLevelSOName);
        StartCoroutine(LoadSceneAfterCrossfade(lastLevelSOName, crossfadeDuration));
    }

    private IEnumerator LoadSceneAfterCrossfade(string sceneName, float crossfadeDuration) {
        AchievementsManager.Instance.SaveSteamStats();

        transitionAnimator.SetTrigger("Start");
        transitionAnimator.speed = 1/crossfadeDuration;

        OnSceneFadeOut?.Invoke(this, new OnSceneFadeOutEventArgs {
            fadeOutTime = crossfadeDuration,
            fadeOutSound = true,
        });

        isCrossfading = true;

        yield return new WaitForSeconds(crossfadeDuration + .2f);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator RemoveBlackBackgroundAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        blackBackground.SetActive(false);
    }
    private IEnumerator SetCrossadeEndedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        isCrossfading = false;
    }

    public void StartFadeOut(bool fadeOutSound) {
        OnSceneFadeIn?.Invoke(this, new OnSceneFadeOutEventArgs {
            fadeOutTime = 1f, 
            fadeOutSound = fadeOutSound
        });
        transitionAnimator.SetTrigger("End");
    }

    public void StartFadeIn(float fadeTime, bool fadeOutSound) {
        OnSceneFadeOut?.Invoke(this, new OnSceneFadeOutEventArgs {
            fadeOutTime = fadeTime,
            fadeOutSound = fadeOutSound
        });
        transitionAnimator.SetTrigger("Start");
    }

    public bool GetIsCrossfading() {
        return isCrossfading;
    }

    private void SetPlayerLeftFromLevel() {
        if (sceneType == SceneType.Level) {
            MetaProgressionManager.Instance.SetPlayerLeftFromLevel(true);
            MetaProgressionManager.Instance.SetLastLevel(SceneManager.GetActiveScene().name);
        }

        if (sceneType == SceneType.HUB) {
            MetaProgressionManager.Instance.SetPlayerLeftFromLevel(false);
        }
    }

    private void OnApplicationQuit() {
        Steamworks.SteamClient.Shutdown();
    }
}
