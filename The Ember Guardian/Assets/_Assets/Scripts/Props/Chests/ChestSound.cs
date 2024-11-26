using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSound : MonoBehaviour
{
    private Chest chest;
    private AudioSource audioSource;
    [SerializeField] private AudioClip startOpenChestAudioClip;
    [SerializeField] private AudioClip unlockChestAudioClip;

    private float sfxVolume;

    private void Awake() {
        chest = GetComponentInParent<Chest>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        chest.OnChestOpened += Chest_OnChestOpened;
        chest.OnChestUnlocked += Chest_OnChestUnlocked;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void Chest_OnChestUnlocked(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(unlockChestAudioClip, .75f * sfxVolume);
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(startOpenChestAudioClip, .75f * sfxVolume);

    }
}
