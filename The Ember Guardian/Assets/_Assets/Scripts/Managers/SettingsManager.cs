using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = .5f;
    [SerializeField] private float musicVolume = .5f;
    [SerializeField] private float dogVolume = .5f;

    private float gammaLevel = 0f;

    public event EventHandler OnMasterVolumeChanged;
    public event EventHandler OnSfxVolumeChanged;
    public event EventHandler OnMusicVolumeChanged;
    public event EventHandler OnDogVolumeChanged;

    public event EventHandler OnHoldToggleRunChanged;
    public event EventHandler OnAutoSwitchLightGunChanged;
    public event EventHandler OnAutoAlignAimWithMovementChanged;
    public event EventHandler OnControllerVibrationsChanged;
    public event EventHandler OnFullScreenChanged;
    public event EventHandler OnAimAssistChanged;
    public event EventHandler OnLanguageChanged;
    public event EventHandler OnAutoReloadChanged;
    public event EventHandler OnSteamerModeChanged;
    public event EventHandler OnShowDamageNumbersChanged;
    public event EventHandler OnWaterPerspectiveChanged;


    private LocalizationManager.Language currentLanguage = LocalizationManager.Language.English;
    private bool holdToRun;
    private bool aimAssist;
    private bool autoAlignAimWithMovement;
    private bool autoSwitchLightGun;
    private bool controllerVibrations;
    private bool fullScreen;
    private bool autoReload;
    private bool streamerMode;
    private bool showDamageNumbers;
    private bool waterPerspective;

    private float settingsVersion;

    private ES3Settings settingsSaveFileSettings;

    private void Awake() {
        Instance = this;
        settingsSaveFileSettings = new ES3Settings("Settings.es3");
        settingsVersion = ES3.Load("settingsVersion", 0f, settingsSaveFileSettings);
        LoadSettings();
    }

    private void LoadSettings() {
        // Create a new ES3Settings to enable encryption.

        masterVolume = ES3.Load("masterVolume", 1f, settingsSaveFileSettings);
        sfxVolume = ES3.Load("sfxVolume", .5f, settingsSaveFileSettings);
        musicVolume = ES3.Load("musicVolume", .5f, settingsSaveFileSettings);
        dogVolume = ES3.Load("dogVolume", .5f, settingsSaveFileSettings);

        gammaLevel = ES3.Load("gammaLevel", 0f, settingsSaveFileSettings);

        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
        holdToRun = ES3.Load("holdToRun", true, settingsSaveFileSettings);
        aimAssist = ES3.Load("aimAssist", true, settingsSaveFileSettings);
        autoAlignAimWithMovement = ES3.Load("autoAlignAimWithMovement", true, settingsSaveFileSettings);
        autoSwitchLightGun = ES3.Load("autoSwitchLightGun", true, settingsSaveFileSettings);
        controllerVibrations = ES3.Load("controllerVibrations", true, settingsSaveFileSettings);
        fullScreen = ES3.Load("fullScreen", true, settingsSaveFileSettings);
        autoReload = ES3.Load("autoReload", true, settingsSaveFileSettings);
        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
        streamerMode = ES3.Load("steamerMode", false, settingsSaveFileSettings);
        showDamageNumbers = ES3.Load("showDamageNumbers", true, settingsSaveFileSettings);
        waterPerspective = ES3.Load("waterPerspective", true, settingsSaveFileSettings);

        if (settingsVersion < 0.9) {
            // Mise à jour vers la version 0.9 : autoReload passe à true
            autoReload = true;
            ES3.Save("autoReload", true, settingsSaveFileSettings);
            ES3.Save("settingsVersion", 0.9f, settingsSaveFileSettings);
        }
    }

    #region SET SETTINGS
    public void SetMasterVolume(float newMasterVolume) {
        masterVolume = newMasterVolume;
        ES3.Save("masterVolume", newMasterVolume, settingsSaveFileSettings);
        OnMasterVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetMusicVolume(float newMusicVolume) {
        musicVolume = newMusicVolume;
        ES3.Save("musicVolume", newMusicVolume, settingsSaveFileSettings);
        OnMusicVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSfxVolume(float newSfxVolume) {
        sfxVolume = newSfxVolume;
        ES3.Save("sfxVolume", newSfxVolume, settingsSaveFileSettings);
        OnSfxVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDogVolume(float newDogVolume) {
        dogVolume = newDogVolume;
        ES3.Save("dogVolume", newDogVolume, settingsSaveFileSettings);
        OnDogVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetGammaLevel(float newGammaLevel) {
        gammaLevel = newGammaLevel;
        ES3.Save("gammaLevel", newGammaLevel, settingsSaveFileSettings);
    }
   

    public void SetLanguage(LocalizationManager.Language language) {
        currentLanguage = language;
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("currentLanguage", currentLanguage, settingsSaveFileSettings);
    }

    public void ChangeHoldToggleRun() {
        holdToRun = !holdToRun;
        OnHoldToggleRunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("holdToRun", holdToRun, settingsSaveFileSettings);
    }
    public void ChangeAutoSwitchGunLight() {
        autoSwitchLightGun = !autoSwitchLightGun;
        OnAutoSwitchLightGunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoSwitchLightGun", autoSwitchLightGun, settingsSaveFileSettings);
    }
    public void ChangeAutoAlignAimWithMovement() {
        autoAlignAimWithMovement = !autoAlignAimWithMovement;
        OnAutoAlignAimWithMovementChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoAlignAimWithMovement", autoAlignAimWithMovement, settingsSaveFileSettings);
    }
    public void ChangeControllerVibrations() {
        controllerVibrations = !controllerVibrations;
        OnControllerVibrationsChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("controllerVibrations", controllerVibrations, settingsSaveFileSettings);
    }

    public void ChangeScreenMode() {
        fullScreen = !fullScreen;
        OnFullScreenChanged?.Invoke(this, EventArgs.Empty);

        Screen.fullScreen = fullScreen;

        ES3.Save("fullScreen", fullScreen, settingsSaveFileSettings);
    }

    public void ChangeAimAssist() {
        aimAssist = !aimAssist;
        OnAimAssistChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("aimAssist", aimAssist, settingsSaveFileSettings);
    }
    public void ChangeAutoReload() {
        autoReload = !autoReload;
        OnAutoReloadChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoReload", autoReload, settingsSaveFileSettings);
    }
    public void ChangeStreamerMode() {
        streamerMode = !streamerMode;
        OnSteamerModeChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("steamerMode", streamerMode, settingsSaveFileSettings);
    }
    public void ChangeShowDamageNumbers() {
        showDamageNumbers = !showDamageNumbers;
        OnShowDamageNumbersChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("showDamageNumbers", showDamageNumbers, settingsSaveFileSettings);
    }
    public void ChangeLanguage() {
        LocalizationManager.Language[] selectableLanguages = new LocalizationManager.Language[] {
        LocalizationManager.Language.English,
        LocalizationManager.Language.French,
        LocalizationManager.Language.German,
        LocalizationManager.Language.Spanish,
        LocalizationManager.Language.Japanese
        // pas de Chinese ici
        };

        // trouver l'index actuel dans cette liste
        int currentIndex = Array.IndexOf(selectableLanguages, currentLanguage);

        // avancer dans la liste
        int nextIndex = (currentIndex + 1) % selectableLanguages.Length;
        currentLanguage = selectableLanguages[nextIndex];

        LocalizationManager.Instance.SetLanguage(currentLanguage);
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("currentLanguage", currentLanguage, settingsSaveFileSettings);
    }

    public void ChangeWaterPerspective() {
        waterPerspective = !waterPerspective;
        Debug.Log("waterPerspective " + waterPerspective);
        ES3.Save("waterPerspective", waterPerspective);

        OnWaterPerspectiveChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
    #region GET SETTINGS
    public bool GetHoldToRun() {
        return holdToRun;
    }
    public bool GetFullScreen() {
        return fullScreen;
    }

    public bool GetAutoSwitchLight() {
        return autoSwitchLightGun;
    }

    public bool GetControllerVibrations() {
        return controllerVibrations;
    }
    public bool GetStreamerMode() {
        return streamerMode;
    }
    public bool GetShowDamageNumbers() {
        return showDamageNumbers;
    }
    public bool GetAlignAimWithMovement() {
        return autoAlignAimWithMovement;
    }

    public bool GetAimAssist() {
        return aimAssist;
    }
    public bool GetAutoReload() {
        return autoReload;
    }



    public LocalizationManager.Language GetLanguage() {
        return currentLanguage;
    }

    public float GetMasterVolume() {
        return masterVolume;
    }

    public float GetDogVolume() {
        return dogVolume;
    }

    public float GetSfxVolume() {
        return sfxVolume;
    }

    public float GetMusicVolume() {
        return musicVolume;
    }
    public float GetGammaLevel() {
        return gammaLevel;
    }

    public bool GetWaterPerspectiveActive() {
        return waterPerspective;
    }
    #endregion
}
