using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFeedbacks_Runner : CreatureFeedbacks {

    [SerializeField] private CreatureAI_Runner runnerAI;
    [SerializeField] private CreatureAnimatorManager_Runner runnerAnimatorManager;
    [SerializeField] private ParticleSystem runPS;
    [SerializeField] private MMF_Player runFootstepFeedbacks;

    protected override void Awake() {
        base.Awake();
        runnerAnimatorManager.OnHeavyFootStepTriggered += RunnerAnimatorManager_OnHeavyFootStepTriggered;
        runnerAI.OnRunStarted += RunnerAI_OnRunStarted;
    }
    private void RunnerAI_OnRunStarted(object sender, System.EventArgs e) {
        StartCoroutine(PlayRunPSAfterDelay());
    }

    private void RunnerAnimatorManager_OnHeavyFootStepTriggered(object sender, System.EventArgs e) {
        runFootstepFeedbacks.PlayFeedbacks();
        runPS.Play();
    }

    private IEnumerator PlayRunPSAfterDelay() {
        yield return new WaitForSeconds(.3f);
        runPS.Play();
    }
}
