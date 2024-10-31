using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    private Creature creature;
    private MobMovement mobMovement;
    private MobAttack mobAttack;

    private float initialMoveSpeed;
    private float attackRange;
    private float maxAttackRange;
    private bool hasSetSpeed;
    private bool detectedAttackTarget;

    private IDamageable attackTarget;

    public enum State {
        walking,
        moveToTarget,
        attacking,
    }

    private State state;

    private void Awake() {
        mobMovement = GetComponent<MobMovement>();
        mobAttack = GetComponent<MobAttack>();
        creature = GetComponent<Creature>();
    }

    private void Start() {
        attackRange = creature.GetCreatureSO().attackRange + Random.Range(-creature.GetCreatureSO().attackRange/10, creature.GetCreatureSO().attackRange/10);
        maxAttackRange = attackRange + attackRange/5;

        initialMoveSpeed = creature.GetCreatureSO().moveSpeed + Random.Range(-creature.GetCreatureSO().moveSpeedRandomizerDelta, creature.GetCreatureSO().moveSpeedRandomizerDelta);

        mobMovement.SetMoveSpeed(initialMoveSpeed);
    }

    private void Update() {

        switch (state) {

            case State.walking:

                MoveTowardsFire();
                if(detectedAttackTarget) {
                    ChangeState(State.moveToTarget);
                }

            break;

            case State.moveToTarget:

                if(!detectedAttackTarget) {
                    ChangeState(State.walking);
                }

                HeadToTarget();

            break;

            case State.attacking:

                if (!detectedAttackTarget) {
                    ChangeState(State.walking);
                    return;
                }

                if(!CheckAttackTargetInRange() && !mobAttack.GetAttackStarted()) {
                    ChangeState(State.moveToTarget);
                    return;
                }

                break;
        }
    }

    private void ChangeState(State newState) {
        Debug.Log(newState);
        mobMovement.SetMoveTarget(transform.position);

        if(newState == State.attacking) {
            mobAttack.SetAttackTarget(attackTarget);
        }

        if(newState == State.walking) {
            mobAttack.RemoveAttackTarget();
        }

        if(newState == State.moveToTarget) {
            mobAttack.RemoveAttackTarget();
        }

        state = newState;
    }

    private bool CheckAttackTargetInRange() {
        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;

        if (Mathf.Abs(transform.position.x - targetPosition.x) < maxAttackRange) {
            return true;
        } else {
            return false;
        }
    }

    private void HeadToTarget() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(initialMoveSpeed * 1.5f);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = (attackTarget as MonoBehaviour).transform.position;
        mobMovement.SetMoveTarget(targetDestination);
        hasSetSpeed = false;

        if (Mathf.Abs(transform.position.x - targetDestination.x) < attackRange) {
            ChangeState(State.attacking);
        }
    }

    private void MoveTowardsFire() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(initialMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = new Vector3(0, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);
        hasSetSpeed = false;
    }

    public void ResetAttackTargetInProximity() {
        detectedAttackTarget = false;
    }

    public void SetAttackTarget(IDamageable iDamageable) {
        detectedAttackTarget = true;

        if (attackTarget == iDamageable) return;
        attackTarget = iDamageable;
    }


}
