using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [SerializeField] private float sfxVolume = .5f;
    [SerializeField] private float musicVolume = .5f;
    private float gammaLevel = 0f;

    public event EventHandler OnSfxVolumeChanged;
    public event EventHandler OnMusicVolumeChanged;

    public event EventHandler OnHoldToggleRunChanged;
    public event EventHandler OnAutoSwitchLightGunChanged;
    public event EventHandler OnAutoAlignAimWithMovementChanged;
    public event EventHandler OnControllerVibrationsChanged;
    public event EventHandler OnFullScreenChanged;
    public event EventHandler OnAimAssistChanged;
    public event EventHandler OnLanguageChanged;
    public event EventHandler OnAutoReloadChanged;


    private LocalizationManager.Language currentLanguage = LocalizationManager.Language.English;
    private bool holdToRun;
    private bool aimAssist;
    private bool autoAlignAimWithMovement;
    private bool autoSwitchLightGun;
    private bool controllerVibrations;
    private bool fullScreen;
    private bool autoReload;

    private ES3Settings settingsSaveFileSettings;

    private void Awake() {
        Instance = this;
        settingsSaveFileSettings = new ES3Settings("Settings.es3");
        LoadSettings();
    }

    private void LoadSettings() {
        // Create a new ES3Settings to enable encryption.

        sfxVolume = ES3.Load("sfxVolume", .5f, settingsSaveFileSettings);

        musicVolume = ES3.Load("musicVolume", .5f, settingsSaveFileSettings);
        gammaLevel = ES3.Load("gammaLevel", 0f, settingsSaveFileSettings);

        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
        holdToRun = ES3.Load("holdToRun", true, settingsSaveFileSettings);
        aimAssist = ES3.Load("aimAssist", true, settingsSaveFileSettings);
        autoAlignAimWithMovement = ES3.Load("autoAlignAimWithMovement", true, settingsSaveFileSettings);
        autoSwitchLightGun = ES3.Load("autoSwitchLightGun", true, settingsSaveFileSettings);
        controllerVibrations = ES3.Load("controllerVibrations", true, settingsSaveFileSettings);
        fullScreen = ES3.Load("fullScreen", true, settingsSaveFileSettings);
        autoReload = ES3.Load("autoReload", false, settingsSaveFileSettings);
        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
    }

    #region SET SETTINGS
    public void SetSfxVolume(float newSfxVolume) {
        sfxVolume = newSfxVolume;
        ES3.Save("sfxVolume", newSfxVolume, settingsSaveFileSettings);
        OnSfxVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetMusicVolume(float newMusicVolume) {
        musicVolume = newMusicVolume;
        ES3.Save("musicVolume", newMusicVolume, settingsSaveFileSettings);
        OnMusicVolumeChanged?.Invoke(this, EventArgs.Empty);
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

    public void ChangeLanguage() {
        int nextIndex = ((int)currentLanguage + 1) % System.Enum.GetValues(typeof(LocalizationManager.Language)).Length;
        currentLanguage = (LocalizationManager.Language)nextIndex;

        LocalizationManager.Instance.SetLanguage(currentLanguage);
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("currentLanguage", currentLanguage, settingsSaveFileSettings);
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
    public float GetSfxVolume() {
        return sfxVolume;
    }

    public float GetMusicVolume() {
        return musicVolume;
    }
    public float GetGammaLevel() {
        return gammaLevel;
    }
    #endregion
}
