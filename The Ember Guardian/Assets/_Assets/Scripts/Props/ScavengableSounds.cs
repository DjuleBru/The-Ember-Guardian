using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableSounds : SoundObject
{
    [SerializeField] private AudioClip backgorundAudioClip;
    [SerializeField] private AudioClip[] minerGarrisonerAudioClip;
    [SerializeField] private AudioClip[] pieceFellAudioClip;
    [SerializeField] private AudioClip toggleMiningAudioClip;
    [SerializeField] private AudioClip[] creaturesSpawnedRumble;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private ScavengableObstacleVisual scavengableObstacleVisual;
    private IScavengable scavengable;

    private bool backgroundPlaying;
    private bool creaturesSpawning;

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

        if(scavengable is ScavengableObstacle) {
            ScavengableObstacle scavengableObstacle = scavengable as ScavengableObstacle;
            scavengableObstacle.OnCreatureSpawned += ScavengableObstacle_OnCreatureSpawned;
            scavengableObstacle.OnObstacleBuilt += ScavengableObstacle_OnObstacleBuilt;
        }
    }

    private void ScavengableObstacle_OnObstacleBuilt(object sender, System.EventArgs e) {
        creaturesSpawning = false;
        MusicManager.Instance.FadeOutMusic(2f);
    }

    private void ScavengableObstacle_OnCreatureSpawned(object sender, System.EventArgs e) {
        PlaySound2D(creaturesSpawnedRumble);
        if(!creaturesSpawning) {
            creaturesSpawning = true;
            MusicManager.Instance.SetClearingObstacleMusic(1f);
        }
    }

    private void ScavengableObstacleVisual_OnPieceFell(object sender, System.EventArgs e) {
        PlaySound2D(pieceFellAudioClip);
    }

    private void Scavengable_OnActivatedMining(object sender, Scavengable.OnScavengableDeactivatedMiningEventArgs e) {
        if (!e.triggerSFX) return;

        PlaySound2D(toggleMiningAudioClip, 2f);
    }

    private void Scavengable_OnDeactivatedMining(object sender, Scavengable.OnScavengableDeactivatedMiningEventArgs e) {
        if (!e.triggerSFX) return;

        PlaySound2D(toggleMiningAudioClip, 2f);
        creaturesSpawning = false;
        MusicManager.Instance.FadeOutMusic(2f);
    }

    private void Scavengable_OnMinerStopsMining(object sender, System.EventArgs e) {
        PlaySound2D(minerGarrisonerAudioClip, .7f);

        if (scavengable.GetIsMine() && scavengable.GetMinerAmountMining() == 0) {
            backgroundAudioSource.Stop();
            backgroundAudioSource.enabled = false;
            backgroundPlaying = false;
        }
    }

    private void Scavengable_OnMinerStartsMining(object sender, System.EventArgs e) {
        PlaySound2D(minerGarrisonerAudioClip, .7f);

        if (backgroundPlaying) return;

        if (scavengable.GetIsMine()) {
            backgroundAudioSource.clip = backgorundAudioClip;
            backgroundAudioSource.enabled = true;
            backgroundAudioSource.Play();
            backgroundPlaying = true;
        }
    }
}
