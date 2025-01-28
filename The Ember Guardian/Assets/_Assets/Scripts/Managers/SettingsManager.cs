using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [SerializeField] private float sfxVolume = .5f;
    [SerializeField] private float musicVolume = .5f;

    public event EventHandler OnSfxVolumeChanged;
    public event EventHandler OnMusicVolumeChanged;

    private void Awake() {
        Instance = this;
        sfxVolume = ES3.Load("sfxVolume", .5f);
        musicVolume = ES3.Load("musicVolume", .5f);
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
}
