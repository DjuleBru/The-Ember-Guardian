using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Boss : CreatureAI
{
    protected override void IdleStateUpdate() {
        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }
    }
}
