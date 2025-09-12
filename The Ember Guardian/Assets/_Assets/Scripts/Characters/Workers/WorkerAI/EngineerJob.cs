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

    private float headingToStructureMoveSpeed = 3.5f;

    private bool isNightOrDusk;

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
    public event EventHandler OnEngineerHideVisual;
    public event EventHandler OnOrbExtractorTriggeredDrill;

    protected void Awake() {
        workerCurrencies = GetComponent<WorkerCurrencies>();
        workerMovement = GetComponent<WorkerMovement>();
        workerCurrencies.OnPaymentFinalized += WorkerCurrencies_OnPaymentFinalized;
        workerCurrencies.OnCurrencyPaid += WorkerCurrencies_OnCurrencyPaid;

        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
    }

    protected override void Start() {
        base.Start();

        worker.OnWorkerDroppedCurrency += Worker_OnWorkerDroppedCurrency;
        worker.OnWorkerCollectedCurrency += Worker_OnWorkerCollectedCurrency;
        workerMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            isNightOrDusk = true;
        }

        turnWrenchDelay = turnWrenchDelay - turnWrenchDelay*WorkerStats.Instance.GetEngineerWrenchSpeedBuff();
    }


    private void Update() {
        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(EngineerState.droppingCurrency);
        }

        if (escorting) {
            switch (state) {

                case EngineerState.followPlayerIdle:

                    FollowPlayer();

                    break;

                case EngineerState.droppingCurrency:
                    DroppingOrbsUpdate();
                 break;
            }
        }
        else {
            closestCreature = workerDetectionCollider.GetClosestCreature();
            isInSafeZone = IsInSafeZone();

            if (CheckBlockedByCreature() && state != EngineerState.blockedByCreatures && !isInSafeZone && state != EngineerState.workingInStructure) {
                ChangeState(EngineerState.blockedByCreatures);
                return;
            };

            switch (state) {

                case EngineerState.idle:

                    if (isInSafeZone) {
                        RoamInCampCenter();
                    }
                    else {
                        HeadToCampCenter();
                    }

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
                    DroppingOrbsUpdate();
                    break;

                case EngineerState.blockedByCreatures:
                    StayAwayFromCreature(closestCreature);

                    if (!CheckBlockedByCreature() || isInSafeZone) {
                        ChangeState(EngineerState.idle);
                    }
                    break;
            }
        }

    }

    private void ChangeState(EngineerState newState) {
        if (newState == state) return;
        if (worker.GetDead()) return;

        if (mobMovement == null) {
            Debug.LogWarning("mobMovement not initialized yet");
            mobMovement = GetComponentInChildren<MobMovement>();
            if(mobMovement == null) {
                return;
            }
        }

        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;
        hasSetCampDestination = false;

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
        Structure highestPriorityAvailableStructure = null;
        highestPriorityAvailableStructure = PlayerCamp.Instance.GetHighestPriorityAvailableEngineerStructure(isNightOrDusk);

        if (highestPriorityAvailableStructure != null) {
            if(highestPriorityAvailableStructure.NeedsWorkingEngineers() && highestPriorityAvailableStructure.NeedsWorking()) {

                AssignStructure(highestPriorityAvailableStructure, true, false);
                ChangeState(EngineerState.headingToStructure);
                return;
            }

            if (highestPriorityAvailableStructure.NeedsRefillingEngineers() && highestPriorityAvailableStructure.NeedsRefill()) {

                FindTargetCurrencyStorage(highestPriorityAvailableStructure);
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

            if(!workerIsCarryingAmmo && !workerIsCarryingSpecialAmmo && !workerIsCarryingOrbs && !workerIsCarryingSmallOrbs) {
                targetCurrencyStorage = null;
            }

            if (targetCurrencyStorage != null) {
                ChangeState(EngineerState.headingToDrop);
                return true;
            }
        }
        

        return false;
    }

    public void AssignStructure(Structure structure, bool working, bool refilling) {
        if (assignedStructure != null && assignedStructure != structure) {
            UnassignStructure();
        }

        assignedStructure = structure;
        Debug.Log("Assign structure " + structure + " working " + working + " refilling " + refilling);

        if (working) {
            assignedStructure.AssignEngineerWorking(this);
        }

        if (refilling) {
            assignedStructure.AssignEngineerRefill(this);
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

            if (assignedStructure.NeedsWorking() && assignedStructure.GetEngineersWorking().Contains(this)) {

                SetEngineerWorking(assignedStructure, true);
                ChangeState(EngineerState.workingInStructure);
                return;

            }

            if (assignedStructure.NeedsRefill() && IsCarryingRequiredCurrencyToRefillStructure()) {

                ChangeState(EngineerState.refillingStructure);
                return;

            }

            ChangeState(EngineerState.idle);
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
    private void FindTargetCurrencyStorage(Structure structure) {
        targetCurrencyStorage = PlayerCamp.Instance.GetCurrencyStorageWithCurrencies(structure.GetRefillCurrencyTypeNeeded(), structure.GetMinimumRefillAmountRequired());

        if (targetCurrencyStorage == null) {
            ChangeState(EngineerState.idle);
        }
        else {
            AssignStructure(structure, false, true);
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

            if (targetCurrencyStorage.GetEngineerCanPickUpOrbs()) {
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
                // Pick up currency only if there is a storage
                if(PlayerCamp.Instance.GetClosestCurrencyStorageWithSpace(transform.position, currencyCrafterAssigned.GetCurrencyTypeCrafted())) {
                    currencyCrafterAssigned.WorkerCollectCurrencyFromCrafter();
                } else {
                    ChangeState(EngineerState.idle);
                }
            }
            else {
                currencyCrafterAssigned.AccelerateCrafting(turnWrenchBoost);
                StartCoroutine(SetRandomDestinationCloseToAssignedStructureAfterDelay(1f));
            }

        }

    }

    public void SetEngineerWorking(Structure structure, bool working) {
        assignedStructure.SetEngineerWorking(this, working);

        if (working) {
            if (structure.GetStructureSO().workingEngineerHideTool) {
                OnEngineerHideTool?.Invoke(this, EventArgs.Empty);
            }
            if (structure.GetStructureSO().workingEngineerHideVisual) {
                OnEngineerHideVisual?.Invoke(this, EventArgs.Empty);
            }
        };
    }
    #endregion

    private void WorkerCurrencies_OnPaymentFinalized(object sender, EventArgs e) {
        Debug.Log("WorkerCurrencies_OnPaymentFinalized ");
        CheckAvailableWork();

        ChangeState(EngineerState.idle);
    }

    private void WorkerCurrencies_OnCurrencyPaid(object sender, WorkerCurrencies.OnCurrencyPaidEventArgs e) {
        Debug.Log("WorkerCurrencies_OnCurrencyPaid ");
        worker.RemoveCurrency(e.currencyType);
    }

    private void Worker_OnWorkerCollectedCurrency(object sender, EventArgs e) {
        if (assignedStructure == null) return;
        
        if (assignedStructure.NeedsRefill()) {
            if (worker.GetCurrencyAmount(assignedStructure.GetRefillCurrencyTypeNeeded()) >= assignedStructure.GetMinimumRefillAmountRequired()) {
                ChangeState(EngineerState.headingToStructureToRefill);
                return;
            }
        };

        // Check held currencies
        if (CheckDropCurrenciesInContainers()) return;

    }

    private void Worker_OnWorkerDroppedCurrency(object sender, EventArgs e) {
        if (assignedStructure == null) return;
        if (!assignedStructure.NeedsRefill()) return;

        //if(!IsCarryingRequiredCurrencyToRefillStructure()) {
        //    ChangeState(EngineerState.idle);
        //}
    }

    private bool IsCarryingRequiredCurrencyToRefillStructure() {
        if (worker.GetCurrencyAmount(assignedStructure.GetRefillCurrencyTypeNeeded()) >= assignedStructure.GetMinimumRefillAmountRequired()) return true;

        return false;
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

    public override void InitializeJob() {
        base.InitializeJob();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(EngineerState.idle);
        }
        else {
            ChangeState(EngineerState.headToSafety);
        }
    }

    protected override void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        base.WorkerAI_OnWorkerFollowPlayerChanged(sender, e);
        if (escorting) {
            ChangeState(EngineerState.followPlayerIdle);
        }
        else {
            ChangeState(EngineerState.idle);
        }
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

    public override void ReturnToPreviousState() {
        ChangeState(previousState);
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        isNightOrDusk = true;

        if (escorting) return;
        if (state == EngineerState.workingInStructure) {
            ChangeState(EngineerState.idle);
        }
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        isNightOrDusk = false;
        if (escorting) return;

        ChangeState(EngineerState.idle);
    }

    public EngineerState GetState() {
        return state;
    }

    public WorkerDetectionCollider GetDetectionCollider() {
        return workerDetectionCollider;
    }

}
