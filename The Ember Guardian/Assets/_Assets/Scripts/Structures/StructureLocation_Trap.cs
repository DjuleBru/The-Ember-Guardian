using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_Trap : StructureLocation {

    [SerializeField] private PayCurrencyTemplateWorldUI payCurrencyTemplate;
    private List<PlayerCurrencies.CurrencyType> trapCurrencyTypesInPlayerInventory = new List<PlayerCurrencies.CurrencyType>();
    private List<PlayerCurrencies.CurrencyType> trapCurrencyTypes = new List<PlayerCurrencies.CurrencyType>();

    private int currentCurrencyIndex = 0;

    public event EventHandler OnStructureSOToBuildChanged;
    public event EventHandler OnTrapTypesAmountInInventoryChanged;

    protected override void Start() {
        base.Start();
        trapCurrencyTypes = CurrenciesManager.Instance.GetCurrencyTypesInCategory(PlayerCurrencies.CurrencyCategory.trap);
        RefreshTrapTypesInPlayerInventory();

        GameInput.Instance.OnPlayerLeftSwitchPerformed += GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerRightSwitchPerformed += GameInput_OnPlayerRightSwitchPerformed;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += PlayerInventoryUI_OnCurrencyDropped;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyRemovedFromBag += PlayerInventoryUI_OnCurrencyRemovedFromBag;
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (trapCurrencyTypesInPlayerInventory.Count <= 1) return;

        currentCurrencyIndex = (currentCurrencyIndex + 1) % trapCurrencyTypesInPlayerInventory.Count;
        SetNewTrapSOToBuild();
    }

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, EventArgs e) {
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
        SetNewTrapSOToBuild();
    }


    private void PlayerInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {

        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void PlayerInventoryUI_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        Debug.Log(e.currencyUIDropped.GetCurrencyType());

        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }
    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!trapCurrencyTypes.Contains(e.currencyUIDropped.GetCurrencyType())) return;
        StartCoroutine(RefreshTrapTypesInPlayerInventoryAfterFrame());
    }

    private void SetNewTrapSOToBuild() {
        PlayerCurrencies.CurrencyType currentTrapType = trapCurrencyTypesInPlayerInventory[currentCurrencyIndex];

        structureSOToBuild = TrapManager.Instance.GetTrapSO(TrapManager.Instance.GetTrapType(currentTrapType)).trapStructureSO;
        payCurrencyTemplate.SetCurrencyTypeToPay(currentTrapType);

        OnStructureSOToBuildChanged?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator RefreshTrapTypesInPlayerInventoryAfterFrame() {
        yield return new WaitForEndOfFrame();
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

    private void OnDestroy() {
        GameInput.Instance.OnPlayerLeftSwitchPerformed -= GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerRightSwitchPerformed -= GameInput_OnPlayerRightSwitchPerformed;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= PlayerInventoryUI_OnCurrencyCollected;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped -= PlayerInventoryUI_OnCurrencyDropped;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyRemovedFromBag -= PlayerInventoryUI_OnCurrencyRemovedFromBag;
    }

}
