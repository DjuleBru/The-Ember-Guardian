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
    public event EventHandler OnControllerVibrationsChanged;
    public event EventHandler OnLanguageChanged;


    private Language currentLanguage = Language.English;
    private bool holdToRun;
    private bool autoSwitchLightGun;
    private bool controllerVibrations;

    private void Awake() {
        Instance = this;

        LoadSettings();
    }

    private void LoadSettings() {
        sfxVolume = ES3.Load("sfxVolume", .5f);
        musicVolume = ES3.Load("musicVolume", .5f);

        currentLanguage = ES3.Load("currentLanguage", Language.English);
        holdToRun = ES3.Load("holdToRun", true);
        autoSwitchLightGun = ES3.Load("autoSwitchLightGun", true);
        controllerVibrations = ES3.Load("controllerVibrations", true);
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

    public bool GetHoldToggleRun() {
        return holdToRun;
    }

    public bool GetAutoSwitchLight() {
        return autoSwitchLightGun;
    }

    public bool GetControllerVibrations() {
        return controllerVibrations;
    }
    public Language GetLanguage() {
        return currentLanguage;
    }


    public void SetLanguage(Language language) {
        currentLanguage = language;
        OnLanguageChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("currentLanguage", currentLanguage);
    }

    public void SetHoldToggleRun(bool newHoldToggleRun) {
        holdToRun = newHoldToggleRun;
        OnHoldToggleRunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("holdToRun", holdToRun);
    }

    public void SetAutoSwitchLight(bool newAutoSwitghLightGun) {
        autoSwitchLightGun = newAutoSwitghLightGun;
        OnAutoSwitchLightGunChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("autoSwitchLightGun", autoSwitchLightGun);
    }

    public void SetControllerVibrations(bool newControllerVibrations) {
        controllerVibrations = newControllerVibrations;
        OnControllerVibrationsChanged?.Invoke(this, EventArgs.Empty);

        ES3.Save("controllerVibrations", controllerVibrations);
    }

}
