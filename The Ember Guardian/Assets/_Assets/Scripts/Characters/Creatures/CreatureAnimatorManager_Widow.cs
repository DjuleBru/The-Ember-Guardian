using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Widow : CreatureAnimatorManager
{
    private CreatureAI_TarnishedWidow widowAI;

    protected override void Awake() {
        base.Awake();
        widowAI = GetComponentInParent<CreatureAI_TarnishedWidow>();
    }

    protected override void Start() {
        base.Start();
        widowAI.OnWidowJumpStarted += WidowAI_OnWidowJumpStarted;
        widowAI.OnWidowLanded += WidowAI_OnWidowLanded;
    }

    private void WidowAI_OnWidowLanded(object sender, System.EventArgs e) {
        animator.SetTrigger("Land");
        animator.ResetTrigger("Jump");
    }

    private void WidowAI_OnWidowJumpStarted(object sender, System.EventArgs e) {
        animator.SetTrigger("Jump");
        animator.ResetTrigger("Land");
    }
}
