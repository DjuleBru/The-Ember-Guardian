using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public enum Language {
        English,
        French,
    }

    [SerializeField] private float sfxVolume = .5f;
    [SerializeField] private float musicVolume = .5f;

    public event EventHandler OnSfxVolumeChanged;
    public event EventHandler OnMusicVolumeChanged;

    public event EventHandler OnHoldToggleRunChanged;
    public event EventHandler OnAutoSwitchLightGunChanged;
    public event EventHandler OnAutoAlignAimWithMovementChanged;
    public event EventHandler OnControllerVibrationsChanged;
    public event EventHandler OnFullScreenChanged;
    public event EventHandler OnAimAssistChanged;
    public event EventHandler OnLanguageChanged;


    private Language currentLanguage = Language.English;
    private bool holdToRun;
    private bool aimAssist;
    private bool autoAlignAimWithMovement;
    private bool autoSwitchLightGun;
    private bool controllerVibrations;
    private bool fullScreen;

    private void Awake() {
        Instance = this;

        LoadSettings();
    }

    private void LoadSettings() {
        sfxVolume = ES3.Load("sfxVolume", .5f);
        musicVolume = ES3.Load("musicVolume", .5f);

        currentLanguage = ES3.Load("currentLanguage", Language.English);
        holdToRun = ES3.Load("holdToRun", true);
        aimAssist = ES3.Load("aimAssist", true);
        autoAlignAimWithMovement = ES3.Load("autoAlignAimWithMovement", true);
        autoSwitchLightGun = ES3.Load("autoSwitchLightGun", true);
        controllerVibrations = ES3.Load("controllerVibrations", true);
        fullScreen = ES3.Load("fullScreen", true);
    }

    public float GetSfxVolume() {
        return sfxVolume;
    }

    public float GetMusicVolume() {
        return musicVolume;
    }

    public void SetSfxVolume(float newSfxVolume) {
        sfxVolume = newSfxVolume;
        ES3.Save("sfxVolume", newSfxVolume);
        OnSfxVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetMusicVolume(float newMusicVolume) {
        musicVolume = newMusicVolume;
        ES3.Save("musicVolume", newMusicVolume);
        OnMusicVolumeChanged?.Invoke(this, EventArgs.Empty);
    }

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

    public Language GetLanguage() {
        return currentLanguage;
    }


    public void SetLanguage(Language language) {
        currentLanguage = language;
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("currentLanguage", currentLanguage);
    }

    public void ChangeHoldToggleRun() {
        holdToRun = !holdToRun;
        OnHoldToggleRunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("holdToRun", holdToRun);
    }
    public void ChangeAutoSwitchGunLight() {
        autoSwitchLightGun = !autoSwitchLightGun;
        OnAutoSwitchLightGunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoSwitchLightGun", autoSwitchLightGun);
    }
    public void ChangeAutoAlignAimWithMovement() {
        autoAlignAimWithMovement = !autoAlignAimWithMovement;
        OnAutoAlignAimWithMovementChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoAlignAimWithMovement", autoAlignAimWithMovement);
    }
    public void ChangeControllerVibrations() {
        controllerVibrations = !controllerVibrations;
        OnControllerVibrationsChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("controllerVibrations", controllerVibrations);
    }

    public void ChangeScreenMode() {
        fullScreen = !fullScreen;
        OnFullScreenChanged?.Invoke(this, EventArgs.Empty);

        Screen.fullScreen = fullScreen;

        ES3.Save("fullScreen", fullScreen);
    }

    public void ChangeAimAssist() {
        aimAssist = !aimAssist;
        OnAimAssistChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("aimAssist", aimAssist);
    }

}
