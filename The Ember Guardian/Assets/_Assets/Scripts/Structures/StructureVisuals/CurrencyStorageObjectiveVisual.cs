using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageObjectiveVisual : MonoBehaviour
{
    private CurrencyStorage_Objective currencyStorageObj;
    private Animator currencyStorageAnimator;

    private void Awake() {
        currencyStorageObj = GetComponentInParent<CurrencyStorage_Objective>();
        currencyStorageAnimator = GetComponent<Animator>();

        currencyStorageObj.OnCurrencyStored += CurrencyStorageObj_OnCurrencyStored;

    }

    private void CurrencyStorageObj_OnCurrencyStored(object sender, CurrencyStorage.OnAnyCurrencyStoredEventArgs e) {
        if (!e.triggerSFX) return;
        currencyStorageAnimator.SetTrigger("ActivateShrine");
    }
}
