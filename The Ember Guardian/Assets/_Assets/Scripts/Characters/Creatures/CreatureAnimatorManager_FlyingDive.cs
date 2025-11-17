using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_FlyingDive : CreatureAnimatorManager
{
    [SerializeField] protected CreatureAI_Flying creatureAI_Flying;

    protected override void Awake() {
        base.Awake();
        creatureAI_Flying.OnCreatureDropOnTarget += CreatureAI_Flying_OnCreatureDropOnTarget;
        creatureAI_Flying.OnCreatureDropOnTargetEnded += CreatureAI_Flying_OnCreatureDropOnTargetEnded;
        creatureAttack.OnMobAttack += CreatureAttack_OnMobAttack;
    }

    protected void CreatureAI_Flying_OnCreatureDropOnTargetEnded(object sender, System.EventArgs e) {
        animator.ResetTrigger("Attack_Start");
        animator.SetTrigger("Attack_End");
    }

    protected virtual void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {
        animator.ResetTrigger("Attack_Start");
        animator.SetTrigger("Attack_End");
    }

    protected void CreatureAI_Flying_OnCreatureDropOnTarget(object sender, System.EventArgs e) {
        animator.ResetTrigger("Attack_End");
        animator.SetTrigger("Attack_Start");
    }
}
