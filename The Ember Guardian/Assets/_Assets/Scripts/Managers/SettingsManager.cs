using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 1f;

    public event EventHandler OnSfxVolumeChanged;
    public event EventHandler OnMusicVolumeChanged;

    private void Awake() {
        Instance = this;
    }

    public float GetSfxVolume() {
        return sfxVolume;
    }

    public float GetMusicVolume() {
        return musicVolume;
    }
}
