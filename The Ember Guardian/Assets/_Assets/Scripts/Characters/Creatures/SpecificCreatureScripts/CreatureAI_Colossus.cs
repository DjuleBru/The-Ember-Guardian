using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Colossus : CreatureAI {

    public event EventHandler OnColossusWake;

    protected override void Awake() {
        base.Awake();
        rangedAttackSO = creature.GetCreatureSO().primaryAttackSO;
        meleeAttackSO = creature.GetCreatureSO().secondaryAttackSO;
        creatureAttack.SetAttackSO(rangedAttackSO);
    }

    protected override void Start() {
        base.Start();
        BossUI.Instance.LinkBoss(creature, false);
    }

    protected override void CheckAttackChange() {
        if ((attackTarget as MonoBehaviour) == null) return;
        if (creatureAttack.GetAttackStarted()) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;

        float aimDir = creatureMovement.GetLastMoveDirFloat();
        float dirToTarget = transform.position.x - targetPosition.x;
        float distanceToTarget = Mathf.Abs(dirToTarget);

        float meleeAttackMaxRange = creature.GetCreatureSO().secondaryAttackSO.maxAttackRange;
        float meleeAttackMinRange = creature.GetCreatureSO().secondaryAttackSO.minAttackRange;

        if (creatureAttack.GetCurrentCreatureAttackSO() == rangedAttackSO) {

            if (distanceToTarget < meleeAttackMaxRange && distanceToTarget > meleeAttackMinRange) {
                creatureAttack.SetAttackSO(meleeAttackSO);
                ChangeState(State.moveToTarget);
            }

        } else {
            if (distanceToTarget >= meleeAttackMaxRange) {
                creatureAttack.SetAttackSO(rangedAttackSO);
                ChangeState(State.moveToTarget);
            }
        }
       
    }

    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        Vector3 destination;

        if(attackTarget is Player) {
            destination = targetPosition + new Vector3(Mathf.Abs(minAttackRange) * flankDirection, 0f, 0f);
        }

        destination = targetPosition + new Vector3(Mathf.Abs(minAttackRange) * flankDirection, 0f, 0f);
        // Distance jusqu’à la position derrière la cible
        float distanceToDestination = Mathf.Abs(transform.position.x - destination.x);

        // Si on y est: attaque
        if (distanceToDestination < 0.1f) {
            ChangeState(State.attacking);
            return;
        }

        // Dans les deux cas, on continue à se déplacer
        creatureMovement.SetMoveTarget(destination);
    }

    public override void TriggerBossSpawnAnimation() {
        OnColossusWake?.Invoke(this, EventArgs.Empty);
    }
}
