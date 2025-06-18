using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableSounds : SoundObject
{
    [SerializeField] private AudioClip backgorundAudioClip;
    [SerializeField] private AudioClip[] minerGarrisonerAudioClip;
    [SerializeField] private AudioSource backgroundAudioSource;
    private Scavengable scavengable;

    private bool backgroundPlaying;

    private void Awake() {
        scavengable = GetComponentInParent<Scavengable>();
    }

    protected override void Start() {
        base.Start();

        scavengable.OnMinerStartsMining += Scavengable_OnMinerStartsMining;
        scavengable.OnMinerStopsMining += Scavengable_OnMinerStopsMining;
    }

    private void Scavengable_OnMinerStopsMining(object sender, System.EventArgs e) {
        backgroundAudioSource.Stop();

        backgroundPlaying = false;
    }

    private void Scavengable_OnMinerStartsMining(object sender, System.EventArgs e) {
        PlaySound2D(minerGarrisonerAudioClip);

        if (backgroundPlaying) return;

        backgroundAudioSource.clip = backgorundAudioClip;
        backgroundAudioSource.Play();
        backgroundPlaying = true;
    }
}
