using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyCrafterVisual_CurrencyBarTemplate : MonoBehaviour
{
    [SerializeField] private Image currencyBarFill;
    [SerializeField] private Material glowMaterial;

    public void SetFillAmount(float amount) {
        currencyBarFill.fillAmount = amount;
    }

    public void SetGlowMaterial() {
        currencyBarFill.material = glowMaterial;
    }
}
