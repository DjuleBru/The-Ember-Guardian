using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PayCurrencyTemplateWorldUI : MonoBehaviour
{

   [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeToPay;
   [SerializeField] private Image orbImageOutline;
    private Color initialImageOutlineColor;

    private bool currencyPaid;

    public event EventHandler OnCurrencyPaid;

    private void Awake() {
        initialImageOutlineColor = orbImageOutline.color;
    }

    public void SetCurrencyPaid(bool paid) {
        currencyPaid = paid;

        if (paid) {
            OnCurrencyPaid?.Invoke(this, EventArgs.Empty);
            orbImageOutline.color = Color.white;
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
}
