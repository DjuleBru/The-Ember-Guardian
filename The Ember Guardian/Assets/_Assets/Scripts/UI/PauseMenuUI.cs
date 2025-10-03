using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;
    public bool isPaused {  get; protected set; }

    [SerializeField] protected GameObject firstSelectedButton;
    [SerializeField] protected GameObject pausePanel;
    [SerializeField] protected GameObject fullGameDescriptionPanel;
    [SerializeField] protected GameObject fullGameDescriptionPanel_WishlistButton;
    [SerializeField] protected GameObject fullGameDescriptionPanel_ExitGameButton;
    [SerializeField] protected GameObject videoTipsButton;
    [SerializeField] protected GameObject ctaButton;
    [SerializeField] protected Button_Confirm buttonConfirm_ExitGame;
    [SerializeField] protected Button_Confirm buttonConfirm_MainMenu;
    [SerializeField] protected TextMeshProUGUI exitGameText;
    [SerializeField] protected TextMeshProUGUI backToMenuText;
    [SerializeField] protected TextMeshProUGUI ctaText;
    [SerializeField] protected Material ctaTextFontAsset;
    [SerializeField] protected Material ctaTextFontAsset_JP;

    [SerializeField] protected Animator progressionSavedTextIndicator;
    [SerializeField] protected Color savedTextColor;
    [SerializeField] protected Color unsavedTextColor;
    [SerializeField] protected Color progressionLostTextColor;
    [SerializeField] protected Button saveButton;
    protected bool progressionSaved;

    public event EventHandler OnPauseMenuOpened;
    public event EventHandler OnPauseMenuClosed;

    protected bool menuOpen;
    protected bool confirmExitGame;
    protected bool confirmBackToMenu;
    protected bool canOpenPauseMenu = true;

    protected void Awake() {
        Instance = this;
    }

    protected virtual void Start() {
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        SettingsManager.Instance.OnLanguageChanged += SettingsManager_OnLanguageChanged;

        buttonConfirm_ExitGame.OnButtonDeselected += ButtonConfirm_ExitGame_OnButtonDeselected;
        buttonConfirm_MainMenu.OnButtonDeselected += ButtonConfirm_MainMenu_OnButtonDeselected;
        buttonConfirm_ExitGame.OnButtonDeHovered += ButtonConfirm_ExitGame_OnButtonDeHovered;
        buttonConfirm_MainMenu.OnButtonDeHovered += ButtonConfirm_MainMenu_OnButtonDeHovered;

        pausePanel.SetActive(false);
        fullGameDescriptionPanel.SetActive(false);

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {

            SetCanSave(false);
         
        } else {

            UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
            UICurrencyManager.HubInventoryUI.OnCurrencyRemovedFromBag += HubInventoryUI_OnCurrencyRemovedFromBag;
            HUBManager.Instance.OnHubSaved += HubManager_OnHubSaved;

        }

        if (!VersioningManager.Instance.GetIsDemo()) {
            ctaButton.gameObject.SetActive(false);
            videoTipsButton.gameObject.SetActive(true);
        } else {
            ctaButton.gameObject.SetActive(true);
            videoTipsButton.gameObject.SetActive(false);
        }

        backToMenuText.text = LocalizationManager.Instance.GetLocalizedText("menu_mainMenu");
        exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_exitGame");

        RefreshFonts();
    }


    private void SettingsManager_OnLanguageChanged(object sender, EventArgs e) {
        RefreshFonts();
    }

    private void RefreshFonts() {
        backToMenuText.font = LocalizationManager.Instance.GetCurrentFont();
        exitGameText.font = LocalizationManager.Instance.GetCurrentFont();

        if(SettingsManager.Instance.GetLanguage() == LocalizationManager.Language.Japanese) {
            ctaText.fontMaterial = ctaTextFontAsset_JP;
        } else {
            ctaText.fontMaterial = ctaTextFontAsset;
        }
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if(GameInput.Instance.IsUsingGamepad() && menuOpen) {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }


    private void HubInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        SetProgressionSaved(false);
    }

    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        SetProgressionSaved(false);
    }

    private void ButtonConfirm_MainMenu_OnButtonDeselected(object sender, EventArgs e) {
        confirmBackToMenu = false;
        backToMenuText.text = LocalizationManager.Instance.GetLocalizedText("menu_mainMenu");
        backToMenuText.font = LocalizationManager.Instance.GetCurrentFont();
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    private void ButtonConfirm_MainMenu_OnButtonDeHovered(object sender, EventArgs e) {
        confirmBackToMenu = false;
        backToMenuText.text = LocalizationManager.Instance.GetLocalizedText("menu_mainMenu");
        backToMenuText.font = LocalizationManager.Instance.GetCurrentFont();
        progressionSavedTextIndicator.SetTrigger("Hide");
    }


    private void ButtonConfirm_ExitGame_OnButtonDeselected(object sender, EventArgs e) {
        confirmExitGame = false;
        exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_exitGame");
        exitGameText.font = LocalizationManager.Instance.GetCurrentFont();
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    private void ButtonConfirm_ExitGame_OnButtonDeHovered(object sender, EventArgs e) {
        confirmExitGame = false;
        exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_exitGame");
        exitGameText.font = LocalizationManager.Instance.GetCurrentFont();
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (PlayerTabMenuUI.Instance.GetTabMenuOpen()) return;

        if (VideoTipUI.Instance.GetPanelOpen()) {
            VideoTipUI.Instance.ClosePanel();
        };

        if (isPaused) {
            ShowPauseMenu(false);
            StartCoroutine(ResumePause());
        }
    }

    protected void GameInput_OnPlayerPausePerformed(object sender, System.EventArgs e) {
        if (PlayerTabMenuUI.Instance.GetTabMenuOpen()) return;
        if (!canOpenPauseMenu) return;
        if (SceneLoader.Instance.GetIsCrossfading()) return;

        if (VideoTipUI.Instance.GetPanelOpen()) {
            VideoTipUI.Instance.ClosePanel();
        };

        OpenClosePauseMenu();
    }

    protected void OpenClosePauseMenu() {
        isPaused = !isPaused;
        ShowPauseMenu(isPaused);
    }

    public void HidePauseMenu() {
        pausePanel.SetActive(false);
        isPaused = false;
    }

    public void ForceClosePauseMenu() {
        isPaused = true;
        OpenClosePauseMenu();
    }

    public void ShowPauseMenu(bool show) {
        pausePanel.SetActive(show);

        if(show) {

            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Time.timeScale = 0f;
            AudioListener.pause = true;
            menuOpen = true;
            OnPauseMenuOpened?.Invoke(this, EventArgs.Empty);

        } else {

            fullGameDescriptionPanel.SetActive(false);
            SettingsMenuUI.Instance.CloseSettingsPanel();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            menuOpen = false;
            OnPauseMenuClosed?.Invoke(this, EventArgs.Empty);

        }
    }

    public void ReturnToPauseMenu() {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    #region PAUSE MENU BUTTONS
    public virtual void SaveGameButton() {
        HUBManager.Instance.SaveHub();
    }

    private void HubManager_OnHubSaved(object sender, EventArgs e) {
        progressionSavedTextIndicator.SetTrigger("Show");
        progressionSavedTextIndicator.SetTrigger("Hide");
        SetProgressionSaved(true);
    }

    public virtual void ResumeButton() {
        ShowPauseMenu(false);
        StartCoroutine(ResumePause());
    }

    private IEnumerator ResumePause() {
        yield return new WaitForSeconds(.1f);
        isPaused = false;
    }

    public virtual void SettingsButton() {
        SettingsMenuUI.Instance.OpenSettingsPanel();
    }

    public virtual void ExitGameButton() {
        if (confirmExitGame || progressionSaved) {
            OpenFullGameDescriptionPanel();
        }

        else {
            confirmExitGame = true;
            exitGameText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
            SetProgressionSaved(false);
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

    public virtual void ExitGameWithNoConfirmation() {
        Application.Quit();
    }

    public virtual void VideoTipsButton() {
        VideoTipUI.Instance.OpenPanel(true);
        HidePauseMenu();
    }

    public virtual void LoadMainMenu() {
        if (confirmBackToMenu || progressionSaved) {
            SceneLoader.Instance.LoadMainMenu(2f);
            OpenClosePauseMenu();
        }

        else {
            confirmBackToMenu = true;
            backToMenuText.text = LocalizationManager.Instance.GetLocalizedText("menu_confirm");
            SetProgressionSaved(false);
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

    public virtual void ResetProgression() {
        ES3.DeleteFile();
    }

    #endregion

    protected void OpenFullGameDescriptionPanel() {
        fullGameDescriptionPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(fullGameDescriptionPanel_WishlistButton);

    }

    public void SetCanOpenPauseMenu(bool canOpen) {
        canOpenPauseMenu = canOpen;
    }

    public void SetCanOpenPauseMenuAfterFrame(bool canOpen) {
        StartCoroutine(SetCanOpenPauseMenuAfterFrameCoroutine(canOpen));
    }

    public IEnumerator SetCanOpenPauseMenuAfterFrameCoroutine(bool canOpen) {
        yield return new WaitForEndOfFrame();

        canOpenPauseMenu = canOpen;
    }

    public void SetCanSave(bool canSave) {
        saveButton.interactable = canSave;
    }

    public void SetProgressionSaved(bool saved) {
        if(saved) {

            progressionSaved = true;
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedText("menu_progressionSaved");
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().color = savedTextColor;

        } else {

            progressionSaved = false;
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedText("menu_progressionNotSaved");
            progressionSavedTextIndicator.GetComponent<TextMeshProUGUI>().color = unsavedTextColor;

        }
    }

    protected void OnDestroy() {
        GameInput.Instance.OnPlayerPausePerformed -= GameInput_OnPlayerPausePerformed;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }

   
}
