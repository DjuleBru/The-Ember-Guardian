using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatureAI_Summoner : CreatureAI {

    [SerializeField] private float distanceToFleeFromTarget;
    [SerializeField] private float minimumFleeTime;
    [SerializeField] private CreatureSpawnerContinuous creatureSpawnerContinuous;

    private GameObject attackTargetGO;
    private float fleeTimer;
    [SerializeField] private float distanceToTargetToStopWalking = 8f;
    private bool fleeing;
    private bool spawning;

    protected override void Start() {
        base.Start();

        creatureSpawnerContinuous.SetCanSpawnAtNight(!creature.IsDayCreature());

        creatureSpawnerContinuous.OnSpawnerSpawnStart += CreatureSpawnerContinuous_OnSpawnerSpawnStart;
        creatureSpawnerContinuous.OnSpawnerSpawned += CreatureSpawnerContinuous_OnSpawnerSpawned;

        distanceToTargetToStopWalking = UnityEngine.Random.Range(distanceToTargetToStopWalking - distanceToTargetToStopWalking / 3, distanceToTargetToStopWalking + distanceToTargetToStopWalking / 3);
    }

    protected override void Update() {
        if(spawning) {
            return;
        }
        if((attackTarget as MonoBehaviour) != null) {
            attackTargetGO = (attackTarget as MonoBehaviour).gameObject;
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

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        float distanceToTargetX = Mathf.Abs(targetPosition.x - transform.position.x);
        float direction = (transform.position.x - targetPosition.x);

        if(direction >= 0) {
            direction = 1;
        } else {
            direction = -1;
        }

        bool isNight = DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night;
        if (!isNight && distanceToTargetX < distanceToFleeFromTarget) {
            // Creature is too close to target

            Vector3 targetDestination = new Vector3(transform.position.x + (distanceToFleeFromTarget - distanceToTargetX) * direction, 0, 0);
            creatureMovement.SetMoveTarget(targetDestination);
            return;
        }

        if(isNight && distanceToTargetX < distanceToTargetToStopWalking) {
            creatureMovement.SetMoveTarget(transform.position);
        }

        if (distanceToTargetX > distanceToTargetToStopWalking) {
            // Creature is too far from target
            creatureMovement.SetMoveTarget(targetPosition);
            return;
        }
    }

    protected override void MoveToTargetStateUpdate() {
        if (fleeing) {
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
                if(Mathf.Abs(transform.position.x) > distanceToTargetToStopWalking) {
                    ChangeState(State.walkingToFire);
                } else {
                    ChangeState(State.idle);
                }
                
                return;
            }
        }

        HeadToTarget();

    }

    protected override void WalkingToFireStateUpdate() {
        CheckDistanceToPlayerOrCampForMoveSpeed();

        if (Mathf.Abs(transform.position.x) > distanceToTargetToStopWalking) {
            MoveTowardsFire();
        }
        else {
            ChangeState(State.idle);
        }

        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }

    }
    protected override void Creature_OnCreatureDied(object sender, EventArgs e) {
        died = true;
        creatureSpawnerContinuous.SetDead();
    }
}
