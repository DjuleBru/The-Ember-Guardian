using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseMenuUI_Tutorial : PauseMenuUI
{
    [SerializeField] protected Button_Confirm buttonConfirm_SkipTutorial;
    [SerializeField] protected TextMeshProUGUI skipTutorialText;

    protected bool confirmSkipTutorial;

    protected override void Start() {
        base.Start();
        skipTutorialText.text = LocalizationManager.Instance.GetLocalizedText("menu_skipTutorial");
        buttonConfirm_SkipTutorial.OnButtonDeselected += ButtonConfirm_SkipTutorial_OnButtonDeselected;
    }

    private void ButtonConfirm_SkipTutorial_OnButtonDeselected(object sender, System.EventArgs e) {
        confirmSkipTutorial = false;
        skipTutorialText.text = LocalizationManager.Instance.GetLocalizedText("menu_skipTutorial");
    }

    public void SkipTutorialButton() {
        if (confirmSkipTutorial) {
            ShowPauseMenu(false);

            MetaProgressionManager.Instance.SetTutorialSkipped();

            SceneLoader.Instance.LoadHub(1f);
        }
        else {
            confirmSkipTutorial = true;
            skipTutorialText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
        }
    }

    public override void ExitGameButton() {
        if (confirmExitGame) {
            Application.Quit();
        }
        else {
            confirmExitGame = true;
            exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
        }
    }
}
