using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimalSounds : SoundObject
{
    [SerializeField] private Animal animal;
    [SerializeField] private AnimalAnimatorManager animatorManager;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float footstepSFXVolume;
    [SerializeField] private float idleSFXVolume;
    [SerializeField] private float damagedSFXVolume;

    protected override void Start() {
        base.Start();
        animal.OnMobDamageTaken += Animal_OnMobDamageTaken;
        animatorManager.OnFootstepTriggered += AnimatorManager_OnFootstepTriggered;
        animatorManager.OnIdleSoundTriggered += AnimatorManager_OnIdleSoundTriggered;

        footstepAudioSource.GetComponent<SoundVolume2D>().SetVolumeMultiplier(footstepSFXVolume);
    }

    private void AnimatorManager_OnIdleSoundTriggered(object sender, System.EventArgs e) {
        PlaySound2D(animal.GetAnimalSO().idleAudioClips, idleSFXVolume * sfxVolume);
    }

    private void Animal_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        PlaySound2D(animal.GetAnimalSO().damagedAudioClips, damagedSFXVolume * sfxVolume);
    }

    private void AnimatorManager_OnFootstepTriggered(object sender, System.EventArgs e) {
        if (!footstepAudioSource.enabled) return;
        if (footstepAudioSource.isPlaying) return;

        footstepAudioSource.clip = animal.GetAnimalSO().footstepAudioClips[UnityEngine.Random.Range(0, animal.GetAnimalSO().footstepAudioClips.Length)];
        footstepAudioSource.Play();
    }
}
