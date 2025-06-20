using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerJob : WorkerJob {

    private MinerState state;
    private MinerState previousState;

    private IScavengable assignedScavengable;
    private Transform assignedScavengableMiningPosition;

    private float headToMineMoveSpeed = 2.5f;
    private float attackRange = 1f;
    private float attackRangeRandomized;

    private bool isNightOrDusk;

    public enum MinerState {
        idle,
        droppingOrbs,
        followPlayerIdle,
        followPlayerAttackCreature,
        blockedByCreatures,
        headToSafety,
        headingToMine,
        pickingUpOrbs,
        Mining,
    }

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
        float workerDetectionColliderRadius = workerDetectionCollider.GetComponent<CircleCollider2D>().radius;
        distanceToFleeFromCreature = UnityEngine.Random.Range(workerDetectionColliderRadius - workerDetectionColliderRadius / 3, workerDetectionColliderRadius - workerDetectionColliderRadius / 4);
        
        maxDistanceToPlayerWhenFollowing = 5f;
        minDistanceToPlayerWhenFollowing = 3f;
        attackRangeRandomized = UnityEngine.Random.Range(attackRange - attackRange / 4, attackRange + attackRange / 4);
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(MinerState.droppingOrbs);
        }

        if (followingPlayer) {
            switch (state) {

                case MinerState.followPlayerIdle:

                    FollowPlayer();
                    if (closestCreature != null) {

                        CheckAggroClosestCreatureSmart(transform.position, minimumDistanceToStaySafeFromCreature);

                        if (aggroedCreature == null) {

                            targetCreature = null;
                            workerAttack.RemoveAttackTarget();

                        }
                        else {
                            if (PlayerIsCloseEnoughFromWorker()) {
                                ChangeState(MinerState.followPlayerAttackCreature);
                            };

                        }
                    }

                break;

                case MinerState.followPlayerAttackCreature:

                    if (!CreatureIsTooClose(closestCreature, minimumDistanceToStaySafeFromCreature)) {
                        ChangeState(MinerState.followPlayerIdle);
                    }

                    CheckAggroClosestCreatureSmart(transform.position, minimumDistanceToStaySafeFromCreature);
                    if (aggroedCreature == null) {
                        targetCreature = null;
                        workerAttack.RemoveAttackTarget();
                        ChangeState(MinerState.followPlayerIdle);
                        return;
                    }

                    if (PlayerIsTooFarFromWorker()) {
                        ChangeState(MinerState.followPlayerIdle);
                        workerAttack.RemoveAttackTarget();
                        return;
                    }

                    if (!TargetIsInRange(aggroedCreature, attackRangeRandomized)) {
                        targetCreature = null;
                        workerAttack.RemoveAttackTarget();

                        mobMovement.SetMoveTarget(aggroedCreature.transform.position);
                        mobMovement.SetMoveSpeed(headToCampMoveSpeed);
                    }
                    else {
                        TargetCreature(aggroedCreature);
                    }
                    break;

                case MinerState.droppingOrbs:

                    DroppingOrbsUpdate();

                break;
            }
        } else {

            if (CheckBlockedByCreature() && state != MinerState.blockedByCreatures) {
                ChangeState(MinerState.blockedByCreatures);
                return;
            };

            switch (state) {

                case MinerState.idle:
                    RoamInCampCenter();
                    CheckAvailableScavengables();

                    if (assignedScavengable != null && !isNightOrDusk) {
                        ChangeState(MinerState.headingToMine);
                    }

                    break;

                case MinerState.headToSafety:
                    if (isInSafeZone) {
                        RoamInCampCenter();
                    }
                    else {
                        HeadToCampCenter();
                    }
                break;

                case MinerState.headingToMine:

                    HeadToMine();

                break;

                case MinerState.Mining:

                    Mine();
                    if(CheckOrbsToCollect()) {
                        ChangeState(MinerState.pickingUpOrbs);
                    }

                break;

                case MinerState.pickingUpOrbs:

                    HeadToPickUpClosestOrb();

                break;

                case MinerState.droppingOrbs:

                    DroppingOrbsUpdate();

                break;

                case MinerState.blockedByCreatures:
                    StayOutOfCreatureRange();

                    if (!CheckBlockedByCreature()) {
                        ChangeState(MinerState.idle);
                    }
                    break;
            }
        }

    }

    public void StayOutOfCreatureRange() {

        mobMovement.SetMoveSpeed(fleeMoveSpeed);

        Creature closestCreature = workerDetectionCollider.GetClosestCreature();

        if (closestCreature != null) {
            if (CreatureIsTooClose(closestCreature, distanceToStartFleeingFromCreature)) {
                StayAwayFromCreature(closestCreature);
                return;
            }
        }
    }

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if (Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    public void HeadToPickUpClosestOrb() {

        if (orbsToCollect.Count == 0) {
            if((assignedScavengable as MonoBehaviour) == null) {
                ChangeState(MinerState.idle);
            } else {
                ChangeState(MinerState.headingToMine);
            }
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);

    }

    private void Mine() {
        workerAttack.SetAttackTarget(assignedScavengable);
    }

    private void HeadToMine() {
        float distanceToScavengable = Mathf.Abs(transform.position.x - assignedScavengableMiningPosition.position.x);

        if(distanceToScavengable < .5f) {
            ChangeState(MinerState.Mining);
            assignedScavengable.MinerStartsMining(this);

            if (assignedScavengable.GetIsMine()) {
                gameObject.SetActive(false);
            }
        } else {
            mobMovement.SetMoveTarget(assignedScavengableMiningPosition.position);
        }
    }

    private void CheckAvailableScavengables() {
        IScavengable assignableScavengable = ScavengableManager.Instance.GetClosestHighestPriorityScavengableToScavenge(transform.position);

        if(assignableScavengable != null) {
            AssignScavengable(assignableScavengable);
        }

    }

    public void AssignScavengable(IScavengable scavengable) {
        assignedScavengable = scavengable;
        scavengable.AssignMiner(this);
        assignedScavengableMiningPosition = scavengable.GetMeleeAttackPosition();
    }

    public void UnAssignScavengable() {
        assignedScavengable.UnassignMiner(this);
        assignedScavengable = null;
        workerAttack.RemoveAttackTarget();

        if (CheckOrbsToCollect()) {
            ChangeState(MinerState.pickingUpOrbs);
        } else {
            ChangeState(MinerState.idle);
        }
    }

    public event EventHandler OnMinerChangedState;

    public override void InitializeJob() {
        base.InitializeJob();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(MinerState.idle);
        }
        else {
            ChangeState(MinerState.headToSafety);
        }
    }

    protected override void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        base.WorkerAI_OnWorkerFollowPlayerChanged(sender, e);
        workerAttack.RemoveAttackTarget();

        if (followingPlayer) {
            ChangeState(MinerState.followPlayerIdle);
        } else {
            ChangeState(MinerState.idle);
        }
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        isNightOrDusk = true;

        if (followingPlayer) return;
        ChangeState(MinerState.headToSafety);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        isNightOrDusk = false;

        if (followingPlayer) return;
        ChangeState(MinerState.idle);
    }

    private void ChangeState(MinerState newState) {
        if (newState == state) return;

        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnMinerChangedState?.Invoke(this, EventArgs.Empty);

        if(state == MinerState.idle) {
            if(assignedScavengable != null) {
                UnAssignScavengable();
            }
            mobMovement.SetMoveSpeed(roamMoveSpeed);
        }
        if (state == MinerState.headingToMine) {
            workerAttack.RemoveAttackTarget();
            mobMovement.SetMoveSpeed(headToMineMoveSpeed);
        }
        if (state == MinerState.pickingUpOrbs) {
            workerAttack.RemoveAttackTarget();
        }
        if (state == MinerState.headToSafety) {
            workerAttack.RemoveAttackTarget();
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
        }
    }
    public override void ReturnToPreviousState() {
        ChangeState(previousState);
    }


    public void ExitFromMine() {
        gameObject.SetActive(true);
    }

    public MinerState GetState() {
        return state;
    }

    public IScavengable GetScavengableAssigned() {
        return assignedScavengable;
    }
}
