using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerCurrencies : MonoBehaviour {

    [SerializeField] private Transform currencySpawnPosition;
    private Worker worker;
    private List<Collectible> collectiblesBeingPaid = new List<Collectible>();
    private Collectible lastCurrencyPaying;

    private bool payingCurrencyJustCanceled;
    private PayCurrencyUI currentPayCurrencyUI;

    public event EventHandler OnPaymentFinalized;
    public event EventHandler<OnCurrencyPaidEventArgs> OnCurrencyPaid;

    public class OnCurrencyPaidEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    private void Awake() {
        worker = GetComponent<Worker>();
    }

    public void SetPayingCurrency(PayCurrencyUI payOrbsUI, PlayerCurrencies.CurrencyType currencyTypeToPay, bool payingCurrency) {
        //Debug.Log("SetPayingCurrency " + payOrbsUI + " payingCurrency " + payingCurrency + " currencyTypeToPay " + currencyTypeToPay);
        if (currentPayCurrencyUI != null) {
            currentPayCurrencyUI.OnSingleCurrencyPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayCurrencyUI.OnCurrencyPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
        }

        if (payingCurrency) {
            currentPayCurrencyUI = payOrbsUI;
            currentPayCurrencyUI.OnSingleCurrencyPaid += CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayCurrencyUI.OnCurrencyPaymentSuccess += CurrentPayOrbsUI_OnOrbPaymentSuccess;
            PayNextCurrency(currencyTypeToPay);
        }

        payingCurrencyJustCanceled = !payingCurrency;
    }

    private void CurrentPayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        currentPayCurrencyUI.OnSingleCurrencyPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
        currentPayCurrencyUI.OnCurrencyPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
    }

    private void CurrentPayOrbsUI_OnSingleOrbPaid(object sender, PayCurrencyUI.OnSingleOrbFilledEventArgs e) {
        if (currentPayCurrencyUI.GetPlayerInteracting() || currentPayCurrencyUI.GetWorkerCurrenciesInteracting() != this) return;
        PlayerCurrencies.CurrencyType nextCurrencyTypeToPay = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay();

        PayNextCurrency(nextCurrencyTypeToPay);
    }

    private void PayNextCurrency(PlayerCurrencies.CurrencyType currencyTypeToPay) {
        //Debug.Log("PayNextCurrency " + currencyTypeToPay);
        PayCurrencyTemplateWorldUI currencyTemplateUI = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI();
        float currencyIndexNormalized = currentPayCurrencyUI.GetCurrencyIndexNormalized();

        int currencyAmount = worker.GetCurrencyAmount(currencyTypeToPay);
        //Debug.Log("currencyTypeToPay " + currencyTypeToPay + " currencyAmount " + currencyAmount);

        if (currencyAmount > 0) {
            StartPayingCurrency(currencyTypeToPay, currencyTemplateUI, currencyIndexNormalized);
        }
        else {
            currentPayCurrencyUI.ResetCurrencyPayment();
            CancelCurrencyPayment(currentPayCurrencyUI.GetCurrenciesFailedToPayFallInWater());
        }
    }

    private void StartPayingCurrency(PlayerCurrencies.CurrencyType currencyType, PayCurrencyTemplateWorldUI destination, float currencyIndexNormalized) {
        //Debug.Log("StartPayingCurrency " + currencyType + " destination " + currencyIndexNormalized);

        OnCurrencyPaid?.Invoke(this, new OnCurrencyPaidEventArgs {
            currencyType = currencyType
        });

        float smoothTime = destination.GetInitialPayCurrencySmoothTime() * (currencyIndexNormalized) + destination.GetInitialPayCurrencySmoothTime();

        Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(currencyType);

        lastCurrencyPaying = Instantiate(currencyPrefab, currencySpawnPosition.transform.position, Quaternion.identity).GetComponent<Collectible>();
        lastCurrencyPaying.SetMovingForPayment(true, smoothTime, destination.transform);

        collectiblesBeingPaid.Add(lastCurrencyPaying);
    }

    public void FinalizeCurrencyPayment() {
        foreach (Collectible collectible in collectiblesBeingPaid) {
            Destroy(collectible.gameObject);
        }
        collectiblesBeingPaid.Clear();

        OnPaymentFinalized?.Invoke(this, EventArgs.Empty);
    }

    public void CancelCurrencyPayment(bool collectiblesFallInWater) {
        foreach (Collectible collectible in collectiblesBeingPaid) {
            collectible.SetMovingForPayment(false);
            collectible.ApplyRandomUpwardsForce(1, 5);

            if (collectiblesFallInWater) {
                collectible.SetCollectibleFellFromBag();
            }

        }

        collectiblesBeingPaid.Clear();
    }

}
