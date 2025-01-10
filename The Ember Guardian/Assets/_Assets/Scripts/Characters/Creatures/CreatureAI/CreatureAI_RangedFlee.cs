using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_RangedFlee : CreatureAI
{
    [SerializeField] private float minAllowedDistanceFromPlayer;

    protected override void Awake() {
        base.Awake();
        minAllowedDistanceFromPlayer = UnityEngine.Random.Range(minAllowedDistanceFromPlayer - minAllowedDistanceFromPlayer / 5, minAllowedDistanceFromPlayer + minAllowedDistanceFromPlayer / 5);
    }

    protected override void HeadToTarget() {
        if (attackTarget == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position; 
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);

        if (distanceToTargetX > minAllowedDistanceFromPlayer) {
            // Head to target, attack
            creatureMovement.SetMoveTarget(targetPosition);

            if (attackTarget == Player.Instance.GetComponent<IDamageable>()) {

                // Take in account player Y position for when he jumps over creatures
                if (Mathf.Abs(transform.position.x - targetPosition.x) < minAttackRange) {
                    ChangeState(State.attacking);
                    return;
                }

                return;
            }

            if (Mathf.Abs(transform.position.x - targetPosition.x) < minAttackRange) {
                ChangeState(State.attacking);
            }

        } else {

            // Flee
            float direction = (transform.position.x - targetPosition.x);

            if (direction >= 0) {
                direction = 1;
            }
            else {
                direction = -1;
            }

            Vector3 targetDestination = new Vector3(transform.position.x + ((10f) * direction), 0, 0);

            creatureMovement.SetMoveTarget(targetDestination);
        }
    }

    protected override void AttackingStateUpdate() {
        base.AttackingStateUpdate();

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);

        if(distanceToTargetX < minAllowedDistanceFromPlayer) {
            ChangeState(State.moveToTarget);
        }

    }
}
