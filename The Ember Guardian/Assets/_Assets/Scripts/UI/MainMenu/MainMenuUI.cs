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

    [SerializeField] protected GameObject mainMenuPanel;
    [SerializeField] protected GameObject fullGameDescriptionPanel;
    [SerializeField] protected Animator mainMenuPanelAnimator;
    [SerializeField] protected Button continueButton;
    [SerializeField] protected Button newGameButton;
    [SerializeField] protected Button discordButton;
    [SerializeField] protected Button wishlistButton_Menu;
    [SerializeField] protected Button wishlistButton_FullGamePanel;
    [SerializeField] protected Button_Confirm buttonConfirm_ResetProgression;
    [SerializeField] protected TextMeshProUGUI continueGameText;
    [SerializeField] protected TextMeshProUGUI newGameText;
    [SerializeField] private GameObject swapCharacter_WorldCanvas;
    [SerializeField] private Image logoImage;
    [SerializeField] private Sprite demoLogo;
    [SerializeField] private Sprite fullGameLogo;

    [SerializeField] protected TextMeshProUGUI ctaText;

    [SerializeField] protected GameObject mainMenuPanelGameObject;

    public event EventHandler OnGameStart;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        SettingsManager.Instance.OnLanguageChanged += SettingsManager_OnLanguageChanged;
        buttonConfirm_ResetProgression.OnButtonDeselected += ButtonConfirm_ResetProgression_OnButtonDeselected;

        InitializeButtonNavigation();

        if (VersioningManager.Instance.GetIsDemo()) {
            logoImage.sprite = demoLogo;
            wishlistButton_Menu.gameObject.SetActive(true);
            fullGameDescriptionPanel.SetActive(true);
        }
        else {
            logoImage.sprite = fullGameLogo;
            wishlistButton_Menu.gameObject.SetActive(false);
            fullGameDescriptionPanel.SetActive(false);
        }

        if (!VersioningManager.Instance.CheckNewSaveFile() && !VersioningManager.Instance.CheckIncompatibleSaveFile()) {

            HandleMenuStartup();

        } else {
            continueButton.interactable = false;
        }

        RefreshFonts();
    }

    public void HandleMenuStartup() {

        if (!CharacterSelectUI.Instance.HasChosenCharacter()) {
            CharacterSelectUI.Instance.OpenPanel();
        }
        else {
            StartCoroutine(FadeInMainMenu(1.5f));
        }
    }

    private void SettingsManager_OnLanguageChanged(object sender, EventArgs e) {
        RefreshFonts();
    }

    private void RefreshFonts() {
        ctaText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
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

            if(!VersioningManager.Instance.GetIsDemo()) {
                SelectDifficultyUI.Instance.OpenPanel();
            } else {
                StartNewGame();
            }

        } else {

            if (confirmResetProgression) {
                ES3.DeleteFile();
                continueButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
                newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_newGame");

                continueGameText.fontMaterial = LocalizationManager.Instance.GetStandardMaterial();
                newGameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
            }
            else {
                confirmResetProgression = true;
                newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetProgression");
            }

        }
    }

    public virtual void SettingsButton() {
        swapCharacter_WorldCanvas.gameObject.SetActive(true);

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
    public void StartNewGame() {
        StartCoroutine(StartNewGameCoroutine());
    }

    private IEnumerator StartNewGameCoroutine() {

        if(VersioningManager.Instance.GetIsDemo()) {
            mainMenuPanelAnimator.SetTrigger("FadeOut");
        }

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

        // DEMO
        if (VersioningManager.Instance.GetIsDemo()) {
            bool demoTutorialCompleted = MetaProgressionManager.Instance.GetTutorialCompletedOrSkipped();
            if (!demoTutorialCompleted) {
                SceneLoader.Instance.LoadDemoIntro(2f);
            }
            else {
                bool playerLeftInLevel = MetaProgressionManager.Instance.GetPlayerLeftInLevel();

                if(playerLeftInLevel) {
                    SceneLoader.Instance.LoadLastLevel(1f);
                } else {
                    SceneLoader.Instance.LoadHub(1f);
                }

            }

        }
        // FULL GAME
        else {
            if (MetaProgressionManager.Instance.GetTutorialCompletedOrSkipped()) {
                bool playerLeftInLevel = MetaProgressionManager.Instance.GetPlayerLeftInLevel();

                if (playerLeftInLevel) {
                    SceneLoader.Instance.LoadLastLevel(1f);
                }
                else {
                    SceneLoader.Instance.LoadHub(1f);
                }
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

    private void InitializeButtonNavigation() {
        Navigation wishlishButtonNav = wishlistButton_Menu.navigation;
        Navigation wishlishButtonFullGamePanelNav = wishlistButton_FullGamePanel.navigation;
        Navigation newGameButtonNav = newGameButton.navigation;
        Navigation resumeGameButtonNav = continueButton.navigation;
        Navigation discordButtonNav = discordButton.navigation;


        if (!MetaProgressionManager.Instance.GetSavedOnce()) {
            discordButtonNav.selectOnLeft = newGameButton;
            discordButton.navigation = discordButtonNav;
        }
        else {
            discordButtonNav.selectOnLeft = continueButton;
            discordButton.navigation = discordButtonNav;

        }

        if (VersioningManager.Instance.GetIsDemo()) {
            // DEMO

            if (!MetaProgressionManager.Instance.GetSavedOnce()) {

                wishlishButtonNav.selectOnDown = newGameButton;
                wishlistButton_Menu.navigation = wishlishButtonNav;

                wishlishButtonFullGamePanelNav.selectOnLeft = newGameButton;
                wishlistButton_FullGamePanel.navigation = wishlishButtonFullGamePanelNav;

                newGameButtonNav.selectOnUp = wishlistButton_Menu;
                newGameButton.navigation = newGameButtonNav;

            }
            else {

                wishlishButtonNav.selectOnDown = continueButton;
                wishlistButton_Menu.navigation = wishlishButtonNav;

                wishlishButtonFullGamePanelNav.selectOnLeft = continueButton;
                wishlistButton_FullGamePanel.navigation = wishlishButtonFullGamePanelNav;

                newGameButtonNav.selectOnUp = continueButton;
                newGameButton.navigation = newGameButtonNav;

            }
        } else {


            if (!MetaProgressionManager.Instance.GetSavedOnce()) {
                newGameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
            }
            else {
                continueGameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
            }

            resumeGameButtonNav.selectOnUp = null;
        }
      
    }

    public void ShowMainMenuButtons() {
        swapCharacter_WorldCanvas.gameObject.SetActive(false);

        StartCoroutine(FadeInMainMenu(0f));
        mainMenuPanelOpen = true;
    }

    public void HideMainMenuButtons() {
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        mainMenuPanelOpen = false;
    }

    public void HideAllMenuUI() {
        mainMenuPanel.gameObject.SetActive(false);
    }

    public void ShowAllMenuUI() {
        mainMenuPanel.gameObject.SetActive(true);
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }

}
