using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSound : SoundObject
{

    [SerializeField] private AudioClip softWindAudioClip;
    [SerializeField] private AudioClip mediumWindAudioClip;
    [SerializeField] private AudioClip strongWindAudioClip;
    [SerializeField] private AudioClip extremeWindAudioClip;

    private float audioSourceVolume = .4f;

    protected override void Start() {
        base.Start();
        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;

        audioSource2D.volume = sfxVolume * audioSourceVolume * masterVolume;
    }

    private void WindManager_OnWindStrengthChanged(object sender, EventArgs e) {
        AudioClip clip = GetWindAudioClip(WindManager.Instance.GetWindStrength());

        if (clip != null) {
            audioSource2D.clip = clip;
            audioSource2D.Play();

            StartCoroutine(ChangeVolumeGradually(sfxVolume * audioSourceVolume * masterVolume));
        }
        else {

            StartCoroutine(ChangeVolumeGradually(0f, stopAfter: true));

        }
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        base.SettingsManager_OnSfxVolumeChanged(sender, e);

        audioSource2D.volume = sfxVolume * audioSourceVolume * masterVolume;
    }

    private AudioClip GetWindAudioClip(WindManager.WindStrength strength) {
        if(strength == WindManager.WindStrength.soft) {
            return softWindAudioClip;
        }
        if (strength == WindManager.WindStrength.medium) {
            return mediumWindAudioClip;
        }
        if (strength == WindManager.WindStrength.strong) {
            return strongWindAudioClip;
        }
        if (strength == WindManager.WindStrength.extreme) {
            return extremeWindAudioClip;
        }
        return null;
    }

    private void OnDestroy() {
        WindManager.Instance.OnWindStrengthChanged -= WindManager_OnWindStrengthChanged;
    }
}
