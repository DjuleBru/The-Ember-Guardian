using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageSounds : StructureSounds
{
    [SerializeField] private AudioClip[] addCurrencyAudioClips;
    [SerializeField] private AudioClip[] removeCurrencyAudioClips;

    [SerializeField] private CurrencyStorage currencyStorage;

    protected override void Start() {
        base.Start();
        currencyStorage.OnCurrencyStored += CurrencyStorage_OnCurrencyStored;
        currencyStorage.OnCurrencyRemoved += CurrencyStorage_OnCurrencyRemoved;
    }

    private void CurrencyStorage_OnCurrencyRemoved(object sender, System.EventArgs e) {
        PlaySound2D(removeCurrencyAudioClips, 2f);
    }

    private void CurrencyStorage_OnCurrencyStored(object sender, System.EventArgs e) {
        PlaySound2D(addCurrencyAudioClips, 2f);

    }
}
