using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [SerializeField] protected StructureSO structureSO;
    [SerializeField] protected bool upgradeUnlocked;
    [SerializeField] protected bool functionUnlocked;

    private CampZoneManager.CampSide campSide;
    protected PayOrbsUI payOrbsUI;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureUpgraded;

    public event EventHandler OnStructureInteractionsUpdated;

    public static event EventHandler OnAnyStructureBuilt;

    protected bool playerInTriggerArea;
    protected bool playerCanInteract;
    protected bool playerInteracting;
    protected int structureLevel = 1;

    public enum StructureInteractionType {
        function,
        upgrade,
    }

    protected List<StructureInteractionType> activeStructureInteractionsTypeList = new List<StructureInteractionType>();
    protected StructureInteractionType currentStructureInteractionType;

    protected virtual void Awake() {
        payOrbsUI = GetComponent<PayOrbsUI>();
        DebugInitializeActiveStructureUITypeList();
    }

    protected virtual void Start() {
        campSide = CampZoneManager.Instance.AssignCampSide(transform.position);

        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;

        payOrbsUI.OnOrbPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;

        OnAnyStructureBuilt?.Invoke(this, EventArgs.Empty);

        RefreshStructureUpgradeInteraction();
    }

    protected virtual void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        payOrbsUI.SetPlayerInteracting(false);
        
        if(currentStructureInteractionType == StructureInteractionType.function) {
            return;
        }

        if(currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
            return;
        }
    }

    protected virtual void UpgradeStructure() {
        structureLevel++;
        RefreshStructureUpgradeInteraction();
        
        OnStructureUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetStructureUpgradableUnlocked(bool upgradable) {
        Debug.Log("SetStructureUpgradableUnlocked " + upgradable);
        upgradeUnlocked = upgradable;
        ActivateStructureUpgradeInteraction(upgradable);
    }

    public void SetStructureFunctionUnlocked(bool unlocked) {
        Debug.Log("SetStructureFunctionUnlocked " + unlocked);
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
    }

    protected virtual void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (!structureSO.functionUsableAtNight) {
            ActivateStructureFunctionInteraction(false);
        }
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
    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;
        payOrbsUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;
        if (!playerInteracting) return;

        playerInteracting = false;
        payOrbsUI.CancelOrbPayment();
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;

        if(playerCanInteract) {
            Player.Instance.SetCanDropOrbOnTheFloor(false);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;
        payOrbsUI.SetPlayerInteracting(false);

        Player.Instance.SetCanDropOrbOnTheFloor(true);
    }

    #region InteractionTypes

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
    #endregion
}
