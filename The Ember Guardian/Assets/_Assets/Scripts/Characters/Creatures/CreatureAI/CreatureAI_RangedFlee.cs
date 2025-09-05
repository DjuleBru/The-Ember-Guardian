using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_RangedFlee : CreatureAI
{
    [SerializeField] private float minAllowedDistanceFromPlayer;

    protected override void Awake() {
        base.Awake();
    }

    protected override void SetAttackRange() {
        base.SetAttackRange();

        minAllowedDistanceFromPlayer = UnityEngine.Random.Range(minAllowedDistanceFromPlayer - minAllowedDistanceFromPlayer / 5, minAllowedDistanceFromPlayer + minAllowedDistanceFromPlayer / 5);
        if (minAllowedDistanceFromPlayer >= minAttackRange) {
            minAllowedDistanceFromPlayer -= minAttackRange / 4f;
        }
    }

    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position; 
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);

        bool isNight = DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night;

        if (isNight || distanceToTargetX > minAllowedDistanceFromPlayer) {
            // Head to target, attack
            creatureMovement.SetMoveTarget(targetPosition);

            if (attackTarget == Player.Instance.GetComponent<IDamageable>()) {

                if (Mathf.Abs(distanceToTargetX) < minAttackRange) {
                    ChangeState(State.attacking);
                    return;
                }

                return;
            }

            if (Mathf.Abs(transform.position.x - targetPosition.x) < minAttackRange) {
                ChangeState(State.attacking);
            }

        } else {

            // Flee : only during day
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
        if((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);

        if(distanceToTargetX < minAllowedDistanceFromPlayer) {
            ChangeState(State.moveToTarget);
        }

    }
}
