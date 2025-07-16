using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack_DieOnAttack : CreatureAttack
{
    protected override IEnumerator AnimatedAttackCoroutine(float totalAttackAnimationTime) {
        attackStarted = true;
        creature.Die();
        yield return new WaitForSeconds(totalAttackAnimationTime);
        attackStarted = false;
    }
}
