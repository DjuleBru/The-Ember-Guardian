using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCrafterVisual_AmmoBarTemplate : MonoBehaviour
{
    [SerializeField] private Image ammoBarFill;
    [SerializeField] private Material glowMaterial;

    public void SetFillAmount(float amount) {
        ammoBarFill.fillAmount = amount;
    }

    public void SetGlowMaterial() {
        ammoBarFill.material = glowMaterial;
    }
}
