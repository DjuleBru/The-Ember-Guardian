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
    protected bool hasResetHordeMode;
    protected bool confirmHordeModeResetProgression;
    protected bool confirmHordeModeResetEntireProgression;
    protected bool justResetEntireHordeProgression;
    protected bool selectingHordeModeContinueOrNewGame;
    protected bool backFromHordeModeAfterDefeat;
    protected bool loadingHordeMode;

    [SerializeField] protected GameObject mainMenuPanel;
    [SerializeField] protected GameObject fullGameDescriptionPanel;
    [SerializeField] protected Animator mainMenuPanelAnimator;
    [SerializeField] protected Button continueButton;
    [SerializeField] protected Button newGameButton;
    [SerializeField] protected Button discordButton;
    [SerializeField] protected Button hordeModeButton;
    [SerializeField] protected Button settingsButton;
    [SerializeField] protected Button wishlistButton_Menu;
    [SerializeField] protected Button wishlistButton_FullGamePanel;
    [SerializeField] protected GameObject standardMenuGO;
    [SerializeField] protected GameObject hordeModeMenuGO;
    [SerializeField] protected Button hordeMode_newGameGameButton;
    [SerializeField] protected Button hordeMode_continueGameButton;
    [SerializeField] protected Button_Confirm buttonConfirm_ResetProgression;
    [SerializeField] protected Button_Confirm buttonConfirm_ResetHordeModeProgression;
    [SerializeField] protected Button_Confirm buttonConfirm_ResetHordeModeEntireProgression;
    [SerializeField] protected HordeModeMenuButton hordeModeMenuButton;
    [SerializeField] protected TextMeshProUGUI newHordeModeGameText;
    [SerializeField] protected TextMeshProUGUI resetEntireHordeModeProgressionText;
    [SerializeField] protected TextMeshProUGUI continueHordeModeGameText;
    [SerializeField] protected TextMeshProUGUI continueGameText;
    [SerializeField] protected TextMeshProUGUI newGameText;
    [SerializeField] private GameObject swapCharacter_WorldCanvas;
    [SerializeField] private Image logoImage;
    [SerializeField] private Sprite demoLogo;
    [SerializeField] private Sprite fullGameLogo;

    [SerializeField] protected TextMeshProUGUI ctaText;

    [SerializeField] protected GameObject mainMenuPanelGameObject;

    public event EventHandler OnGameStart;
    public event EventHandler OnHordeModeProgressionReset;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        SettingsManager.Instance.OnLanguageChanged += SettingsManager_OnLanguageChanged;
        buttonConfirm_ResetProgression.OnButtonDeselected += ButtonConfirm_ResetProgression_OnButtonDeselected;
        buttonConfirm_ResetHordeModeProgression.OnButtonDeselected += ButtonConfirm_ResetHordeModeProgression_OnButtonDeselected;
        buttonConfirm_ResetHordeModeEntireProgression.OnButtonDeselected += ButtonConfirm_ResetHordeModeEntireProgression_OnButtonDeselected;

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

        CheckBackFromHordeMode();
        RefreshHordeModeContinueVsNewGameButtonEnabledAndFonts();

        if (!VersioningManager.Instance.CheckNewSaveFile() && !VersioningManager.Instance.CheckIncompatibleSaveFile()) {

            HandleMenuStartup();

        } else {
            continueButton.interactable = false;
        }

        RefreshFonts();
        InitializeHordeModeButton();
    }


    public void InitializeHordeModeButton() {
        hordeModeMenuGO.SetActive(false);

        // CHECK HORDE MODE UNLOCKED
        if (DebugManager.Instance.GetHordeModeActiveDebug()) return;

        if (!MetaProgressionManager.Instance.GetHordeModeUnlocked()) {
            hordeModeButton.interactable = false;
        } else {
            hordeModeButton.interactable = true;
        }

        if (!VersioningManager.Instance.GetHordeModeImplemented()) {
            hordeModeButton.gameObject.SetActive(false);
        }

        hordeModeMenuButton.RefreshHordeModeUnlocked();

    }

    private void CheckBackFromHordeMode() {
        ES3Settings hordeModeSaveFileSettings = new ES3Settings("SaveFile_HordeMode.es3");

        backFromHordeModeAfterDefeat = ES3.Load("backFromHordeModeAfterDefeat", false, hordeModeSaveFileSettings);
        if (backFromHordeModeAfterDefeat) {
            ES3.Save("backFromHordeModeAfterDefeat", false, hordeModeSaveFileSettings);
            ES3.DeleteFile("HordeLevelSave.es3");
            NewHordeModeGame(true);
        }
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if(selectingHordeModeContinueOrNewGame) {
            OpenCloseHordeModeSelectButtons(false);
        }
    }

    public void HandleMenuStartup() {

        if (!CharacterSelectUI.Instance.HasChosenCharacter()) {
            CharacterSelectUI.Instance.OpenPanel();
        }
        else {
            if (backFromHordeModeAfterDefeat) {
                return;
            };

            StartCoroutine(FadeInMainMenu(1.5f));
        }
    }

    private void SettingsManager_OnLanguageChanged(object sender, EventArgs e) {
        RefreshFonts();
    }

    private void RefreshFonts() {
        ctaText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
        RefreshHordeModeContinueVsNewGameButtonEnabledAndFonts();
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

            SelectDifficultyUI.Instance.OpenPanel(false);

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

    public void MainMenuHordeModeButton() {
        HordeModeButton();
    }

    public virtual void HordeModeButton(bool changeCameraTarget = true) {

        OpenCloseHordeModeSelectButtons(true);
        return;

        if (!ES3.FileExists("HordeLevelSave.es3")) {
            NewHordeModeGame(changeCameraTarget);
        } else {
            if (hasResetHordeMode) {
                NewHordeModeGame(changeCameraTarget);
            } else {
                OpenCloseHordeModeSelectButtons(true);
            }
        }
    }

    public void NewHordeModeGame_ButtonConfirm() {
        if(justResetEntireHordeProgression || !ES3.FileExists("HordeLevelSave.es3")) {
            NewHordeModeGame(true);
            return;
        }

        if (confirmHordeModeResetProgression) {
            hasResetHordeMode = true;
            ES3.DeleteFile("HordeLevelSave.es3");
            NewHordeModeGame();
        }
        else {
            confirmHordeModeResetProgression = true;
            newHordeModeGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetHordeMode");
        }
    }

    public void ResetHordeProgression_ButtonConfirm() {
        if (confirmHordeModeResetEntireProgression) {

            ES3.DeleteFile("SaveFile_HordeMode.es3");
            ES3.DeleteFile("HordeLevelSave.es3");
            buttonConfirm_ResetHordeModeEntireProgression.GetComponent<Button>().interactable = false;
            resetEntireHordeModeProgressionText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetEntireHordeMode");
            RefreshHordeModeContinueVsNewGameButtonEnabledAndFonts();
            HordeModeProgressionManager.Instance.ResetHordeProgression();

            ES3Settings settings = new ES3Settings("SaveFile_HordeMode.es3");
            ES3.Save("ResettedHordeMode", true, settings);

            justResetEntireHordeProgression = true;
            OnHordeModeProgressionReset?.Invoke(this, EventArgs.Empty);

        }
        else {

            confirmHordeModeResetEntireProgression = true;
            resetEntireHordeModeProgressionText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetEntireHordeMode2");

        }
    }

    private void RefreshHordeModeContinueVsNewGameButtonEnabledAndFonts() {
        if (!ES3.FileExists("HordeLevelSave.es3")) {

            hordeMode_continueGameButton.interactable = false;
            newHordeModeGameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
            continueHordeModeGameText.fontMaterial = LocalizationManager.Instance.GetStandardMaterial();

        }
        else {

            hordeMode_continueGameButton.interactable = true;
            newHordeModeGameText.fontMaterial = LocalizationManager.Instance.GetStandardMaterial();
            continueHordeModeGameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();

        }

        if(!HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Armorer)) {
            buttonConfirm_ResetHordeModeEntireProgression.GetComponent<Button>().interactable = false;
        }
    }

    public void NewHordeModeGame(bool changeCameraTarget = true) {
        HordeModeUI.Instance.OpenHordeModePanel(changeCameraTarget);
        HideMainMenuButtons();
    }

    public void StartHordeMode() {
        if (loadingHordeMode) return;
        loadingHordeMode = true;
        StartCoroutine(StartHordeModeCoroutine());
    }

    public void BackFromHordeModeMenu() {
        OpenCloseHordeModeSelectButtons(false);
    }

    public void OpenCloseHordeModeSelectButtons(bool open) {
        selectingHordeModeContinueOrNewGame = open;
        hordeModeMenuGO.SetActive(open);
        standardMenuGO.SetActive(!open);

        if(open) {

            if(!ES3.FileExists("HordeLevelSave.es3")) {
                EventSystem.current.SetSelectedGameObject(hordeMode_newGameGameButton.gameObject);
            } else {
                EventSystem.current.SetSelectedGameObject(hordeMode_continueGameButton.gameObject);
            }


        } else {
            EventSystem.current.SetSelectedGameObject(hordeModeButton.gameObject);
        }
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

        OnGameStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        MusicManager.Instance.FadeOutMusic(1f);
        MetaProgressionManager.Instance.SetSavedOnce();

        SceneLoader.Instance.LoadTutorial(2f);
    }

    private IEnumerator StartHordeModeCoroutine() {
        OnGameStart?.Invoke(this, EventArgs.Empty);
        yield return new WaitForEndOfFrame();

        MusicManager.Instance.FadeOutMusic(1f);
        MetaProgressionManager.Instance.SetSavedOnce();

        SceneLoader.Instance.LoadHordeMode(2f);
    }

    private IEnumerator ResumeCurrentSaveCoroutine() {
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        OnGameStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

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
        //Debug.Log("ButtonConfirm_ResetProgression_OnButtonDeselected");
        confirmResetProgression = false;
        newGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_newGame");
    }

    private void ButtonConfirm_ResetHordeModeProgression_OnButtonDeselected(object sender, EventArgs e) {
        confirmHordeModeResetProgression = false;
        newHordeModeGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_startHordeMode");

        RefreshHordeModeContinueVsNewGameButtonEnabledAndFonts();
    }

    private void ButtonConfirm_ResetHordeModeEntireProgression_OnButtonDeselected(object sender, EventArgs e) {
        confirmHordeModeResetEntireProgression = false;
        resetEntireHordeModeProgressionText.text = LocalizationManager.Instance.GetLocalizedText("menu_resetEntireHordeMode");
    }

    private void InitializeButtonNavigation() {
        Navigation wishlishButtonNav = wishlistButton_Menu.navigation;
        Navigation wishlishButtonFullGamePanelNav = wishlistButton_FullGamePanel.navigation;
        Navigation newGameButtonNav = newGameButton.navigation;
        Navigation resumeGameButtonNav = continueButton.navigation;
        Navigation discordButtonNav = discordButton.navigation;
        Navigation settingsButtonNav = settingsButton.navigation;


        if (!MetaProgressionManager.Instance.GetSavedOnce()) {
            discordButtonNav.selectOnLeft = newGameButton;
            discordButton.navigation = discordButtonNav;
        }
        else {
            discordButtonNav.selectOnLeft = continueButton;
            discordButton.navigation = discordButtonNav;

        }


        if (!VersioningManager.Instance.GetHordeModeImplemented() || VersioningManager.Instance.GetIsDemo()) {
            newGameButtonNav.selectOnDown = settingsButton;
            settingsButtonNav.selectOnUp = newGameButton;

            settingsButton.navigation = settingsButtonNav;
            newGameButton.navigation = newGameButtonNav;
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
        mainMenuPanelAnimator.ResetTrigger("FadeOut");
        mainMenuPanelOpen = true;
    }

    public void HideMainMenuButtons() {
        mainMenuPanelAnimator.SetTrigger("FadeOut");
        mainMenuPanelAnimator.ResetTrigger("FadeIn");
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
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }

}
