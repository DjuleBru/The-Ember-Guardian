using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorage_Objective : CurrencyStorage
{

    [SerializeField] private List<int> maxCurrencyStorageList;
    [SerializeField] private List<float> difficultyReductionFactorsList;
    private int maxCurrencyStorageIndex;

    public event EventHandler OnMaxCurrencyAmountReached;
    public static event EventHandler OnAnyMaxCurrencyAmountReached;
    public event EventHandler OnMaxCurrencyAmountChanged;
    public event EventHandler OnMaxCurrencyStorageIndexLoaded;

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
            ActivateWaveDifficultyReductionEffect(maxCurrencyStorageIndex);

            if ((maxCurrencyStorageIndex+1) < maxCurrencyStorageList.Count) {
                RefreshMaxCurrencyAmountStored();
                currencyAmountStored = 0;
            }

            RefreshInteractable();
        }

    }

    private void ActivateWaveDifficultyReductionEffect(int maxCurrencyStorageIndex) {
        CreaturesSpawnManager.Instance.ApplyPermanentShockwaveEffect(difficultyReductionFactorsList[maxCurrencyStorageIndex]);
        DayNightVisualsManager.Instance.RefreshSunColorBasedOnDifficulty(.75f);
        DayNightVisualsManager.Instance.RefreshMoonColorBasedOnDifficulty(.75f);
    }

    private void RefreshMaxCurrencyAmountStored() {
        maxCurrencyStorageIndex++;
        maxCurrencyAmountStored = maxCurrencyStorageList[maxCurrencyStorageIndex];
        OnMaxCurrencyAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetMaxCurrencyStorageIndex() {
        return maxCurrencyStorageIndex;
    }
    public void SetMaxCurrencyStorageIndex(int maxCurrencyStorageIndex) {
        this.maxCurrencyStorageIndex = maxCurrencyStorageIndex;
        maxCurrencyAmountStored = maxCurrencyStorageList[maxCurrencyStorageIndex];
        OnMaxCurrencyStorageIndexLoaded?.Invoke(this, EventArgs.Empty);

        for(int i = 0; i < maxCurrencyStorageIndex; i++) {
            ActivateWaveDifficultyReductionEffect(i);
        }
    }

    protected override void GameInput_OnCurrencyCollectedFromContainer(object sender, EventArgs e) {
    
    }
}
