using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CurrencyCrafter : Structure
{
    private float currencyCraftTimer;

    [SerializeField] private float currencyCraftTime = 45f;
    private float primaryCurrencyCraftTime;
    private float secondaryCurrencyCraftTime;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeCrafted;

    [SerializeField] private int currencyCraftAmount = 3;
    [SerializeField] private ShowTooltipOnTrigger showTooltipOnTrigger;

    [SerializeField] private bool useDebugBatchCapacity;
    [SerializeField] private int debugBatchCapacity;
    [SerializeField] private bool debugSpecialAmmoUnlocked;

    private PlayerCurrencies.CurrencyType currencyTypeBeingCrafted;
    private int batchCapacity = 1;
    private int currentBatches;
    private bool craftingCurrency;
    private bool craftedCurrency;
    private bool collectingCurrency;

    private bool specialAmmoUnlocked;

    public event EventHandler<OnNewCurrencyBatchCraftingStartedEventArgs> OnNewCurrencyBatchCraftingStarted;
    public static event EventHandler OnAnyNewCurrencyBatchCraftingStarted;
    public event EventHandler OnCurrencyCraftingEnded;
    public static event EventHandler OnAnyCurrencyCraftingEnded;
    public event EventHandler OnPlayerCollectedCurrency;
    public static event EventHandler OnPlayerCollectedAnyCurrency;
    public event EventHandler OnMaxCurrencyBatchCraftingStarted;
    public event EventHandler<OnCurrencyInstantiatedEventArgs> OnCurrencyInstantiated;
    public event EventHandler OnCurrencyTypeBeingCraftedLoaded;

    public class OnCurrencyInstantiatedEventArgs:EventArgs {
        public Collectible collectible;
        public bool instantiatedByWorker;
    }
    public class OnNewCurrencyBatchCraftingStartedEventArgs : EventArgs {
        public bool triggerStartCraftingSFX;
    }

    protected override void Start() {
        base.Start();
        SetStructurePrimaryFunctionUnlocked(true);
        ActivateStructurePrimaryFunctionInteraction(true);

        if(!SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            needsRefill = true;
            needsWorking = false;
        }


        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyCraftAmount = StructureStats.Instance.GetAmmoCrafterMaxAmmoPerBatch();
            currencyCraftTime = StructureStats.Instance.GetAmmoCrafterSingleAmmoCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetAmmoCrafterBatchCapacity();
            primaryCurrencyCraftTime = currencyCraftTime;
            secondaryCurrencyCraftTime = currencyCraftTime * 2;

            PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
            specialAmmoUnlocked = MetaProgressionManager.Instance.GetSpecialAmmoUnlocked();
            RefreshSpecialAmmoUnlocked();
        }

        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            currencyCraftAmount = StructureStats.Instance.GetOrbProcessorMaxOrbsPerBatch();
            currencyCraftTime = StructureStats.Instance.GetInitialSingleOrbCraftDuration() * currencyCraftAmount;
            batchCapacity = StructureStats.Instance.GetOrbProcessorBatchCapacity();
        }

        primaryCurrencyCraftTime = currencyCraftTime;
        if (useDebugBatchCapacity) {
            batchCapacity = debugBatchCapacity;
        }

    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (specialAmmoUnlocked) return;

        RefreshSpecialAmmoUnlocked();
    }

    private void RefreshSpecialAmmoUnlocked() {

        GunSO primaryGunSO = PlayerShoot.Instance.GetPrimaryGunSO();
        GunSO secondaryGunSO = PlayerShoot.Instance.GetSecondaryGunSO();
        GunSO heldGunSO = PlayerShoot.Instance.GetHeldGunSO();

        bool unlockSpecialAmmo = false;

        if (primaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            unlockSpecialAmmo = true;
        }
        if(secondaryGunSO != null && secondaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            unlockSpecialAmmo = true;
        }
        if(heldGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            unlockSpecialAmmo = true;
        }

        if(debugSpecialAmmoUnlocked) {
            unlockSpecialAmmo = true;
        }

        if (unlockSpecialAmmo) {
            SetStructureSecondaryFunctionUnlocked(true);
            specialAmmoUnlocked = true;
        } else {
            SetStructureSecondaryFunctionUnlocked(false);
            specialAmmoUnlocked = false;
        }
    }

    private void Update() {
        if (!craftingCurrency) return;

        currencyCraftTimer -= Time.deltaTime;

        if(currencyCraftTimer < 0) {
            craftingCurrency = false;
            craftedCurrency = true;
            playerCanInteract = true;
            needsWorking = false;
            currencyCraftTimer = 0;

            OnCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
            OnAnyCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);

            if(currencyTypeBeingCrafted == PlayerCurrencies.CurrencyType.ammo) {
                SetStructurePrimaryFunctionUnlocked(false);
                if(specialAmmoUnlocked) {
                    SetStructureSecondaryFunctionUnlocked(false);
                }
            }
            if (currencyTypeBeingCrafted == PlayerCurrencies.CurrencyType.ammo_special) {
                SetStructurePrimaryFunctionUnlocked(false);
                SetStructureSecondaryFunctionUnlocked(false);
            }
        }
    }

    protected override void TriggerStructurePrimaryFunction() {
        if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyTypeBeingCrafted = PlayerCurrencies.CurrencyType.ammo;
            currencyCraftTime = primaryCurrencyCraftTime;
        }

        TriggerCrafterFunction();
        SetStructureSecondaryFunctionUnlocked(false);
    }

    protected override void TriggerStructureSecondaryFunction() {
        if (currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyTypeBeingCrafted = PlayerCurrencies.CurrencyType.ammo_special;
            currencyCraftTime = secondaryCurrencyCraftTime;
        }

        TriggerCrafterFunction();
        SetStructurePrimaryFunctionUnlocked(false);
    }

    private void TriggerCrafterFunction() {
        if (!craftedCurrency) {
            // No batch is being crafted
            OnNewCurrencyBatchCraftingStarted?.Invoke(this, new OnNewCurrencyBatchCraftingStartedEventArgs {
                triggerStartCraftingSFX = true
            });
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

            StartCoroutine(CollectCurrencyFromCrafter(.2f, false, currencyTypeBeingCrafted));

            if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                SetStructurePrimaryFunctionUnlocked(true);
            }

            if(currencyTypeCrafted == PlayerCurrencies.CurrencyType.ammo) {
                SetStructurePrimaryFunctionUnlocked(true);
                SetStructureSecondaryFunctionUnlocked(specialAmmoUnlocked);

                if (currencyTypeBeingCrafted == PlayerCurrencies.CurrencyType.ammo_special) {

                    ActivateStructureSecondaryFunctionInteraction(true);
                    SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction, true);

                } else {

                    ActivateStructurePrimaryFunctionInteraction(true);
                    SetCurrentStructureInteractionType(StructureInteractionType.primaryFunction, true);

                }

            }

            RefreshPlayerCanInteract();
            if(showTooltipOnTrigger != null) {
                showTooltipOnTrigger.SetShowTooltips(true);
            }
        }

        StartCoroutine(SetPlayerInteractionAfterDelay(.1f));

    }
    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
       base.GameInput_OnPlayerInteractCanceled(sender, e);
        if(collectingCurrency) {
            collectingCurrency = false;
            RefreshPlayerCanInteract();
        }
    }

    private IEnumerator SetPlayerInteractionAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
    }

    public void WorkerCollectCurrencyFromCrafter() {
        craftedCurrency = false;
        StartCoroutine(CollectCurrencyFromCrafter(.2f, true, currencyTypeBeingCrafted));
        SetStructurePrimaryFunctionUnlocked(true);
        SetStructureSecondaryFunctionUnlocked(specialAmmoUnlocked);
    }

    private IEnumerator CollectCurrencyFromCrafter(float delayBetweenAmmoInstantiation, bool collectedByWorker, PlayerCurrencies.CurrencyType currencyTypeCrafter) {
        craftedCurrency = false;
        collectingCurrency = true;

        payCurrencyUI.ResetCurrencyPayment();

        int currentBatchesCopy = currentBatches;
        currentBatches = 0;

        OnPlayerCollectedCurrency?.Invoke(this, EventArgs.Empty);
        OnPlayerCollectedAnyCurrency?.Invoke(this, EventArgs.Empty);

        for (int i = 0; i < currencyCraftAmount * currentBatchesCopy; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeCrafter), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(1.5f);
            collectible.ApplyRandomForce(-1, 1, 3, 5);

            if(collectedByWorker) {
                collectible.SetCanBePickedUpByWorkerAfterDelay(0);
            }


            OnCurrencyInstantiated?.Invoke(this, new OnCurrencyInstantiatedEventArgs {
                collectible = collectible,
                instantiatedByWorker = collectedByWorker,
            });

            yield return new WaitForSeconds(delayBetweenAmmoInstantiation);
        }

        needsRefill = true;
        needsWorking = false;
    }

    public void AccelerateCrafting(float accelerationTime) {
        currencyCraftTimer -= accelerationTime;
    }
    public float GetCurrencyCraftTimer() {
        return currencyCraftTimer;
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
    public float GetCurrencyCraftTime() {
        return currencyCraftTime;
    }

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (currencyCraftTimer / (currencyCraftTime * currentBatches));
    }

    public void SetCurrentBatches(int currentBatches) {
        for (int i = 0; i < currentBatches; i++) {
            OnNewCurrencyBatchCraftingStarted?.Invoke(this, new OnNewCurrencyBatchCraftingStartedEventArgs {
                triggerStartCraftingSFX = false
            });
            this.currentBatches++;
        }

        if(currentBatches > 0) {
            needsRefill = false;
            needsWorking = true;
        }

        if (currentBatches == batchCapacity) {
            OnMaxCurrencyBatchCraftingStarted?.Invoke(this, EventArgs.Empty);
            SetStructurePrimaryFunctionUnlocked(false);
            SetStructureSecondaryFunctionUnlocked(false);
        }

        RefreshPlayerCanInteract();
    }

    public void SetCurrencyCraftTimer(float currencyCraftTimer) {
        this.currencyCraftTimer = currencyCraftTimer;
    }

    public void SetCraftingCurrency(bool craftingCurrency) {
        this.craftingCurrency = craftingCurrency;

        if(craftingCurrency) {
            needsWorking = true;
            SetStructurePrimaryFunctionUnlocked(false);
            SetStructureSecondaryFunctionUnlocked(false);
        }
    }

    public void SetCraftedCurrency(bool craftedCurrency) {
        StartCoroutine(SetCraftedCurrencyAfterFrame(craftedCurrency));
    }

    public virtual void SetCurrencyTypeBeingCrafted(PlayerCurrencies.CurrencyType currencyTypeBeingCrafted) {
        this.currencyTypeBeingCrafted = currencyTypeBeingCrafted;

        if(currencyTypeBeingCrafted == PlayerCurrencies.CurrencyType.ammo_special) {
            currencyCraftTime = secondaryCurrencyCraftTime;
            SetStructureSecondaryFunctionUnlocked(true);
            SetStructurePrimaryFunctionUnlocked(false);
            ActivateStructureSecondaryFunctionInteraction(true);
            SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction);
        }

        if(currencyTypeBeingCrafted == PlayerCurrencies.CurrencyType.ammo) {
            currencyCraftTime = primaryCurrencyCraftTime;
            SetCurrentStructureInteractionType(StructureInteractionType.primaryFunction);
        }

        OnCurrencyTypeBeingCraftedLoaded?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator SetCraftedCurrencyAfterFrame(bool craftedCurrency) {
        yield return new WaitForEndOfFrame();
        this.craftedCurrency = craftedCurrency; 

        if(craftedCurrency) {
            needsWorking = false;
            OnCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
        }

        RefreshPlayerCanInteract(); 
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeCrafted() {
        return currencyTypeCrafted;
    }
    public PlayerCurrencies.CurrencyType GetCurrencyTypeBeingCrafted() {
        return currencyTypeBeingCrafted;
    }
    protected override void RefreshPlayerCanInteract() {
        if(craftingCurrency && currentBatches == batchCapacity) {
            playerCanInteract = false;
            return;
        }

        if(collectingCurrency) {
            playerCanInteract = false;
            return;
        }

        if (craftedCurrency) {
            playerCanInteract = true;
            return;
        }

        if (activeStructureInteractionsTypeList.Count == 0) {
            playerCanInteract = false;
        }
        else {
            playerCanInteract = true;
        }

    }
}
