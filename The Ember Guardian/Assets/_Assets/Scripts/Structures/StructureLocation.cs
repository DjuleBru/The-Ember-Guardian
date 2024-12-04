using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation : MonoBehaviour {

    protected PayCurrencyUI payOrbsUI;
    [SerializeField] protected StructureSO structureSOToBuild;
    [SerializeField] protected Transform orbTemplateWorldUIParent;
    [SerializeField] protected bool isAlwaysUnlocked;

    protected List<PayCurrencyTemplateWorldUI> buildStructureOrbTemplates = new List<PayCurrencyTemplateWorldUI>();

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureLocationUnlocked;
    public static event EventHandler OnAnyStructureBuilt;

    protected bool structureLocationUnlocked;
    protected bool playerInTriggerArea;

    protected void Awake() {
        payOrbsUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        if(isAlwaysUnlocked) {
            structureLocationUnlocked = true;
        }
    }

    protected void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;

        payOrbsUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payOrbsUI.SetOrbTemplateUIList(buildStructureOrbTemplates);
    }


    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        BuildStructure();
    }

    protected virtual void BuildStructure() {

        Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity);
        OnAnyStructureBuilt?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payOrbsUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payOrbsUI.SetPlayerInteracting(false);
        payOrbsUI.ResetCurrencyPayment();
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetCanDropOrbOnTheFloor(false);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetCanDropOrbOnTheFloor(true);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        payOrbsUI.SetPlayerInteracting(false);
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

}
