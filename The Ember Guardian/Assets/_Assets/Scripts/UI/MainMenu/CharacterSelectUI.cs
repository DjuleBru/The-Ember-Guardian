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

    private bool characterType;
    private bool hasChosenCharacter;

    public event EventHandler OnCharacterChanged;

    private void Awake() {
        panel.SetActive(false);
        swapCharacter_WorldCanvas.SetActive(false);
        Instance = this;

        hasChosenCharacter = ES3.Load("hasChosenCharacter", false);
        characterType = ES3.Load("characterType", false);
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
    }

    private void SelectCharacter(bool female) {
        ES3.Save("characterType", female);  
        ES3.Save("hasChosenCharacter", true);

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

    }

    public void ClosePanel() {
        panel.gameObject.SetActive(false);
        swapCharacter_WorldCanvas.gameObject.SetActive(false);

        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.ShowAllMenuUI();
            MainMenuUI.Instance.ShowMainMenuButtons();

            if (!MusicManager.Instance.GetPlayingMusic()) {
                MusicManager.Instance.PlayMusic();
            }
        }
    }

    public bool HasChosenCharacter() {
        return hasChosenCharacter;
    }

    public bool GetChosenCharacterIsFemale() {
        return characterType;
    }
}
