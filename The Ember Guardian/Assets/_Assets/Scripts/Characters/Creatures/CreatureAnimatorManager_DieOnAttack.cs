using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_DieOnAttack : CreatureAnimatorManager
{

    [SerializeField] private float attackAnimationDuration;
    private bool hasAttacked;

    protected override void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        base.MobAttack_OnMobAttack(sender, e);
        hasAttacked = true;
    }

    protected override void Creature_OnMobDied(object sender, System.EventArgs e) {
        if (hasAttacked) {
            StartCoroutine(DisableAnimatorAfterDelay(attackAnimationDuration));
            return;
        };

        base.Creature_OnMobDied(sender, e);
    }

    private IEnumerator DisableAnimatorAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        animator.enabled = false;

    }
}
