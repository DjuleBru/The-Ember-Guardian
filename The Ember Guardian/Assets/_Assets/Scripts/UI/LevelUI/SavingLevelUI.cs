using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingLevelUI : MonoBehaviour
{
    [SerializeField] private Animator savingLevelCanvasAnimator;
    [SerializeField] private Animator savingLevelIconAnimator;
    [SerializeField] private bool isLoadingUI;

    private void Start() {

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {

            SavingManager_Level.Instance.OnSaveGameStarted += SavingManager_OnSaveGameStarted;
            SavingManager_Level.Instance.OnSaveGameEnded += SavingManager_OnSaveGameEnded;
            SavingManager_Level.Instance.OnLoadGameEnded += SavingManager_OnLoadGameEnded;

            if (!isLoadingUI) return;
            if (SavingManager_Level.Instance.GetLoadingSavedLevel()) {
                savingLevelCanvasAnimator.ResetTrigger("Hide");
                savingLevelCanvasAnimator.SetTrigger("Show");
            }
        }
    }

    private void SavingManager_OnLoadGameEnded(object sender, System.EventArgs e) {
        if (!isLoadingUI) return;
        savingLevelCanvasAnimator.ResetTrigger("Show");
        savingLevelCanvasAnimator.SetTrigger("Hide");
    }

    private void SavingManager_OnSaveGameEnded(object sender, System.EventArgs e) {
        if (isLoadingUI) return;
        savingLevelCanvasAnimator.ResetTrigger("Show");
        savingLevelCanvasAnimator.SetTrigger("Hide");
    }

    private void SavingManager_OnSaveGameStarted(object sender, System.EventArgs e) {
        if (isLoadingUI) return;
        savingLevelCanvasAnimator.ResetTrigger("Hide");
        savingLevelCanvasAnimator.SetTrigger("Show");
    }
}
