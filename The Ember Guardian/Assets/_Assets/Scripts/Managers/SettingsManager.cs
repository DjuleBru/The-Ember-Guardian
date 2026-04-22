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
    [SerializeField] private float zoomLevel = .8f;

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
    public event EventHandler OnWaterReflectionsChanged;
    public event EventHandler OnZoomLevelChanged;
    public event EventHandler OnUIDisplayChanged;
    public event EventHandler OnDifficultyChanged;
    public event EventHandler OnPhotosensitivityChanged;
    public event EventHandler OnVSyncChanged;
    public event EventHandler OnMaxFPSChanged;

    public enum UIDisplayType {
        Adaptive,
        Essentials,
        Persistent,
    }
    public enum ScreenMode {
        Windowed,
        MaximisedWindow,
        Fullscreen
    }

    public enum AutoAimMode {
        Off,
        FlyingOnly,
        On
    }

    public enum Difficulty {
        Easy,
        Medium,
        Hard
    }

    private Difficulty difficulty;
    private Difficulty hordeDifficulty;

    private UIDisplayType currentUIDisplayType;

    private LocalizationManager.Language currentLanguage = LocalizationManager.Language.English;
    private AutoAimMode currentAutoAimMode = AutoAimMode.On;
    private Resolution resolution;
    private bool holdToRun;
    private bool autoAlignAimWithMovement;
    private bool autoSwitchLightGun;
    private bool controllerVibrations; 
    private ScreenMode currentScreenMode = ScreenMode.Fullscreen;
    private bool autoReload;
    private bool streamerMode;
    private bool showDamageNumbers;
    private bool photosensitivityMode;
    private bool waterReflections;
    private bool vSync;
    private int maxFPS;

    private static bool displaySettingsAppliedThisSession = false;

    private float settingsVersion;

    private ES3Settings settingsSaveFileSettings;

    private void Awake() {
        Instance = this;
        settingsSaveFileSettings = new ES3Settings("Settings.es3");
        settingsVersion = ES3.Load("settingsVersion", 0f, settingsSaveFileSettings);

        LoadSettings();

        ApplyFrameSettings();
    }

    private void LoadSettings() {
        // Create a new ES3Settings to enable encryption.

        masterVolume = ES3.Load("masterVolume", 1f, settingsSaveFileSettings);
        sfxVolume = ES3.Load("sfxVolume", .5f, settingsSaveFileSettings);
        musicVolume = ES3.Load("musicVolume", .5f, settingsSaveFileSettings);
        dogVolume = ES3.Load("dogVolume", .5f, settingsSaveFileSettings);
        zoomLevel = ES3.Load("zoomLevel", .8f, settingsSaveFileSettings);

        gammaLevel = ES3.Load("gammaLevel", 0f, settingsSaveFileSettings);
        currentUIDisplayType = ES3.Load("currentUIDisplayType", UIDisplayType.Adaptive, settingsSaveFileSettings);
        difficulty = ES3.Load("difficulty", Difficulty.Medium, settingsSaveFileSettings);
        hordeDifficulty = ES3.Load("hordeDifficulty", Difficulty.Medium, settingsSaveFileSettings);

        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
        holdToRun = ES3.Load("holdToRun", true, settingsSaveFileSettings);
        autoAlignAimWithMovement = ES3.Load("autoAlignAimWithMovement", true, settingsSaveFileSettings);
        autoSwitchLightGun = ES3.Load("autoSwitchLightGun", true, settingsSaveFileSettings);
        controllerVibrations = ES3.Load("controllerVibrations", true, settingsSaveFileSettings);
        currentScreenMode = ES3.Load("currentScreenMode", ScreenMode.Fullscreen, settingsSaveFileSettings);
        autoReload = ES3.Load("autoReload", true, settingsSaveFileSettings);
        currentLanguage = ES3.Load("currentLanguage", LocalizationManager.Language.English, settingsSaveFileSettings);
        streamerMode = ES3.Load("steamerMode", false, settingsSaveFileSettings);
        showDamageNumbers = ES3.Load("showDamageNumbers", true, settingsSaveFileSettings);
        waterReflections = ES3.Load("waterReflections", true, settingsSaveFileSettings);
        photosensitivityMode = ES3.Load("photosensitivityMode", false, settingsSaveFileSettings);

        vSync = ES3.Load("vSync", true, settingsSaveFileSettings);
        maxFPS = ES3.Load("maxFPS", 60, settingsSaveFileSettings);

        currentAutoAimMode = ES3.Load("currentAutoAimMode", AutoAimMode.On, settingsSaveFileSettings);

        Resolution defaultRes = Screen.currentResolution;
        resolution = ES3.Load("resolution", resolution, settingsSaveFileSettings);
        // sécurité : résolution invalide
        if (resolution.width <= 0 || resolution.height <= 0) {
            resolution = defaultRes;
        }

        if(!displaySettingsAppliedThisSession) {
            ApplyScreenMode(currentScreenMode);
            SetResolution(resolution);

            displaySettingsAppliedThisSession = true;
        }

        if (settingsVersion < 0.9) {
            // Mise à jour vers la version 0.9 : autoReload passe à true
            autoReload = true;
            ES3.Save("autoReload", true, settingsSaveFileSettings);
            ES3.Save("settingsVersion", 0.9f, settingsSaveFileSettings);
        }
    }

    private void ApplyFrameSettings() {

        bool vSyncActive = vSync;
        int fps = maxFPS;

        if (vSyncActive) {

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;

        }
        else {

            QualitySettings.vSyncCount = 0;

            if (fps <= 0) {
                Application.targetFrameRate = -1;
            }
            else {
                Application.targetFrameRate = fps;
            }

        }

    }

    #region SET SETTINGS

    public void ChangeVSyncMode() {
        vSync = !vSync;
        ES3.Save("vSync", vSync, settingsSaveFileSettings);
        OnVSyncChanged?.Invoke(this, EventArgs.Empty);

        ApplyFrameSettings();
    }

    public void ChangeMaxFPS(int maxFPS) {
        this.maxFPS = maxFPS;
        ES3.Save("maxFPS", maxFPS, settingsSaveFileSettings);
        OnMaxFPSChanged?.Invoke(this, EventArgs.Empty);

        ApplyFrameSettings();
    }
    public void ChangePhotosensitivityMode() {
        photosensitivityMode = !photosensitivityMode;
        ES3.Save("photosensitivityMode", photosensitivityMode, settingsSaveFileSettings);
        OnPhotosensitivityChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDifficulty(Difficulty difficulty) {
        this.difficulty = difficulty;
        ES3.Save("difficulty", difficulty, settingsSaveFileSettings);
        OnDifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetHordeDifficulty(Difficulty difficulty) {
        this.hordeDifficulty = difficulty;
        ES3.Save("hordeDifficulty", difficulty, settingsSaveFileSettings);
        OnDifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeDifficulty() {
        switch (difficulty) {
            case Difficulty.Easy:
                difficulty = Difficulty.Medium;
                break;
            case Difficulty.Medium:
                difficulty = Difficulty.Hard;
                break;
            case Difficulty.Hard:
                difficulty = Difficulty.Easy;
                break;
        }

        ES3.Save("difficulty", difficulty, settingsSaveFileSettings);
        OnDifficultyChanged?.Invoke(this, EventArgs.Empty);
    }

    public Difficulty GetDifficulty() {
        return difficulty;
    }

    public Difficulty GetHordeDifficulty() {
        return hordeDifficulty;
    }

    public void ChangeUIDisplayType() {
        switch (currentUIDisplayType) {
            case UIDisplayType.Adaptive:
                currentUIDisplayType = UIDisplayType.Essentials;
            break;
            case UIDisplayType.Essentials:
                currentUIDisplayType = UIDisplayType.Persistent;
                break;
            case UIDisplayType.Persistent:
                currentUIDisplayType = UIDisplayType.Adaptive;
                break;
        }

        ES3.Save("currentUIDisplayType", currentUIDisplayType, settingsSaveFileSettings);
        OnUIDisplayChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeAutoAimMode() {

        switch (currentAutoAimMode) {
            case AutoAimMode.On:
                currentAutoAimMode = AutoAimMode.FlyingOnly;
                break;
            case AutoAimMode.FlyingOnly:
                currentAutoAimMode = AutoAimMode.Off;
                break;
            case AutoAimMode.Off:
                currentAutoAimMode = AutoAimMode.On;
                break;
        }

        ES3.Save("currentAutoAimMode", currentAutoAimMode, settingsSaveFileSettings);
        OnAimAssistChanged?.Invoke(this, EventArgs.Empty);
    }

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
    public void SetZoomLevel(float newZoomLevel) {
        zoomLevel = newZoomLevel;
        ES3.Save("zoomLevel", newZoomLevel, settingsSaveFileSettings);
        OnZoomLevelChanged?.Invoke(this, EventArgs.Empty);
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
        // fait tourner entre les 3 modes
        switch (currentScreenMode) {
            case ScreenMode.Windowed:
                currentScreenMode = ScreenMode.MaximisedWindow;
                break;
            case ScreenMode.MaximisedWindow:
                currentScreenMode = ScreenMode.Fullscreen;
                break;
            case ScreenMode.Fullscreen:
                currentScreenMode = ScreenMode.Windowed;
                break;
        }

        ApplyScreenMode(currentScreenMode);
        ES3.Save("currentScreenMode", currentScreenMode, settingsSaveFileSettings);
        OnFullScreenChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyScreenMode(ScreenMode mode) {
        switch (mode) {
            case ScreenMode.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                break;

            case ScreenMode.MaximisedWindow:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                Screen.fullScreen = true;
                break;

            case ScreenMode.Fullscreen:
                Resolution native = Screen.currentResolution;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }

        StartCoroutine(VerifyFullscreenMode());
    }


    private IEnumerator VerifyFullscreenMode() {
        yield return new WaitForSeconds(0.5f);
        if (Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen)
            Debug.LogWarning("Unity n'a pas pu passer en ExclusiveFullScreen (probablement borderless).");
    }

    public ScreenMode GetCurrentScreenMode() => currentScreenMode;
   
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

    public void SetResolution(Resolution res) {
        resolution = res;
        ApplyResolution(res);
        ES3.Save("resolution", resolution, settingsSaveFileSettings);
    }

    private void ApplyResolution(Resolution res) {
        Screen.SetResolution(res.width,res.height,Screen.fullScreenMode,res.refreshRateRatio);
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
        LocalizationManager.Language.Japanese,
        LocalizationManager.Language.Chinese,
        LocalizationManager.Language.Korean,
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

    public void ChangeWaterReflections() {
        waterReflections = !waterReflections;
        Debug.Log("waterReflections " + waterReflections);
        ES3.Save("waterReflections", waterReflections, settingsSaveFileSettings);

        OnWaterReflectionsChanged?.Invoke(this, EventArgs.Empty);
        WaterManager.Instance.RefreshReflectionsActive();
    }

    #endregion

    #region GET SETTINGS
    public bool GetVSyncActive() {
        return vSync;
    }
    public int GetMaxFPS() {
        return maxFPS;
    }
    public bool GetPhotosensitivityMode() {
        return photosensitivityMode;
    }

    public UIDisplayType GetCurrentUIDisplayType() {
        return currentUIDisplayType;
    }

    public bool GetHoldToRun() {
        return holdToRun;
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

    public AutoAimMode GetAutoAimMode() {
        return currentAutoAimMode;
    }
    public bool GetAutoAimActive() {
        return currentAutoAimMode == AutoAimMode.On || currentAutoAimMode == AutoAimMode.FlyingOnly;
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
    public float GetZoomLevel() {
        return zoomLevel;
    }

    public float GetMusicVolume() {
        return musicVolume;
    }
    public float GetGammaLevel() {
        return gammaLevel;
    }

    public bool GetWaterReflectionsActive() {
        return waterReflections;
    }

    #endregion
}
