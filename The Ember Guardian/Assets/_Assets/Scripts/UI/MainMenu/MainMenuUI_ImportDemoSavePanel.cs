using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI_ImportDemoSavePanel : MonoBehaviour {
    public static MainMenuUI_ImportDemoSavePanel Instance;

    [SerializeField] private GameObject startupMessagePanel;
    [SerializeField] protected Button importSaveButton;
    [SerializeField] protected TextMeshProUGUI panelName;
    [SerializeField] protected TextMeshProUGUI panelText;

    [SerializeField] protected Animator panelNameImageAnimator;

    private bool panelOpen;

    private void Awake() {
        Instance = this;
        startupMessagePanel.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += Gameinput_OnPlayerBackPerformed;

        panelText.font = LocalizationManager.Instance.GetCurrentFont();
        panelName.font = LocalizationManager.Instance.GetCurrentFont();
        string loadedText = LocalizationManager.Instance.GetLocalizedText("importProgressionPanel_Text");
        loadedText = loadedText.Replace("\\n", "\n");
        panelName.text = LocalizationManager.Instance.GetLocalizedText("importProgressionPanel_Name");
        panelText.text = loadedText;

        panelNameImageAnimator.SetTrigger("DeletedSaveFile");
    }

    private void Gameinput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!panelOpen) return;
        ClosePanel();
    }

    public void OpenPanel() {
        panelOpen = true;
        EventSystem.current.SetSelectedGameObject(importSaveButton.gameObject);
        startupMessagePanel.gameObject.SetActive(true);
    }

    public void ClosePanel() {
        panelOpen = false;
        startupMessagePanel.gameObject.SetActive(false);

        MainMenuUI.Instance.ShowAllMenuUI();
        MainMenuUI.Instance.ShowMainMenuButtons();

        if (!MusicManager.Instance.GetPlayingMusic()) {
            MusicManager.Instance.PlayMusic();
        }

        MainMenuUI.Instance.InitializeHordeModeButton();
    }

    public void ImportDemoSave() {
        DemoSaveImportManager.Instance.ImportDemoSave();
        ClosePanel();
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= Gameinput_OnPlayerBackPerformed;
    }
}
