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

    private float audioSourceVolume = .2f;

    protected override void Start() {
        base.Start();
        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;

        audioSource2D.volume = sfxVolume * audioSourceVolume;
    }

    private void WindManager_OnWindStrengthChanged(object sender, EventArgs e) {
        AudioClip clip = GetWindAudioClip(WindManager.Instance.GetWindStrength());

        if (clip != null) {
            audioSource2D.clip = clip;
            audioSource2D.Play();

            StartCoroutine(ChangeVolumeGradually(sfxVolume * audioSourceVolume));
        }
        else {
            StartCoroutine(ChangeVolumeGradually(0f, stopAfter: true));
        }
    }

    private IEnumerator ChangeWindAudio(AudioClip newClip) {
        // Réduction progressive du volume avant de changer le clip
        yield return StartCoroutine(ChangeVolumeGradually(0f));

        // Changer le clip et le jouer
        audioSource2D.clip = newClip;
        audioSource2D.Play();

        // Remonter progressivement le volume
        yield return StartCoroutine(ChangeVolumeGradually(sfxVolume * audioSourceVolume));
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        base.SettingsManager_OnSfxVolumeChanged(sender, e);

        audioSource2D.volume = sfxVolume * audioSourceVolume;
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
