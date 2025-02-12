using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectileSounds : SoundObject
{
    [SerializeField] private AudioClip[] projectileSFXAudioClips;
    [SerializeField] private float projectileSFXVolumeMultiplier;
    private AudioSource audioSource;


    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void TriggerProjectileSFX() {
        audioSource.PlayOneShot(projectileSFXAudioClips[Random.Range(0, projectileSFXAudioClips.Length)], projectileSFXVolumeMultiplier * sfxVolume);
    }
}
