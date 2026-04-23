using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyStorageVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] fillingSpriteList;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CurrencyStorage currencyStorage;
    [SerializeField] private GameObject currencyStoragePickupInstruction;

    private void Awake() {
        currencyStoragePickupInstruction.SetActive(false);
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
        fillAmountNormalized = Mathf.Clamp01(fillAmountNormalized);
        int spriteIndex = Mathf.FloorToInt(fillAmountNormalized * (fillingSpriteList.Length - 1));
        if(fillAmountNormalized != 0 && spriteIndex == 0) {
            spriteIndex = 1;
        }
        spriteRenderer.sprite = fillingSpriteList[spriteIndex];

        if(currencyStorage.GetCurrencyAmountStored() == 0) {
            currencyStoragePickupInstruction.SetActive(false);
        } else {
            currencyStoragePickupInstruction.SetActive(true);
        }
    }
}
