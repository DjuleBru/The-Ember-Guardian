using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject swapCharacter_WorldCanvas;
    [SerializeField] private Button swapCharacter_WorldCanvasButton;
    [SerializeField] private Button maleCharacterButton;
    [SerializeField] private Button femaleCharacterButton;

    private bool panelOpen;
    private bool characterType;
    private bool hasChosenCharacter;

    public event EventHandler OnCharacterChanged;

    private void Awake() {
        panel.SetActive(false);
        swapCharacter_WorldCanvas.SetActive(false);
        Instance = this;

        ES3Settings settingsSaveFileSettings = new ES3Settings("Settings.es3");

        hasChosenCharacter = ES3.Load("hasChosenCharacter", false, settingsSaveFileSettings);
        characterType = ES3.Load("characterType", false, settingsSaveFileSettings);
    }

    private void Start() {
        swapCharacter_WorldCanvasButton.onClick.AddListener(() => {
            SelectCharacter(!characterType);
        });

        maleCharacterButton.onClick.AddListener(() => {
            SelectCharacter(false);
            ClosePanel();
        });

        femaleCharacterButton.onClick.AddListener(() => {
            SelectCharacter(true);
            ClosePanel();
        });

        GameInput.Instance.OnPlayerInputChanged += Instance_OnPlayerInputChanged;
    }

    private void SelectCharacter(bool female) {
        ES3Settings settingsSaveFileSettings = new ES3Settings("Settings.es3");

        ES3.Save("characterType", female, settingsSaveFileSettings);  
        ES3.Save("hasChosenCharacter", true, settingsSaveFileSettings);

        characterType = female;
        OnCharacterChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OpenPanel() {
        panel.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(maleCharacterButton.gameObject);

        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.HideAllMenuUI();
            MainMenuUI.Instance.HideMainMenuButtons();
        }
        panelOpen = true;
    }

    private void Instance_OnPlayerInputChanged(object sender, EventArgs e) {
        if (!panelOpen) return;
        if(GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(maleCharacterButton.gameObject);
        }

    }

    public void ClosePanel() {
        panel.gameObject.SetActive(false);
        panelOpen = false;
        swapCharacter_WorldCanvas.gameObject.SetActive(false);

        if (MainMenuUI.Instance != null) {

            if(DemoSaveImportManager.Instance.HasDemoSaveToImport()) {

                MainMenuUI_ImportDemoSavePanel.Instance.OpenPanel();

            } else {

                MainMenuUI.Instance.ShowAllMenuUI();
                MainMenuUI.Instance.ShowMainMenuButtons();

                if (!MusicManager.Instance.GetPlayingMusic()) {
                    MusicManager.Instance.PlayMusic();
                }

            }

          
        }
    }

    public bool HasChosenCharacter() {
        return hasChosenCharacter;
    }

    public bool GetChosenCharacterIsFemale() {
        return characterType;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= Instance_OnPlayerInputChanged;
    }
}
