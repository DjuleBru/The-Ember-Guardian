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
    [SerializeField] protected Button_Confirm buttonConfirm_ExitGame;
    [SerializeField] protected TextMeshProUGUI exitGameText;

    [SerializeField] protected Animator progressionSavedTextIndicator;
    [SerializeField] protected Button saveButton;

    public event EventHandler OnPauseMenuOpened;
    public event EventHandler OnPauseMenuClosed;

    protected bool menuOpen;
    protected bool confirmExitGame;
    protected bool canOpenPauseMenu = true;

    protected void Awake() {
        Instance = this;
    }

    protected virtual void Start() {
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;

        buttonConfirm_ExitGame.OnButtonDeselected += ButtonConfirm_ExitGame_OnButtonDeselected;

        pausePanel.SetActive(false);

        if(SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {
            SetCanSave(false);
        }
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if(GameInput.Instance.IsUsingGamepad() && menuOpen) {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private void ButtonConfirm_ExitGame_OnButtonDeselected(object sender, EventArgs e) {
        confirmExitGame = false;
        exitGameText.text = "Exit Game";
        progressionSavedTextIndicator.SetTrigger("Hide");
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (isPaused) {
            ShowPauseMenu(false);
            StartCoroutine(ResumePause());
        }
    }

    protected void GameInput_OnPlayerPausePerformed(object sender, System.EventArgs e) {
        if (!canOpenPauseMenu) return;

        OpenClosePauseMenu();
    }

    private void OpenClosePauseMenu() {
        isPaused = !isPaused;
        ShowPauseMenu(isPaused);
    }

    public void ShowPauseMenu(bool show) {
        pausePanel.SetActive(show);

        if(show) {

            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Player.Instance.DisableControlInputs();
            Time.timeScale = 0f;
            AudioListener.pause = true;
            menuOpen = true;
            OnPauseMenuOpened?.Invoke(this, EventArgs.Empty);

        } else {

            Player.Instance.EnableControlInputs();
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
        progressionSavedTextIndicator.SetTrigger("Show");
        progressionSavedTextIndicator.SetTrigger("Hide");
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
        if (confirmExitGame) {
            Application.Quit();
        }
        else {
            HUBManager.Instance.SaveHub();
            confirmExitGame = true;
            exitGameText.text = "Confirm ?";
            progressionSavedTextIndicator.SetTrigger("Show");
        }
    }

    public virtual void ResetProgression() {
        ES3.DeleteFile();
    }

    #endregion
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

    protected void OnDestroy() {
        GameInput.Instance.OnPlayerPausePerformed -= GameInput_OnPlayerPausePerformed;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
