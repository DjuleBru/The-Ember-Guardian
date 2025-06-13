using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineerJob : WorkerJob {

    public enum EngineerState {
        idle,
        droppingCurrency,
        pickingUpCurrency,
        followPlayerIdle,
        blockedByCreatures,
        headToSafety,

        headingToDrop,
        headingToStructureToRefill,
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
    private CurrencyCrafter currencyCrafterAssigned;

    private EngineerState previousState;
    private EngineerState state;

    private float turnWrenchTimer;
    private float turnWrenchDelay = 3f;
    private float turnWrenchBoost = 2f;

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
            ChangeState(EngineerState.droppingCurrency);
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

                case EngineerState.headingToStructureToRefill:

                    HeadToStructure();

                    break;

                case EngineerState.headingToRefill:

                    HeadToRefill();

                break;

                case EngineerState.headingToDrop:

                    HeadToDrop();

                break;

                case EngineerState.workingInStructure:

                    WorkInStructure();
                    if (CheckOrbsToCollect()) {
                        ChangeState(EngineerState.pickingUpCurrency);
                    }

                    break;

                case EngineerState.pickingUpCurrency:

                    HeadToPickUpClosestCurrency();

                    break;

                case EngineerState.droppingCurrency:

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

        if (state == EngineerState.headingToStructure || state == EngineerState.headingToRefill || state == EngineerState.refillingStructure || state == EngineerState.headingToDrop) {
            mobMovement.SetMoveSpeed(headingToStructureMoveSpeed);
        }


        if (state == EngineerState.headToSafety) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
        }
    }

    private void CheckAvailableWork() {

        // Check held currencies
        if(CheckDropCurrenciesInContainers()) return;
        
        // Check structures
        Structure closestAvailableStructure = null;
        closestAvailableStructure = PlayerCamp.Instance.GetHighestPriorityAvailableEngineerStructureToWork(isNightOrDusk);

        if (closestAvailableStructure != null) {
            AssignStructure(closestAvailableStructure);

            if (closestAvailableStructure.NeedsEngineerRefill()) {

                FindTargetCurrencyStorage();

            } else {

                ChangeState(EngineerState.headingToStructure);

            }
        }
    }

    public bool CheckDropCurrenciesInContainers() {
        bool workerIsCarryingAmmo = worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.ammo) > 0;
        bool workerIsCarryingSpecialAmmo = worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.ammo_special) > 0;
        bool workerIsCarryingOrbs = worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.bigBlueOrb) > 0;
        bool workerIsCarryingSmallOrbs = worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.smallBlueOrb) > 0;

        List<PlayerCurrencies.CurrencyType> ammoList = new List<PlayerCurrencies.CurrencyType>();
        if (workerIsCarryingAmmo) {
            ammoList.Add(PlayerCurrencies.CurrencyType.ammo);
        }
        if (workerIsCarryingSpecialAmmo) {
            ammoList.Add(PlayerCurrencies.CurrencyType.ammo_special);
        }
        Structure structureNeedingAmmo = PlayerCamp.Instance.GetClosestDefensiveStructureNeedingCurrency(transform.position, ammoList);

        if (structureNeedingAmmo != null) {
            // Ammo sink found

        }
        else {
            // Ammo sink not found : drop in storage

            if (workerIsCarryingAmmo) {
                targetCurrencyStorage = PlayerCamp.Instance.GetClosestCurrencyStorageWithSpace(transform.position, PlayerCurrencies.CurrencyType.ammo);
            }

            if (workerIsCarryingSpecialAmmo) {
                targetCurrencyStorage = PlayerCamp.Instance.GetClosestCurrencyStorageWithSpace(transform.position, PlayerCurrencies.CurrencyType.ammo_special);
            }

            if (workerIsCarryingOrbs) {
                targetCurrencyStorage = PlayerCamp.Instance.GetClosestCurrencyStorageWithSpace(transform.position, PlayerCurrencies.CurrencyType.bigBlueOrb);
            }

            if (workerIsCarryingSmallOrbs) {
                targetCurrencyStorage = PlayerCamp.Instance.GetClosestCurrencyStorageWithSpace(transform.position, PlayerCurrencies.CurrencyType.smallBlueOrb);
            }

            if (targetCurrencyStorage != null) {
                ChangeState(EngineerState.headingToDrop);
                return true;
            }
        }
        

        return false;
    }

    public void AssignStructure(Structure structure) {
        assignedStructure = structure;
        structure.AssignEngineer(this);

        if (structure.GetStructureSO().workingEngineerHideTool) {
            OnEngineerHideTool?.Invoke(this, EventArgs.Empty);
        }

        if (structure is CurrencyCrafter) {
            CurrencyCrafter currencyCrafter = (CurrencyCrafter)structure;
            currencyCrafter.OnCurrencyInstantiated += CurrencyCrafter_OnCurrencyInstantiated;
        }
    }

    public void UnassignStructure() {

        if (assignedStructure is CurrencyCrafter) {
            CurrencyCrafter currencyCrafter = (CurrencyCrafter)assignedStructure;
            currencyCrafter.OnCurrencyInstantiated -= CurrencyCrafter_OnCurrencyInstantiated;
        }

        assignedStructure.RemoveAssignedEngineer(this);
        assignedStructure = null;
    }

    #region MOVEMENT
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

    private void HeadToDrop() {
        float distanceToStorage = Mathf.Abs(transform.position.x - targetCurrencyStorage.transform.position.x);

        if (distanceToStorage < .5f) {
            TryDropCurrencyInStorage();
            targetCurrencyStorage = null;
            ChangeState(EngineerState.idle);
        }
        else {
            mobMovement.SetMoveTarget(targetCurrencyStorage.transform.position);
        }
    }

    public void HeadToPickUpClosestCurrency() {
        if (orbsToCollect.Count == 0) {
            ChangeState(EngineerState.idle);
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);
    }
    #endregion

    #region STORAGE
    private void FindTargetCurrencyStorage() {
        targetCurrencyStorage = PlayerCamp.Instance.GetCurrencyStorageWithCurrencies(assignedStructure.GetRefillCurrencyTypeNeeded(), assignedStructure.GetMinimumRefillAmountRequired());

        if (targetCurrencyStorage == null) {
            ChangeState(EngineerState.idle);
        }
        else {
            ChangeState(EngineerState.headingToRefill);
        }

    }
    private void TryPickupCurrenciesFromStorage() {
        if (targetCurrencyStorage.GetCurrencyAmountStored() < assignedStructure.GetMinimumRefillAmountRequired()) {
            ChangeState(EngineerState.idle);
            return;
        }

        for (int i = 0; i < assignedStructure.GetMinimumRefillAmountRequired(); i++) {
            targetCurrencyStorage.RemoveCurrency();
            worker.CollectCurrency(assignedStructure.GetRefillCurrencyTypeNeeded());
        }
    }
    private void TryDropCurrencyInStorage() {
        PlayerCurrencies.CurrencyType currencyTypeToStore = targetCurrencyStorage.GetCurrencyTypeStored();

        for (int i = 0; i < worker.GetCurrencyAmount(currencyTypeToStore); i++) {

            if (targetCurrencyStorage.GetStorageFull()) {
                ChangeState(EngineerState.idle);
                return;
            }

            targetCurrencyStorage.StoreCurrency();
            worker.RemoveCurrency(currencyTypeToStore);
        }
    }

    #endregion

    #region WORK

    private void WorkInStructure() {
        if (assignedStructure is CurrencyCrafter) {
            currencyCrafterAssigned = (CurrencyCrafter)assignedStructure;
            WorkInCurrencyCrafter();
        }
    }

    private void WorkInCurrencyCrafter() {
        if (!reachedAssignedStructureWorkingPoint) return;

        turnWrenchTimer -= Time.deltaTime;
        if (turnWrenchTimer <= 0) {
            OnEngineerTurnsWrench?.Invoke(this, EventArgs.Empty);
            turnWrenchTimer = turnWrenchDelay;

            if (currencyCrafterAssigned.GetCraftedCurrency()) {
                currencyCrafterAssigned.WorkerCollectCurrencyFromCrafter();
            }
            else {
                currencyCrafterAssigned.AccelerateCrafting(turnWrenchBoost);
                StartCoroutine(SetRandomDestinationCloseToAssignedStructureAfterDelay(1f));
            }

        }

    }
    #endregion

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
            ChangeState(EngineerState.headingToStructureToRefill);
        }
    }

    private void Worker_OnWorkerDroppedCurrency(object sender, EventArgs e) {
        if (assignedStructure == null) return;
        if (!assignedStructure.NeedsEngineerRefill()) return;

        if (worker.GetCurrencyAmount(assignedStructure.GetRefillCurrencyTypeNeeded()) < assignedStructure.GetMinimumRefillAmountRequired()) {
            ChangeState(EngineerState.idle);
        }
    }

    private void WorkerMovement_OnDestinationReached(object sender, EventArgs e) {
        reachedAssignedStructureWorkingPoint = true;
    }

    private IEnumerator SetRandomDestinationCloseToAssignedStructureAfterDelay(float delay) {
        reachedAssignedStructureWorkingPoint = false;
        yield return new WaitForSeconds(delay);

        if (assignedStructure == null) yield break;

        float randomX = UnityEngine.Random.Range(assignedStructure.transform.position.x - 1f, assignedStructure.transform.position.x + 1f);
        assignedStructureRandomDestinationPoint = new Vector3(randomX, 0, 0);
        mobMovement.SetMoveTarget(assignedStructureRandomDestinationPoint);
    }

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if (Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    private void CurrencyCrafter_OnCurrencyInstantiated(object sender, CurrencyCrafter.OnCurrencyInstantiatedEventArgs e) {
        AssignCollectible(e.collectible);
        ChangeState(EngineerState.pickingUpCurrency);
    }

    public void OrbExtractorTriggerDrill() {
        OnOrbExtractorTriggeredDrill?.Invoke(this, EventArgs.Empty);
    }

    protected override bool CheckDropCurrenciesToPlayer() {

        if(targetCurrencyStorage != null && (state == EngineerState.refillingStructure || state == EngineerState.headingToRefill || state == EngineerState.headingToStructureToRefill || state == EngineerState.headingToDrop)) {
            // worker is transferring currencies
            return false;
        }


        if (worker.GetPlayerIsClose() && worker.GetTotalCurrencyAmount() > 0) {
            return true;
        }
        return false;
    }


    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        isNightOrDusk = true;
        ChangeState(EngineerState.idle);
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        isNightOrDusk = false;
        ChangeState(EngineerState.idle);
    }

    public EngineerState GetState() {
        return state;
    }
}
