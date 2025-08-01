using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardJob : WorkerJob {

    private GuardState state;
    private GuardState previousState;

    private float attackRange = 1f;
    private float followPlayerTargetingRange = 10f;
    private float attackRangeRandomized;
    private float distanceToOuterWallWhenGuarding = 3f;
    private float distanceToOuterWallToTargetCreatureAtNight = 14f;

    private IEscortable assignedEscortable;
    private Transform escortTransform;

    public enum GuardState {
        headingToEscort,
        escorting,
        guardingDay,
        droppingOrbs,
        headingToGuard,
        guardingNight,
    }
    public event EventHandler OnGuardChangedState;

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
        float workerDetectionColliderRadius = workerDetectionCollider.GetComponent<CircleCollider2D>().radius;
        distanceToFleeFromCreature = UnityEngine.Random.Range(workerDetectionColliderRadius - workerDetectionColliderRadius / 3, workerDetectionColliderRadius - workerDetectionColliderRadius / 4);
        attackRangeRandomized = UnityEngine.Random.Range(attackRange - attackRange / 4, attackRange + attackRange / 4);

        maxDistanceToEscortTargetWhenEscorting = 10f;
    }

    protected override void Start() {
        base.Start();
        ScavengableObstacle.OnAnyScavengableObstacleActivatedMining += ScavengableObstacle_OnAnyScavengableObstacleActivatedMining;
        ScavengableObstacle.OnAnyObstacleBuilt += ScavengableObstacle_OnAnyObstacleBuilt;
        ScavengableObstacle.OnAnyScavengableObstacleDeActivatedMining += ScavengableObstacle_OnAnyScavengableObstacleDeActivatedMining;
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (targetCreature != null) {
            Debug.DrawLine(mobMovement.transform.position, targetCreature.transform.position, Color.red);
        }

        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(GuardState.droppingOrbs);
        }

        if(escorting) {
            switch (state) {

                case GuardState.headingToEscort:

                    HeadToEscort();

                    break;

                case GuardState.escorting:


                    CheckAggroClosestCreatureSmart(transform.position, followPlayerTargetingRange);
                    if (aggroedCreature == null) {
                        targetCreature = null;
                        workerAttack.RemoveAttackTarget();
                        ChangeState(GuardState.headingToEscort);
                        return;
                    }

                    if (GuardIsTooFarFromEscortedTarget()) {
                        ChangeState(GuardState.headingToEscort);
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
                    DroppingOrbsUpdate();
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
                    DroppingOrbsUpdate();

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

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
       
        ChangeState(GuardState.headingToGuard);
    }
    private void HeadToEscort() {
        mobMovement.SetMoveSpeed(fleeOrHeadToEscortMoveSpeed);
        Vector3 destination = escortTransform.position;
        if (Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
        else {
            ChangeState(GuardState.escorting);
        }
    }

    private bool GuardIsTooFarFromEscortedTarget() {
        Vector3 destination = escortTransform.position;
        if (Mathf.Abs(destination.x - transform.position.x) > maxDistanceToEscortTargetWhenEscorting) {
            return true;
        }
        else {
            return false;
        }
    }

    protected override void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        base.WorkerAI_OnWorkerFollowPlayerChanged(sender, e);
        workerAttack.RemoveAttackTarget();

        if (escorting) {
            state = GuardState.headingToEscort;
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

            Vector3 safePosition = new Vector3(closestCreature.transform.position.x + direction * distanceToFleeFromCreature, 0, 0);
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
        if (escorting) return;
        ChangeState(GuardState.headingToGuard);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (escorting) return;
        ChangeState(GuardState.headingToEscort);
    }

    private void ChangeState(GuardState newState) {
        if (newState == state) return;
        previousState = state;

        if(newState == GuardState.headingToEscort) {
            workerAttack.RemoveAttackTarget();
        }

        Vector3 targetDestination = mobMovement.transform.position;

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnGuardChangedState?.Invoke(this, EventArgs.Empty);
    }
    public override void ReturnToPreviousState() {
        ChangeState(previousState);
    }

    private void ScavengableObstacle_OnAnyScavengableObstacleActivatedMining(object sender, EventArgs e) {
        ScavengableObstacle scavengableObstacle = sender as ScavengableObstacle;

        if (!scavengableObstacle.GetEscortedByWorkers()) return;
        // Check if obstacle is on the same side
        if ((scavengableObstacle.transform.position.x > 0 && worker.GetCampSideAddigned() == CampZoneManager.CampSide.left) || (scavengableObstacle.transform.position.x < 0 && worker.GetCampSideAddigned() == CampZoneManager.CampSide.right)) return; ;

        assignedEscortable = scavengableObstacle;
        escortTransform = assignedEscortable.GetEscortTransform();
        escorting = true;
        workerAI.SetEscorting(true);
        ChangeState(GuardState.headingToEscort);
    }

    private void ScavengableObstacle_OnAnyObstacleBuilt(object sender, EventArgs e) {
        escorting = false;
        workerAI.SetEscorting(false);
        ChangeState(GuardState.guardingDay);
    }


    private void ScavengableObstacle_OnAnyScavengableObstacleDeActivatedMining(object sender, EventArgs e) {
        escorting = false;
        ChangeState(GuardState.guardingDay);
        workerAI.SetEscorting(false);
    }

    public GuardState GetState() {
        return state;
    }
    private void OnDestroy() {
        ScavengableObstacle.OnAnyScavengableObstacleActivatedMining -= ScavengableObstacle_OnAnyScavengableObstacleActivatedMining;
        ScavengableObstacle.OnAnyObstacleBuilt -= ScavengableObstacle_OnAnyObstacleBuilt;
        ScavengableObstacle.OnAnyScavengableObstacleDeActivatedMining -= ScavengableObstacle_OnAnyScavengableObstacleDeActivatedMining;
    }
}
