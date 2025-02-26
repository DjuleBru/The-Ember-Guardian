using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation : MonoBehaviour {

    protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected StructureSO structureSOToBuild;
    [SerializeField] protected Transform orbTemplateWorldUIParent;
    [SerializeField] protected bool isAlwaysUnlocked;

    protected List<PayCurrencyTemplateWorldUI> buildStructureOrbTemplates = new List<PayCurrencyTemplateWorldUI>();

    public event EventHandler OnStructureLocationLoaded_Locked;
    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureLocationUnlocked;
    public static event EventHandler OnAnyStructureBuilt;

    protected bool structureLocationUnlocked;
    protected bool playerInTriggerArea;

    protected void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        if(isAlwaysUnlocked) {
            structureLocationUnlocked = true;
        }
    }

    protected virtual void Start() {
        LoadStructureLocationBought();
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.SetOrbTemplateUIList(buildStructureOrbTemplates);
    }


    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        BuildStructure();
    }

    public virtual Structure BuildStructure() {

        Structure structure = Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity).GetComponent<Structure>();
        OnAnyStructureBuilt?.Invoke(this, EventArgs.Empty);

        StartCoroutine(DestroyGameObjectAfterFrame());
        return structure;
    }

    protected IEnumerator DestroyGameObjectAfterFrame() {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        Player.Instance.SetInPayCurrencyArea(true);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetInPayCurrencyArea(false);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        payCurrencyUI.SetPlayerInteracting(false);
        playerInTriggerArea = false;
    }

    public StructureSO GetStructureSOToBuild() {
        return structureSOToBuild;
    }

    public void UnlockStructureLocation() {
        structureLocationUnlocked = true;
        OnStructureLocationUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnAnyStructureBuilt() {
        OnAnyStructureBuilt?.Invoke(this, EventArgs.Empty);
    }

    protected void InitializeOrbTemplateList() {
        PayCurrencyTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<PayCurrencyTemplateWorldUI>(); 

        foreach(PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            buildStructureOrbTemplates.Add(orbTemplateWorldUI);
        }
    }

    protected void LoadStructureLocationBought() {
        if (DebugManager.Instance.GetAllStructuresUnlocked()) {
            gameObject.SetActive(true);
            return;
        }

        if (structureSOToBuild.level1StructureInitiallyUnlocked) return;

        // Unlock upgrades if unlocked at gem merchant
        string saveString = structureSOToBuild.structureType.ToString() + (1);

        if (!MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            OnStructureLocationLoaded_Locked?.Invoke(this, EventArgs.Empty);
            Debug.Log(saveString + " location has NOT been bought at merchant ");
        }
        else {
            gameObject.SetActive(true);
            Debug.Log(saveString + " location has been bought at merchant ");
        }

    }

}
