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
   [SerializeField] private Animator payCurrencyUIAnimator;
    [SerializeField] private float initialPayCurrencySmoothTime = 4f;

    [SerializeField] private bool setInitialImageColorManual;
    [SerializeField] private Color initialImageOutlineColor_ManualSet;
    private Color initialImageOutlineColor;

    private bool currencyPaid;

    public event EventHandler OnCurrencyPaid;

    private void Awake() {
        if (setInitialImageColorManual) {
            initialImageOutlineColor = initialImageOutlineColor_ManualSet;
        } else {
            initialImageOutlineColor = orbImageOutline.color;
        }
    }

    public void SetCurrencyPaid(bool paid) {
        currencyPaid = paid;
        SetHovered(paid);

        if (paid) {
            Color fullColor = initialImageOutlineColor;
            fullColor.a = 1f;
            orbImageOutline.color = fullColor;
            OnCurrencyPaid?.Invoke(this, EventArgs.Empty);
        } else {

            if (setInitialImageColorManual) {
                orbImageOutline.color = initialImageOutlineColor_ManualSet;
            }
            else {
                orbImageOutline.color = initialImageOutlineColor;
            }
        }
    }

    public bool GetCurrencyPaid() {
        return currencyPaid;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeToPay() {
        return currencyTypeToPay;
    }

    public float GetInitialPayCurrencySmoothTime() {
        return initialPayCurrencySmoothTime;
    }

    public void SetHovered(bool hovered) {
        if(payCurrencyUIAnimator == null) {
            Debug.Log("payCurrencyUIAnimator is null !");
            return;
        }

        if (hovered) {
            payCurrencyUIAnimator.SetTrigger("Hover");
            payCurrencyUIAnimator.ResetTrigger("Unhover");
        }
        else {
            payCurrencyUIAnimator.SetTrigger("Unhover");
            payCurrencyUIAnimator.ResetTrigger("Hover");
        }
    }

    public void SetCurrencyTypeToPay(PlayerCurrencies.CurrencyType currencyTypeToPay) {
        this.currencyTypeToPay = currencyTypeToPay;
    }

}
