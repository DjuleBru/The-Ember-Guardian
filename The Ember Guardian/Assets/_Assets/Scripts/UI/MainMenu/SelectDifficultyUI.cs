using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectDifficultyUI : MonoBehaviour
{
    public static SelectDifficultyUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button mediumDifficultyButton;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform hordeModeCameraTarget;
    [SerializeField] private Transform hordeModeMenuCameraTarget;
    [SerializeField] private Animator canvasAnimator;
    [SerializeField] private GameObject easyGO;
    [SerializeField] private GameObject mediumGO;
    [SerializeField] private GameObject hardGO;
    [SerializeField] private TextMeshProUGUI hordeXPText;

    private bool panelOpen;
    private bool fromHordeMode;

    private void Awake() {
        panel.SetActive(false);
        panelOpen = false;

        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += Gameinput_OnPlayerBackPerformed;
    }

    private void Gameinput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!panelOpen) return;
        ClosePanel();
    }

    public void OpenPanel(bool fromHordeMode) {
        this.fromHordeMode = fromHordeMode;

        if(fromHordeMode) {
            hordeXPText.gameObject.SetActive(true);
            hordeXPText.text = " +30% " + LocalizationManager.Instance.GetLocalizedText("difficulty_hordeBonus");
        } else {
            hordeXPText.gameObject.SetActive(false);
        }

        StartCoroutine(OpenPanelCoroutine());
    }

    private IEnumerator OpenPanelCoroutine() {
        if (MainMenuUI.Instance != null) {

            MainMenuUI.Instance.HideAllMenuUI();
            MainMenuUI.Instance.HideMainMenuButtons();

        }

        if(fromHordeMode) {
            CameraManager.Instance.ChangeCameraTarget(hordeModeCameraTarget, false);
        } else {
            CameraManager.Instance.ChangeCameraTarget(cameraTarget, false);
        }
        
        MainMenuUI.Instance.HideMainMenuButtons();

        yield return new WaitForSeconds(2f);
        panel.gameObject.SetActive(true);
        canvasAnimator.SetTrigger("Show");

        if(GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(mediumDifficultyButton.gameObject);
        }

        panelOpen = true;
    }

    public void ClosePanel() {
        canvasAnimator.SetTrigger("Hide");

        panel.gameObject.SetActive(false);

        if (fromHordeMode) {
            CameraManager.Instance.ChangeCameraTarget(hordeModeMenuCameraTarget);
        }
        else {
            CameraManager.Instance.ResetCameraTarget();
        }

        if (MainMenuUI.Instance != null) {

            if(fromHordeMode) {
                HordeModeUI.Instance.FadeInHordeModePanel();
            } else {
                MainMenuUI.Instance.ShowAllMenuUI();
                MainMenuUI.Instance.ShowMainMenuButtons();
            }

        }

        panelOpen = false;
    }

    private IEnumerator ClosePanelAfterDifficultySelect() {

        yield return new WaitForSeconds(.5f);

        if(fromHordeMode) {
            CameraManager.Instance.ChangeCameraTarget(hordeModeMenuCameraTarget);
        } else {
            CameraManager.Instance.ResetCameraTarget();
        }

        canvasAnimator.SetTrigger("Hide");
        panelOpen = false;

        if(fromHordeMode) {
            MainMenuUI.Instance.StartHordeMode();
        } else {
            MainMenuUI.Instance.StartNewGame();
        }

    }

    public void EasyDifficultyButton() {
        if(fromHordeMode) {
            SettingsManager.Instance.SetHordeDifficulty(SettingsManager.Difficulty.Easy);
        } else {
            SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Easy);
        }

        StartCoroutine(ClosePanelAfterDifficultySelect());
        mediumGO.SetActive(false);
        hardGO.SetActive(false);
    }

    public void MediumDifficultyButton() {

        if (fromHordeMode) {
            SettingsManager.Instance.SetHordeDifficulty(SettingsManager.Difficulty.Medium);
        }
        else {
            SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Medium);
        }

        StartCoroutine(ClosePanelAfterDifficultySelect());
        easyGO.SetActive(false);
        hardGO.SetActive(false);
    }

    public void HardDifficultyButton() {

        if (fromHordeMode) {
            SettingsManager.Instance.SetHordeDifficulty(SettingsManager.Difficulty.Hard);
        }
        else {
            SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Hard);
        }

        StartCoroutine(ClosePanelAfterDifficultySelect());
        easyGO.SetActive(false);
        mediumGO.SetActive(false);
    }
}
