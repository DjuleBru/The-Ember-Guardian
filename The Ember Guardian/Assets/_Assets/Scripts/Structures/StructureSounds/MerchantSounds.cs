using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantSounds : StructureSounds
{
    private AudioSource audioSource;
    [SerializeField] private AudioSource idleAudioSource;
    [SerializeField] private Merchant merchant;
    [SerializeField] private StructureUI_Merchant merchantUI;
    [SerializeField] private AudioClip panelAppearAudioClip;
    [SerializeField] private AudioClip hoverItemAudioClip;
    [SerializeField] private AudioClip merchantIdleAudioClip;
    [SerializeField] private AudioClip buyMajorItemAudioClip;
    [SerializeField] private AudioClip buyMinorItemAudioClip;

    private float merchantSFXVolume = .5f;
    private float buyMajorItemSFXVolume = 1f;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Start() {
        base.Start();

        merchantUI.OnNewItemHovered += MerchantUI_OnNewItemHovered;
        merchantUI.OnDescriptionPanelOpened += MerchantUI_OnDescriptionPanelOpened;
        merchantUI.OnPlayerBoughtMajorItem += MerchantUI_OnPlayerBoughtMajorItem;
        merchantUI.OnPlayerBoughtMinorItem += MerchantUI_OnPlayerBoughtMinorItem;

        idleAudioSource.clip = merchantIdleAudioClip;
        idleAudioSource.Play();
    }
    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        idleAudioSource.volume = sfxVolume;
    }

    private void MerchantUI_OnPlayerBoughtMinorItem(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(buyMinorItemAudioClip, merchantSFXVolume * sfxVolume);
    }

    private void MerchantUI_OnPlayerBoughtMajorItem(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(buyMajorItemAudioClip, buyMajorItemSFXVolume * sfxVolume);
    }

    private void MerchantUI_OnDescriptionPanelOpened(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(panelAppearAudioClip, merchantSFXVolume * sfxVolume);
    }

    private void MerchantUI_OnNewItemHovered(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(hoverItemAudioClip, merchantSFXVolume * sfxVolume);
    }
}
