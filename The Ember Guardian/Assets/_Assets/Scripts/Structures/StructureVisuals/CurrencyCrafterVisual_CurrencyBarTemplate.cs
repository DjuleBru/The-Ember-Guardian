using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyCrafterVisual_CurrencyBarTemplate : MonoBehaviour
{
    [SerializeField] private Image currencyBarFill;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Sprite craftingSprited;
    [SerializeField] private Sprite craftedSprited;

    public void SetFillAmount(float amount) {
        currencyBarFill.fillAmount = amount;
    }

    public void SetCrafted() {
        currencyBarFill.material = glowMaterial;

        if(craftedSprited != null) {
            currencyBarFill.sprite = craftedSprited;
        }
    }

    public void SetUncrafted() {
        if (craftingSprited != null) {
            currencyBarFill.sprite = craftingSprited;
        }
    }
}
