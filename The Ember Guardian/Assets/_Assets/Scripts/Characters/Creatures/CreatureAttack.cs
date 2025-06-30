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
        creature.OnCreatureStunStarted += Creature_OnCreatureStunStarted;
        creature.OnCreatureStunStopped += Creature_OnCreatureStunStopped;
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

                attackTargetIDamageable.TakeDamage(creature.GetCreatureSO().damageToFire, transform, false, attackIgnoresTemporaryInvincibility);

                if (!creature.GetCreatureSO().isBoss) {
                    mob.Die();
                }

            } else {

                attackTargetIDamageable.TakeDamage(attackDamage, transform, false, attackIgnoresTemporaryInvincibility);

                if(!GetIsRangedAttack()) {
                    // Melee attack creature

                    Barricade barricade = attackTargetIDamageable as Barricade;
                    if (barricade != null) {
                        if (!barricade.GetBarricadeSpiked()) return;
                        creature.TakeDamage(barricade.GetSpikeDamage(), barricade.transform);
                    }
                }
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

    protected override Vector3 GetEndPointRandomOffstetValue() {
        float distanceToTargetNormalized = Mathf.Abs(attackTargetGameObject.transform.position.x - transform.position.x)/ creature.GetCreatureSO().minAttackRange;

        Vector3 endPointRandomized = new Vector3(distanceToTargetNormalized * creature.GetCreatureSO().attackRangeMaxDistanceMiss, 0, 0);
        
        return endPointRandomized;
    }

    public override void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
    }

    private void Creature_OnCreatureStunStopped(object sender, EventArgs e) {
        stunned = true;
    }

    private void Creature_OnCreatureStunStarted(object sender, EventArgs e) {
        stunned = false;
    }

}
