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

    private void Awake() {
        portal = GetComponentInParent<Portal>();
        teleporterAudioSource = GetComponent<AudioSource>();
    }

    protected override void Start() {
        base.Start();

        if(!portal.GetPortalUnlocked()) {
            teleporterIdleAudioSource.enabled = false;
        }

        portal.OnPortalUnlocked += Portal_OnPortalUnlocked;
        portal.OnPortalActivated += Portal_OnPortalActivated;
        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;
        portal.OnPortalAppeared += Portal_OnPortalAppeared;
        portal.OnPortalDisappeared += Portal_OnPortalDisappeared;
        portal.OnPlayerEnteredTriggerArea += Portal_OnPlayerEnteredTriggerArea;
        portal.OnPlayerExitedTriggerArea += Portal_OnPlayerExitedTriggerArea;
        portal.OnTeleporterActivatedOut += Portal_OnTeleporterActivatedOut;
        teleporterIdleAudioSource.clip = idleAudioClip;

        if(teleporterIdleAudioSource.enabled) {
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
    }

    private void Portal_OnPlayerExitedTriggerArea(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(beamLightOffAudioClip, .5f * sfxVolume);
    }

    private void Portal_OnPlayerEnteredTriggerArea(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(beamLightOnAudioClip, .5f * sfxVolume);
    }

    private void Portal_OnPortalAppeared(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(appearAudioClip, .15f * sfxVolume);
    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(teleportAudioClip, sfxVolume);
    }

    private IEnumerator PlayShortTeleportSoundAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        teleporterAudioSource.PlayOneShot(teleportShortAudioClip, .5f * sfxVolume);
    }

}
