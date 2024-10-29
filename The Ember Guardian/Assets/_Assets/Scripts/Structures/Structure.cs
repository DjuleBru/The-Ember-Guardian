using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {

    [SerializeField] protected StructureSO structureSO;
    [SerializeField] protected bool upgradeUnlocked;
    [SerializeField] protected bool functionUnlocked;

    protected PayOrbsUI payOrbsUI;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureUpgraded;

    public event EventHandler OnStructureFunctionLocked;
    public event EventHandler OnStructureFunctionUnlocked;

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
        InitializeActiveStructureUITypeList();
    }

    protected virtual void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;

        payOrbsUI.OnOrbPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
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

        Debug.Log(structureLevel);
        if (structureLevel == structureSO.maxLevel) {
            SetStructureUpgradableUnlocked(false);
        }

        OnStructureUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetStructureUpgradableUnlocked(bool upgradable) {
        upgradeUnlocked = upgradable;
        ActivateStructureUpgradeInteraction(upgradable);
    }

    public void SetStructureFunctionLocked() {
        functionUnlocked = false;
        ActivateStructureFunctionInteraction(false);
        OnStructureFunctionLocked?.Invoke(this, EventArgs.Empty);
    }

    public void SetStructureFunctionUnlocked() {
        functionUnlocked = true;
        ActivateStructureFunctionInteraction(true);
        OnStructureFunctionUnlocked?.Invoke(this, EventArgs.Empty);
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

    public StructureSO GetStructureSO() {
        return structureSO;
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

    protected void InitializeActiveStructureUITypeList() {

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
