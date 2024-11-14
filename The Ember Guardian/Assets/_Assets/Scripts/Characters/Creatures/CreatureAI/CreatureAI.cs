using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    private Creature creature;
    private MobMovement mobMovement;
    private MobAttack mobAttack;

    private float foundTargetMoveSpeedBuff = 1.5f;
    private float attackRange;
    private float maxAttackRange;
    private bool followingTargetBuffedSpeed;
    private bool detectedAttackTarget;

    private IDamageable attackTarget;

    private Vector3 positionToRoamAmound;
    private float roamChangeDestinationRate;
    private float roamRadius;
    private float roamTimer;

    private bool aggroedRecently;
    private bool died;
    private float aggroTimer;
    private float aggroDelay = 3f;
    public event EventHandler OnCreatureAggro;

    public enum State {
        idle,
        walkingToFire,
        walkingToSpawner,
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
        attackRange = creature.GetCreatureSO().attackRange + UnityEngine.Random.Range(-creature.GetCreatureSO().attackRangeRandomizer, creature.GetCreatureSO().attackRangeRandomizer);
        maxAttackRange = attackRange + attackRange/5;

        if (creature.IsDayCreature()) {

            positionToRoamAmound = creature.GetMobSpawner().transform.position;
            roamChangeDestinationRate = 10f;
            roamRadius = 3f;

            ChangeState(State.idle);

        } else {

            ChangeState(State.walkingToFire);

        }
    }

    private void Update() {
        if (died) return;

        HandleAggroRecently();

        switch (state) {

            case State.idle:

                if (detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

                Roam();

                break;

            case State.walkingToFire:

                MoveTowardsFire();
                if(detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

            break;

            case State.walkingToSpawner:

                MoveTowardsSpawner();
                if (detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

                break;

            case State.moveToTarget:

                if(!detectedAttackTarget) {
                    if(creature.IsDayCreature()) {
                        ChangeState(State.walkingToSpawner);
                    } else {
                        ChangeState(State.walkingToFire);
                    }
                }

                HeadToTarget();

            break;

            case State.attacking:

                if (!detectedAttackTarget) {
                    ChangeState(State.walkingToFire);
                    return;
                }

                if(!CheckAttackTargetInRange() && !mobAttack.GetAttackStarted()) {
                    ChangeState(State.moveToTarget);
                    return;
                }

                break;
        }
    }
    private void Roam() {

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(mobMovement, roamRadius, positionToRoamAmound);
        }
    }

    private void HandleAggroRecently() {
        if (aggroedRecently) {

            aggroTimer -= Time.deltaTime;

            if (aggroTimer < 0) {
                aggroTimer = aggroDelay;
                aggroedRecently = false;
            }
        }
    } 

    private void ChangeState(State newState) {

        if(newState == State.attacking) {
            mobMovement.SetMoveTarget(transform.position);
            mobAttack.SetAttackTarget(attackTarget);

            if (followingTargetBuffedSpeed) {
                mobMovement.DebuffMoveSpeed(foundTargetMoveSpeedBuff);
                followingTargetBuffedSpeed = false;
            }

        }

        if(newState == State.walkingToFire) {
            mobMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            if(followingTargetBuffedSpeed) {
                mobMovement.DebuffMoveSpeed(foundTargetMoveSpeedBuff);
                followingTargetBuffedSpeed = false;
            }
        }

        if (newState == State.walkingToSpawner) {
            mobMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            if (followingTargetBuffedSpeed) {
                mobMovement.DebuffMoveSpeed(foundTargetMoveSpeedBuff);
                followingTargetBuffedSpeed = false;
            }
        }

        if (newState == State.moveToTarget) {

            if(!aggroedRecently) {
                aggroedRecently = true;
                aggroTimer = aggroDelay;
                TriggerAggoFeedbacks();
            }

            mobMovement.BuffMoveSpeed(foundTargetMoveSpeedBuff);
            followingTargetBuffedSpeed = true;
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
        if (attackTarget == null) return;
        Vector3 targetDestination = attackTarget.GetMeleeAttackPosition().position;

        mobMovement.SetMoveTarget(targetDestination);

        if(attackTarget == Player.Instance.GetComponent<IDamageable>()) {

            // Take in account player Y position for when he jumps over creatures
            if (Mathf.Abs(transform.position.x - targetDestination.x) < attackRange && ((Player.Instance.transform.position.y - 1.51f) < creature.GetCreatureSO().attackRange)) {
                ChangeState(State.attacking);
                return;
            }

            return;
        }

        if (Mathf.Abs(transform.position.x - targetDestination.x) < attackRange) {
            ChangeState(State.attacking);
        }
    }

    private void MoveTowardsFire() {
        Vector3 targetDestination = new Vector3(0, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);
    }

    private void MoveTowardsSpawner() {
        Vector3 targetDestination = creature.GetMobSpawner().transform.position;

        mobMovement.SetMoveTarget(targetDestination);
    }

    public void ResetAttackTargetInProximity() {
        detectedAttackTarget = false;
    }

    public void SetAttackTarget(IDamageable iDamageable) {
        if(iDamageable == null) {
            detectedAttackTarget = false;
            attackTarget = null;
            return;
        }

        detectedAttackTarget = true;

        if (attackTarget == iDamageable) return;

        attackTarget = iDamageable;

        ChangeState(State.moveToTarget);
    }

    private void TriggerAggoFeedbacks() {
        if (attackTarget is Barricade) return;
        OnCreatureAggro?.Invoke(this, EventArgs.Empty);
    }

}
