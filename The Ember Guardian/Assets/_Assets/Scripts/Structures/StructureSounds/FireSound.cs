using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSound : MonoBehaviour
{
    private Fire fire;
    private AudioSource audioSource;
    private SoundVolume2D volume2D;

    [SerializeField] private AudioClip calmFireAudioClip;
    [SerializeField] private AudioClip mildFireAudioClip;
    [SerializeField] private AudioClip wildFireAudioClip;
    [SerializeField] private AudioClip insaneFireAudioClip;

    [SerializeField] private AudioClip[] orbDroppedInFireAudioClipArray;

    private void Awake() { 
        fire = GetComponentInParent<Fire>();
        audioSource = GetComponent<AudioSource>();
        volume2D = GetComponent<SoundVolume2D>();
        fire.OnFireChangedState += Fire_OnFireChangedState;
        fire.OnFireFuelled += Fire_OnFireFuelled;

    }

    private void Start() {
        Invoke("StartFireSound", 0.1f); // Délai de 0.1 seconde
    }

    private void StartFireSound() {

        audioSource.clip = calmFireAudioClip;
        audioSource.Play();
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(orbDroppedInFireAudioClipArray[Random.Range(0, orbDroppedInFireAudioClipArray.Length)]);
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {

        Debug.Log("Fire_OnFireChangedState " + fire.GetState());

        if (fire.GetState() == Fire.State.calm) {
            audioSource.clip = calmFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetCalmFireRadius());
        }

        if (fire.GetState() == Fire.State.mild) {
            audioSource.clip = mildFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetMildFireRadius());
        }

        if (fire.GetState() == Fire.State.wild) {
            audioSource.clip = wildFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetWildFireRadius());
        }

        if (fire.GetState() == Fire.State.insane) {
            audioSource.clip = insaneFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetInsaneFireRadius());
        }

        if (fire.GetState() == Fire.State.extinguished) {
            audioSource.Pause();
        } else {
            audioSource.Play();
        }
    }
}
