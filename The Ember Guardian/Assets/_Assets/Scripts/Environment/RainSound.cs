using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAudioSource;
using UnityEngine;

public class RainSound : SoundObject
{

    [SerializeField] private AudioClip sparseRainAudioClip;
    [SerializeField] private AudioClip mediumRainAudioClip;
    [SerializeField] private AudioClip strongRainAudioClip;
    [SerializeField] private AudioClip extremeRainAudioClip;

    private float sparseRainAudioVolume = .6f;
    private float mediumRainAudioVolume = .4f;
    private float strongRainAudioVolume = .4f;
    private float extremeRainAudioVolume = .4f;

    protected override void Start()
    {
        base.Start();
        RainManager.Instance.OnRainIntensityChanged += WindManager_OnRainIntensityChanged;

        audioSource2D.volume = 0;
    }

    private void WindManager_OnRainIntensityChanged(object sender, EventArgs e)
    {
        AudioClip clip = GetRainAudioClip(RainManager.Instance.GetRainIntensity());

        StartCoroutine(ChangeVolumeGradually(GetRainAudioVolume(RainManager.Instance.GetRainIntensity()) * sfxVolume));

        if (clip != null)
        {
            audioSource2D.clip = clip;
            audioSource2D.Play();
        }

    }
    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e)
    {
        base.SettingsManager_OnSfxVolumeChanged(sender, e);

        audioSource2D.volume = sfxVolume;
    }

    private AudioClip GetRainAudioClip(RainManager.RainIntensity intensity)
    {
        if (intensity == RainManager.RainIntensity.sparse)
        {
            return sparseRainAudioClip;
        }
        if (intensity == RainManager.RainIntensity.medium)
        {
            return mediumRainAudioClip;
        }
        if (intensity == RainManager.RainIntensity.strong)
        {
            return strongRainAudioClip;
        }
        if (intensity == RainManager.RainIntensity.extreme)
        {
            return extremeRainAudioClip;
        }
        return null;
    }
    private float GetRainAudioVolume(RainManager.RainIntensity intensity)
    {
        if (intensity == RainManager.RainIntensity.sparse)
        {
            return sparseRainAudioVolume;
        }
        if (intensity == RainManager.RainIntensity.medium)
        {
            return mediumRainAudioVolume;
        }
        if (intensity == RainManager.RainIntensity.strong)
        {
            return strongRainAudioVolume;
        }
        if (intensity == RainManager.RainIntensity.extreme)
        {
            return extremeRainAudioVolume;
        }
        return 0;
    }

    private void OnDestroy()
    {
        RainManager.Instance.OnRainIntensityChanged -= WindManager_OnRainIntensityChanged;
    }
}
