using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSound : StructureSounds
{
    private Fire fire;
    private SoundVolume2D volume2D;

    [SerializeField] private StructureUI_Fire fireUI;
    [SerializeField] private AudioSource extractingEmberAudioSource;

    [SerializeField] private AudioClip calmFireAudioClip;
    [SerializeField] private AudioClip mildFireAudioClip;
    [SerializeField] private AudioClip wildFireAudioClip;
    [SerializeField] private AudioClip insaneFireAudioClip;

    [SerializeField] private AudioClip[] orbDroppedInFireAudioClipArray1;
    [SerializeField] private AudioClip[] orbDroppedInFireAudioClipArray2;
    [SerializeField] private AudioClip[] orbPaidAudioClipArray;
    [SerializeField] private AudioClip[] fireDamagedAudioClipArray;
    [SerializeField] private AudioClip secondaryFireTickRemovedAudioClip;
    [SerializeField] private AudioClip extractingEmberAudioClip;

    protected override void Awake() { 
        base.Awake();
        fire = GetComponentInParent<Fire>();
        audioSource = GetComponent<AudioSource>();
        volume2D = GetComponent<SoundVolume2D>();

        extractingEmberAudioSource.clip = extractingEmberAudioClip;

        fire.OnFireChangedState += Fire_OnFireChangedState;
        fire.OnFireFuelled += Fire_OnFireFuelled;
        fire.OnStructureFunctionUsed += Fire_OnStructureFunctionUsed;
        fire.OnFireDamageTaken += Fire_OnFireDamageTaken;
        fire.OnFireEmberExtractionStopped += Fire_OnFireEmberExtractionStopped;
        fire.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;

        if(fireUI != null) {
            fireUI.OnFireTickRemoved += FireUI_OnFireTickRemoved;
        }
    }

    private void Fire_OnStructureFunctionUsed(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(orbPaidAudioClipArray[Random.Range(0, orbPaidAudioClipArray.Length)], sfxVolume);
    }

    private void FireUI_OnFireTickRemoved(object sender, StructureUI_Fire.OnFireTickRemovedEventArgs e) {
        if (!fire.GetIsSecondaryFire()) return;
        audioSource.PlayOneShot(secondaryFireTickRemovedAudioClip, sfxVolume);
    }

    protected override void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();

        extractingEmberAudioSource.volume = sfxVolume * masterVolume;
        audioSource.volume = sfxVolume * masterVolume;
    }

    protected override void SettingsManager_OnMasterVolumeChanged(object sender, System.EventArgs e) {
        masterVolume = SettingsManager.Instance.GetMasterVolume();

        extractingEmberAudioSource.volume = sfxVolume * masterVolume;
        audioSource.volume = sfxVolume * masterVolume;
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        extractingEmberAudioSource.Play();
    }

    private void Fire_OnFireEmberExtractionStopped(object sender, System.EventArgs e) {
        extractingEmberAudioSource.Stop();
    }

    private void Fire_OnFireDamageTaken(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(fireDamagedAudioClipArray[Random.Range(0, fireDamagedAudioClipArray.Length)], sfxVolume * masterVolume);
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(orbDroppedInFireAudioClipArray1[Random.Range(0, orbDroppedInFireAudioClipArray1.Length)], .7f * sfxVolume * masterVolume);
        audioSource.PlayOneShot(orbDroppedInFireAudioClipArray2[Random.Range(0, orbDroppedInFireAudioClipArray2.Length)], .7f * sfxVolume * masterVolume);
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {

        if (fire.GetState() == Fire.State.calm) {
            audioSource.clip = calmFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetLevel1FireRadius());
        }

        if (fire.GetState() == Fire.State.mild) {
            audioSource.clip = mildFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetMildFireRadius());
        }

        if (fire.GetState() == Fire.State.wild) {
            audioSource.clip = wildFireAudioClip;
            volume2D.SetMaxDistanceToHear(fire.GetLevel2FireRadius());
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
