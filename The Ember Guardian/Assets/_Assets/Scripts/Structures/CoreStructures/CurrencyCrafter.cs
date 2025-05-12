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

    private PlayerCurrencies.CurrencyType currencyTypeBeingCrafted;
    private int batchCapacity = 1;
    private int currentBatches;
    private bool craftingCurrency;
    private bool craftedCurrency;

    private bool specialAmmoUnlocked;

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
        SetStructurePrimaryFunctionUnlocked(true);
        ActivateStructurePrimaryFunctionInteraction(true);

        if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyCraftAmount = StructureStats.Instance.GetAmmoCrafterMaxAmmoPerBatch();
            currencyCraftTime = StructureStats.Instance.GetAmmoCrafterSingleAmmoCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetAmmoCrafterBatchCapacity();

            specialAmmoUnlocked = MetaProgressionManager.Instance.GetSpecialAmmoUnlocked();
            if (specialAmmoUnlocked) {
                SetStructureSecondaryFunctionUnlocked(true);
            }
        }

        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            currencyCraftAmount = StructureStats.Instance.GetOrbProcessorMaxOrbsPerBatch();
            currencyCraftTime = StructureStats.Instance.GetInitialSingleOrbCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetOrbProcessorBatchCapacity();
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
        if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyTypeBeingCrafted = PlayerCurrencies.CurrencyType.ammo;
        }

        TriggerCrafterFunction();
        SetStructureSecondaryFunctionUnlocked(false);
    }

    protected override void TriggerStructureSecondaryFunction() {
        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyTypeBeingCrafted = PlayerCurrencies.CurrencyType.ammo_special;
        }

        TriggerCrafterFunction();
        SetStructurePrimaryFunctionUnlocked(false);
    }

    private void TriggerCrafterFunction() {
        if (!craftedCurrency) {
            // No batch is being crafted

            OnNewCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);
            OnAnyNewCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);

            if (showTooltipOnTrigger != null) {
                showTooltipOnTrigger.HideTooltipShown();
                showTooltipOnTrigger.SetShowTooltips(false);
            }

            craftingCurrency = true;

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
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);

        if(currentBatches < batchCapacity && !craftedCurrency) {

            payCurrencyUI.SetPlayerInteracting(true);

        } else {

            if (!craftedCurrency) return;
            // Ammo has not finished crafting

            StartCoroutine(CollectCurrencyFromCrafter(.2f));

            SetStructurePrimaryFunctionUnlocked(true);
            SetStructureSecondaryFunctionUnlocked(specialAmmoUnlocked);
            showTooltipOnTrigger.SetShowTooltips(true);

        }

        StartCoroutine(SetPlayerInteractionAfterDelay(.1f));

    }

    private IEnumerator SetPlayerInteractionAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
    }

    private IEnumerator CollectCurrencyFromCrafter(float delayBetweenAmmoInstantiation) {
        craftedCurrency = false;
        payCurrencyUI.ResetCurrencyPayment();

        int currentBatchesCopy = currentBatches;
        currentBatches = 0;

        OnPlayerCollectedCurrency?.Invoke(this, EventArgs.Empty);
        OnPlayerCollectedAnyCurrency?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < currencyCraftAmount * currentBatchesCopy; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeBeingCrafted), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(1.5f);
            collectible.ApplyRandomForce(-1, 1, 3, 5);

            OnCurrencyInstantiated?.Invoke(this, EventArgs.Empty);
            yield return new WaitForSeconds(delayBetweenAmmoInstantiation);
        }

    }
    public bool GetCraftingCurrency() {
        return craftingCurrency;
    }
    public int GetAmmoCraftAmount() {
        return currencyCraftAmount;
    }

    public int GetCurrentBatch() {
        return currentBatches;
    }

    public int GetBatchCapacity() {
        return batchCapacity;
    }

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (currencyCraftTimer / (currencyCraftTime * currentBatches));
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeCrafted() {
        return currencyTypeCrafted;
    }
}
