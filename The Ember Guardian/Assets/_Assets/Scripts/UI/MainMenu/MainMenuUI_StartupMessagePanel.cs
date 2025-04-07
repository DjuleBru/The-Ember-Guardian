using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI_StartupMessagePanel : MonoBehaviour
{
    public static MainMenuUI_StartupMessagePanel Instance;

    [SerializeField] private GameObject startupMessagePanel;
    [SerializeField] protected Button backButton;
    [SerializeField] protected TextMeshProUGUI panelName;
    [SerializeField] protected TextMeshProUGUI panelText;

    [SerializeField] protected Animator panelNameImageAnimator;


    private void Awake() {
        Instance = this;
        startupMessagePanel.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += Gameinput_OnPlayerBackPerformed;
    }

    private void Gameinput_OnPlayerBackPerformed(object sender, EventArgs e) {
        ClosePanel();
    }

    public void OpenPanel() {
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        startupMessagePanel.gameObject.SetActive(true);
    }

    public void ClosePanel() {
        startupMessagePanel.gameObject.SetActive(false);
        MainMenuUI.Instance.ShowMainMenuButtons();
    }

    public void SetErasedSaveFilePanel() {
        panelName.text = "Warning";
        panelText.text = "Your previous save file was incompatible with the current version of the game, your progression has been reset.\n\nSorry for the inconvenience.";
        panelNameImageAnimator.SetTrigger("DeletedSaveFile");
    }
    public void SetNewTesterPanel() {
        panelName.text = "Hello, Emberling !";
        panelText.text = "Welcome to The Ember Guardian! Please note that the game is still in early development, so you may come across some bugs. I hope you enjoy the experience!\n\nThank you for keeping the flame alive!";
        panelNameImageAnimator.SetTrigger("NewPlayer");
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= Gameinput_OnPlayerBackPerformed;
    }
}
