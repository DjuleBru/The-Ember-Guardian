using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation : MonoBehaviour {

    private PayOrbsUI payOrbsUI;
    [SerializeField] private StructureSO structureSOToBuild;
    [SerializeField] protected Transform orbTemplateWorldUIParent;

    private List<OrbTemplateWorldUI> buildStructureOrbTemplates = new List<OrbTemplateWorldUI>();

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnStructureLocationUnlocked;
    public static event EventHandler OnAnyStructureBuilt;

    private bool structureLocationUnlocked;
    private bool playerInTriggerArea;

    private void Awake() {
        payOrbsUI = GetComponent<PayOrbsUI>();
        InitializeOrbTemplateList();

    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;

        payOrbsUI.OnOrbPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payOrbsUI.SetOrbTemplateUIList(buildStructureOrbTemplates);
    }

    private void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        BuildStructure();
    }

    private void BuildStructure() {

        Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity);
        OnAnyStructureBuilt?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payOrbsUI.SetPlayerInteracting(true);
    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!structureLocationUnlocked) return;

        payOrbsUI.CancelOrbPayment();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetCanDropOrbOnTheFloor(false);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
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

    private void InitializeOrbTemplateList() {
        OrbTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<OrbTemplateWorldUI>(); 

        foreach(OrbTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            buildStructureOrbTemplates.Add(orbTemplateWorldUI);
        }
    }

}
