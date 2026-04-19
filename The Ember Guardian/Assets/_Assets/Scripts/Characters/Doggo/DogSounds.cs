using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogSounds : SoundObject
{
    [SerializeField] private DogAI_DarkCompanion darkCompanionAI;
    [SerializeField] private AudioSource dogAudioSource;
    [SerializeField] private AudioSource dogOtherSFXAudioSource;
    [SerializeField] private AudioSource dogRollingAudioSource;
    [SerializeField] private AudioSource dogBreatheAudioSource;
    [SerializeField] private AudioSource dogFootstepsAudioSource;
    [SerializeField] private AudioSource dogPantAudioSource;
    [SerializeField] private AudioClip[] footStepAudioClips;
    [SerializeField] private AudioClip[] breatheAudioClips;
    [SerializeField] private AudioClip[] sniffAudioClips;
    [SerializeField] private AudioClip[] pantAudioClips;
    [SerializeField] private AudioClip[] groanAudioClips;
    [SerializeField] private AudioClip[] groanAudioClips_darkCompanion;
    [SerializeField] private AudioClip[] growlAudioClips;
    [SerializeField] private AudioClip[] growlAudioClips_darkCompanion;
    [SerializeField] private AudioClip[] barkAudioClips;
    [SerializeField] private AudioClip[] barkAudioClips_retreiver;
    [SerializeField] private AudioClip[] barkAudioClips_darkCompanion;
    [SerializeField] private AudioClip[] biteAudioClips;
    [SerializeField] private AudioClip[] biteAudioClips_darkCompanion;

    [SerializeField] private AudioClip[] petStartAudioClips;
    [SerializeField] private AudioClip[] petLoopAudioClips;
    [SerializeField] private AudioClip[] petBarkAudioClips;
    [SerializeField] private AudioClip[] petTapAudioClips;
    [SerializeField] private AudioClip[] skidAudioClips;
    [SerializeField] private AudioClip rollingAudioClip;
    [SerializeField] private AudioClip laserAttackAudioClip;
    [SerializeField] private AudioClip stompAttackAudioClip;

    [SerializeField] private DogAnimatorManager dogAnimator; 

    private float dogVolume;

    private float growlTimer;
    private float growlRate = 3f;

    private float barkTimer;
    private float barkRateWhenRunningToAttack = .8f;
    private bool runningToAttack;
    private bool running;

    private bool pettingDogSFXPlaying;
    private bool pettingDogBarkPlaying;
    private bool pettingDogTapPlaying;


    protected override void Start() {
        base.Start();

        dogFootstepsAudioSource.GetComponent<SoundVolume2D>().SetVolumeMultiplier(.5f);
        dogBreatheAudioSource.GetComponent<SoundVolume2D>().SetVolumeMultiplier(.8f);
        dogPantAudioSource.GetComponent<SoundVolume2D>().SetVolumeMultiplier(.2f);

        dogVolume = SettingsManager.Instance.GetDogVolume();
        SettingsManager.Instance.OnDogVolumeChanged += SettingsManager_OnDogVolumeChanged;

        dogAnimator.OnFootstepTriggered += PlayerAnimator_OnFootStepTriggered;
        dogAnimator.OnDogSniffed += DogAnimator_OnDogSniffed;
        dogAnimator.OnDogPant += DogAnimator_OnDogPant;
        dogAnimator.OnDogBreathe += DogAnimator_OnDogBreathe;
        dogAnimator.OnDogGroan += DogAnimator_OnDogGroan;
        dogAnimator.OnDogGrowl += DogAnimator_OnDogGrowl;
        dogAnimator.OnDogBark += DogAnimator_OnDogBark;
        dogAnimator.OnDogBite += DogAnimator_OnDogBite;
        darkCompanionAI.OnLaserAbilityStarted += DarkCompanionAI_OnLaserAbilityStarted;
        darkCompanionAI.OnStompAbilityStarted += DarkCompanionAI_OnStompAbilityStarted;

        PetDog.Instance.OnPlayerStoppedPettingDog += PetDog_OnPlayerStoppedPettingDog;
        PetDog.Instance.OnPlayerStartedPettingDog += PetDot_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerRefreshedPettingDog += PetDog_OnPlayerRefreshedPettingDog;
        PetDog.Instance.OnPlayerEndedPettingDog += PetDog_OnPlayerEndedPettingDog;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        foreach(DogAI dogAI in Dog.Instance.GetDogAIList()) {
            dogAI.OnStateChanged += DogAI_OnStateChanged;
            dogAI.OnDogBite += DogAI_OnDogBite;
        }

        PreloadAudioClips();
    }

    private void SettingsManager_OnDogVolumeChanged(object sender, System.EventArgs e) {
        dogVolume = SettingsManager.Instance.GetDogVolume();
    }

    private void Dog_OnDogTypeChanged(object sender, System.EventArgs e) {
        dogAudioSource.Stop();
        dogOtherSFXAudioSource.Stop();
        dogRollingAudioSource.Stop();
    }

    private void DarkCompanionAI_OnStompAbilityStarted(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(stompAttackAudioClip, masterVolume * dogVolume);
    }

    private void DarkCompanionAI_OnLaserAbilityStarted(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(laserAttackAudioClip, masterVolume * dogVolume);
    }

    private void DogAI_OnDogBite(object sender, System.EventArgs e) {
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            dogAudioSource.PlayOneShot(biteAudioClips_darkCompanion[Random.Range(0, biteAudioClips_darkCompanion.Length)], masterVolume * dogVolume * .7f);
        }
    }

    private void PetDog_OnPlayerEndedPettingDog(object sender, System.EventArgs e) {
        if(Dog.Instance.GetDogType() == Dog.DogType.GermanShepherd || Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            dogAudioSource.PlayOneShot(petBarkAudioClips[Random.Range(0, petBarkAudioClips.Length)], masterVolume * dogVolume * 3f);
        }
    }

    private void PetDog_OnPlayerRefreshedPettingDog(object sender, System.EventArgs e) {

        if(!pettingDogTapPlaying) {

            AudioClip tapAudioClip = petTapAudioClips[Random.Range(0, petTapAudioClips.Length)];
            dogAudioSource.PlayOneShot(tapAudioClip, masterVolume * dogVolume / 2);
            StartCoroutine(SetTappingDogSFXAfterSFXEnd(tapAudioClip.length * 5f));
        }

        if (!pettingDogSFXPlaying) {

            AudioClip audioClip = petLoopAudioClips[Random.Range(0, petLoopAudioClips.Length)];
            dogAudioSource.PlayOneShot(audioClip, masterVolume * dogVolume * 2);
            StartCoroutine(SetPettingDogSFXAfterSFXEnd(audioClip.length));

        }

        if (!pettingDogBarkPlaying) {

            AudioClip audioClip = null;
            if (Dog.Instance.GetDogType() == Dog.DogType.GermanShepherd || Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver || Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
                audioClip = petBarkAudioClips[Random.Range(0, petBarkAudioClips.Length)];
            }
            if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
                audioClip = barkAudioClips_darkCompanion[Random.Range(0, barkAudioClips_darkCompanion.Length)];
            }

            dogAudioSource.PlayOneShot(audioClip, masterVolume * dogVolume * 2);
            StartCoroutine(SetPettingDogBarkSFXAfterSFXEnd(audioClip.length));

        }

    }

    private void PetDot_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        StartCoroutine(PlayPetDogSFXAfterAnimationStart(.6f));
    }

    private void PetDog_OnPlayerStoppedPettingDog(object sender, System.EventArgs e) {

    }
    private IEnumerator SetTappingDogSFXAfterSFXEnd(float sfxDuration) {
        pettingDogTapPlaying = true;
        yield return new WaitForSeconds(sfxDuration);
        pettingDogTapPlaying = false;
    }

    private IEnumerator PlayPetDogSFXAfterAnimationStart(float animationStartDuration) {
        yield return new WaitForSeconds(animationStartDuration);    
        AudioClip audioClip = petStartAudioClips[Random.Range(0, petStartAudioClips.Length)];
        dogAudioSource.PlayOneShot(audioClip, masterVolume * dogVolume);
        StartCoroutine(SetPettingDogSFXAfterSFXEnd(audioClip.length));
    }

    private IEnumerator SetPettingDogSFXAfterSFXEnd(float sfxDuration) {
        pettingDogSFXPlaying = true;
        yield return new WaitForSeconds(sfxDuration);
        pettingDogSFXPlaying = false;
    }

    private IEnumerator SetPettingDogBarkSFXAfterSFXEnd(float sfxDuration) {
        pettingDogBarkPlaying = true;
        yield return new WaitForSeconds(sfxDuration * 3);
        pettingDogBarkPlaying = false;
    }

    private void DogAI_OnStateChanged(object sender, System.EventArgs e) {
        DogAI.State state = Dog.Instance.GetCurrentDogAI().GetState();
        if(state != DogAI.State.growling) {
            dogOtherSFXAudioSource.Stop();
        }

        if(state == DogAI.State.attacking) {
            runningToAttack = true;
        }

        if(Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            if(!running && Dog.Instance.GetCurrentDogAI().GetRunning()) {
                dogRollingAudioSource.clip = rollingAudioClip;
                dogRollingAudioSource.Play();

                running = true;
            };

            if(running && !Dog.Instance.GetCurrentDogAI().GetRunning()) {
                dogRollingAudioSource.Stop();
                PlaySound2D(skidAudioClips, .3f);
                running = false;
            }
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

        if (Dog.Instance.GetDogType() != Dog.DogType.GermanShepherd) return; 
        dogAudioSource.PlayOneShot(biteAudioClips[Random.Range(0, biteAudioClips.Length)], masterVolume * dogVolume);
    }


    private void DogAnimator_OnDogBark(object sender, System.EventArgs e) {
        if (Dog.Instance.GetDogType() == Dog.DogType.GermanShepherd) {
            dogAudioSource.PlayOneShot(barkAudioClips[Random.Range(0, barkAudioClips.Length)], masterVolume * dogVolume * .7f);
        }

        if (Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            dogAudioSource.PlayOneShot(barkAudioClips_retreiver[Random.Range(0, barkAudioClips_retreiver.Length)], masterVolume * dogVolume);
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            dogAudioSource.PlayOneShot(barkAudioClips_darkCompanion[Random.Range(0, barkAudioClips_darkCompanion.Length)], masterVolume * dogVolume * 2f);
        }
    }

    private void DogAnimator_OnDogGrowl(object sender, System.EventArgs e) {
        if(growlTimer < 0) {

            AudioClip audioClip = growlAudioClips[Random.Range(0, growlAudioClips.Length)];
            if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
                audioClip = growlAudioClips_darkCompanion[Random.Range(0, growlAudioClips_darkCompanion.Length)];
            }

            dogOtherSFXAudioSource.clip = audioClip;
            dogOtherSFXAudioSource.Play();
            dogOtherSFXAudioSource.volume = masterVolume * dogVolume * .7f;
            growlTimer = growlRate;
        }
    }

    private void DogAnimator_OnDogGroan(object sender, System.EventArgs e) {
        AudioClip audioClip = groanAudioClips[Random.Range(0, groanAudioClips.Length)];
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            audioClip = groanAudioClips_darkCompanion[Random.Range(0, groanAudioClips_darkCompanion.Length)];
        }

        dogAudioSource.PlayOneShot(audioClip, masterVolume * dogVolume * .7f);
    }

    private void DogAnimator_OnDogBreathe(object sender, System.EventArgs e) {
        if (dogBreatheAudioSource.isPlaying) return;

        dogBreatheAudioSource.clip = breatheAudioClips[Random.Range(0, breatheAudioClips.Length)];
        dogBreatheAudioSource.Play();
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (dogFootstepsAudioSource.isPlaying) return;

        dogFootstepsAudioSource.clip = footStepAudioClips[Random.Range(0, footStepAudioClips.Length)];
        dogFootstepsAudioSource.Play();
    }

    private void DogAnimator_OnDogPant(object sender, System.EventArgs e) {
        if (dogPantAudioSource.isPlaying) return;

        dogPantAudioSource.clip = pantAudioClips[Random.Range(0, pantAudioClips.Length)];
        dogPantAudioSource.Play();

    }

    private void DogAnimator_OnDogSniffed(object sender, System.EventArgs e) {
        dogAudioSource.PlayOneShot(sniffAudioClips[Random.Range(0, sniffAudioClips.Length)], masterVolume * dogVolume * .15f);

    }
    private void PreloadAudioClips() {

        AudioClip[][] allClipArrays = new AudioClip[][] {
        footStepAudioClips,
        breatheAudioClips,
        sniffAudioClips,
        pantAudioClips,
        groanAudioClips,
        groanAudioClips_darkCompanion,
        growlAudioClips,
        growlAudioClips_darkCompanion,
        barkAudioClips,
        barkAudioClips_retreiver,
        barkAudioClips_darkCompanion,
        biteAudioClips,
        biteAudioClips_darkCompanion,
        petStartAudioClips,
        petLoopAudioClips,
        petBarkAudioClips,
        petTapAudioClips,
        skidAudioClips
    };

        for (int i = 0; i < allClipArrays.Length; i++) {

            AudioClip[] array = allClipArrays[i];

            if (array == null) continue;

            for (int j = 0; j < array.Length; j++) {

                AudioClip clip = array[j];

                if (clip == null) continue;

                if (clip.loadState == AudioDataLoadState.Unloaded) {
                    clip.LoadAudioData();
                }
            }
        }

        // Clips unitaires
        AudioClip[] singleClips = new AudioClip[] {
        rollingAudioClip,
        laserAttackAudioClip,
        stompAttackAudioClip
    };

        for (int i = 0; i < singleClips.Length; i++) {

            AudioClip clip = singleClips[i];

            if (clip == null) continue;

            if (clip.loadState == AudioDataLoadState.Unloaded) {
                clip.LoadAudioData();
            }
        }
    }
}
