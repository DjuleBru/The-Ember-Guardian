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

    private bool craftingCurrency;
    private bool craftedCurrency;

    public event EventHandler OnCurrencyCraftingStarted;
    public event EventHandler OnCurrencyCraftingEnded;
    public event EventHandler OnPlayerCollectedCurrency;
    public event EventHandler OnCurrencyInstantiated;

    protected override void Start() {
        base.Start();
        ActivateStructurePrimaryFunctionInteraction(true);
    }

    private void Update() {
        if (!craftingCurrency) return;

        currencyCraftTimer -= Time.deltaTime;

        if(currencyCraftTimer < 0) {
            OnCurrencyCraftingEnded?.Invoke(this, EventArgs.Empty);
            craftingCurrency = false;
            craftedCurrency = true;
        }
    }

    protected override void TriggerStructurePrimaryFunction() {
        if(!craftedCurrency) {

            craftingCurrency = true;
            currencyCraftTimer = currencyCraftTime;
            OnCurrencyCraftingStarted?.Invoke(this, EventArgs.Empty);

        }
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;

        if(!craftedCurrency && !craftingCurrency) {

            payCurrencyUI.SetPlayerInteracting(true);

        } else {

            if (!craftedCurrency) return;
            // Ammo has not finished crafting

            StartCoroutine(CollectCurrencyFromCrafter(.2f));

        }
    }

    private IEnumerator CollectCurrencyFromCrafter(float delayBetweenAmmoInstantiation) {
        craftedCurrency = false;
        payCurrencyUI.ResetCurrencyPayment();
        OnPlayerCollectedCurrency?.Invoke(this, EventArgs.Empty);

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

    public float GetAmmoCraftTimerNormalized() {
        return 1 - (currencyCraftTimer / currencyCraftTime);
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeCrafted() {
        return currencyTypeCrafted;
    }
}
