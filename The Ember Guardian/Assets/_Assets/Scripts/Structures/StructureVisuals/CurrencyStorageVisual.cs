using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] fillingSpriteList;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CurrencyStorage currencyStorage;

    private void Awake() {
        currencyStorage.OnCurrencyStored += CurrencyStorage_OnCurrencyStored;
        currencyStorage.OnCurrencyRemoved += CurrencyStorage_OnCurrencyRemoved;
    }

    private void CurrencyStorage_OnCurrencyRemoved(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void CurrencyStorage_OnCurrencyStored(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void RefreshVisual() {
        float fillAmountNormalized = currencyStorage.GetCurrencyStoredAmountNormalized();

        int spriteIndex = Mathf.FloorToInt(fillAmountNormalized * (fillingSpriteList.Length - 1));
        spriteRenderer.sprite = fillingSpriteList[spriteIndex];
    }
}
