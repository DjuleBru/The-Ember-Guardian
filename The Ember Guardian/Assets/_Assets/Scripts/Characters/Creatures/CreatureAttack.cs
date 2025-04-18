using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack : MobAttack
{
    private Creature creature;
    private float enteredLightAttackSpeedDebuff;
    private float nightAttackSpeedBuff = 1.5f;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();

        attackCooldown = creature.GetCreatureSO().attackRate;
        attackDamage = creature.GetCreatureSO().damage;
        attackAnimationDelay = creature.GetCreatureSO().attackAnimationDelay;
        totalAttackAnimationTime = creature.GetCreatureSO().totalAttackAnimationTime;
        enteredLightAttackSpeedDebuff = creature.GetCreatureSO().enteredLightattackRateDebuff;
    }

    protected void Start() {
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;

        if(creature.GetIsEliteDamageCreature()) {
            attackDamage *= 2;
        }
        if(!creature.IsDayCreature()) {
            attackCooldown /= nightAttackSpeedBuff;
        }
    }

    public override void DealDamage() {

        if (attackTargetIDamageable != null) {

            if ((attackTargetIDamageable as MonoBehaviour) == Fire.Instance) {
                if (creature.GetCreatureSO().isBoss) return;
                attackTargetIDamageable.TakeDamage(creature.GetCreatureSO().damageToFire, transform, false, attackIgnoresTemporaryInvincibility);
                mob.Die();
            } else {
                attackTargetIDamageable.TakeDamage(attackDamage, transform, false, attackIgnoresTemporaryInvincibility);
            }

        }


        InvokeAttackHit();
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        attackCooldown /= enteredLightAttackSpeedDebuff;
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        attackCooldown *= enteredLightAttackSpeedDebuff;
    }

    protected override Vector3 GetEndPointRandomized() {
        float distanceToTargetNormalized = Mathf.Abs(attackTargetGameObject.transform.position.x - transform.position.x)/ creature.GetCreatureSO().minAttackRange;

        Vector3 endPointRandomized = new Vector3(distanceToTargetNormalized * creature.GetCreatureSO().attackRangeMaxDistanceMiss, 0, 0);
        
        return endPointRandomized;
    }

    public override void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
    }
}
