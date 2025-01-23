using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogSounds : SoundObject
{
    [SerializeField] private AudioSource dogAudioSource;
    [SerializeField] private AudioSource dogOtherSFXAudioSource;
    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private AudioClip[] breatheAudioClips;
    [SerializeField] private AudioClip[] sniffAudioClips;
    [SerializeField] private AudioClip[] pantAudioClips;
    [SerializeField] private AudioClip[] groanAudioClips;
    [SerializeField] private AudioClip[] growlAudioClips;
    [SerializeField] private AudioClip[] barkAudioClips;
    [SerializeField] private AudioClip[] biteAudioClips;

    [SerializeField] private DogAnimatorManager dogAnimator; 
    [SerializeField] private DogAI dogAI; 

    private float growlTimer;
    private float growlRate = 2.5f;

    private float barkTimer;
    private float barkRateWhenRunningToAttack = .8f;
    private bool runningToAttack;

    protected override void Start() {
        base.Start();

        dogAnimator.OnFootstepTriggered += PlayerAnimator_OnFootStepTriggered;
        dogAnimator.OnDogSniffed += DogAnimator_OnDogSniffed;
        dogAnimator.OnDogPant += DogAnimator_OnDogPant;
        dogAnimator.OnDogBreathe += DogAnimator_OnDogBreathe;
        dogAnimator.OnDogGroan += DogAnimator_OnDogGroan;
        dogAnimator.OnDogGrowl += DogAnimator_OnDogGrowl;
        dogAnimator.OnDogBark += DogAnimator_OnDogBark;
        dogAnimator.OnDogBite += DogAnimator_OnDogBite;

        dogAI.OnStateChanged += DogAI_OnStateChanged;
    }

    private void DogAI_OnStateChanged(object sender, System.EventArgs e) {
        if(dogAI.GetState() != DogAI.State.growling) {
            dogOtherSFXAudioSource.Stop();
        }

        if(dogAI.GetState() == DogAI.State.attacking) {
            runningToAttack = true;
        }
    }

    private void Update() {
        growlTimer -= Time.deltaTime;

        if(runningToAttack) {
            //barkTimer -= Time.deltaTime;
            //if(barkTimer < 0) {
            //    barkTimer = barkRateWhenRunningToAttack;
            //    dogAudioSource.PlayOneShot(barkAudioClips[Random.Range(0, barkAudioClips.Length)], sfxVolume * .7f);
            //}
        }

    }

    private void DogAnimator_OnDogBite(object sender, System.EventArgs e) {
        runningToAttack = false;

        dogAudioSource.PlayOneShot(biteAudioClips[Random.Range(0, biteAudioClips.Length)], sfxVolume);
    }


    private void DogAnimator_OnDogBark(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(barkAudioClips[Random.Range(0, barkAudioClips.Length)], sfxVolume * .7f);
    }

    private void DogAnimator_OnDogGrowl(object sender, System.EventArgs e) {
        if(growlTimer < 0) {
            dogOtherSFXAudioSource.clip = growlAudioClips[Random.Range(0, growlAudioClips.Length)];
            dogOtherSFXAudioSource.Play();
            dogOtherSFXAudioSource.volume = sfxVolume * .7f;
            growlTimer = growlRate;
        }
    }

    private void DogAnimator_OnDogGroan(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(groanAudioClips[Random.Range(0, groanAudioClips.Length)], sfxVolume * .7f);
    }

    private void DogAnimator_OnDogBreathe(object sender, System.EventArgs e) {
        AudioClip audioClip = breatheAudioClips[Random.Range(0, breatheAudioClips.Length)];
        dogAudioSource.PlayOneShot(audioClip, sfxVolume * .8f);
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(footStepAudioClips[Random.Range(0, footStepAudioClips.Length)], sfxVolume * .5f);
    }

    private void DogAnimator_OnDogPant(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(pantAudioClips[Random.Range(0, pantAudioClips.Length)], sfxVolume * .2f);
        dogAudioSource.PlayOneShot(pantAudioClips[Random.Range(0, pantAudioClips.Length)], sfxVolume * .2f);

    }

    private void DogAnimator_OnDogSniffed(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(sniffAudioClips[Random.Range(0, sniffAudioClips.Length)], sfxVolume * .15f);

    }

}
