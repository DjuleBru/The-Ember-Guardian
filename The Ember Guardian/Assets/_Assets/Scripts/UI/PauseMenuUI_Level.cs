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
    }

    private void ButtonConfirm_BackToHub_OnButtonDeselected(object sender, System.EventArgs e) {
        confirmBackToHub = false;
        backToHubText.text = "Back To Hub";
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    public void BackToHubButton() {
        if (confirmBackToHub) {
            ShowPauseMenu(false);
            SceneLoader.Instance.LoadHub(1f);
        }
        else {
            confirmBackToHub = true;
            backToHubText.text = "Confirm ?";
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

    public override void ExitGameButton() {
        if (confirmExitGame) {
            Application.Quit();
        }
        else {
            confirmExitGame = true;
            exitGameText.text = "Confirm ?";
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

}
