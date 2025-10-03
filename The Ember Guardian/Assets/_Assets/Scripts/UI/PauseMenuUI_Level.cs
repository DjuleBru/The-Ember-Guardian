using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI_Level : PauseMenuUI {

    [SerializeField] protected Button_Confirm buttonConfirm_BackToHub;
    [SerializeField] protected TextMeshProUGUI backToHubText;

    protected bool confirmBackToHub;

    protected override void Start() {
        base.Start();
        buttonConfirm_BackToHub.OnButtonDeselected += ButtonConfirm_BackToHub_OnButtonDeselected;

        backToHubText.text = LocalizationManager.Instance.GetLocalizedText("menu_backToHub");
        backToHubText.font = LocalizationManager.Instance.GetCurrentFont();
    }

    private void ButtonConfirm_BackToHub_OnButtonDeselected(object sender, System.EventArgs e) {
        confirmBackToHub = false;
        backToHubText.text = LocalizationManager.Instance.GetLocalizedText("menu_backToHub");
        backToHubText.font = LocalizationManager.Instance.GetCurrentFont();
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    public override void LoadMainMenu() {

        if (confirmBackToMenu || progressionSaved) {
            SceneLoader.Instance.LoadMainMenu(2f);
            OpenClosePauseMenu();
        }

        else {
            confirmBackToMenu = true;
            ShowProgressionSavedText();
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

    public void BackToHubButton() {

        if (confirmBackToHub) {
        
            ShowPauseMenu(false);
            SceneLoader.Instance.LoadHub(1f);

            if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
                ReturnToHubFromGame();
            }

            if (DemoMainLevelManager.Instance != null) {
                DemoMainLevelManager.Instance.AddLevelLostAmount();
            }


        }
        else {

            confirmBackToHub = true;
            backToHubText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
            backToHubText.font = LocalizationManager.Instance.GetCurrentFont();
            ShowProgressionWillBeLost();

        }
    }

    public void ReturnToHubFromGame() {
        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber(0f);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(true);
    }

    public override void ExitGameButton() {
        if (confirmExitGame) {
            OpenFullGameDescriptionPanel();
        }
        else {
            confirmExitGame = true;
            exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
            ShowProgressionSavedText();
        }
    }

    private void ShowProgressionSavedText() {
        progressionSavedTextIndicator.SetTrigger("Show");

        if (SavingManager_Level.Instance.GetSavedOnce()) {

            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().text = SavingManager_Level.Instance.GetLocalizedLastSaveText();
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().color = savedTextColor;

        } else {

            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedText("menu_progressionNotSaved");
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().color = unsavedTextColor;

        }

    }

    private void ShowProgressionWillBeLost() {
        progressionSavedTextIndicator.SetTrigger("Show");


        progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedText("menu_progressionWillBeLost");
        progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().color = progressionLostTextColor;


    }


}
