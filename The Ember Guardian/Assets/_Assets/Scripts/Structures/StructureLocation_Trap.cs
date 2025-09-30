using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_Trap : StructureLocation {

    [SerializeField] private PayCurrencyTemplateWorldUI payCurrencyTemplate;
    private List<PlayerCurrencies.CurrencyType> trapCurrencyTypesInPlayerInventory = new List<PlayerCurrencies.CurrencyType>();
    private List<PlayerCurrencies.CurrencyType> trapCurrencyTypes = new List<PlayerCurrencies.CurrencyType>();

    private int currentCurrencyIndex = 0;
    private bool trapLocationActive = true;

    public event EventHandler OnTrapTypesAmountInInventoryChanged;

    protected override void Start() {
        base.Start();
        trapCurrencyTypes = CurrenciesManager.Instance.GetCurrencyTypesInCategory(PlayerCurrencies.CurrencyCategory.trap);
        StartCoroutine(InitializeTrapTypesInPlayerInventory());

        GameInput.Instance.OnPlayerLeftSwitchPerformed += GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerRightSwitchPerformed += GameInput_OnPlayerRightSwitchPerformed;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += PlayerInventoryUI_OnCurrencyDropped;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyRemovedFromBag += PlayerInventoryUI_OnCurrencyRemovedFromBag;


    }

    private IEnumerator InitializeTrapTypesInPlayerInventory() {
        yield return new WaitForEndOfFrame();

        if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            yield return new WaitForEndOfFrame();
        }

        RefreshTrapTypesInPlayerInventory();
    }

    public void SetStructureSOToBuild(StructureSO structureSO) {
        this.structureSOToBuild = structureSO;
    }

    public override Structure BuildStructure(bool buildOnLoad = false) {

        Structure_Trap trap = Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity).GetComponent<Structure_Trap>();
        trap.SetTrapStructureLocation(this);
        trap.SetStructureBuiltOnLoad(buildOnLoad);
        InvokeOnAnyStructureBuilt(trap, buildOnLoad);

        StructuresManager.Instance.RemoveStructureLocation(this);
        StructuresManager.Instance.AddBuiltStructure(trap);

        trapLocationActive = false;
        gameObject.SetActive(false);
        return trap;
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, EventArgs e) {
        if (!structureLocationUnlocked) return;
        if (!trapLocationActive) return;
        if (!playerInTriggerArea) return;
        if (trapCurrencyTypesInPlayerInventory.Count <= 1) return;

        currentCurrencyIndex = (currentCurrencyIndex + 1) % trapCurrencyTypesInPlayerInventory.Count;
        SetNewTrapSOToBuild();
    }

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, EventArgs e) {
        if (!structureLocationUnlocked) return;
        if (!trapLocationActive) return;
        if (!playerInTriggerArea) return;
        if (trapCurrencyTypesInPlayerInventory.Count <= 1) return;

        currentCurrencyIndex = (currentCurrencyIndex - 1 + trapCurrencyTypesInPlayerInventory.Count) % trapCurrencyTypesInPlayerInventory.Count;
        SetNewTrapSOToBuild();
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (trapCurrencyTypesInPlayerInventory.Count == 0) return;
        base.OnTriggerEnter2D(collision);
        
        if (!structureLocationUnlocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        currentCurrencyIndex = 0;
        SetTrapSOToBuild();
    }

    private void PlayerInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!trapLocationActive) return;
        if (!structureLocationUnlocked) return;

        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void PlayerInventoryUI_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!trapLocationActive) return;
        if (!structureLocationUnlocked) return;

        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!trapLocationActive) return;
        if (!structureLocationUnlocked) return;

        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void SetNewTrapSOToBuild() {
        PlayerCurrencies.CurrencyType currentTrapType = trapCurrencyTypesInPlayerInventory[currentCurrencyIndex];

        Debug.Log("SetNewTrapSOToBuild " + currentTrapType);

        structureSOToBuild = TrapManager.Instance.GetTrapSO(TrapManager.Instance.GetTrapType(currentTrapType)).trapStructureSO;
        payCurrencyTemplate.SetCurrencyTypeToPay(currentTrapType);
        InvokeOnStructureSOToBuildChanged();
        InvokeOnAnyStructureSOToBuildChanged();
    }
    private void SetTrapSOToBuild() {
        PlayerCurrencies.CurrencyType currentTrapType = trapCurrencyTypesInPlayerInventory[currentCurrencyIndex];

        structureSOToBuild = TrapManager.Instance.GetTrapSO(TrapManager.Instance.GetTrapType(currentTrapType)).trapStructureSO;
        payCurrencyTemplate.SetCurrencyTypeToPay(currentTrapType);
        InvokeOnStructureSOToBuildChanged();
    }

    private IEnumerator RefreshTrapTypesInPlayerInventoryAfterFrame() {
        yield return new WaitForEndOfFrame();
        RefreshTrapTypesInPlayerInventory();
    }

    public override void UnlockStructureLocation() {
        base.UnlockStructureLocation();
        RefreshTrapTypesInPlayerInventory();
    }

    private void RefreshTrapTypesInPlayerInventory() {
        trapCurrencyTypesInPlayerInventory.Clear();

        foreach (PlayerCurrencies.CurrencyType currencyType in trapCurrencyTypes) {

            List<Currency_UI> trapsInBagOfType = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(currencyType);

            if (trapsInBagOfType.Count > 0 ) {
                trapCurrencyTypesInPlayerInventory.Add(currencyType);
            }
        }

        OnTrapTypesAmountInInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTrapTypeAmountInInventory() {
        return trapCurrencyTypesInPlayerInventory.Count;
    }

    public void ReActivateTrapStructureLocation() {
        structureLocationUnlocked = true;
        trapLocationActive = true;
        gameObject.SetActive(true);

        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerLeftSwitchPerformed -= GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerRightSwitchPerformed -= GameInput_OnPlayerRightSwitchPerformed;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= PlayerInventoryUI_OnCurrencyCollected;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped -= PlayerInventoryUI_OnCurrencyDropped;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyRemovedFromBag -= PlayerInventoryUI_OnCurrencyRemovedFromBag;
    }

}
