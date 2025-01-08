using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    private Creature creature;
    private CreatureMovement creatureMovement;
    private MobAttack mobAttack;

    private float minAttackRange;
    private float maxAttackRange;
    private bool followingTargetBuffedSpeed;
    private bool detectedAttackTarget;

    private IDamageable attackTarget;
    private IDamageable previousAttackTarget;

    private Vector3 positionToRoamAmound;
    private float roamChangeDestinationRate;
    private float roamRadius;
    private float roamTimer;

    private bool aggroedRecently;
    private bool died;
    private float aggroTimer;
    private float aggroDelay = 3f;
    public event EventHandler OnCreatureAggro;
    public static event EventHandler OnAnyCreatureAggro;

    public enum State {
        idle,
        walkingToFire,
        walkingToSpawner,
        moveToTarget,
        attacking,
    }

    private State state;

    private void Awake() {
        creatureMovement = GetComponent<CreatureMovement>();
        mobAttack = GetComponent<MobAttack>();
        creature = GetComponent<Creature>();
        
    }

    private void Start() {
        creature.OnCreatureDied += Creature_OnCreatureDied;

        if(creature.GetCreatureSO().isRangedAttack) {
            minAttackRange = creature.GetCreatureSO().minAttackRange + UnityEngine.Random.Range(-creature.GetCreatureSO().attackRangeRandomizer, creature.GetCreatureSO().attackRangeRandomizer);
            maxAttackRange = creature.GetCreatureSO().maxAttackRange + UnityEngine.Random.Range(-creature.GetCreatureSO().attackRangeRandomizer, creature.GetCreatureSO().attackRangeRandomizer);
        } else {
            minAttackRange = creature.GetCreatureSO().minAttackRange;
            maxAttackRange = creature.GetCreatureSO().maxAttackRange;
        }

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
                if (!detectedAttackTarget) {
                    if(creature.IsDayCreature()) {
                        ChangeState(State.walkingToSpawner);
                        return;
                    } else {
                        ChangeState(State.walkingToFire);
                        return;
                    }
                }

                HeadToTarget();

            break;

            case State.attacking:

                if (!detectedAttackTarget) {
                    if(creature.IsDayCreature()) {
                        ChangeState(State.walkingToSpawner);
                    } else {
                        ChangeState(State.walkingToFire);
                    }
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
            RoamBehavior.RoamAroundPoint(creatureMovement, roamRadius, positionToRoamAmound);
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
        if (died) return;

        if (newState == State.attacking) {
            creatureMovement.SetMoveTarget(transform.position);
            mobAttack.SetAttackTarget(attackTarget);

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }
        }

        if(newState == State.walkingToFire) {
            creatureMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            if(followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }
        }

        if (newState == State.walkingToSpawner) {
            creatureMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }
        }

        if (newState == State.moveToTarget) {
            creatureMovement.SetCreatureAggroMoveSpeed(true);
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

        creatureMovement.SetMoveTarget(targetDestination);

        if(attackTarget == Player.Instance.GetComponent<IDamageable>()) {

            // Take in account player Y position for when he jumps over creatures
            if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange && ((Player.Instance.transform.position.y - 1.51f) < creature.GetCreatureSO().minAttackRange)) {
                ChangeState(State.attacking);
                return;
            }

            return;
        }

        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.attacking);
        }
    }

    private void MoveTowardsFire() {
        Vector3 targetDestination = new Vector3(0, 0, 0);

        creatureMovement.SetMoveTarget(targetDestination);
    }

    private void MoveTowardsSpawner() {
        Vector3 targetDestination = creature.GetMobSpawner().transform.position;

        creatureMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.idle);
        }
    }

    public void ResetAttackTargetInProximity() {
        detectedAttackTarget = false;
    }

    public void SetAttackTarget(IDamageable iDamageable, List<IDamageable> iDamageablesInRange) {
        if(iDamageablesInRange.Count == 0) {
            detectedAttackTarget = false;
            attackTarget = null;
            return;
        } 

        if(iDamageable != null) {
            detectedAttackTarget = true;

            if (attackTarget == iDamageable) return;

            attackTarget = iDamageable;

            if (!aggroedRecently) {
                aggroedRecently = true;
                aggroTimer = aggroDelay;
                TriggerAggoFeedbacks();
            }

            ChangeState(State.moveToTarget);
        }
    }

    private void TriggerAggoFeedbacks() {
        if (attackTarget is Player) {
            OnCreatureAggro?.Invoke(this, EventArgs.Empty);
            OnAnyCreatureAggro?.Invoke(this, EventArgs.Empty);
        };
    }

    private void Creature_OnCreatureDied(object sender, EventArgs e) {
        died = true;
    }

    public State GetState() {
        return state;
    }
}
