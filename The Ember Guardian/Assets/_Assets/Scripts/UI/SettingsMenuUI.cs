using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{

    public static SettingsMenuUI Instance;

    [SerializeField] protected GameObject settingsPanelGameObject;
    [SerializeField] protected GameObject keyboardMappingPanelGameObject;
    [SerializeField] protected GameObject keyboardMappingPanelBackButton;
    [SerializeField] protected GameObject keyboardMappingButton;
    [SerializeField] protected GameObject controllerMappingPanelGameObject;
    [SerializeField] protected GameObject controllerMappingBackButton;
    [SerializeField] protected GameObject controllerMappingButton;
    [SerializeField] protected Slider masterVolumeSlider;
    [SerializeField] protected Slider musicVolumeSlider;
    [SerializeField] protected Slider sfxVolumeSlider;
    [SerializeField] protected Slider dogVolumeSlider;
    [SerializeField] protected GameObject firstSelectedButton;

    // Valeurs actuelles de volume
    protected float currentMasterVolume;
    protected float currentMusicVolume;
    protected float currentSfxVolume;
    protected float currentDogVolume;

    protected bool panelOpen;

    protected void Awake() {
        Instance = this;
        settingsPanelGameObject.gameObject.SetActive(false);
        keyboardMappingPanelGameObject.gameObject.SetActive(false);
        controllerMappingPanelGameObject.gameObject.SetActive(false);
    }

    protected void Start() {
        currentMasterVolume = SettingsManager.Instance.GetMasterVolume();
        currentMusicVolume = SettingsManager.Instance.GetMusicVolume();
        currentSfxVolume = SettingsManager.Instance.GetSfxVolume();
        currentDogVolume = SettingsManager.Instance.GetDogVolume();

        // Initialiser les Sliders avec les valeurs actuelles
        masterVolumeSlider.value = currentMasterVolume;
        musicVolumeSlider.value = currentMusicVolume;
        sfxVolumeSlider.value = currentSfxVolume;
        dogVolumeSlider.value = currentDogVolume;

        // Ajouter des listeners pour détecter les changements de valeur
        masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);
        dogVolumeSlider.onValueChanged.AddListener(UpdateDogVolume);

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (!panelOpen) return;
        CloseSettingsPanel();
    }
    protected void UpdateMasterVolume(float value) {
        currentMasterVolume = value;
        // Implémenter ici l'ajustement du volume de la musique (ex: AudioManager)
        SettingsManager.Instance.SetMasterVolume(currentMasterVolume);
    }

    // Méthode pour mettre à jour le volume de la musique
    protected void UpdateMusicVolume(float value) {
        currentMusicVolume = value;
        // Implémenter ici l'ajustement du volume de la musique (ex: AudioManager)
        SettingsManager.Instance.SetMusicVolume(currentMusicVolume);
    }

    // Méthode pour mettre à jour le volume des effets sonores
    protected void UpdateSfxVolume(float value) {
        currentSfxVolume = value;
        // Implémenter ici l'ajustement du volume des effets sonores (ex: AudioManager)
        SettingsManager.Instance.SetSfxVolume(currentSfxVolume);
    }

    protected void UpdateDogVolume(float value) {
        currentDogVolume = value;
        // Implémenter ici l'ajustement du volume des effets sonores (ex: AudioManager)
        SettingsManager.Instance.SetDogVolume(currentDogVolume);
    }

    public void OpenSettingsPanel() {
        panelOpen = true;
        settingsPanelGameObject.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void CloseSettingsPanel() {
        panelOpen = false;
        settingsPanelGameObject.gameObject.SetActive(false);
        controllerMappingPanelGameObject.gameObject.SetActive(false);
        keyboardMappingPanelGameObject.gameObject.SetActive(false);

        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.ShowMainMenuButtons();
        }

        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.ReturnToPauseMenu();
        }
    }

    public void HideSettingsPanel() {
        panelOpen = false;
        settingsPanelGameObject.gameObject.SetActive(false);
        controllerMappingPanelGameObject.gameObject.SetActive(false);
        keyboardMappingPanelGameObject.gameObject.SetActive(false);
    }

    public void OpenKeyboardMappingGameObject() {
        keyboardMappingPanelGameObject.gameObject.SetActive(true);
        KeyRebindingUI.Instance.UpdateVisual();
        EventSystem.current.SetSelectedGameObject(keyboardMappingPanelBackButton.gameObject);
    }

    public void CloseKeyboardMappingGameObject() {
        keyboardMappingPanelGameObject.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(keyboardMappingButton);
    }

    public void OpenControllerMappingGameObject() {
        controllerMappingPanelGameObject.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controllerMappingBackButton);
    }

    public void CloseControllerMappingGameObject() {
        controllerMappingPanelGameObject.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(controllerMappingButton);
    }


    protected void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}

