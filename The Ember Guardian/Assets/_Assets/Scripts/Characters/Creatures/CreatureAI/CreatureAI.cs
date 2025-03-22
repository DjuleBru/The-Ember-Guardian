using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    protected Creature creature;
    protected CreatureMovement creatureMovement;
    protected MobAttack mobAttack;

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
    public event EventHandler OnCreatureUntargetPlayer;
    public event EventHandler OnCreatureTargetPlayer;
    public static event EventHandler OnAnyCreatureAggro;

    public enum State {
        idle,
        walkingToFire,
        walkingToSpawner,
        moveToTarget,
        attacking,
    }

    protected State state;

    protected virtual void Awake() {
        creatureMovement = GetComponent<CreatureMovement>();
        mobAttack = GetComponent<MobAttack>();
        creature = GetComponent<Creature>();

        spawned = false;
        StartCoroutine(SetSpawnedAfterDelay(creature.GetCreatureSO().spawnAnimationDuration));
    }

    protected virtual void Start() {
        creature.OnCreatureDied += Creature_OnCreatureDied;

        SetAttackRange();
    }

    protected virtual void SetAttackRange() {

        minAttackRange = creature.GetCreatureSO().minAttackRange + UnityEngine.Random.Range(-creature.GetCreatureSO().attackRangeRandomizer, creature.GetCreatureSO().attackRangeRandomizer);
        maxAttackRange = creature.GetCreatureSO().maxAttackRange + UnityEngine.Random.Range(-creature.GetCreatureSO().attackRangeRandomizer, creature.GetCreatureSO().attackRangeRandomizer);
    
    }

    private void SetInitialState() {


        if (creature.IsDayCreature()) {

            positionToRoamAmound = creature.GetMobSpawner().transform.position;
            roamRadius = creature.GetMobSpawner().GetRadiusToRoamAround();

            ChangeState(State.idle);

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
        if(!nightMoveSpeedReset) {

            float distanceToCampZoneLimit = Mathf.Abs(CampZoneManager.Instance.GetClosestExteriorZoneLimit(transform.position).x - transform.position.x);
            float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);

            if (distanceToCampZoneLimit < distanceToCampOrPlayerToSetStandardSpeed || distanceToPlayer < distanceToCampOrPlayerToSetStandardSpeed) {
                creatureMovement.InitializeCreatureMoveSpeed();
                nightMoveSpeedReset = true;
            }
        }
    }

    protected virtual void WalkingToSpawnerStateUpdate() {
        MoveTowardsSpawner();
        if (detectedAttackTarget && !aggroedRecently) {
            ChangeState(State.moveToTarget);
        }
    }
    protected virtual void MoveToTargetStateUpdate() {
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

    protected virtual void AttackingStateUpdate() {
        if (!detectedAttackTarget) {
            if (creature.IsDayCreature()) {
                ChangeState(State.walkingToSpawner);
            }
            else {
                ChangeState(State.walkingToFire);
            }
            return;
        }

        if (attackTarget == null || (!CheckAttackTargetInRange() && !mobAttack.GetAttackStarted())) {
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
            mobAttack.SetAttackTarget(attackTarget);

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }
        }

        if(newState == State.walkingToFire) {
            creatureMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            creatureMovement.SetMoveSpeed(walkingToFireMoveSpeed);
            followingTargetBuffedSpeed = true;
        }

        if (newState == State.walkingToSpawner) {
            creatureMovement.SetMoveTarget(transform.position);
            mobAttack.RemoveAttackTarget();

            if (followingTargetBuffedSpeed) {
                creatureMovement.SetCreatureAggroMoveSpeed(false);
                followingTargetBuffedSpeed = false;
            }

            if(state == State.moveToTarget) {
                // Creature un aggro player
                OnCreatureUntargetPlayer?.Invoke(this, EventArgs.Empty);
            }
        }

        if (newState == State.moveToTarget) {
            creatureMovement.SetCreatureAggroMoveSpeed(true);
            followingTargetBuffedSpeed = true;
            mobAttack.RemoveAttackTarget();
        }

        if (newState == State.idle) {
            creatureMovement.SetMoveTarget(transform.position);
            creatureMovement.SetCreatureAggroMoveSpeed(false);
            followingTargetBuffedSpeed = false;
            mobAttack.RemoveAttackTarget();
        }

        state = newState;
    }

    protected bool CheckAttackTargetInRange() {
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
        Vector3 targetDestination = creature.GetMobSpawner().transform.position;

        creatureMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < minAttackRange) {
            ChangeState(State.idle);
        }
    }

    public void ResetAttackTargetInProximity() {
        detectedAttackTarget = false;
        attackTarget = null;
    }

    public void SetAttackTarget(IDamageable iDamageable, List<IDamageable> iDamageablesInRange) {
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

    protected virtual void Creature_OnCreatureDied(object sender, EventArgs e) {
        died = true;
    }

    public State GetState() {
        return state;
    }
}
