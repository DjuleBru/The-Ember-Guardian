using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    [SerializeField] protected GameObject controllerBindingsViewPanelGameObject;
    [SerializeField] protected GameObject controllerViewBackButton;
    [SerializeField] protected GameObject controllerMappingBackButton;
    [SerializeField] protected GameObject controllerMappingButton;
    [SerializeField] protected Slider masterVolumeSlider;
    [SerializeField] protected Slider musicVolumeSlider;
    [SerializeField] protected Slider sfxVolumeSlider;
    [SerializeField] protected Slider dogVolumeSlider;
    [SerializeField] protected Slider waterReflectionsSlider;
    [SerializeField] protected Slider zoomLevelSlider;
    [SerializeField] protected Button UIDisplayButton;
    [SerializeField] protected GameObject firstSelectedButton;
    [SerializeField] protected GameObject difficultyButton;
    [SerializeField] protected Animator takesEffectOnReloadAnimator;

    // Valeurs actuelles de volume
    protected float currentMasterVolume;
    protected float currentMusicVolume;
    protected float currentSfxVolume;
    protected float currentDogVolume;
    protected float waterReflectionsLevel;
    protected float zoomLevel;

    protected bool panelOpen;

    protected void Awake() {
        Instance = this;
        settingsPanelGameObject.gameObject.SetActive(false);
        keyboardMappingPanelGameObject.gameObject.SetActive(false);
        controllerMappingPanelGameObject.gameObject.SetActive(false);
        controllerBindingsViewPanelGameObject.gameObject.SetActive(false);
    }

    protected void Start() {
        currentMasterVolume = SettingsManager.Instance.GetMasterVolume();
        currentMusicVolume = SettingsManager.Instance.GetMusicVolume();
        currentSfxVolume = SettingsManager.Instance.GetSfxVolume();
        currentDogVolume = SettingsManager.Instance.GetDogVolume();
        waterReflectionsLevel = WaterManager.Instance.GetWaterReflectionLevel();
        zoomLevel = SettingsManager.Instance.GetZoomLevel();

        // Initialiser les Sliders avec les valeurs actuelles
        masterVolumeSlider.value = currentMasterVolume;
        musicVolumeSlider.value = currentMusicVolume;
        sfxVolumeSlider.value = currentSfxVolume;
        dogVolumeSlider.value = currentDogVolume;
        dogVolumeSlider.value = currentDogVolume;
        waterReflectionsSlider.value = waterReflectionsLevel;
        zoomLevelSlider.value = zoomLevel;

        // Ajouter des listeners pour détecter les changements de valeur
        masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);
        dogVolumeSlider.onValueChanged.AddListener(UpdateDogVolume);
        waterReflectionsSlider.onValueChanged.AddListener(UpdateWaterReflections);
        zoomLevelSlider.onValueChanged.AddListener(UpdateZoomLevel);

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        if(VersioningManager.Instance.GetIsDemo()) {
            difficultyButton.gameObject.SetActive(false);
            Navigation zoomSliderNav = zoomLevelSlider.navigation;
            Navigation UIDisplayNav = UIDisplayButton.navigation;

            zoomSliderNav.selectOnDown = UIDisplayButton;
            UIDisplayNav.selectOnUp = zoomLevelSlider;

            zoomLevelSlider.navigation = zoomSliderNav;
            UIDisplayButton.navigation = UIDisplayNav;
        }
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (!panelOpen) return;
        if (PauseMenuUI.Instance.GetRebindingKeys()) return;

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

    protected void UpdateWaterReflections(float value) {
        waterReflectionsLevel = value;

        //StartCoroutine(SetTakesEffectOnReloadAnimatorAfterFrame());
        WaterManager.Instance.SetReflectionLevel(waterReflectionsLevel);
    }

    protected void UpdateZoomLevel(float value) {
        zoomLevel = value;

        SettingsManager.Instance.SetZoomLevel(value);
    }

    private IEnumerator SetTakesEffectOnReloadAnimatorAfterFrame() {
        AnimatorStateInfo stateInfo = takesEffectOnReloadAnimator.GetCurrentAnimatorStateInfo(0);
        // Vérifie que l'animator n'est pas déjà dans le state "Blink"

        if (!stateInfo.IsName("Blink")) {
            takesEffectOnReloadAnimator.SetTrigger("Show");
        }

        yield return new WaitForEndOfFrame();

        takesEffectOnReloadAnimator.ResetTrigger("Show");

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
        KeyRebindingUI_Controller.Instance_Controller.UpdateVisual();
        EventSystem.current.SetSelectedGameObject(controllerMappingBackButton);
    }

    public void CloseControllerMappingGameObject() {
        controllerMappingPanelGameObject.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(controllerMappingButton);
    }
    public void OpenControllerViewGameObject() {
        controllerBindingsViewPanelGameObject.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controllerViewBackButton);
    }

    public void CloseControllerViewGameObject() {
        controllerBindingsViewPanelGameObject.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(controllerMappingBackButton);
    }



    protected void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}

