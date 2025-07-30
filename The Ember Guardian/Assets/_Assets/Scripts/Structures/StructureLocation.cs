using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation : MonoBehaviour {

    protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected StructureSO structureSOToBuild;
    [SerializeField] protected Transform orbTemplateWorldUIParent;
    [SerializeField] protected bool debugStructureTypeBought;
    [SerializeField] protected bool isAlwaysUnlocked;
    [SerializeField] protected float worldLocationScaleX;
    [SerializeField] protected ShowTooltipOnTrigger showTooltipOnTrigger;

    protected List<PayCurrencyTemplateWorldUI> buildStructureOrbTemplates = new List<PayCurrencyTemplateWorldUI>();

    public event EventHandler OnStructureLocationLoaded_Locked;
    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureLocationUnlocked;
    public event EventHandler OnStructureSOToBuildChanged;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public static event EventHandler OnAnyStructureSOToBuildChanged;
    public static event EventHandler<OnAnyStructureBuiltEventArgs> OnAnyStructureBuilt;

    public class OnAnyStructureBuiltEventArgs : EventArgs {
        public Structure structureBuilt;
    }

    protected bool structureLocationUnlocked;
    protected bool playerInTriggerArea;
    protected bool isBeingDestroyed;
    protected bool isWorldLocation;

    protected virtual void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        if(isAlwaysUnlocked) {
            structureLocationUnlocked = true;
            showTooltipOnTrigger.SetShowTooltips(true);
        } else {
            showTooltipOnTrigger.SetShowTooltips(false);
        }
    }

    protected virtual void Start() {
        LoadStructureLocationBought();
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.SetOrbTemplateUIList(buildStructureOrbTemplates);
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        Player.Instance.SetInPayCurrencyArea(false);
        playerInTriggerArea = false;
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
    }

    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        showTooltipOnTrigger.HideTooltipShown();
        BuildStructure();
    }

    public virtual Structure BuildStructure() {

        Structure structure = Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity).GetComponent<Structure>();
        OnAnyStructureBuilt?.Invoke(this, new OnAnyStructureBuiltEventArgs {
            structureBuilt = structure
        });
        if(isWorldLocation) {
            structure.SetAsWorldStructure(worldLocationScaleX);
        }
        
        StartCoroutine(DestroyGameObjectAfterFrame());
        return structure;
    }

    protected IEnumerator DestroyGameObjectAfterFrame() {
        isBeingDestroyed = true;
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (isBeingDestroyed) return;
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (isBeingDestroyed) return;
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;
        if (!structureSOToBuild.buildableAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        //return;

        Player.Instance.SetInPayCurrencyArea(true);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (!playerInTriggerArea) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetInPayCurrencyArea(false);
        playerInTriggerArea = false;
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);

        if (!structureSOToBuild.buildableAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        payCurrencyUI.SetPlayerInteracting(false);
    }

    public StructureSO GetStructureSOToBuild() {
        return structureSOToBuild;
    }

    public virtual void UnlockStructureLocation() {
        structureLocationUnlocked = true;
        OnStructureLocationUnlocked?.Invoke(this, EventArgs.Empty);
        showTooltipOnTrigger.SetShowTooltips(true);
    }

    public bool GetStructureLocationUnlocked() {
        return structureLocationUnlocked;
    }

    public void InvokeOnAnyStructureBuilt(Structure structure) {
        OnAnyStructureBuilt?.Invoke(this, new OnAnyStructureBuiltEventArgs {
            structureBuilt = structure
        });
    }

    protected void InitializeOrbTemplateList() {
        PayCurrencyTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<PayCurrencyTemplateWorldUI>(); 

        foreach(PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            buildStructureOrbTemplates.Add(orbTemplateWorldUI);
        }
    }

    protected void LoadStructureLocationBought() {
        if (DebugManager.Instance.GetAllStructuresUnlocked() || debugStructureTypeBought || structureSOToBuild.level1StructureInitiallyUnlocked) {
            gameObject.SetActive(true);
            return;
        }

        // Unlock upgrades if unlocked at gem merchant
        string saveString = structureSOToBuild.structureType.ToString() + (1);

        //Debug.Log("saveString " + saveString);
        if (!MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            OnStructureLocationLoaded_Locked?.Invoke(this, EventArgs.Empty);
            gameObject.SetActive(false);
            Debug.Log(saveString + " location has NOT been bought at merchant ");
        }
        else {
            gameObject.SetActive(true);
            //Debug.Log(saveString + " location has been bought at merchant ");
        }

    }

    protected virtual void OnDestroy() {
        if (GameInput.Instance != null) {
            GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
            GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractStarted;
        }

        if (DayNightManager.Instance != null) {
            DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
        }

        if (payCurrencyUI != null) {
            payCurrencyUI.OnCurrencyPaymentSuccess -= PayOrbsUI_OnOrbPaymentSuccess;
        }
    }

    protected void InvokeOnStructureSOToBuildChanged() {
        OnStructureSOToBuildChanged?.Invoke(this, EventArgs.Empty);
    }
    protected void InvokeOnAnyStructureSOToBuildChanged() {
        OnAnyStructureSOToBuildChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetAsWorldStructureLocation() {
        isWorldLocation = true;
    }
    public bool GetIsWorldStructureLocation() {
        return isWorldLocation;
    }

    public float GetStructureLocationWorldScaleX() {
        return worldLocationScaleX;
    }

}
