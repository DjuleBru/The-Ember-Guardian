using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableSounds : SoundObject
{
    [SerializeField] private AudioClip backgorundAudioClip;
    [SerializeField] private AudioClip[] minerGarrisonerAudioClip;
    [SerializeField] private AudioClip[] pieceFellAudioClip;
    [SerializeField] private AudioClip toggleMiningAudioClip;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private ScavengableObstacleVisual scavengableObstacleVisual;
    private IScavengable scavengable;

    private bool backgroundPlaying;

    private void Awake() {
        scavengable = GetComponentInParent<IScavengable>();
    }

    protected override void Start() {
        base.Start();

        scavengable.OnMinerStartsMining += Scavengable_OnMinerStartsMining;
        scavengable.OnMinerStopsMining += Scavengable_OnMinerStopsMining;
        scavengable.OnDeactivatedMining += Scavengable_OnDeactivatedMining;
        scavengable.OnActivatedMining += Scavengable_OnActivatedMining;

        if(scavengableObstacleVisual != null) {
            scavengableObstacleVisual.OnPieceFell += ScavengableObstacleVisual_OnPieceFell;
        }
    }

    private void ScavengableObstacleVisual_OnPieceFell(object sender, System.EventArgs e) {
        PlaySound2D(pieceFellAudioClip);
    }

    private void Scavengable_OnActivatedMining(object sender, System.EventArgs e) {
        PlaySound2D(toggleMiningAudioClip);
    }

    private void Scavengable_OnDeactivatedMining(object sender, System.EventArgs e) {
        PlaySound2D(toggleMiningAudioClip);
    }

    private void Scavengable_OnMinerStopsMining(object sender, System.EventArgs e) {
        PlaySound2D(minerGarrisonerAudioClip, .7f);

        if (scavengable.GetIsMine()) {
            backgroundAudioSource.Stop();
            backgroundPlaying = false;
        }
    }

    private void Scavengable_OnMinerStartsMining(object sender, System.EventArgs e) {
        PlaySound2D(minerGarrisonerAudioClip, .7f);

        if (backgroundPlaying) return;
        if (scavengable.GetIsMine()) {
            backgroundAudioSource.clip = backgorundAudioClip;
            backgroundAudioSource.Play();
            backgroundPlaying = true;
        }
    }
}
