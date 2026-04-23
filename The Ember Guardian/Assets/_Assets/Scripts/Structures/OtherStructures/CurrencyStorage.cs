using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorage : Structure
{
    [SerializeField] protected PlayerCurrencies.CurrencyType currencyTypeStored;
    [SerializeField] protected Transform currencySpawnPoint;
    [SerializeField] protected int maxCurrencyAmountStored = 15;
    protected bool engineersCanPickUpOrbs = true;
    protected int currencyAmountStored;

    public event EventHandler<OnAnyCurrencyStoredEventArgs> OnCurrencyStored;
    public event EventHandler OnCurrencyAmountStoredLoaded;
    public event EventHandler OnCurrencyRemoved;
    public static event EventHandler<OnAnyCurrencySpawnedEventArgs> OnAnyCurrencySpawned;
    public class OnAnyCurrencySpawnedEventArgs:EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }
    public class OnAnyCurrencyStoredEventArgs : EventArgs {
        public bool triggerSFX;
    }


    protected override void Awake() {
        base.Awake();
        RefreshInteractable();
        PlayerCamp.Instance.AddCurrencyStorage(this);
    }

    protected override void Start() {
        base.Start();
        GameInput.Instance.OnCurrencyCollectedFromContainer += GameInput_OnCurrencyCollectedFromContainer;

        maxCurrencyAmountStored = maxCurrencyAmountStored + Mathf.RoundToInt(StructureStats.Instance.GetEngineerContainerSizeBuff() * maxCurrencyAmountStored);
    }

    protected virtual void GameInput_OnCurrencyCollectedFromContainer(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (currencyAmountStored <= 0) return;

        RemoveCurrency();

        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeStored), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.SetCollectibleUnInteractable(1.5f);
        collectible.ApplyRandomForce(-1, 1, 3, 5);

        OnAnyCurrencySpawned?.Invoke(this, new OnAnyCurrencySpawnedEventArgs {
            currencyType = currencyTypeStored,
        });
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();

        StoreCurrency();

        Debug.Log("GetHasCurrenciesToPay() " + GetHasCurrenciesToPay());
        Debug.Log("playerInteracting() " + playerInteracting);
        Debug.Log("currencyAmountStored < maxCurrencyAmountStored " + (currencyAmountStored < maxCurrencyAmountStored));
        if (GetHasCurrenciesToPay() && playerInteracting && currencyAmountStored < maxCurrencyAmountStored) {
            payCurrencyUI.SetPlayerInteractingContinuous(.05f); // Continue l'interaction
        }
        else {
            payCurrencyUI.SetPlayerInteracting(false);
        }
    }

    public override PlayerCurrencies.CurrencyType GetRefillCurrencyTypeNeeded() {
        return currencyTypeStored;
    }


    public void StoreCurrency() {
        currencyAmountStored++;
        OnCurrencyStored?.Invoke(this, new OnAnyCurrencyStoredEventArgs {
            triggerSFX = true
        });

        RefreshInteractable();
    }

    public void RemoveCurrency() {
        currencyAmountStored--;
        OnCurrencyRemoved?.Invoke(this, EventArgs.Empty);

        RefreshInteractable();
    }

    public int GetCurrencyAmountStored() {
        return currencyAmountStored;
    }

    public void SetCurrencyAmountStored(int currencyAmountStored) {
        this.currencyAmountStored = currencyAmountStored;
        OnCurrencyStored?.Invoke(this, new OnAnyCurrencyStoredEventArgs {
            triggerSFX = false
        });
        RefreshInteractable();


        OnCurrencyAmountStoredLoaded?.Invoke(this, EventArgs.Empty);
    }

    public int GetMaxCurrencyAmountStored() {
        return maxCurrencyAmountStored;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeStored() {
        return currencyTypeStored;
    }

    protected void RefreshInteractable() {
        if(currencyAmountStored < maxCurrencyAmountStored) {
            ActivateStructurePrimaryFunctionInteraction(true);
        } else {
            ActivateStructurePrimaryFunctionInteraction(false);
        }
    }

    public float GetCurrencyStoredAmountNormalized() {
        if(currencyTypeStored == PlayerCurrencies.CurrencyType.smallRedOrb || currencyTypeStored == PlayerCurrencies.CurrencyType.bigRedOrb) {
            return (float)currencyAmountStored / (float)30;
        }

        return (float)currencyAmountStored/ (float)maxCurrencyAmountStored;
    }

    public bool GetEngineerCanPickUpOrbs() {
        return currencyAmountStored >= maxCurrencyAmountStored && engineersCanPickUpOrbs;
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {

        if (currentStructureInteractionType == StructureInteractionType.primaryFunction) {
            TriggerStructurePrimaryFunction();
        }

        if (currentStructureInteractionType == StructureInteractionType.secondaryFunction) {
            TriggerStructureSecondaryFunction();
        }

        if (currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
        }

        //payCurrencyUI.SetPlayerInteracting(false);
        //payCurrencyUI.StopWorkerInteraction();
        //isBeingRefilledByEngineer = false;
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject.GetComponent<Player>() == null) return;
        Player.Instance.SetInCurrencyStorageArea(true);
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);
        if (collision.gameObject.GetComponent<Player>() == null) return;
        Player.Instance.SetInCurrencyStorageArea(false);
    }

}
