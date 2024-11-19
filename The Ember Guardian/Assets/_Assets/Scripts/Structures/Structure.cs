using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [SerializeField] protected StructureSO structureSO;
    [SerializeField] protected bool upgradeUnlocked;
    [SerializeField] protected bool functionUnlocked;

    private CampZoneManager.CampSide campSide;
    protected PayCurrencyUI payOrbsUI;

    public event EventHandler OnPlayerTriggeredIn;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public static event EventHandler OnAnyPlayerTriggeredOut;
    public event EventHandler OnStructureUpgraded;
    public static event EventHandler OnAnyStructureUpgraded;
    public static event EventHandler OnAnyStructureFunctionUsed;
    public event EventHandler OnStructureInteractionsUpdated;

    protected bool playerInTriggerArea;
    protected bool playerCanInteract;
    protected bool playerInteracting;
    protected int structureLevel = 1;

    private float playerInteractingTimer;

    public enum StructureInteractionType {
        function,
        upgrade,
    }

    protected List<StructureInteractionType> activeStructureInteractionsTypeList = new List<StructureInteractionType>();
    protected StructureInteractionType currentStructureInteractionType;

    protected virtual void Awake() {
        payOrbsUI = GetComponent<PayCurrencyUI>();
        DebugInitializeActiveStructureUITypeList();
    }

    protected virtual void Start() {
        campSide = CampZoneManager.Instance.AssignCampSide(transform.position);

        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;

        payOrbsUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;

        StructuresManager.Instance.AddStructure(this);

        RefreshStructureUpgradeInteraction();
    }


    protected virtual void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        payOrbsUI.SetPlayerInteracting(false);
        
        if(currentStructureInteractionType == StructureInteractionType.function) {
            TriggerStructureFunction();
            return;
        }

        if(currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
            return;
        }
    }

    protected virtual void TriggerStructureFunction() {
        Debug.Log("TriggerStructureFunction");
        OnAnyStructureFunctionUsed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void UpgradeStructure() {
        structureLevel++;
        RefreshStructureUpgradeInteraction();
        
        OnStructureUpgraded?.Invoke(this, EventArgs.Empty);
        OnAnyStructureUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetStructureUpgradableUnlocked(bool upgradable) {
        upgradeUnlocked = upgradable;
        ActivateStructureUpgradeInteraction(upgradable);
    }

    public void SetStructureFunctionUnlocked(bool unlocked) {
        functionUnlocked = unlocked;
        ActivateStructureFunctionInteraction(unlocked);
    }

    public bool GetUpgradableUnlocked() {
        return upgradeUnlocked;
    }

    public bool GetFunctionUnlocked() {
        return functionUnlocked;
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

    protected virtual void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (!structureSO.functionUsableAtNight && functionUnlocked) {
            ActivateStructureFunctionInteraction(true);
        }

        if(upgradeUnlocked) {
            ActivateStructureUpgradeInteraction(true);
        }
    }

    protected virtual void DayNightManager_OnNightStart(object sender, EventArgs e) {

        if (!structureSO.functionUsableAtNight) {
            ActivateStructureFunctionInteraction(false);
        }

        ActivateStructureUpgradeInteraction(false);
    }

    protected virtual void RefreshStructureUpgradeInteraction() {
        // Unlock upgrades if tent upgrade allows for new unlocks

        if (structureLevel == structureSO.maxLevel) {
            SetStructureUpgradableUnlocked(false);
            return;
        }

        if(structureLevel == 1) {
            if (Tent.Instance.GetStructureLevel() >= structureSO.level2UpgradeTentNecessaryLevel) {
                SetStructureUpgradableUnlocked(true);
            } else {
                SetStructureUpgradableUnlocked(false);
            }
        }
        
        if(structureLevel == 2) {
            if (Tent.Instance.GetStructureLevel() >= structureSO.level3UpgradeTentNecessaryLevel) {
                SetStructureUpgradableUnlocked(true);
            }
            else {
                SetStructureUpgradableUnlocked(false);
            }
        }
       
        if(structureLevel == 3) {
            if (Tent.Instance.GetStructureLevel() >= structureSO.level4UpgradeTentNecessaryLevel) {
                SetStructureUpgradableUnlocked(true);
            }
            else {
                SetStructureUpgradableUnlocked(false);
            }
        }

        if (structureLevel == 4) {
            Debug.LogWarning("structure level 4 and less than max level ?");
        }

    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;

        if(playerCanInteract) {
            //Player.Instance.SetCanDropOrbOnTheFloor(false);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;
        payOrbsUI.SetPlayerInteracting(false);

        Player.Instance.SetCanDropOrbOnTheFloor(true);
    }

    #region InteractionTypes

    protected virtual void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
    }

    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;
        if (!playerInteracting) return;

        playerInteracting = false;
        payOrbsUI.SetPlayerInteracting(false);
    }

    private void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;
        payOrbsUI.SetPlayerInteracting(true);
    }

    protected void DebugInitializeActiveStructureUITypeList() {

        if (functionUnlocked) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.function)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.function);
            }
        }

        if (upgradeUnlocked) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.upgrade)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.upgrade);
            }
        }
        RefreshPlayerCanInteract();
    }

    protected void ActivateStructureFunctionInteraction(bool active) {
        if (active) {
            if (!activeStructureInteractionsTypeList.Contains(StructureInteractionType.function)) {
                activeStructureInteractionsTypeList.Add(StructureInteractionType.function);
            }

        }
        else {
            if (activeStructureInteractionsTypeList.Contains(StructureInteractionType.function)) {
                activeStructureInteractionsTypeList.Remove(StructureInteractionType.function);
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

    #endregion
}
