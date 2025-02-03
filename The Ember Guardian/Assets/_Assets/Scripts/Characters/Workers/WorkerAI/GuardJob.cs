using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardJob : MonoBehaviour
{
    private Worker worker;
    private WorkerAI workerAI;
    private MobMovement mobMovement;
    private WorkerAnimatorManager workerAnimatorManager;
    private MobAttack guardAttack;
    private Creature targetCreature;
    private Creature aggroedCreature;
    private Creature closestCreature;
    private WorkerDetectionCollider workerDetectionCollider;

    private GuardState state;
    private GuardState previousState;

    private bool followingPlayer; 

    private bool hasSetSpeed; 
    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float headToCampMoveSpeed = 3f;
    [SerializeField] private float roamChangeDestinationRate = 10f;

    private float attackRange = 1f;
    private float targetingRange = 8f;
    private float attackRangeRandomized;
    private float roamTimer;
    private float distanceToOuterWallWhenGuarding = 3f;
    private float distanceToStaySafeFromCreature = 20f;
    private float distanceToOuterWallToTargetCreatureAtNight = 14f;

    private float checkClosestTargetTimer;
    private float checkClosestTargetCooldown = .3f;

    public enum GuardState {
        idle,
        blockedByCreatures,
        droppingOrbs,
        headingToGuard,
        guarding,
    }
    public event EventHandler OnGuardChangedState;

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
        float workerDetectionColliderRadius = workerDetectionCollider.GetComponent<CircleCollider2D>().radius;
        distanceToStaySafeFromCreature = UnityEngine.Random.Range(workerDetectionColliderRadius - workerDetectionColliderRadius / 3, workerDetectionColliderRadius - workerDetectionColliderRadius / 4);
        attackRangeRandomized = UnityEngine.Random.Range(attackRange - attackRange / 4, attackRange + attackRange / 4);
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (targetCreature != null) {
            Debug.DrawLine(mobMovement.transform.position, targetCreature.transform.position, Color.red);
        }

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            CheckDropCurrenciesToPlayer();
        }

        if(followingPlayer) {
            switch (state) {

                case GuardState.idle:
                    FollowPlayer();
                break;
            }
        } else {
            switch (state) {

                case GuardState.idle:
                    Roam();

                    if (CheckBlockedByCreature()) {
                        ChangeState(GuardState.blockedByCreatures);
                        return;
                    };
                    break;

                case GuardState.blockedByCreatures:
                    StayOutOfCreatureRange();

                    if (!CheckBlockedByCreature()) {
                        ChangeState(GuardState.idle);
                    }

                    break;

                case GuardState.headingToGuard:

                    HeadToMostExteriorBarricade();
                    break;

                case GuardState.guarding:

                    Vector3 guardingPosition = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), -distanceToOuterWallWhenGuarding);

                    if (closestCreature == null) {
                        // Keep checking if camp limits changed

                        if (Mathf.Abs(transform.position.x - guardingPosition.x) > 2f) {
                            ChangeState(GuardState.headingToGuard);
                            return;
                        }

                    }

                    else {

                        CheckAggroClosestCreatureFromOuterWallSmart();

                        if (aggroedCreature == null) {
                            targetCreature = null;
                            guardAttack.RemoveAttackTarget();

                        }
                        else {
                            if (!TargetIsInRange(aggroedCreature)) {
                                targetCreature = null;
                                guardAttack.RemoveAttackTarget();

                                if (TargetIsCloseToOuterWall(aggroedCreature)) {
                                    mobMovement.SetMoveTarget(aggroedCreature.transform.position);
                                }
                                else {
                                    mobMovement.SetMoveTarget(guardingPosition);
                                }

                            }
                            else {
                                TargetCreature(aggroedCreature);
                            }
                        }

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

        }
    }

    public void InitializeGuardJob() {
        mobMovement = GetComponentInChildren<MobMovement>();
        workerAnimatorManager = GetComponentInChildren<WorkerAnimatorManager>();
        guardAttack = GetComponent<MobAttack>();
        worker = GetComponent<Worker>();
        workerAI = GetComponent<WorkerAI>();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        mobMovement = GetComponent<MobMovement>();
        mobMovement.SetMoveTarget(mobMovement.transform.position);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(GuardState.idle);
        }
        else {
            ChangeState(GuardState.headingToGuard);
        }
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
        state = GuardState.idle;
        roamTimer = 0;
    }

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if(Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    private bool CheckBlockedByCreature() {
        if (closestCreature != null) {
            return true;
        }
        else {
            return false;
        }

    }
    public void Roam() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(mobMovement, 10f, CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned()), false);
        }
    }

    private bool TargetIsInRange(IDamageable iDamageable) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < (attackRangeRandomized)) {
            return true;
        }
        else {
            return false;
        }
    }
    private bool CreatureIsTargetable(Creature creature) {
        if (creature.GetCreatureSO().flying) {
            return false;
        }
        else {
            return true;
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

    private bool CheckClosestCreatureSmart() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Creature newTargetCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(mobMovement.transform.position, attackRangeRandomized, guardAttack.GetAttackDamage(), false);

            if (newTargetCreature == null) {
                targetCreature = null;
                return false;

            }

            else {
                TargetCreature(newTargetCreature);
                return true;
            }
        }

        return false;
    }
    private bool CheckAggroClosestCreatureFromOuterWallSmart() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Vector2 outerWallPosition = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned());
            Creature newTargetCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(outerWallPosition, distanceToOuterWallToTargetCreatureAtNight, guardAttack.GetAttackDamage(), false);

            if (newTargetCreature == null) {
                aggroedCreature = null;
                return false;

            }

            else {
                aggroedCreature = newTargetCreature;
                return true;
            }
        }

        return false;
    }

    private void TargetCreature(Creature newTargetCreature) {
        if (targetCreature == newTargetCreature) return;

        if (targetCreature != null) {
            targetCreature.OnMobDied -= TargetCreature_OnMobDied;
        }

        mobMovement.SetMoveTarget(transform.position);
        guardAttack.SetAttackTarget(newTargetCreature);
        targetCreature = newTargetCreature;
        targetCreature.OnMobDied += TargetCreature_OnMobDied;
    }

    private void TargetCreature_OnMobDied(object sender, System.EventArgs e) {
        targetCreature = null;
        guardAttack.RemoveAttackTarget();
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
            ChangeState(GuardState.blockedByCreatures);

            return;
        }

    }

    private void CheckDropCurrenciesToPlayer() {
        if (worker.GetPlayerIsClose() && worker.GetTotalCurrencyAmount() > 0) {
            ChangeState(GuardState.droppingOrbs);
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
            ChangeState(GuardState.guarding);
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
        ChangeState(GuardState.idle);
    }

    private void ChangeState(GuardState newState) {
        if (newState == state) return;
        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnGuardChangedState?.Invoke(this, EventArgs.Empty);
    }

    public GuardState GetState() {
        return state;
    }
}
