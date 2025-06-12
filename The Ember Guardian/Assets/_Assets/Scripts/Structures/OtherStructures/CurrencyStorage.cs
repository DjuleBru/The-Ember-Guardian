using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorage : Structure
{
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeStored;
    [SerializeField] private int maxCurrencyAmountStored = 15;
    private int currencyAmountStored;

    public event EventHandler OnCurrencyStored;
    public event EventHandler OnCurrencyRemoved;

    protected override void Awake() {
        base.Awake();
        RefreshInteractable();
        PlayerCamp.Instance.AddCurrencyStorage(this);
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();

        currencyAmountStored++;
        OnCurrencyStored?.Invoke(this, EventArgs.Empty);

        RefreshInteractable();

        if (GetHasCurrenciesToPay() && playerInteracting && currencyAmountStored < maxCurrencyAmountStored) {
            payCurrencyUI.SetPlayerInteractingContinuous(); // Continue l'interaction
        }
        else {
            payCurrencyUI.SetPlayerInteracting(false);
        }
    }
    private bool GetHasCurrenciesToPay() {
        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(currencyTypeStored).Count > 0) {
            return true; // Continue à payer
        }
        return false;
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
        Debug.Log("currencyAmountStored " + currencyAmountStored);
        if(currencyAmountStored < maxCurrencyAmountStored) {
            ActivateStructurePrimaryFunctionInteraction(true);
        } else {
            ActivateStructurePrimaryFunctionInteraction(false);
        }
    }

    public float GetCurrencyStoredAmountNormalized() {
        return (float)currencyAmountStored/ (float)maxCurrencyAmountStored;
    }
}
