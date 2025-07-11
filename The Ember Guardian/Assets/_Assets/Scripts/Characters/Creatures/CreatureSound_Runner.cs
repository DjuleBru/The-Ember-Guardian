using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound_Runner : CreatureSound
{
    [SerializeField] private CreatureAI_Runner runnerAI;
    [SerializeField] private CreatureAnimatorManager_Runner runnerAnimatorManager;
    [SerializeField] private AudioClip[] runFootstepAudioClips;
    [SerializeField] private AudioClip[] runStartedAudioClips;
    [SerializeField] private float runFootstepsVolumeMultiplier = 1f;
    [SerializeField] private float runStartedVolumeMultiplier = 1f;

    protected override void Awake() {
        base.Awake();
        runnerAnimatorManager.OnHeavyFootStepTriggered += RunnerAnimatorManager_OnHeavyFootStepTriggered;
        runnerAI.OnRunStarted += RunnerAI_OnRunStarted;
    }

    private void RunnerAI_OnRunStarted(object sender, System.EventArgs e) {
        PlaySound2D(runStartedAudioClips, runStartedVolumeMultiplier);
    }

    private void RunnerAnimatorManager_OnHeavyFootStepTriggered(object sender, System.EventArgs e) {
        PlaySound2D(runFootstepAudioClips,  runFootstepsVolumeMultiplier);
    }
}
