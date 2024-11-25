using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantSounds : MonoBehaviour
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

    private float volume = .5f;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        merchantUI.OnNewItemHovered += MerchantUI_OnNewItemHovered;
        merchantUI.OnDescriptionPanelOpened += MerchantUI_OnDescriptionPanelOpened;
        merchantUI.OnPlayerBoughtMajorItem += MerchantUI_OnPlayerBoughtMajorItem;
        merchantUI.OnPlayerBoughtMinorItem += MerchantUI_OnPlayerBoughtMinorItem;

        idleAudioSource.clip = merchantIdleAudioClip;
        idleAudioSource.Play();
    }

    private void MerchantUI_OnPlayerBoughtMinorItem(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(buyMinorItemAudioClip, volume);
    }

    private void MerchantUI_OnPlayerBoughtMajorItem(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(buyMajorItemAudioClip, volume);
    }

    private void MerchantUI_OnDescriptionPanelOpened(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(panelAppearAudioClip, volume);
    }

    private void MerchantUI_OnNewItemHovered(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(hoverItemAudioClip, volume);
    }
}
