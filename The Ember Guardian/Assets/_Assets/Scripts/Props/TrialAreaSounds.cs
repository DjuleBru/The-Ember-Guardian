using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialAreaSounds : SoundObject
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip rocksMovingAudioClip;
    [SerializeField] private AudioClip trialStartAudioClip;
    [SerializeField] private AudioClip chestUnlockedAudioClip;

    [SerializeField] private TrialArea trialArea;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Start() {
        base.Start();
        trialArea.OnTrialCompleted += TrialArea_OnTrialCompleted;
        trialArea.OnTrialPaid += TrialArea_OnTrialPaid;
        trialArea.OnTrialWallsLifted += TrialArea_OnTrialWallsLifted;
        trialArea.OnTrialChestUnlocked += TrialArea_OnTrialChestUnlocked;
    }

    private void TrialArea_OnTrialWallsLifted(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(rocksMovingAudioClip);
    }


    private void TrialArea_OnTrialChestUnlocked(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(chestUnlockedAudioClip);
    }

    private void TrialArea_OnTrialPaid(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(trialStartAudioClip);
    }

    private void TrialArea_OnTrialCompleted(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(rocksMovingAudioClip);
    }
}
