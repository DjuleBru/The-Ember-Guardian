using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance;

    protected bool mainMenuPanelOpen;
    protected bool confirmExitGame;
    protected bool confirmResetProgression;

    [SerializeField] protected Animator mainMenuPanelAnimator;
    [SerializeField] protected Button continueButton;
    [SerializeField] protected Button newGameButton;
    [SerializeField] protected Button_Confirm buttonConfirm_ResetProgression;
    [SerializeField] protected TextMeshProUGUI continueGameText;
    [SerializeField] protected TextMeshProUGUI newGameText;

    [SerializeField] protected GameObject mainMenuPanelGameObject;

    public event EventHandler OnGameStart;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        buttonConfirm_ResetProgression.OnButtonDeselected += ButtonConfirm_ResetProgression_OnButtonDeselected;

        if(!VersioningManager.Instance.CheckNewSaveFile() && !VersioningManager.Instance.CheckIncompatibleSaveFile()) {
            StartCoroutine(FadeInMainMenu(1.5f));
        } else {

            Debug.Log("Set interactable false");
            continueButton.interactable = false;
        }
    }

 
    private void SetFirstSelectedButton() {
        if (!MetaProgressionManager.Instance.GetSavedOnce()) {
            continueButton.interactable = false;
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        }
        else {
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }

    }

    #region MAIN MENU BUTTONS

    public virtual void ResumeCurrentSaveButton() {
        StartCoroutine(ResumeCurrentSaveCoroutine());
       
    }

    public virtual void NewGameButton() {

        if (!MetaProgressionManager.Instance.GetSavedOnce()) {

            StartNewGame();

        } else {

            if (confirmResetProgression) {
                ES3.DeleteFile();
                continueButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
                newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_newGame");
            }
            else {
                confirmResetProgression = true;
                newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetProgression");
            }

        }
    }

    public virtual void SettingsButton() {
        SettingsMenuUI.Instance.OpenSettingsPanel();
        HideMainMenuButtons();
    }

    public virtual void ExitGameButton() {
        Application.Quit();
    }

    #endregion

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if (!mainMenuPanelOpen) return;
        if (GameInput.Instance.IsUsingGamepad()) {
            SetFirstSelectedButton();
        }
    }
    private void StartNewGame() {
        StartCoroutine(StartNewGameCoroutine());
    }

    private IEnumerator StartNewGameCoroutine() {
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        OnGameStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        MusicManager.Instance.FadeOutMusic(1f);
        MetaProgressionManager.Instance.SetSavedOnce();

        if(VersioningManager.Instance.GetIsDemo()) {
            SceneLoader.Instance.LoadDemoIntro(2f);

        } else {
            SceneLoader.Instance.LoadTutorial(2f);
        }

    }
    private IEnumerator ResumeCurrentSaveCoroutine() {
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        OnGameStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        if (VersioningManager.Instance.GetIsDemo()) {
            bool demoTutorialCompleted = MetaProgressionManager.Instance.tutorialComplete;
            if (!demoTutorialCompleted) {
                SceneLoader.Instance.LoadDemoIntro(2f);
            }
            else {
                bool playerLeftDemoInLevel = ES3.Load("playerLeftInLevel", false);

                if(playerLeftDemoInLevel) {
                    SceneLoader.Instance.LoadLastLevel(1f);
                } else {
                    SceneLoader.Instance.LoadHub(1f);
                }

            }

        }
        else {
            if (MetaProgressionManager.Instance.tutorialComplete) {
                SceneLoader.Instance.LoadHub(1f);
            }
            else {
                SceneLoader.Instance.LoadTutorial(1f);
            }
        }

        

        MusicManager.Instance.FadeOutMusic(1f);
    }
    private IEnumerator FadeInMainMenu(float delay) {

        if (!MetaProgressionManager.Instance.GetSavedOnce()) {
            continueButton.interactable = false;
        }

        yield return new WaitForSeconds(delay);
        mainMenuPanelAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1.5f);
        SetFirstSelectedButton();
    }

    private void ButtonConfirm_ResetProgression_OnButtonDeselected(object sender, EventArgs e) {
        confirmResetProgression = false;
        newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_newGame");
    }
    public void ShowMainMenuButtons() {
        StartCoroutine(FadeInMainMenu(0f));
        mainMenuPanelOpen = true;
    }

    public void HideMainMenuButtons() {
        Debug.Log("HideMainMenuButtons");
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        mainMenuPanelOpen = false;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }

}
