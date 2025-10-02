using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageSounds : StructureSounds
{
    [SerializeField] protected AudioClip[] addCurrencyAudioClips;
    [SerializeField] protected AudioClip[] removeCurrencyAudioClips;

    [SerializeField] protected CurrencyStorage currencyStorage;

    protected override void Start() {
        base.Start();
        currencyStorage.OnCurrencyStored += CurrencyStorage_OnCurrencyStored;
        currencyStorage.OnCurrencyRemoved += CurrencyStorage_OnCurrencyRemoved;
    }

    protected void CurrencyStorage_OnCurrencyRemoved(object sender, System.EventArgs e) {
        PlaySound2D(removeCurrencyAudioClips, 2f);
    }

    protected void CurrencyStorage_OnCurrencyStored(object sender, CurrencyStorage.OnAnyCurrencyStoredEventArgs e) {
        if (!e.triggerSFX) return;
        PlaySound2D(addCurrencyAudioClips, 2f);

    }
}
