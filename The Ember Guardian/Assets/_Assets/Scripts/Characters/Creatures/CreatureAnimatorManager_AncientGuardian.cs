using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_AncientGuardian : CreatureAnimatorManager
{
    [SerializeField] private CreatureAI_AncientGuardian ancientGuardian;

    protected override void Start() {
        base.Start();
        ancientGuardian.OnGuardianEndedTP += AncientGuardian_OnGuardianEndedTP;
        ancientGuardian.OnGuardianStartedTP += AncientGuardian_OnGuardianStartedTP;
    }

    private void AncientGuardian_OnGuardianStartedTP(object sender, System.EventArgs e) {
        animator.SetTrigger("Disappear");
    }

    private void AncientGuardian_OnGuardianEndedTP(object sender, System.EventArgs e) {
        animator.SetTrigger("Appear");
    }

    protected override void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        if (creatureAttack.GetIsPrimaryAttack()) {
            animator.SetTrigger("Attack");
        } else if (creatureAttack.GetIsSecondaryAttack()) {
            animator.SetTrigger("Attack_Secondary");
        } else {
            animator.SetTrigger("Special");
        }
    }
}
