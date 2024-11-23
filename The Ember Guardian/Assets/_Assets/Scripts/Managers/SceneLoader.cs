using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneType sceneType;
    [SerializeField] private Animator transitionAnimator;
    public static SceneLoader Instance;

    public enum SceneType {
        MainMenu,
        HUB,
        Level,
    }

    private void Awake() {
        Instance = this;
    }

    public SceneType GetSceneType() {
        return sceneType;
    }

    public void LoadHub() {
        StartCoroutine(LoadSceneAfterDelay("HUB"));
    }

    public void LoadTestLevel() {
        StartCoroutine(LoadSceneAfterDelay("PrototypeLevel"));
    }


    private IEnumerator LoadSceneAfterDelay(string sceneName) {
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1.1f);


        SceneManager.LoadScene(sceneName);
    }

}
