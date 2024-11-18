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

    private bool orbPaid;

    public event EventHandler OnOrbPaid;

    private void Awake() {
        initialImageOutlineColor = orbImageOutline.color;
    }

    public void SetCurrencyPaid(bool paid) {

        if(paid) {
            OnOrbPaid?.Invoke(this, EventArgs.Empty);
            orbImageOutline.color = Color.white;
        } else {
            orbImageOutline.color = initialImageOutlineColor;
        }
        orbPaid = paid;
    }

    public bool GetCurrencyPaid() {
        return orbPaid;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeToPay() {
        return currencyTypeToPay;
    }
}
