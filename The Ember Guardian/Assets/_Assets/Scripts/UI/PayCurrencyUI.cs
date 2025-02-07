using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayCurrencyUI : MonoBehaviour
{
    [SerializeField] private bool currenciesFailedToPayFallInWater = false;

    protected List<PayCurrencyTemplateWorldUI> currencyTemplateWorldUIList = new List<PayCurrencyTemplateWorldUI>();
    protected int currencyIndex;

    protected bool playerInteracting;

    public event EventHandler OnCurrencyPaymentSuccess;
    public event EventHandler<OnSingleOrbFilledEventArgs> OnSingleCurrencyPaid;
    public static event EventHandler<OnSingleOrbFilledEventArgs> OnAnySingleCurrencyPaid;

    public class OnSingleOrbFilledEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
        public int currencyIndex;
    }

    protected void Update() {
        if (playerInteracting) {
                if (!playerInteracting) return;
                // Build was canceled due do lack of resources
        }
    }

    public virtual void ResetCurrencyPayment() {
        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in currencyTemplateWorldUIList) {
            orbTemplateWorldUI.SetCurrencyPaid(false);
        }

        currencyIndex = 0;

        playerInteracting = false;
    }

    public void SetPlayerInteracting(bool isInteracting) {
        if (playerInteracting == isInteracting) return;

        PlayerCurrencies.CurrencyType currencyTypeToPay = currencyTemplateWorldUIList[0].GetCurrencyTypeToPay();
        playerInteracting = isInteracting;
        currencyIndex = 0;

        if (!isInteracting) {
            foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in currencyTemplateWorldUIList) {
                orbTemplateWorldUI.SetCurrencyPaid(false);
            }

            PlayerCurrencies.Instance.CancelCurrencyPayment(currenciesFailedToPayFallInWater);
            UICurrencyManager.PlayerInventoryUI.SetPayingCurrency(this, currencyTypeToPay, false);

        } else {
            UICurrencyManager.PlayerInventoryUI.SetPayingCurrency(this, currencyTypeToPay, true);
        }
    }

    public void SetPlayerInteractingContinuous() {
        PlayerCurrencies.CurrencyType currencyTypeToPay = currencyTemplateWorldUIList[0].GetCurrencyTypeToPay();
        currencyIndex = 0;
        UICurrencyManager.PlayerInventoryUI.SetPayingCurrency(this, currencyTypeToPay, true);
    }

    public void SetOrbTemplateUIList(List<PayCurrencyTemplateWorldUI> orbTemplateList) {

        foreach (PayCurrencyTemplateWorldUI orbTemplate in currencyTemplateWorldUIList) {
            orbTemplate.OnCurrencyPaid -= OrbTemplate_OnOrbPaid;
        }
        currencyTemplateWorldUIList = orbTemplateList;

        foreach(PayCurrencyTemplateWorldUI orbTemplate in currencyTemplateWorldUIList) {
            orbTemplate.OnCurrencyPaid += OrbTemplate_OnOrbPaid;
        }
    }

    protected virtual void OrbTemplate_OnOrbPaid(object sender, EventArgs e) {
        // For sound
        OnAnySingleCurrencyPaid?.Invoke(this, new OnSingleOrbFilledEventArgs {
            currencyType = GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay(),
            currencyIndex = currencyIndex,
        });

        currencyIndex++;
        if (currencyIndex == currencyTemplateWorldUIList.Count) {

            PlayerCurrencies.Instance.FinalizeCurrencyPayment();
            OnCurrencyPaymentSuccess?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnSingleCurrencyPaid?.Invoke(this, new OnSingleOrbFilledEventArgs {
                currencyType = GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay(),
                currencyIndex = currencyIndex,
            });
        }
    }

    public bool GetCurrenciesFailedToPayFallInWater() {
        return currenciesFailedToPayFallInWater;
    }

    public bool GetIsLastCurrencyPaid() {
        return (currencyIndex+1) == currencyTemplateWorldUIList.Count;
    }

    public float GetCurrencyIndexNormalized() {
        return (float)currencyIndex / (float)currencyTemplateWorldUIList.Count;
    }

    public bool GetPlayerInteracting() {
        return playerInteracting;
    }

    public PayCurrencyTemplateWorldUI GetCurrentCurrencyTemplateWorldUI() {
        return currencyTemplateWorldUIList[currencyIndex];
    }

}
