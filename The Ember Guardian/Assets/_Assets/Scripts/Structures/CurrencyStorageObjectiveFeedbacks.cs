using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageObjectiveFeedbacks : MonoBehaviour
{
    private CurrencyStorage_Objective currencyStorageObj;

    [SerializeField] private MMF_Player maxStorageAmountReachedFeedbacks;
    [SerializeField] private ParticleSystem addedOrbPS;
    [SerializeField] private GameObject shockwaveGO;

    private void Awake() {
        currencyStorageObj = GetComponentInParent<CurrencyStorage_Objective>();
        currencyStorageObj.OnMaxCurrencyAmountReached += CurrencyStorageObj_OnMaxCurrencyAmountReached;
        currencyStorageObj.OnCurrencyStored += CurrencyStorageObj_OnCurrencyStored;
        shockwaveGO.SetActive(false);
    }

    private void CurrencyStorageObj_OnCurrencyStored(object sender, System.EventArgs e) {
        addedOrbPS.Play();
    }

    private void CurrencyStorageObj_OnMaxCurrencyAmountReached(object sender, System.EventArgs e) {
        maxStorageAmountReachedFeedbacks.PlayFeedbacks();
        shockwaveGO.SetActive(true);
    }
}
