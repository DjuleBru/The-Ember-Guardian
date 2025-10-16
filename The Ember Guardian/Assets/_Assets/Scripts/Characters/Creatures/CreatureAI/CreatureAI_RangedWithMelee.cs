using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_RangedWithMelee : CreatureAI
{
    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        Vector3 destination;

        destination = targetPosition + new Vector3(Mathf.Abs(minAttackRange) * flankDirection, 0f, 0f);
        // Distance jusqu’à la position derrière la cible
        float distanceToDestination = Mathf.Abs(transform.position.x - destination.x);
        float distanceToTarget = Mathf.Abs(transform.position.x - attackTarget.GetMeleeAttackPosition().position.x);

        // Si on y est: attaque
        if (distanceToDestination < 0.1f || distanceToTarget < maxAttackRange) {
            ChangeState(State.attacking);
            return;
        }

        // Dans les deux cas, on continue à se déplacer
        creatureMovement.SetMoveTarget(destination);
    }
}
