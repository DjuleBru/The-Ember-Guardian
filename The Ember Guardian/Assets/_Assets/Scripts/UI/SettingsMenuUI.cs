using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{

    public static SettingsMenuUI Instance;

    [SerializeField] private GameObject settingsPanelGameObject;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private GameObject firstSelectedButton;

    // Valeurs actuelles de volume
    private float currentMusicVolume;
    private float currentSfxVolume;

    private bool panelOpen;

    private void Awake() {
        Instance = this;
        settingsPanelGameObject.gameObject.SetActive(false);
    }

    private void Start() {
        currentMusicVolume = SettingsManager.Instance.GetMusicVolume();
        currentSfxVolume = SettingsManager.Instance.GetSfxVolume();

        // Initialiser les Sliders avec les valeurs actuelles
        musicVolumeSlider.value = currentMusicVolume;
        sfxVolumeSlider.value = currentSfxVolume;

        // Ajouter des listeners pour détecter les changements de valeur
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (!panelOpen) return;
        CloseSettingsPanel();
    }

    // Méthode pour mettre à jour le volume de la musique
    private void UpdateMusicVolume(float value) {
        currentMusicVolume = value;
        // Implémenter ici l'ajustement du volume de la musique (ex: AudioManager)
        SettingsManager.Instance.SetMusicVolume(currentMusicVolume);
    }

    // Méthode pour mettre à jour le volume des effets sonores
    private void UpdateSfxVolume(float value) {
        currentSfxVolume = value;
        // Implémenter ici l'ajustement du volume des effets sonores (ex: AudioManager)
        SettingsManager.Instance.SetSfxVolume(currentSfxVolume);
    }

    public void OpenSettingsPanel() {
        panelOpen = true;
        settingsPanelGameObject.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void CloseSettingsPanel() {
        panelOpen = false;
        settingsPanelGameObject.gameObject.SetActive(false);

        if(MainMenuUI.Instance != null) {
            MainMenuUI.Instance.ShowMainMenuButtons();
        }

        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.ReturnToPauseMenu();
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}

