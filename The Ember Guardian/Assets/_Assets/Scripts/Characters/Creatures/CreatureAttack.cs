using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack : MobAttack
{
    private Creature creature;
    private float enteredLightAttackSpeedDebuff = 1.4f;

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
