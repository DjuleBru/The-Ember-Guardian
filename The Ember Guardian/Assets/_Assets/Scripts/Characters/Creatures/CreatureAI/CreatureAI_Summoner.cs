using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Summoner : CreatureAI {

    [SerializeField] private float distanceToFleeFromPlayer;
    [SerializeField] private float minimumFleeTime;
    [SerializeField] private CreatureSpawnerContinuous creatureSpawnerContinuous;

    private float fleeTimer;
    private bool fleeing;
    private bool spawning;

    protected override void Start() {
        base.Start();
        creatureSpawnerContinuous.OnSpawnerSpawnStart += CreatureSpawnerContinuous_OnSpawnerSpawnStart;
        creatureSpawnerContinuous.OnSpawnerSpawned += CreatureSpawnerContinuous_OnSpawnerSpawned;
    }

    protected override void Update() {
        if(spawning) {
            return;
        }
        base.Update();
    }

    private void CreatureSpawnerContinuous_OnSpawnerSpawned(object sender, EventArgs e) {
        spawning = false;
    }

    private void CreatureSpawnerContinuous_OnSpawnerSpawnStart(object sender, EventArgs e) {
        spawning = true;
        creatureMovement.SetMoveTarget(transform.position);
    }

    protected override void HeadToTarget() {
        if (attackTarget == null) return;

        if(!fleeing) {
            fleeing = true;
            fleeTimer = minimumFleeTime;
        }

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);
        float direction = (targetPosition.x - transform.position.x);

        if(direction >= 0) {
            direction = 1;
        } else {
            direction = -1;
        }

        Vector3 targetDestination = new Vector3(transform.position.x + (distanceToFleeFromPlayer - distanceToTargetX) * direction, 0, 0);

        creatureMovement.SetMoveTarget(targetDestination);
    }

    protected override void MoveToTargetStateUpdate() {
        if(fleeing) {
            fleeTimer -= Time.deltaTime;
            if (fleeTimer > 0) {
                HeadToTarget();
                return;
            } else {
                fleeing = false;
            }
        }

        if (!detectedAttackTarget) {
            if (creature.IsDayCreature()) {
                ChangeState(State.walkingToSpawner);
                return;
            }
            else {
                ChangeState(State.walkingToFire);
                return;
            }
        }

        HeadToTarget();

    }

    protected override void Creature_OnCreatureDied(object sender, EventArgs e) {
        died = true;
        creatureSpawnerContinuous.SetDead();
    }
}
