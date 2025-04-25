using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCrafter : Structure
{
    private float currencyCraftTimer;

    [SerializeField] private float currencyCraftTime = 45f;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeCrafted;

    [SerializeField] private int currencyCraftAmount = 3;
    [SerializeField] private ShowTooltipOnTrigger showTooltipOnTrigger;

    private int batchCapacity = 1;
    private int currentBatches;
    private bool craftingCurrency;
    private bool craftedCurrency;

    public event EventHandler OnNewCurrencyBatchCraftingStarted;
    public static event EventHandler OnAnyNewCurrencyBatchCraftingStarted;
    public event EventHandler OnCurrencyCraftingEnded;
    public static event EventHandler OnAnyCurrencyCraftingEnded;
    public event EventHandler OnPlayerCollectedCurrency;
    public static event EventHandler OnPlayerCollectedAnyCurrency;
    public event EventHandler OnMaxCurrencyBatchCraftingStarted;
    public event EventHandler OnCurrencyInstantiated;

    protected override void Start() {
        base.Start();
        ActivateStructurePrimaryFunctionInteraction(true);

        if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyCraftAmount = StructureStats.Instance.GetAmmoCrafterMaxAmmoPerBatch();
            currencyCraftTime = StructureStats.Instance.GetAmmoCrafterSingleAmmoCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetAmmoCrafterBatchCapacity();
        }
    }

    private void Update() {
        if (!craftingCurrency) return;

        currencyCraftTimer -= Time.deltaTime;

        if(currencyCraftTimer < 0) {
            OnCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
            OnAnyCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
            craftingCurrency = false;
            craftedCurrency = true;
            playerCanInteract = true;
        }
    }

    protected override void TriggerStructurePrimaryFunction() {
        if(!craftedCurrency) {
            // No batch is being crafted

            craftingCurrency = true;
            OnNewCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);
            OnAnyNewCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);
            showTooltipOnTrigger.HideTooltipShown();
            showTooltipOnTrigger.SetShowTooltips(false);

        }

        currentBatches++;
        currencyCraftTimer += currencyCraftTime;

        if (currentBatches == batchCapacity) {
            playerCanInteract = false; 
            OnMaxCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;

        if(currentBatches < batchCapacity && !craftingCurrency) {

            payCurrencyUI.SetPlayerInteracting(true);

        } else {

            if (!craftedCurrency) return;
            // Ammo has not finished crafting

            StartCoroutine(CollectCurrencyFromCrafter(.2f));
            showTooltipOnTrigger.SetShowTooltips(true);

        }
    }

    private IEnumerator CollectCurrencyFromCrafter(float delayBetweenAmmoInstantiation) {
        craftedCurrency = false;
        payCurrencyUI.ResetCurrencyPayment();
        OnPlayerCollectedCurrency?.Invoke(this, EventArgs.Empty);
        OnPlayerCollectedAnyCurrency?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < currencyCraftAmount; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeCrafted), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(1.5f);
            collectible.ApplyRandomForce(-1, 1, 3, 5);

            OnCurrencyInstantiated?.Invoke(this, EventArgs.Empty);
            yield return new WaitForSeconds(delayBetweenAmmoInstantiation);
        }
    }

    public int GetAmmoCraftAmount() {
        return currencyCraftAmount;
    }

    public int GetBatchCapacity() {
        return batchCapacity;
    }

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (currencyCraftTimer / currencyCraftTime);
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeCrafted() {
        return currencyTypeCrafted;
    }
}
