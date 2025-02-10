using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewItemsForSaleSound : SoundObject
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] dingAudioClips;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void ExclamationMarkBounce() {
        audioSource.PlayOneShot(dingAudioClips[UnityEngine.Random.Range(0, dingAudioClips.Length)], sfxVolume);
    }

}
