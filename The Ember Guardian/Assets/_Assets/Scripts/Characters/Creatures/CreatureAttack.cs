using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack : MobAttack
{
    private Creature creature;

    protected void Awake() {
        creature = GetComponent<Creature>();

        attackRate = creature.GetCreatureSO().attackRate;
        attackDamage = creature.GetCreatureSO().damage;
        attackAnimationDelay = creature.GetCreatureSO().attackAnimationDelay;
        totalAttackAnimationTime = creature.GetCreatureSO().totalAttackAnimationTime;
    }

    public override void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
        attackTimer = 0;
    }
}
