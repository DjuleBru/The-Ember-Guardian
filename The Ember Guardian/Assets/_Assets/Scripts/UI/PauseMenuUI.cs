using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button saveButton;

    private bool canOpenPauseMenu = true;
    private bool pausePanelOpen;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        pausePanel.SetActive(false);
    }

    public void SaveGame() {
        HUBManager.Instance.SaveHub();
    }

    private void GameInput_OnPlayerPausePerformed(object sender, System.EventArgs e) {
        if (!canOpenPauseMenu) return;

        pausePanelOpen = !pausePanelOpen;
        ShowPauseMenu(pausePanelOpen);
    }

    private void ShowPauseMenu(bool show) {
        pausePanel.SetActive(show);

        if(show) {
            Player.Instance.DisableControlInputs();
            Time.timeScale = 0f;
            AudioListener.pause = true;
        } else {
            Player.Instance.EnableControlInputs();
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

    }

    public void SetCanOpenPauseMenu(bool canOpen) {
        canOpenPauseMenu = canOpen;
    }

    public void SetCanSave(bool canSave) {
        saveButton.interactable = canSave;
    }

    public bool GetGamePaused() {
        return pausePanelOpen;
    }
}
