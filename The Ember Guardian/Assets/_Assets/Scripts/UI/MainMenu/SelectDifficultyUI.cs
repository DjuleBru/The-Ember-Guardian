using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectDifficultyUI : MonoBehaviour
{
    public static SelectDifficultyUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button mediumDifficultyButton;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Animator canvasAnimator;
    [SerializeField] private GameObject easyGO;
    [SerializeField] private GameObject mediumGO;
    [SerializeField] private GameObject hardGO;
    private bool panelOpen;

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

    public void OpenPanel() {
        StartCoroutine(OpenPanelCoroutine());
    }

    private IEnumerator OpenPanelCoroutine() {
        if (MainMenuUI.Instance != null) {

            MainMenuUI.Instance.HideAllMenuUI();
            MainMenuUI.Instance.HideMainMenuButtons();

        }

        CameraManager.Instance.ChangeCameraTarget(cameraTarget, false);
        MainMenuUI.Instance.HideMainMenuButtons();
        MusicManager.Instance.SetAudioVolume(.75f);

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

        CameraManager.Instance.ResetCameraTarget();
        MusicManager.Instance.SetAudioVolume(1f);

        if (MainMenuUI.Instance != null) {

            MainMenuUI.Instance.ShowAllMenuUI();
            MainMenuUI.Instance.ShowMainMenuButtons();

        }

        panelOpen = false;
    }

    private IEnumerator ClosePanelAfterDifficultySelect() {

        yield return new WaitForSeconds(.5f);
        CameraManager.Instance.ResetCameraTarget();
        canvasAnimator.SetTrigger("Hide");
        panelOpen = false;
        MainMenuUI.Instance.StartNewGame();
    }

    public void EasyDifficultyButton() {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Easy);
        StartCoroutine(ClosePanelAfterDifficultySelect());
        mediumGO.SetActive(false);
        hardGO.SetActive(false);
    }

    public void MediumDifficultyButton() {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Medium);

        StartCoroutine(ClosePanelAfterDifficultySelect());
        easyGO.SetActive(false);
        hardGO.SetActive(false);
    }

    public void HardDifficultyButton() {
        SettingsManager.Instance.SetDifficulty(SettingsManager.Difficulty.Hard);

        StartCoroutine(ClosePanelAfterDifficultySelect());
        easyGO.SetActive(false);
        mediumGO.SetActive(false);
    }
}
