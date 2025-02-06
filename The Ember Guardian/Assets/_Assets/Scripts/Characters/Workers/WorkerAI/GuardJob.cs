using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardJob : WorkerJob {

    private GuardState state;
    private GuardState previousState;

    private bool followingPlayer; 

    private float attackRange = 1f;
    private float followPlayerTargetingRange = 10f;
    private float attackRangeRandomized;
    private float distanceToOuterWallWhenGuarding = 3f;
    private float distanceToOuterWallToTargetCreatureAtNight = 14f;

    public enum GuardState {
        followPlayerIdle,
        followPlayerAttackCreature,
        guardingDay,
        droppingOrbs,
        headingToGuard,
        guardingNight,
    }
    public event EventHandler OnGuardChangedState;

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
        float workerDetectionColliderRadius = workerDetectionCollider.GetComponent<CircleCollider2D>().radius;
        distanceToStaySafeFromCreature = UnityEngine.Random.Range(workerDetectionColliderRadius - workerDetectionColliderRadius / 3, workerDetectionColliderRadius - workerDetectionColliderRadius / 4);
        attackRangeRandomized = UnityEngine.Random.Range(attackRange - attackRange / 4, attackRange + attackRange / 4);

        maxDistanceToPlayerWhenFollowing = 10f;
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (targetCreature != null) {
            Debug.DrawLine(mobMovement.transform.position, targetCreature.transform.position, Color.red);
        }

        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(GuardState.droppingOrbs);
        }

        if(followingPlayer) {
            switch (state) {

                case GuardState.followPlayerIdle:

                    FollowPlayer();

                    if (closestCreature != null) {

                        CheckAggroClosestCreatureSmart(transform.position, followPlayerTargetingRange);

                        if (aggroedCreature == null) {

                            targetCreature = null;
                            workerAttack.RemoveAttackTarget();

                        }
                        else {
                            if (!PlayerIsTooFarFromWorker()) {
                                ChangeState(GuardState.followPlayerAttackCreature);
                            };

                        }
                    }

                break;

                case GuardState.followPlayerAttackCreature:

                    CheckAggroClosestCreatureSmart(transform.position, followPlayerTargetingRange);
                    if (aggroedCreature == null) {
                        targetCreature = null;
                        workerAttack.RemoveAttackTarget();
                        ChangeState(GuardState.followPlayerIdle);
                        return;
                    }

                    if (PlayerIsTooFarFromWorker()) {
                        ChangeState(GuardState.followPlayerIdle);
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

                case GuardState.droppingOrbs:

                    if (worker.GetTotalCurrencyAmount() == 0) {
                        ChangeState(previousState);
                        return;
                    }

                    if (worker.PlayerIsCloseAndStayedAround()) {
                        worker.DropCurrencies();
                        return;
                    }

                    if (!worker.GetPlayerIsClose()) {
                        ChangeState(previousState);
                        return;
                    }

                break;

            }
        } else {
            switch (state) {

                case GuardState.guardingDay:

                    GuardCamp(true, 5f);

                    break;

                case GuardState.headingToGuard:

                    HeadToMostExteriorBarricade();
                    break;

                case GuardState.guardingNight:

                    GuardCamp(false, 2f);

                    break;

                case GuardState.droppingOrbs:

                    if (worker.GetTotalCurrencyAmount() == 0) {
                        ChangeState(previousState);
                        return;
                    }

                    if (worker.PlayerIsCloseAndStayedAround()) {
                        worker.DropCurrencies();
                        return;
                    }

                    if (!worker.GetPlayerIsClose()) {
                        ChangeState(previousState);
                        return;
                    }

                    break;

            }

        }
    }

    private void GuardCamp(bool roamAroundGuardingPosition, float maxDistanceToGuardingPosition) {
        Vector3 guardingPosition = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), -distanceToOuterWallWhenGuarding);

        if (closestCreature == null) {
            // Keep checking if camp limits changed

            if (Mathf.Abs(transform.position.x - guardingPosition.x) > maxDistanceToGuardingPosition) {
                ChangeState(GuardState.headingToGuard);
                return;
            }

            if(roamAroundGuardingPosition) {
                Roam(5f, guardingPosition);
            }

        }

        else {

            Vector2 outerWallPosition = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned());
            CheckAggroClosestCreatureSmart(outerWallPosition, distanceToOuterWallToTargetCreatureAtNight);

            if (aggroedCreature == null) {
                targetCreature = null;
                workerAttack.RemoveAttackTarget();
            }

            else {
                if (!TargetIsInRange(aggroedCreature, attackRangeRandomized)) {
                    targetCreature = null;
                    workerAttack.RemoveAttackTarget();

                    if (TargetIsCloseToOuterWall(aggroedCreature)) {
                        mobMovement.SetMoveTarget(aggroedCreature.transform.position);
                        mobMovement.SetMoveSpeed(headToCampMoveSpeed);
                    }
                    else {
                        mobMovement.SetMoveTarget(guardingPosition);
                        mobMovement.SetMoveSpeed(roamMoveSpeed);
                    }

                }
                else {
                    TargetCreature(aggroedCreature);
                }
            }

        }
    }

    public override void InitializeJob() {
        base.InitializeJob();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
       
        ChangeState(GuardState.headingToGuard);
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
        if(followingPlayer) {
            state = GuardState.followPlayerIdle;
        } else {
            state = GuardState.headingToGuard;
        }
        roamTimer = 0;
    }

 

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if(Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    private bool TargetIsCloseToOuterWall(IDamageable iDamageable) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), -distanceToOuterWallWhenGuarding).x) < (distanceToOuterWallToTargetCreatureAtNight)) {
            return true;
        }
        else {
            return false;
        }
    }

    public void StayOutOfCreatureRange() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        Creature closestCreature = workerDetectionCollider.GetClosestCreature();

        if (closestCreature != null) {
            bool playerIsInFrontOfHunter = Mathf.Abs(transform.position.x) - Mathf.Abs((Player.Instance.transform.position.x)) < 0;

            if (!playerIsInFrontOfHunter) {
                StayAwayFromCreatures(closestCreature);
                return;
            }
        }
    }

    public void StayAwayFromCreatures(Creature closestCreature) {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        if (closestCreature != null) {

            float direction = closestCreature.transform.position.x - transform.position.x;
            if (direction > 0) {
                direction = -1;
            }
            else {
                direction = 1;
            }

            Vector3 safePosition = new Vector3(closestCreature.transform.position.x + direction * distanceToStaySafeFromCreature, 0, 0);
            RoamBehavior.RoamAroundPoint(mobMovement, 3f, safePosition, false);

            return;
        }

    }

    public void HeadToMostExteriorBarricade() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), -3f);
        Vector3 targetDestinationRandomized = new Vector3(targetDestination.x + UnityEngine.Random.Range(-1f, 1f), 0, 0);

        mobMovement.SetMoveTarget(targetDestinationRandomized);

        if (Mathf.Abs(transform.position.x - targetDestinationRandomized.x) < 0.1f) {
            if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dawn || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Day) {
                ChangeState(GuardState.guardingDay);
            } else {
                ChangeState(GuardState.guardingNight);
            }
        
        }

        hasSetSpeed = false;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (followingPlayer) return;
        ChangeState(GuardState.headingToGuard);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (followingPlayer) return;
        ChangeState(GuardState.followPlayerIdle);
    }

    private void ChangeState(GuardState newState) {
        if (newState == state) return;
        previousState = state;

        if(newState == GuardState.followPlayerIdle) {
            workerAttack.RemoveAttackTarget();
        }

        Vector3 targetDestination = mobMovement.transform.position;

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnGuardChangedState?.Invoke(this, EventArgs.Empty);
    }

    public GuardState GetState() {
        return state;
    }
}
