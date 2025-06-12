using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineerJob : WorkerJob {

    public enum EngineerState {
        idle,
        droppingOrbs,
        pickingUpOrbs,
        followPlayerIdle,
        blockedByCreatures,
        headToSafety,

        headingToStructure,
        workingInStructure,
        headingToRefill,
        refillingStructure,

        manningDefensiveTower,
    }

    private WorkerCurrencies workerCurrencies;
    private WorkerMovement workerMovement;

    private float headingToStructureMoveSpeed = 2.5f;

    private bool isNightOrDusk;
    private bool followingPlayer;

    private bool reachedAssignedStructureWorkingPoint;
    private Structure assignedStructure;
    private Vector3 assignedStructureRandomDestinationPoint;
    private CurrencyStorage targetCurrencyStorage;

    private EngineerState previousState;
    private EngineerState state;

    private float turnWrenchTimer;
    private float turnWrenchDelay = 3f;

    public event EventHandler OnEngineerTurnsWrench;
    public event EventHandler OnEngineerChangedState;
    public event EventHandler OnEngineerHideTool;
    public event EventHandler OnOrbExtractorTriggeredDrill;

    protected void Awake() {
        workerCurrencies = GetComponent<WorkerCurrencies>();
        workerMovement = GetComponent<WorkerMovement>();
        workerCurrencies.OnPaymentFinalized += WorkerCurrencies_OnPaymentFinalized;
        workerCurrencies.OnCurrencyPaid += WorkerCurrencies_OnCurrencyPaid;

    }

    protected void Start() {
        worker.OnWorkerDroppedCurrency += Worker_OnWorkerDroppedCurrency;
        worker.OnWorkerCollectedCurrency += Worker_OnWorkerCollectedCurrency;
        workerMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            isNightOrDusk = true;
        }
    }

    private void Update() {
        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(EngineerState.droppingOrbs);
        }

        if (followingPlayer) {
            switch (state) {

                case EngineerState.followPlayerIdle:

                    FollowPlayer();

                    break;
            }
        }
        else {

            if (CheckBlockedByCreature() && state != EngineerState.blockedByCreatures) {
                ChangeState(EngineerState.blockedByCreatures);
                return;
            };

            switch (state) {

                case EngineerState.idle:
                    RoamInCampCenter();
                    CheckAvailableWork();
                    break;

                case EngineerState.headToSafety:
                    if (isInSafeZone) {
                        RoamInCampCenter();
                    }
                    else {
                        HeadToCampCenter();
                    }
                    break;

                case EngineerState.refillingStructure:

                    assignedStructure.SetWorkerRefillingStructure(workerCurrencies, true);


                break;

                case EngineerState.headingToStructure:

                    HeadToStructure();

                break;

                case EngineerState.headingToRefill:

                    HeadToRefill();

                break;

                case EngineerState.workingInStructure:

                    WorkInStructure();
                    if (CheckOrbsToCollect()) {
                        ChangeState(EngineerState.pickingUpOrbs);
                    }

                    break;

                case EngineerState.pickingUpOrbs:

                    HeadToPickUpClosestOrb();

                    break;

                case EngineerState.droppingOrbs:

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

                case EngineerState.blockedByCreatures:
                    StayAwayFromCreature(closestCreature);

                    if (!CheckBlockedByCreature()) {
                        ChangeState(EngineerState.idle);
                    }
                    break;
            }
        }

    }

    private void ChangeState(EngineerState newState) {
        if (newState == state) return;

        Debug.Log("ChangeState " + newState);

        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnEngineerChangedState?.Invoke(this, EventArgs.Empty);

        if (state == EngineerState.idle) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            if (assignedStructure != null) {
                UnassignStructure();
            }
        }

        if (state == EngineerState.headingToStructure || state == EngineerState.headingToRefill || state == EngineerState.refillingStructure) {
            mobMovement.SetMoveSpeed(headingToStructureMoveSpeed);
        }


        if (state == EngineerState.headToSafety) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
        }
    }

    public EngineerState GetState() {
        return state;
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        isNightOrDusk = true;
        ChangeState(EngineerState.idle);
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        isNightOrDusk = false;
        ChangeState(EngineerState.idle);
    }

    private void CheckAvailableWork() {
        Structure closestAvailableStructure = null;

        if (!isNightOrDusk) {
            // Day Job
            closestAvailableStructure = PlayerCamp.Instance.GetClosestAvailableEngineerDayStructureToWork(transform.position);

        } else {
            // Night Job
            closestAvailableStructure = PlayerCamp.Instance.GetClosestAvailableEngineerNightStructureToWork(transform.position);
        }


        if (closestAvailableStructure != null) {
            AssignStructure(closestAvailableStructure);

            if (closestAvailableStructure.NeedsEngineerRefill()) {

                FindTargetCurrencyStorage();

            } else {

                ChangeState(EngineerState.headingToStructure);

            }
        }
    }

    private void FindTargetCurrencyStorage() {
        targetCurrencyStorage = PlayerCamp.Instance.GetCurrencyStorageWithCurrencies(assignedStructure.GetRefillCurrencyTypeNeeded(), assignedStructure.GetMinimumRefillAmountRequired());

        if (targetCurrencyStorage == null) {
            ChangeState(EngineerState.idle);
        } else {
            ChangeState(EngineerState.headingToRefill);
        }

    }

    private void HeadToStructure() {
        float distanceToStructure = Mathf.Abs(transform.position.x - assignedStructure.transform.position.x);

        if (distanceToStructure < .5f) {

            if(assignedStructure.NeedsEngineerRefill()) {

                ChangeState(EngineerState.refillingStructure);

            } else {

                assignedStructure.SetEngineerWorking(this, true);
                ChangeState(EngineerState.workingInStructure);

            }

        }
        else {

            mobMovement.SetMoveTarget(assignedStructure.transform.position);

        }
    }
    private void HeadToRefill() {
        float distanceToStorage = Mathf.Abs(transform.position.x - targetCurrencyStorage.transform.position.x);

        if (distanceToStorage < .5f) {
            TryPickupCurrenciesFromStorage();
        }
        else {
            mobMovement.SetMoveTarget(targetCurrencyStorage.transform.position);
        }
    }

    private void TryPickupCurrenciesFromStorage() {
        if(targetCurrencyStorage.GetCurrencyAmountStored() < assignedStructure.GetMinimumRefillAmountRequired()) {
            ChangeState(EngineerState.idle);
            return;
        }

        for (int i = 0; i < assignedStructure.GetMinimumRefillAmountRequired(); i++) {
            targetCurrencyStorage.RemoveCurrency();
            worker.CollectCurrency(assignedStructure.GetRefillCurrencyTypeNeeded());
        }
    }

    private void WorkerCurrencies_OnPaymentFinalized(object sender, EventArgs e) {
        ChangeState(EngineerState.idle);
    }

    private void WorkerCurrencies_OnCurrencyPaid(object sender, WorkerCurrencies.OnCurrencyPaidEventArgs e) {
        worker.RemoveCurrency(e.currencyType);
    }

    private void Worker_OnWorkerCollectedCurrency(object sender, EventArgs e) {
        if (assignedStructure == null) return;
        if (!assignedStructure.NeedsEngineerRefill()) return;

        if(worker.GetCurrencyAmount(assignedStructure.GetRefillCurrencyTypeNeeded()) >= assignedStructure.GetMinimumRefillAmountRequired()) {
            ChangeState(EngineerState.headingToStructure);
        }
    }

    private void Worker_OnWorkerDroppedCurrency(object sender, EventArgs e) {
        if (assignedStructure == null) return;
        if (!assignedStructure.NeedsEngineerRefill()) return;

        if (worker.GetCurrencyAmount(assignedStructure.GetRefillCurrencyTypeNeeded()) < assignedStructure.GetMinimumRefillAmountRequired()) {
            ChangeState(EngineerState.idle);
        }
    }

    private void WorkInStructure() {
        if(assignedStructure is CurrencyCrafter) {
            WorkInCurrencyCrafter();
        }
    }

    private void WorkInCurrencyCrafter() {
        if (!reachedAssignedStructureWorkingPoint) return;
        
        turnWrenchTimer -= Time.deltaTime;
        if(turnWrenchTimer <= 0) {
            OnEngineerTurnsWrench?.Invoke(this, EventArgs.Empty);
            turnWrenchTimer = turnWrenchDelay;
            SetRandomDestinationCloseToAssignedStructure();
        }
    }

    private void WorkerMovement_OnDestinationReached(object sender, EventArgs e) {
        reachedAssignedStructureWorkingPoint = true;
    }

    private void SetRandomDestinationCloseToAssignedStructure() {
        reachedAssignedStructureWorkingPoint = false;
        float randomX = UnityEngine.Random.Range(assignedStructure.transform.position.x - 1f, assignedStructure.transform.position.x + 1f);
        assignedStructureRandomDestinationPoint = new Vector3(randomX, 0, 0);
        mobMovement.SetMoveTarget(assignedStructureRandomDestinationPoint);
    }

    public void HeadToPickUpClosestOrb() {

        if (orbsToCollect.Count == 0) {
            ChangeState(EngineerState.idle);
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);
    }

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if (Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    public void AssignStructure(Structure structure) {
        assignedStructure = structure;
        structure.AssignEngineer(this);

        if(structure.GetStructureSO().workingEngineerHideTool) {
            OnEngineerHideTool?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UnassignStructure() {
        assignedStructure.RemoveAssignedEngineer(this);
        assignedStructure = null;
    }

    public void OrbExtractorTriggerDrill() {
        OnOrbExtractorTriggeredDrill?.Invoke(this, EventArgs.Empty);
    }
    protected override bool CheckDropCurrenciesToPlayer() {
        if(targetCurrencyStorage != null && (state == EngineerState.refillingStructure || state == EngineerState.headingToRefill || state == EngineerState.headingToStructure)) {
            // worker is close to storage
            if (Mathf.Abs(transform.position.x - targetCurrencyStorage.transform.position.x) < 2f) {

                return false;
            }
        }

        if (worker.GetPlayerIsClose() && worker.GetTotalCurrencyAmount() > 0) {
            return true;
        }
        return false;
    }

}
