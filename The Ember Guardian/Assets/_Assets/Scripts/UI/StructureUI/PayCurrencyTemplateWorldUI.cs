using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PayCurrencyTemplateWorldUI : MonoBehaviour
{

   [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeToPay;
   [SerializeField] private Image orbImageOutline;
   private Animator payCurrencyUIAnimator;

    private Color initialImageOutlineColor;

    private bool currencyPaid;

    public event EventHandler OnCurrencyPaid;

    private void Awake() {
        initialImageOutlineColor = orbImageOutline.color;
        payCurrencyUIAnimator = GetComponent<Animator>();
    }

    public void SetCurrencyPaid(bool paid) {
        currencyPaid = paid;

        if (paid) {
            OnCurrencyPaid?.Invoke(this, EventArgs.Empty);
            Color fullColor = initialImageOutlineColor;
            fullColor.a = 1f;
            orbImageOutline.color = fullColor;
        } else {
            orbImageOutline.color = initialImageOutlineColor;
        }
    }

    public bool GetCurrencyPaid() {
        return currencyPaid;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeToPay() {
        return currencyTypeToPay;
    }

    public void SetHovered(bool hovered) {
        if (hovered) {
            payCurrencyUIAnimator.SetTrigger("Hover");
            payCurrencyUIAnimator.ResetTrigger("Unhover");
        }
        else {
            payCurrencyUIAnimator.SetTrigger("Unhover");
            payCurrencyUIAnimator.ResetTrigger("Hover");
        }

    }
}
