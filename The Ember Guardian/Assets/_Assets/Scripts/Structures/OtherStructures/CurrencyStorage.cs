using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorage : Structure
{
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeStored;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private int maxCurrencyAmountStored = 15;
    private int currencyAmountStored;

    public event EventHandler OnCurrencyStored;
    public event EventHandler OnCurrencyRemoved;
    public static event EventHandler<OnAnyCurrencySpawnedEventArgs> OnAnyCurrencySpawned;
    public class OnAnyCurrencySpawnedEventArgs:EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    protected override void Awake() {
        base.Awake();
        RefreshInteractable();
        PlayerCamp.Instance.AddCurrencyStorage(this);
    }

    protected override void Start() {
        base.Start();
        GameInput.Instance.OnCurrencyCollectedFromContainer += GameInput_OnCurrencyCollectedFromContainer;

        Debug.Log("maxCurrencyAmountStored " + maxCurrencyAmountStored);
        maxCurrencyAmountStored = maxCurrencyAmountStored + Mathf.RoundToInt(StructureStats.Instance.GetEngineerContainerSizeBuff() * maxCurrencyAmountStored);
        Debug.Log("NewMaxCurrencyAmountStored " + maxCurrencyAmountStored);
    }

    private void GameInput_OnCurrencyCollectedFromContainer(object sender, EventArgs e) {
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

        if (GetHasCurrenciesToPay() && playerInteracting && currencyAmountStored < maxCurrencyAmountStored) {
            payCurrencyUI.SetPlayerInteractingContinuous(.2f); // Continue l'interaction
        }
        else {
            payCurrencyUI.SetPlayerInteracting(false);
        }
    }

    public void StoreCurrency() {
        currencyAmountStored++;
        OnCurrencyStored?.Invoke(this, EventArgs.Empty);

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

    public PlayerCurrencies.CurrencyType GetCurrencyTypeStored() {
        return currencyTypeStored;
    }

    private void RefreshInteractable() {
        if(currencyAmountStored < maxCurrencyAmountStored) {
            ActivateStructurePrimaryFunctionInteraction(true);
        } else {
            ActivateStructurePrimaryFunctionInteraction(false);
        }
    }

    public float GetCurrencyStoredAmountNormalized() {
        return (float)currencyAmountStored/ (float)maxCurrencyAmountStored;
    }

    public bool GetStorageFull() {
        return currencyAmountStored >= maxCurrencyAmountStored;
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
