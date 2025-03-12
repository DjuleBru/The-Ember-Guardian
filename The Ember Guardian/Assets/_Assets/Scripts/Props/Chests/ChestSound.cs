using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSound : SoundObject
{
    private Chest chest;
    private AudioSource audioSource;

    [SerializeField] private AudioClip startOpenChestAudioClip;
    [SerializeField] private AudioClip startOpenChestAudioClip_AmmoChest;
    [SerializeField] private AudioClip unlockChestAudioClip;


    private void Awake() {
        chest = GetComponentInParent<Chest>();
        audioSource = GetComponent<AudioSource>();
    }
    protected override void Start() {
        base.Start();
        chest.OnChestOpened += Chest_OnChestOpened;
        chest.OnChestUnlocked += Chest_OnChestUnlocked;
    }


    private void Chest_OnChestUnlocked(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(unlockChestAudioClip, .75f * sfxVolume);
    }

    private void Chest_OnChestOpened(object sender, System.EventArgs e) {
        AudioClip audioClip = startOpenChestAudioClip;

        if(chest.GetChestType() == Chest.ChestType.ammoChest) {
            audioClip = startOpenChestAudioClip_AmmoChest;
        }
        audioSource.PlayOneShot(audioClip, .75f * sfxVolume);

    }
}
