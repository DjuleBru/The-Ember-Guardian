using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Runner : CreatureAnimatorManager
{
    private CreatureAI_Runner runner;
    public event EventHandler OnHeavyFootStepTriggered;

    protected override void Awake() {
        base.Awake();
        runner = GetComponentInParent<CreatureAI_Runner>();
        runner.OnRunStarted += Runner_OnRunStarted;
        runner.OnRunStopped += Runner_OnRunStopped;
    }

    private void Runner_OnRunStopped(object sender, System.EventArgs e) {
        animator.SetBool("Running", false);
    }

    private void Runner_OnRunStarted(object sender, System.EventArgs e) {
        animator.SetBool("Running", true);
    }
    public void HeavyFootStepEvent() {
        OnHeavyFootStepTriggered?.Invoke(this, EventArgs.Empty);
    }
}
