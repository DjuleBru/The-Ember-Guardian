using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSound : MonoBehaviour
{

    [SerializeField] private AudioClip teleportAudioClip;
    [SerializeField] private AudioClip idleAudioClip;
    [SerializeField] private AudioClip appearAudioClip;

    [SerializeField] private AudioSource teleporterIdleAudioSource;
    private AudioSource teleporterAudioSource;
    private Portal portal;

    private void Awake() {
        portal = GetComponentInParent<Portal>();
        teleporterAudioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;

        teleporterIdleAudioSource.clip = idleAudioClip;
        teleporterIdleAudioSource.Play();
        teleporterAudioSource.PlayOneShot(appearAudioClip, .7f);
    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        teleporterAudioSource.PlayOneShot(teleportAudioClip);
    }
}
