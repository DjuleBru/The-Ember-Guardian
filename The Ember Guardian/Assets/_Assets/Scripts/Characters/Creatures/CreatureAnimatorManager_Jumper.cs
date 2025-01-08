using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Jumper : CreatureAnimatorManager
{

    private CreatureMovement_Jumper creatureMovement_Jumper;

    protected override void Awake() {
        base.Awake();
        creatureMovement_Jumper = GetComponentInParent<CreatureMovement_Jumper>();
    }

    protected override void Start() {
        base.Start();
        creatureMovement_Jumper.OnJumpStarted += CreatureMovement_Jumper_OnJumpStarted;
        creatureMovement_Jumper.OnJumpLanded += CreatureMovement_Jumper_OnJumpLanded;
    }

    private void CreatureMovement_Jumper_OnJumpLanded(object sender, System.EventArgs e) {
        animator.SetTrigger("Land");

    }

    private void CreatureMovement_Jumper_OnJumpStarted(object sender, System.EventArgs e) {
        animator.ResetTrigger("Land");
        animator.SetTrigger("Jump");
    }
}
