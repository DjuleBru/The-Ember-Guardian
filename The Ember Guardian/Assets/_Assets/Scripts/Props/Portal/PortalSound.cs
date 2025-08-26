using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSound : SoundObject {

    [SerializeField] private float delayToPlayShortTeleportOut;
    [SerializeField] private AudioClip teleportAudioClip;
    [SerializeField] private AudioClip teleportShortAudioClip;
    [SerializeField] private AudioClip idleAudioClip;
    [SerializeField] private AudioClip appearAudioClip;
    [SerializeField] private AudioClip beamLightOnAudioClip;
    [SerializeField] private AudioClip beamLightOffAudioClip;
    [SerializeField] private AudioClip sideLightsAudioClip;
    [SerializeField] private AudioClip unlockPortalAudioClip;

    [SerializeField] private AudioSource teleporterIdleAudioSource;
    private AudioSource teleporterAudioSource;
    private Portal portal;
    private SoundVolume2D soundVolume2D;

    private void Awake() {
        portal = GetComponentInParent<Portal>();
        teleporterAudioSource = GetComponent<AudioSource>();
        portal.OnPortalUnlocked += Portal_OnPortalUnlocked;
        portal.OnPortalActivated += Portal_OnPortalActivated;
        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;
        portal.OnPortalAppeared += Portal_OnPortalAppeared;
        portal.OnPortalDisappeared += Portal_OnPortalDisappeared;
        portal.OnPlayerEnteredTriggerArea += Portal_OnPlayerEnteredTriggerArea;
        portal.OnPlayerExitedTriggerArea += Portal_OnPlayerExitedTriggerArea;
        portal.OnTeleporterActivatedOut += Portal_OnTeleporterActivatedOut;

        soundVolume2D = teleporterIdleAudioSource.GetComponent<SoundVolume2D>();
    }

    protected override void Start() {
        base.Start();

        if(!portal.GetPortalUnlocked()) {
            teleporterIdleAudioSource.enabled = false;
        }
       
        teleporterIdleAudioSource.clip = idleAudioClip;
        if(portal.GetIsHUBTeleporter()) {
            teleporterIdleAudioSource.Play();
        }
    }

    private void Portal_OnPortalActivated(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(sideLightsAudioClip, sfxVolume);
    }

    private void Portal_OnPortalUnlocked(object sender, System.EventArgs e) {
        teleporterIdleAudioSource.enabled = true;
        teleporterIdleAudioSource.Play();
    }

    private void Portal_OnTeleporterActivatedOut(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(beamLightOnAudioClip, .5f * sfxVolume);
        StartCoroutine(PlayShortTeleportSoundAfterDelay(delayToPlayShortTeleportOut));
    }

    private void Portal_OnPortalDisappeared(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(appearAudioClip, .15f * sfxVolume);
        FadeOutIdleAudio(2f);
    }

    private void Portal_OnPlayerExitedTriggerArea(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(beamLightOffAudioClip, .5f * sfxVolume);
    }

    private void Portal_OnPlayerEnteredTriggerArea(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(beamLightOnAudioClip, .5f * sfxVolume);
    }

    private void Portal_OnPortalAppeared(object sender, System.EventArgs e) {
        StartCoroutine(PlayDelayed(.1f, appearAudioClip, .3f));
        FadeInIdleAudio(2f);
    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(teleportAudioClip, sfxVolume);
    }

    private IEnumerator PlayShortTeleportSoundAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        teleporterAudioSource.PlayOneShot(teleportShortAudioClip, .5f * sfxVolume);
    }

    private IEnumerator PlayDelayed(float delay, AudioClip audioClip, float volume) {
        yield return new WaitForSeconds(delay);
        teleporterAudioSource.PlayOneShot(audioClip, volume * sfxVolume);
    }
    public void FadeInIdleAudio(float duration = 1f) {
        if (!teleporterIdleAudioSource.isPlaying)
            teleporterIdleAudioSource.Play();
        StartCoroutine(FadeIdleAudioVolume(0f, 1f, duration));
    }

    public void FadeOutIdleAudio(float duration = 1f) {
        StartCoroutine(FadeIdleAudioVolume(1f, 0f, duration, stopAfterFade: true));
    }

    private IEnumerator FadeIdleAudioVolume(float from, float to, float duration, bool stopAfterFade = false) {
        float t = 0f;

        while (t < duration) {
            t += Time.deltaTime;
            float value = Mathf.Lerp(from, to, t / duration);
            soundVolume2D.SetFadeMultiplier(value);
            yield return null;
        }

        soundVolume2D.SetFadeMultiplier(to);

        if (stopAfterFade && to == 0f) {
            teleporterIdleAudioSource.Stop();
        }
    }

}
