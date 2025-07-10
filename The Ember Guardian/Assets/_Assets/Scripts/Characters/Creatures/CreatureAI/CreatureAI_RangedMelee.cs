using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_RangedMelee : CreatureAI
{
    protected CreatureAttackSO rangedAttackSO;
    protected CreatureAttackSO meleeAttackSO;

    protected override void Awake() {
        base.Awake();
        rangedAttackSO = creature.GetCreatureSO().primaryAttackSO;
        meleeAttackSO = creature.GetCreatureSO().secondaryAttackSO;
    }

    protected override void Update() {
        CheckAttackChange();
        base.Update();
    }

    protected void CheckAttackChange() {
        if ((attackTarget as MonoBehaviour) == null) return;
        if (creatureAttack.GetAttackStarted()) return;

        float meleeAttackMaxRange = creature.GetCreatureSO().secondaryAttackSO.maxAttackRange;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;

        if (Mathf.Abs(transform.position.x - targetPosition.x) < meleeAttackMaxRange && creatureAttack.GetCurrentCreatureAttackSO() == rangedAttackSO) {
            creatureAttack.SetAttackSO(meleeAttackSO);
        }


        if (Mathf.Abs(transform.position.x - targetPosition.x) >= meleeAttackMaxRange && creatureAttack.GetCurrentCreatureAttackSO() == meleeAttackSO) {
            creatureAttack.SetAttackSO(rangedAttackSO);
        }
    }
}
