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

    private CampZoneManager.CampSide campSide;
    protected PayCurrencyUI payCurrencyUI;

    public event EventHandler OnPlayerTriggeredIn;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public static event EventHandler OnAnyPlayerTriggeredOut;
    public event EventHandler OnStructureUpgraded;
    public static event EventHandler OnAnyStructureUpgraded;
    public event EventHandler OnStructurePrimaryFunctionUsed;
    public static event EventHandler OnAnyStructurePrimaryFunctionUsed;
    public event EventHandler OnStructureInteractionsUpdated;
    public event EventHandler OnInitialCampStructureBuilt;

    protected bool playerInTriggerArea;
    protected bool playerCanInteract;
    protected bool playerInteracting;
    protected int structureLevel = 1;

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
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
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

        RefreshStructureUpgradeInteraction();
    }

    protected virtual void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        payCurrencyUI.SetPlayerInteracting(false);
        
        if(currentStructureInteractionType == StructureInteractionType.primaryFunction) {
            TriggerStructurePrimaryFunction();
            return;
        }

        if(currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
            return;
        }
    }

    protected virtual void TriggerStructurePrimaryFunction() {
        OnAnyStructurePrimaryFunctionUsed?.Invoke(this, EventArgs.Empty);
        OnStructurePrimaryFunctionUsed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void UpgradeStructure() {
        structureLevel++;
        RefreshStructureUpgradeInteraction();
        
        OnStructureUpgraded?.Invoke(this, EventArgs.Empty);
        OnAnyStructureUpgraded?.Invoke(this, EventArgs.Empty);
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
        OnInitialCampStructureBuilt?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (!structureSO.functionUsableAtNight && primaryFunctionUnlocked) {
            ActivateStructurePrimaryFunctionInteraction(true);
        }

        if(upgradeUnlocked) {
            ActivateStructureUpgradeInteraction(true);
        }
    }

    protected virtual void DayNightManager_OnNightStart(object sender, EventArgs e) {

        if (!structureSO.functionUsableAtNight) {
            ActivateStructurePrimaryFunctionInteraction(false);
        }

        ActivateStructureUpgradeInteraction(false);
    }

    private void Tent_OnStructureUpgraded(object sender, EventArgs e) {
        RefreshStructureUpgradeInteraction();
    }
    protected virtual void RefreshStructureUpgradeInteraction() {
        if (!upgradable) return;
        bool ungradeUnlocked = false;

        // Check if upgrade has been unlocked at gem merchant
        string saveString = structureSO.structureType.ToString() + (structureLevel+1);

        if (!MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            //Debug.Log(saveString + " has NOT been bought at merchant ");
            ungradeUnlocked = false;
        } else {
            ungradeUnlocked = true;
        }

        if (ungradeUnlocked)
        {
            // Check if tent is high level enough
            if (structureLevel == 1 && Tent.Instance.GetStructureLevel() >= structureSO.tentLevelRequiredForLevel2) {
                ungradeUnlocked = true;
            }
            if (structureLevel == 2 && Tent.Instance.GetStructureLevel() >= structureSO.tentLevelRequiredForLevel3) {
                ungradeUnlocked = true;
            }

            if (structureLevel == 3 && Tent.Instance.GetStructureLevel() >= structureSO.tentLevelRequiredForLevel4) {
                ungradeUnlocked = true;
            }
        }

        if(DebugManager.Instance.GetAllStructureUpgradesUnlocked()) {
            ungradeUnlocked = true;
        }
        
        SetStructureUpgradableUnlocked(ungradeUnlocked);
        OnStructureInteractionsUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        //return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;

        if(playerCanInteract) {
            Player.Instance.SetInPayCurrencyArea(true);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;
        payCurrencyUI.SetPlayerInteracting(false);

        Player.Instance.SetInPayCurrencyArea(false);
    }

    #region InteractionTypes

    protected virtual void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
    }

    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
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

    protected void ActivateStructurePrimaryFunctionInteraction(bool active) {
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

    protected void ActivateStructureSecondaryFunctionInteraction(bool active) {
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

    protected void RefreshPlayerCanInteract() {
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

    public void SetCurrentStructureInteractionType(StructureInteractionType interactionType) {
        currentStructureInteractionType = interactionType;
    }

    public void InvokeOnStructureUpgraded() {
        OnStructureUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public GameObject GetVisualIndicator() {
        return visualIndicator;
    }

    #endregion
}
