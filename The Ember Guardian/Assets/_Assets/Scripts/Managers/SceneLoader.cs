using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneType sceneType;
    [SerializeField] private Animator transitionAnimator;
    public static SceneLoader Instance;

    public event EventHandler OnSceneFadeOut;
    public enum SceneType {
        MainMenu,
        HUB,
        Level,
        Tutorial,
    }

    private void Awake() {
        Instance = this;

        transitionAnimator.speed = .5f;
    }

    public SceneType GetSceneType() {
        return sceneType;
    }
    public void LoadTutorial(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("Level0_Tutorial", crossfadeDuration));
    }
    public void LoadHub(float crossfadeDuration) {
        StartCoroutine(LoadSceneAfterCrossfade("HUB", crossfadeDuration));
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


        SceneManager.LoadScene(sceneName);
    }

    public void StartFadeOut() {
        transitionAnimator.SetTrigger("Start");
    }

}
