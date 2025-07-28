using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_LandMine : CreatureAI
{

    protected bool hidden;
    public event EventHandler OnMineDig;

    protected override void MoveToTargetStateUpdate() {
        if(!hidden) {
            OnMineDig?.Invoke(this, EventArgs.Empty);
            hidden = true;
            return;
        }

        Vector3 targetPosition = Player.Instance.transform.position;
        // Distance jusqu’à la position derrière la cible
        float distanceToDestination = Mathf.Abs(transform.position.x - targetPosition.x);

        // Si on y est: attaque
        if (distanceToDestination < 0.1f) {
            ChangeState(State.attacking);
            return;
        }
    }
}
