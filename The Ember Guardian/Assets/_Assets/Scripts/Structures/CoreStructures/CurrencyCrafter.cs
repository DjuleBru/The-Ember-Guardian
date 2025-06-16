using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CurrencyCrafter : Structure
{
    private float currencyCraftTimer;

    [SerializeField] private float currencyCraftTime = 45f;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeCrafted;

    [SerializeField] private int currencyCraftAmount = 3;
    [SerializeField] private ShowTooltipOnTrigger showTooltipOnTrigger;

    [SerializeField] private int debugBatchCapacity;
    [SerializeField] private bool debugSpecialAmmoUnlocked;

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
    public event EventHandler<OnCurrencyInstantiatedEventArgs> OnCurrencyInstantiated;

    public class OnCurrencyInstantiatedEventArgs:EventArgs {
        public Collectible collectible;
    }

    protected override void Start() {
        base.Start();
        SetStructurePrimaryFunctionUnlocked(true);
        ActivateStructurePrimaryFunctionInteraction(true);
        needsRefill = true;
        needsWorking = false;

        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyCraftAmount = StructureStats.Instance.GetAmmoCrafterMaxAmmoPerBatch();
            currencyCraftTime = StructureStats.Instance.GetAmmoCrafterSingleAmmoCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetAmmoCrafterBatchCapacity();

            specialAmmoUnlocked = MetaProgressionManager.Instance.GetSpecialAmmoUnlocked();
            if (specialAmmoUnlocked || debugSpecialAmmoUnlocked) {
                SetStructureSecondaryFunctionUnlocked(true);
            }
        }

        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            currencyCraftAmount = StructureStats.Instance.GetOrbProcessorMaxOrbsPerBatch();
            currencyCraftTime = StructureStats.Instance.GetInitialSingleOrbCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetOrbProcessorBatchCapacity();
        }

        if(debugBatchCapacity != 0) {
            batchCapacity = debugBatchCapacity;
        }
    }

    private void Update() {
        if (!craftingCurrency) return;

        currencyCraftTimer -= Time.deltaTime;

        if(currencyCraftTimer < 0) {
            craftingCurrency = false;
            craftedCurrency = true;
            playerCanInteract = true;

            OnCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
            OnAnyCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
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

            needsRefill = false;
            needsWorking = true;
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

    public void WorkerCollectCurrencyFromCrafter() {
        craftedCurrency = false;
        StartCoroutine(CollectCurrencyFromCrafter(.2f));
        SetStructurePrimaryFunctionUnlocked(true);
        SetStructureSecondaryFunctionUnlocked(specialAmmoUnlocked);
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
            collectible.SetCanBePickedUpByWorkerAfterDelay(0);

            OnCurrencyInstantiated?.Invoke(this, new OnCurrencyInstantiatedEventArgs {
                collectible = collectible
            });

            yield return new WaitForSeconds(delayBetweenAmmoInstantiation);
        }

        needsRefill = true;
        needsWorking = false;
    }

    public void AccelerateCrafting(float accelerationTime) {
        currencyCraftTimer -= accelerationTime;
    }

    public bool GetCraftingCurrency() {
        return craftingCurrency;
    }
    public bool GetCraftedCurrency() {
        return craftedCurrency;
    }
    public int GetCurrencyCraftAmount() {
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
