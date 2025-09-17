using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageObjectiveSounds : CurrencyStorageSounds
{
    private CurrencyStorage_Objective currencyStorageObjective;
    [SerializeField] private AudioClip maxCurrencyAmountReachedAucioClipWoosh;
    [SerializeField] private AudioClip maxCurrencyAmountReachedAucioClip;

    protected override void Awake() {
        base.Awake();
        currencyStorageObjective = currencyStorage as CurrencyStorage_Objective;
        currencyStorageObjective.OnMaxCurrencyAmountReached += CurrencyStorageObjective_OnMaxCurrencyAmountReached;
    }

    private void CurrencyStorageObjective_OnMaxCurrencyAmountReached(object sender, System.EventArgs e) {
        PlaySound2D(maxCurrencyAmountReachedAucioClipWoosh, 2f);
        PlaySFXAfterDelay(maxCurrencyAmountReachedAucioClip, .5f, 2f);
    }
}
