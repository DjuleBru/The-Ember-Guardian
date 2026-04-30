using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [SerializeField] protected StructureSO structureSO;
    [SerializeField] protected bool upgradeUnlocked;
    [SerializeField] protected bool primaryFunctionUnlocked;
    [SerializeField] protected bool secondaryFunctionUnlocked;
    [SerializeField] protected GameObject visualIndicator;
    protected bool upgradable;
    protected bool setBuiltOnLoad;

    private CampZoneManager.CampSide campSide;
    protected PayCurrencyUI payCurrencyUI;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnWorkerStartedRefilling;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public static event EventHandler OnAnyPlayerTriggeredOut;
    public event EventHandler<OnStructureUpgradedEventArgs> OnStructureUpgraded;
    public static event EventHandler OnAnyStructureUpgraded;
    public event EventHandler OnStructureFunctionUsed;
    public static event EventHandler OnAnyStructurePrimaryFunctionUsed;
    public event EventHandler OnStructureInteractionsUpdated;
    public event EventHandler OnInitialCampStructureBuilt;
    public event EventHandler OnForceUpdateInteractionTypeUI;

    public class OnStructureUpgradedEventArgs:EventArgs {
        public bool upgradedOnLoad;
    }

    protected bool playerInTriggerArea;
    protected bool playerCanInteract;
    protected bool playerInteracting;
    protected bool isWorldStructure;
    protected int structureLevel = 1;
    private float worldScaleX;

    protected int maxEngineersAssignedWorking;
    protected int engineersGarrisoned;
    protected int maxEngineersAssignedRefilling;
    protected bool needsRefill;
    protected bool needsWorking;
    protected bool isBeingRefilledByEngineer;
    protected List<EngineerJob> engineersAssignedRefilling = new List<EngineerJob>();
    protected List<EngineerJob> engineersAssignedWorking = new List<EngineerJob>();
    private float playerInteractingTimer;

    public enum StructureInteractionType {
        primaryFunction,
        secondaryFunction,
        upgrade,
    }

    protected List<StructureInteractionType> activeStructureInteractionsTypeList = new List<StructureInteractionType>();
    protected StructureInteractionType currentStructureInteractionType;

    protected virtual void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        DebugInitializeActiveStructureUITypeList();
    }

    protected virtual void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;

        if(SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
            campSide = CampZoneManager.Instance.AssignCampSide(transform.position);
            PlayerCamp.Instance.AddStructure(this);
            Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;
            upgradable = structureSO.upgradeable;
        }

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.OnCurrencyPaymentFailedOrCanceled += PayCurrencyUI_OnCurrencyPaymentFailedOrCanceled;

        RefreshStructureUpgradeInteraction();

        // Check if its night
        if (!structureSO.upgradeableAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            ActivateStructureUpgradeInteraction(false);
        }

        maxEngineersAssignedRefilling = structureSO.maxEngineersAssignedRefilling;
        maxEngineersAssignedWorking = structureSO.maxEngineersAssignedWorking;

        //Debug.Log(this + " structureLevel " + structureLevel + " maxEngineersAssignedWorking " + maxEngineersAssignedWorking);
    }

    protected virtual void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        //Debug.Log("PayOrbsUI_OnOrbPaymentSuccess " + currentStructureInteractionType);
        if (currentStructureInteractionType == StructureInteractionType.primaryFunction) {
            TriggerStructurePrimaryFunction();
        }

        if (currentStructureInteractionType == StructureInteractionType.secondaryFunction) {
            TriggerStructureSecondaryFunction();
        }

        if (currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
        }

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.StopWorkerInteraction();
        isBeingRefilledByEngineer = false;
    }

    protected virtual void TriggerStructurePrimaryFunction() {
        OnAnyStructurePrimaryFunctionUsed?.Invoke(this, EventArgs.Empty);
        OnStructureFunctionUsed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void TriggerStructureSecondaryFunction() {
        OnAnyStructurePrimaryFunctionUsed?.Invoke(this, EventArgs.Empty);
        OnStructureFunctionUsed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void UpgradeStructure() {
        structureLevel++;
        RefreshStructureUpgradeInteraction();

        OnStructureUpgraded?.Invoke(this, new OnStructureUpgradedEventArgs {
            upgradedOnLoad = false
        });
        OnAnyStructureUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public virtual void SetStructureLevel(int structureLevel) {
        if (structureLevel == 1) return;

        this.structureLevel = structureLevel;
        RefreshStructureUpgradeInteraction();
        RefreshMaxEngineersAssignedAndWorking();

        OnStructureUpgraded?.Invoke(this, new OnStructureUpgradedEventArgs {
            upgradedOnLoad = true
        });
    }

    protected virtual void RefreshMaxEngineersAssignedAndWorking() {

    }

    public bool GetUpgradableUnlocked() {
        return upgradeUnlocked;
    }

    public bool GetPrimaryFunctionUnlocked() {
        return primaryFunctionUnlocked;
    }

    public int GetStructureLevel() {
        return structureLevel;
    }

    public CampZoneManager.CampSide GetCampSide() {
        return campSide;
    }

    public StructureSO GetStructureSO() {
        return structureSO;
    }

    public void BuildInitialCampStructure() {
        StartCoroutine(BuildInitialCampStructureCoroutine());
    }

    private IEnumerator BuildInitialCampStructureCoroutine() {
        yield return new WaitForEndOfFrame();
        OnInitialCampStructureBuilt?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (!structureSO.functionUsableAtNight && primaryFunctionUnlocked) {
            ActivateStructurePrimaryFunctionInteraction(true);
        }

        if(!structureSO.upgradeableAtNight && upgradeUnlocked) {
            ActivateStructureUpgradeInteraction(true);
        }
    }

    protected virtual void DayNightManager_OnNightStart(object sender, EventArgs e) {

        if (!structureSO.functionUsableAtNight) {

            ActivateStructurePrimaryFunctionInteraction(false);
            if (playerInTriggerArea) {
                OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
                OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
                playerInTriggerArea = false;
                payCurrencyUI.SetPlayerInteracting(false);

                Player.Instance.SetInPayCurrencyArea(false);
            }

        }

        if(!structureSO.upgradeableAtNight) {
            ActivateStructureUpgradeInteraction(false);
        }
    }

    protected void Tent_OnStructureUpgraded(object sender, EventArgs e) {
        RefreshStructureUpgradeInteraction();
    }


    protected virtual void RefreshStructureUpgradeInteraction() {
        if (!upgradable) return;
        bool upgradeUnlocked = false;

        // Check if upgrade has been unlocked at gem merchant
        string saveString = structureSO.structureType.ToString() + (structureLevel+1);

        if (!MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            //Debug.Log(saveString + " has NOT been bought at merchant ");
            upgradeUnlocked = false;
        } else {
            upgradeUnlocked = true;
        }

       
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level && LevelManager.Instance.IsHordeMode()) {
            upgradeUnlocked = GetNextStructureUpgradeInteractionUnlocked_HordeMode();
        }

        if (DebugManager.Instance.GetAllStructureUpgradesUnlocked() && structureLevel < structureSO.maxLevel) {
            upgradeUnlocked = true;
        }

        if(structureLevel == structureSO.maxLevel) {
            upgradeUnlocked = false;
        }
        

        SetStructureUpgradableUnlocked(upgradeUnlocked);
        OnStructureInteractionsUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected bool GetNextStructureUpgradeInteractionUnlocked_HordeMode() {
        bool unlocked = false;

        if(structureSO.structureType == StructureSO.StructureType.tent) {
            if(structureLevel == 1) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Tent2);
            }
            if (structureLevel == 2) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Tent3);
            }
        }

        if (structureSO.structureType == StructureSO.StructureType.barricade || structureSO.structureType == StructureSO.StructureType.tower) {
            if (structureLevel == 1) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Barricade2_Tower2);
            }
            if (structureLevel == 2) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Barricade3_Tower3);
            }
            if (structureLevel == 3) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Tower4_Barricade4);
            }
        }

        if (structureSO.structureType == StructureSO.StructureType.sniperTower) {
            if (structureLevel == 1) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.SniperTower2);
            }
        }
        if (structureSO.structureType == StructureSO.StructureType.mortarTower) {
            if (structureLevel == 1) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.MortarTower);
            }
        }
        if (structureSO.structureType == StructureSO.StructureType.machineGunTower) {
            if (structureLevel == 1) {
                unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.MGTower2);
            }
        }

        return unlocked;
    }

    public virtual void SetStructureBuiltOnLoad(bool setBuiltOnLoad = false) {
        this.setBuiltOnLoad = setBuiltOnLoad;
    }

    public bool GetStructureBuiltOnLoad() {
        return setBuiltOnLoad;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (Player.Instance.GetDead()) return;
        if (Player.Instance.GetInPortalTriggerArea()) return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;

        //Debug.Log("playerInTriggerArea " + playerInTriggerArea);
        if (playerCanInteract) {
            Player.Instance.SetInPayCurrencyArea(true);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        if (!payCurrencyUI.GetPlayerInteracting()) {
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        };

        playerInTriggerArea = false;

        Player.Instance.SetInPayCurrencyArea(false);
    }

    protected void PayCurrencyUI_OnCurrencyPaymentFailedOrCanceled(object sender, EventArgs e) {
        if (playerInTriggerArea) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;

        Player.Instance.SetInPayCurrencyArea(false);
    }


    public void AssignEngineerRefill(EngineerJob engineerJob) {
        if (engineersAssignedRefilling.Contains(engineerJob)) return;

        engineersAssignedRefilling.Add(engineerJob);
    }
    public void AssignEngineerWorking(EngineerJob engineerJob) {
        // Only assignment : not actually working
        if (engineersAssignedWorking.Contains(engineerJob)) return;

        engineersAssignedWorking.Add(engineerJob);
    }

    public void RemoveAssignedEngineer(EngineerJob engineerJob) {

        if (engineersAssignedRefilling.Contains(engineerJob)) {
            engineersAssignedRefilling.Remove(engineerJob);
        }

        if (engineersAssignedWorking.Contains(engineerJob)) {
            engineersAssignedWorking.Remove(engineerJob);
            SetEngineerWorking(engineerJob, false);
        }

    }

    public List<EngineerJob> GetEngineersWorking() {
        return engineersAssignedWorking;
    }

    public int GetEngineersGarrisoned() {
        return engineersGarrisoned;
    }
    public int GetMaxEngineersWorking() {
        return maxEngineersAssignedWorking;
    }

    public virtual void SetEngineerWorking(EngineerJob engineer, bool working) {
        // Actually working

        if(working) {

            if (!engineersAssignedWorking.Contains(engineer)) {
                engineersAssignedWorking.Add(engineer);
                engineersGarrisoned++;
            };

        } else {

            if (engineersAssignedWorking.Contains(engineer)) {
                engineersAssignedWorking.Remove(engineer);
                engineersGarrisoned--;
            };

        }

        if(engineersGarrisoned == maxEngineersAssignedWorking) {

            needsWorking = false;

        } else {

            bool isNightOrDusk = DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk;

            if (structureSO.engineerCanWorkByDay && (!isNightOrDusk) && structureSO.structureCategory != StructureSO.StructureCategory.tower) {
                needsWorking = true;
                return;
            }

            if (structureSO.engineerCanWorkByNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                needsWorking = true;
            }

        }
    }

    public bool NeedsEngineering() {
        bool needsRefillingEngineers = engineersAssignedRefilling.Count < maxEngineersAssignedRefilling;
        bool needsWorkingEngineers = engineersAssignedWorking.Count < maxEngineersAssignedWorking;

        return (needsRefillingEngineers && needsRefill) || (needsWorkingEngineers && needsWorking);
    }

    public bool NeedsRefillingEngineers() {
        bool needsRefillingEngineers = engineersAssignedRefilling.Count < maxEngineersAssignedRefilling;
        return needsRefillingEngineers;
    }

    public bool NeedsWorkingEngineers() {
        bool needsWorkingEngineers = engineersAssignedWorking.Count < maxEngineersAssignedWorking;
        return needsWorkingEngineers;
    }

    public bool NeedsRefill() {
        return needsRefill;
    }

    public bool NeedsWorking() {
        return needsWorking;
    }

    public virtual PlayerCurrencies.CurrencyType GetRefillCurrencyTypeNeeded() {
        return structureSO.refillCurrencyTypeNeeded;
    }

    public virtual int GetMinimumRefillAmountRequired() {
        return payCurrencyUI.GetCurrencyAmountToPay();
    }

    #region InteractionTypes

    protected virtual void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
    }

    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerCanInteract) return;
        if (!playerInteracting) return;

        playerInteracting = false;
        payCurrencyUI.SetPlayerInteracting(false);
    }

    protected virtual void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;
        payCurrencyUI.SetPlayerInteracting(true);
    }

    public virtual void SetStructureUpgradableUnlocked(bool upgradable) {
        upgradeUnlocked = upgradable;
        ActivateStructureUpgradeInteraction(upgradable);
    }

    public void SetStructurePrimaryFunctionUnlocked(bool unlocked) {
        if (primaryFunctionUnlocked == unlocked) return;

        primaryFunctionUnlocked = unlocked;
        ActivateStructurePrimaryFunctionInteraction(unlocked);
    }

    public void SetStructureSecondaryFunctionUnlocked(bool unlocked) {
        if (secondaryFunctionUnlocked == unlocked) return;
        secondaryFunctionUnlocked = unlocked;
        ActivateStructureSecondaryFunctionInteraction(unlocked);
    }

    protected void DebugInitializeActiveStructureUITypeList() {

        if (primaryFunctionUnlocked) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.primaryFunction)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.primaryFunction);
            }
        }

        if (secondaryFunctionUnlocked) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.secondaryFunction)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.secondaryFunction);
            }
        }

        if (upgradeUnlocked) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.upgrade)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.upgrade);
            }
        }
        RefreshPlayerCanInteract();
    }

    protected virtual void ActivateStructurePrimaryFunctionInteraction(bool active) {
        //Debug.Log(this + " ActivateStructurePrimaryFunctionInteraction " + active);
        if (active) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.primaryFunction)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.primaryFunction);
            }

        }
        else {
            if (activeStructureInteractionsTypeList.Contains(StructureInteractionType.primaryFunction)) {
                activeStructureInteractionsTypeList.Remove(StructureInteractionType.primaryFunction);
            }
        }

        OnStructureInteractionsUpdated?.Invoke(this, EventArgs.Empty);
        RefreshPlayerCanInteract();
    }

    protected virtual void ActivateStructureSecondaryFunctionInteraction(bool active) {
        //Debug.Log(this + " ActivateStructureSecondaryFunctionInteraction " + active);
        if (active) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.secondaryFunction)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.secondaryFunction);
            }

        }
        else {
            if (activeStructureInteractionsTypeList.Contains(StructureInteractionType.secondaryFunction)) {
                activeStructureInteractionsTypeList.Remove(StructureInteractionType.secondaryFunction);
            }
        }

        OnStructureInteractionsUpdated?.Invoke(this, EventArgs.Empty);
        RefreshPlayerCanInteract();
    }

    protected void ActivateStructureUpgradeInteraction(bool active) {

        if (active) {

            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.upgrade)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.upgrade);
            }

        }
        else {

            if (activeStructureInteractionsTypeList.Contains(StructureInteractionType.upgrade)) {
                activeStructureInteractionsTypeList.Remove(StructureInteractionType.upgrade);
            }
        }

        OnStructureInteractionsUpdated?.Invoke(this, EventArgs.Empty);
        RefreshPlayerCanInteract();
    }

    protected void ActivateStructurePrimaryFunctionInteractionAfterFrame(bool active) {
        StartCoroutine(ActivateStructurePrimaryFunctionInteractionAfterFrameCoroutine(active));
    }

    protected IEnumerator ActivateStructurePrimaryFunctionInteractionAfterFrameCoroutine(bool active) {
        yield return new WaitForEndOfFrame();
        ActivateStructurePrimaryFunctionInteraction(active);
    }

    protected virtual void RefreshPlayerCanInteract() {
        if (activeStructureInteractionsTypeList.Count == 0) {
            playerCanInteract = false;
        } else {
            playerCanInteract = true;
        }
    }

    public List<StructureInteractionType> GetActiveStructureInteractionTypeList() {
        return activeStructureInteractionsTypeList;
    }

    public StructureInteractionType GetCurrentStructureInteractionType() {
        return currentStructureInteractionType;
    }

    public void SetCurrentStructureInteractionType(StructureInteractionType interactionType, bool forceUpdateUI= false) {
        //Debug.Log(this + " SetCurrentStructureInteractionType " + interactionType);
        currentStructureInteractionType = interactionType;

        if(forceUpdateUI) {
            OnForceUpdateInteractionTypeUI?.Invoke(this, EventArgs.Empty);
        }
    }

    public GameObject GetVisualIndicator() {
        return visualIndicator;
    }

    public bool GetPlayerCanInteract() {
        return playerCanInteract;
    }
    protected bool GetHasCurrenciesToPay() {
        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(GetRefillCurrencyTypeNeeded()).Count > 0) {
            return true; // Continue à payer
        }
        return false;
    }

    public void SetWorkerRefillingStructure(WorkerCurrencies workerCurrencies, bool refilling) {
        if (isBeingRefilledByEngineer) return;

        isBeingRefilledByEngineer = true;
        payCurrencyUI.SetWorkerInteracting(workerCurrencies, refilling);
        OnWorkerStartedRefilling?.Invoke(this, EventArgs.Empty);
    }

    public PayCurrencyUI GetPayCurrencyUI() {
        return payCurrencyUI;
    }
    public void SetAsWorldStructure(float scaleX) {
        //Debug.Log(this + " SetAsWorldStructure");
        isWorldStructure = true;
        worldScaleX = scaleX;
    }

    public bool GetIsWorldStructure() {
        return isWorldStructure;
    }

    public float GetWorldScaleX() {
        return worldScaleX;
    }

    public bool GetPlayerInTriggerArea() {
        return playerInTriggerArea;
    }


    #endregion

    protected virtual void OnDestroy() {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB && SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.MainMenu) {
            DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
            Tent.Instance.OnStructureUpgraded -= Tent_OnStructureUpgraded;
        }
    }
}
