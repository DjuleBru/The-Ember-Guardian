using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    protected Creature creature;
    protected CreatureMovement creatureMovement;
    protected CreatureAttack creatureAttack;

    protected float minAttackRange;
    protected float maxAttackRange;
    protected bool followingTargetBuffedSpeed;
    protected bool detectedAttackTarget;

    protected IDamageable attackTarget;
    protected IDamageable previousAttackTarget;

    protected Vector3 positionToRoamAmound;
    [SerializeField] protected float roamChangeDestinationRate = 10f;
    [SerializeField] protected float roamRadius = 3f;
    protected float roamTimer;

    protected bool aggroedRecently;
    protected bool died;
    protected bool spawned = true;
    protected bool nightMoveSpeedReset;
    protected float aggroDelay = 3f;
    protected float aggroTimer;

    protected float walkingToFireMoveSpeed = 3.5f;
    protected float distanceToCampOrPlayerToSetStandardSpeed = 25f;

    public event EventHandler OnCreatureAggro;
    public static event EventHandler OnAnyCreatureAggro;
    public event EventHandler OnCreatureUntargetPlayer;
    public static event EventHandler OnAnyCreatureUntargetPlayer;
    public event EventHandler OnCreatureTargetPlayer;

    public enum State {
        idle,
        walkingToFire,
        walkingToSpawner,
        moveToTarget,
        attacking,
        walkingToPlayer,
    }

    protected State state;

    protected virtual void Awake() {
        creatureMovement = GetComponent<CreatureMovement>();
        creatureAttack = GetComponent<CreatureAttack>();
        creature = GetComponent<Creature>();

        spawned = false;
        StartCoroutine(SetSpawnedAfterDelay(creature.GetCreatureSO().spawnAnimationDuration));
    }

    protected virtual void Start() {
        creature.OnCreatureDied += Creature_OnCreatureDied;
        creature.OnMobHitObstacle += Creature_OnMobHitObstacle;

        SetAttackRange();
    }


    protected virtual void SetAttackRange() {
        if (creatureAttack.GetCurrentCreatureAttackSO() == null) return;
        float minAttackRangeSO = creatureAttack.GetCurrentCreatureAttackSO().minAttackRange;
        float maxAttackRangeSO = creatureAttack.GetCurrentCreatureAttackSO().maxAttackRange;
        float attackRangeRandomizerSO = creatureAttack.GetCurrentCreatureAttackSO().attackRangeRandomizer;

        minAttackRange = minAttackRangeSO + UnityEngine.Random.Range(-attackRangeRandomizerSO, attackRangeRandomizerSO);
        maxAttackRange = maxAttackRangeSO + UnityEngine.Random.Range(-attackRangeRandomizerSO, attackRangeRandomizerSO);
    
    }

    private void SetInitialState() {

        if (creature.IsDayCreature()) {

            positionToRoamAmound = creature.GetMobSpawner().transform.position;
            roamRadius = creature.GetMobSpawner().GetRadiusToRoamAround();

            if(creature.IsAgressiveDayCreature()) {
                ChangeState(State.walkingToPlayer);
            } else {
                ChangeState(State.idle);
            }

        }
        else {

            ChangeState(State.walkingToFire);

        }
    }

    private IEnumerator SetSpawnedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        spawned = true;
        creatureMovement.SetSpawned();
        SetInitialState();
    }

    protected virtual void Update() {
        if (died) return;
        if (!spawned) return;

        HandleAggroRecently();
        StateSwitch();
    }

    protected virtual void StateSwitch() {
        switch (state) {

            case State.idle:

                IdleStateUpdate();

                break;

            case State.walkingToFire:
                WalkingToFireStateUpdate();

                break;

            case State.walkingToSpawner:
                WalkingToSpawnerStateUpdate();

                break;

            case State.walkingToPlayer:
                WalkingToPlayerStateUpdate();

                break;

            case State.moveToTarget:
                MoveToTargetStateUpdate();

                break;

            case State.attacking:
                AttackingStateUpdate();

                break;
        }
    }

    #region STATE UPDATES
    protected virtual void IdleStateUpdate() {
        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }

        Roam();
    }

    protected virtual void WalkingToFireStateUpdate() {
        MoveTowardsFire();
        CheckDistanceToPlayerOrCampForMoveSpeed();

        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }
    }

    protected void CheckDistanceToPlayerOrCampForMoveSpeed() {
        //if(!nightMoveSpeedReset) {

        //    float distanceToCampZoneLimit = Mathf.Abs(CampZoneManager.Instance.GetClosestExteriorZoneLimit(transform.position).x - transform.position.x);
        //    float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);

        //    if (distanceToCampZoneLimit < distanceToCampOrPlayerToSetStandardSpeed || distanceToPlayer < distanceToCampOrPlayerToSetStandardSpeed) {
        //        creatureMovement.InitializeCreatureMoveSpeed();
        //        nightMoveSpeedReset = true;
        //    }
        //}
    }

    protected virtual void WalkingToSpawnerStateUpdate() {
        MoveTowardsSpawner();
        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }
    }
    protected virtual void WalkingToPlayerStateUpdate() {
        MoveTowardsPlayer();
        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }
    }


    protected virtual void MoveToTargetStateUpdate() {
        if (!detectedAttackTarget) {
            if (creature.IsDayCreature()) {
                if(creature.IsAgressiveDayCreature()) {
                    ChangeState(State.walkingToPlayer);
                } else {
                    ChangeState(State.walkingToSpawner);
                }
                return;
            }
            else {
                ChangeState(State.walkingToFire);
                return;
            }
        }

        HeadToTarget();

    }

    protected virtual void AttackingStateUpdate() {
        if (!detectedAttackTarget) {
            if (creature.IsDayCreature()) {
                if(creature.IsAgressiveDayCreature()) {
                    ChangeState(State.walkingToPlayer);
                } else {
                    ChangeState(State.walkingToSpawner);
                }

            }
            else {
                ChangeState(State.walkingToFire);
            }
            return;
        }

        if (attackTarget == null || (!CheckAttackTargetInRange() && !creatureAttack.GetAttackStarted())) {
            ChangeState(State.moveToTarget);
            return;
        }
    }
    #endregion

    protected void Roam() {

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0 && positionToRoamAmound != Vector3.zero) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(creatureMovement, roamRadius, positionToRoamAmound, creature.GetCreatureSO().flying);
        }
    }

    protected void HandleAggroRecently() {
        if (aggroedRecently) {

            aggroTimer -= Time.deltaTime;

            if (aggroTimer < 0) {
                aggroTimer = aggroDelay;
                aggroedRecently = false;
            }
        }
    } 

    protected virtual void ChangeState(State newState) {
        if (died) return;
        if (newState == State.attacking) {
            creatureMovement.SetMoveTarget(transform.position);
            creatureAttack.SetAttackTarget(attackTarget);

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }
        }

        if(newState == State.walkingToFire) {
            creatureMovement.SetMoveTarget(transform.position);
            creatureAttack.RemoveAttackTarget();

            //creatureMovement.SetMoveSpeed(walkingToFireMoveSpeed);
            followingTargetBuffedSpeed = true;
        }

        if (newState == State.walkingToPlayer) {
            creatureMovement.SetMoveTarget(Player.Instance.transform.position);
            creatureMovement.SetCreatureAggroMoveSpeed(true);
            followingTargetBuffedSpeed = true;
        }

        if (newState == State.walkingToSpawner) {
            creatureMovement.SetMoveTarget(transform.position);
            creatureAttack.RemoveAttackTarget();

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }

            if(state == State.moveToTarget) {
                // Creature un aggro player
                OnCreatureUntargetPlayer?.Invoke(this, EventArgs.Empty);
                OnAnyCreatureUntargetPlayer?.Invoke(this, EventArgs.Empty);
            }
        }

        if (newState == State.moveToTarget) {
            if(creature.IsDayCreature()) {
                creatureMovement.SetCreatureAggroMoveSpeed(true);
                followingTargetBuffedSpeed = true;
            }

            creatureAttack.RemoveAttackTarget();
        }

        if (newState == State.idle) {
            if (creature.IsDayCreature()) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }

            creatureMovement.SetMoveTarget(transform.position);
            creatureAttack.RemoveAttackTarget();
        }

        state = newState;
    }

    protected virtual bool CheckAttackTargetInRange() {
        if ((attackTarget as MonoBehaviour) == null) return false;
        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;

        if (Mathf.Abs(transform.position.x - targetPosition.x) < maxAttackRange) {
            return true;
        } else {
            return false;
        }
    }

    protected virtual void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;
        
        Vector3 targetDestination = attackTarget.GetMeleeAttackPosition().position;

        creatureMovement.SetMoveTarget(targetDestination);
        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.attacking);
        }
    }

    protected virtual void MoveTowardsFire() {
        creatureMovement.SetMoveTarget(Vector3.zero);
    }

    protected void MoveTowardsSpawner() {
        Vector3 targetDestination = transform.position;

        if (creature.GetMobSpawner() != null) {
            targetDestination = creature.GetMobSpawner().transform.position;
        }
       
        creatureMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.idle);
        }
    }
    protected void MoveTowardsPlayer() {
        Vector3 targetDestination = Player.Instance.transform.position;

        creatureMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.idle);
        }
    }


    public void ResetAttackTargetInProximity() {
        detectedAttackTarget = false;
        attackTarget = null;
    }

    public virtual void SetAttackTarget(IDamageable iDamageable, List<IDamageable> iDamageablesInRange) {
        if (!spawned) return;

        if (iDamageablesInRange.Count == 0) {
            detectedAttackTarget = false;
            attackTarget = null;
            return;
        } 

        if(iDamageable != null) {
            detectedAttackTarget = true;

            if (attackTarget == iDamageable) return;

            attackTarget = iDamageable;

            CheckPlayerTargetAndAggroState();

            ChangeState(State.moveToTarget);
        } else {
            detectedAttackTarget = false;
            attackTarget = null;
        }
    }

    public IDamageable GetAttackTarget() {
        return attackTarget;
    }

    protected void CheckPlayerTargetAndAggroState() {
        if (!creature.IsDayCreature()) return;

        if (!aggroedRecently) {
            aggroedRecently = true;
            aggroTimer = aggroDelay;
        }

        if (attackTarget is Player) {
            OnCreatureAggro?.Invoke(this, EventArgs.Empty);
            OnAnyCreatureAggro?.Invoke(this, EventArgs.Empty);
            OnCreatureTargetPlayer?.Invoke(this, EventArgs.Empty);
        };
    }

    protected void Creature_OnMobHitObstacle(object sender, EventArgs e) {
        roamTimer = 0;
        ChangeState(State.idle);
    }

    protected virtual void Creature_OnCreatureDied(object sender, EventArgs e) {
        died = true;
    }

    public State GetState() {
        return state;
    }
}
