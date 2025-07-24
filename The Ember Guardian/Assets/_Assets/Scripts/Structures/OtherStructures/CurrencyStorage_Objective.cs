using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorage_Objective : CurrencyStorage
{

    [SerializeField] private List<int> maxCurrencyStorageList;
    private int maxCurrencyStorageIndex;

    public event EventHandler OnMaxCurrencyAmountReached;
    public static event EventHandler OnAnyMaxCurrencyAmountReached;
    public event EventHandler OnMaxCurrencyAmountChanged;

    protected override void Awake() {
        base.Awake();
        maxCurrencyAmountStored = maxCurrencyStorageList[0];

        engineersCanPickUpOrbs = false;
    }
    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();

        if(currencyAmountStored == maxCurrencyAmountStored) {
            OnMaxCurrencyAmountReached?.Invoke(this, EventArgs.Empty);
            OnAnyMaxCurrencyAmountReached?.Invoke(this, EventArgs.Empty);

            if ((maxCurrencyStorageIndex+1) < maxCurrencyStorageList.Count) {
                RefreshMaxCurrencyAmountStored();
                currencyAmountStored = 0;
            }

            RefreshInteractable();
        }

    }

    private void RefreshMaxCurrencyAmountStored() {
        maxCurrencyStorageIndex++;
        maxCurrencyAmountStored = maxCurrencyStorageList[maxCurrencyStorageIndex];
        OnMaxCurrencyAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void GameInput_OnCurrencyCollectedFromContainer(object sender, EventArgs e) {
    
    }
}
