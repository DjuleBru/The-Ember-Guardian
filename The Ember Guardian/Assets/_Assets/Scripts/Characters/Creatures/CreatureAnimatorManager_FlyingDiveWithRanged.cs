using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_FlyingDiveWithRanged : CreatureAnimatorManager_FlyingDive {
    [SerializeField] protected CreatureAI_Flying_DiveAttackAndRanged creatureAI_FlyingDiveWithRanged;

    protected override void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {

        if(creatureAI_FlyingDiveWithRanged.GetIsDiveAttack()) {
            animator.ResetTrigger("Attack_Start");
            animator.SetTrigger("Attack_End");
        } else {
            animator.SetTrigger("Attack_Ranged");
        }

    }
}
