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
    [SerializeField] private LevelSO defaultLevelSO;
    [SerializeField] private bool isDemoIntro;

    private bool isCrossfading;

    public static SceneLoader Instance;

    public event EventHandler<OnSceneFadeOutEventArgs> OnSceneFadeOut;
    public event EventHandler OnSceneFadeIn;

    public class OnSceneFadeOutEventArgs : EventArgs {
        public float fadeOutTime;
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
        Application.targetFrameRate = 60;
       
        StartCoroutine(RemoveBlackBackgroundAfterDelay(.1f));
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
        transitionAnimator.SetTrigger("Start");
        transitionAnimator.speed = 1/crossfadeDuration;

        OnSceneFadeOut?.Invoke(this, new OnSceneFadeOutEventArgs {
            fadeOutTime = crossfadeDuration
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
    public void StartFadeOut() {
        OnSceneFadeIn?.Invoke(this, EventArgs.Empty);
        transitionAnimator.SetTrigger("Start");
    }

    public bool GetIsCrossfading() {
        return isCrossfading;
    }

}
