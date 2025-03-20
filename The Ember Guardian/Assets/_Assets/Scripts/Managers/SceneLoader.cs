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
    [SerializeField] private bool isDemoIntro;

    public static SceneLoader Instance;

    public event EventHandler OnSceneFadeOut;
    public enum SceneType {
        MainMenu,
        HUB,
        Level,
        Tutorial,
        WarmupScene,
    }

    private void Awake() {
        Instance = this;
        transitionAnimator.speed = .5f;
        if(sceneType == SceneType.WarmupScene) {
            LoadMainMenu(0f);
            Debug.Log("Load Main Menu Start");
        }

        StartCoroutine(RemoveBlackBackgroundAfterDelay(.1f));
    }


    public SceneType GetSceneType() {
        return sceneType;
    }
    public void LoadTutorial(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("Level0_Tutorial", crossfadeDuration));
    }
    public void LoadDemoIntro(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("DemoLevel_Intro", crossfadeDuration));
    }

    public void LoadMainMenu(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("MainMenu", crossfadeDuration));
    }

    public void LoadHub(float crossfadeDuration) {
        if(isDemoIntro || DemoMainLevelManager.Instance != null) {
            StartCoroutine(LoadSceneAfterCrossfade("HUB_Demo", crossfadeDuration));
        } else {
            StartCoroutine(LoadSceneAfterCrossfade("HUB", crossfadeDuration));
        }
    }

    public void LoadTestLevel(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("PrototypeLevel", crossfadeDuration));
    }

    public void LoadLevel(LevelSO levelSO, float crossfadeDuration) {
        string sceneName = levelSO.linkedSceneName;
        StartCoroutine(LoadSceneAfterCrossfade(sceneName, crossfadeDuration));
    }

    private IEnumerator LoadSceneAfterCrossfade(string sceneName, float crossfadeDuration) {
        transitionAnimator.SetTrigger("Start");
        transitionAnimator.speed = 1/crossfadeDuration;
        OnSceneFadeOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(crossfadeDuration + .2f);

        Debug.Log("Load Main Menu");
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator RemoveBlackBackgroundAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        blackBackground.SetActive(false);
    }

    public void StartFadeOut() {
        transitionAnimator.SetTrigger("Start");
    }

}
