using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayOrbsUI : MonoBehaviour
{

    [SerializeField] protected Transform blueOrbPrefab;

    [SerializeField] protected float fillPaymentOrbTime = 1f;
    [SerializeField] protected float initialPaymentOrbSpeed = 1f;
    [SerializeField] protected float fillPaymentOrbRateIncrease = .3f;

    protected List<OrbTemplateWorldUI> orbTemplateWorldUIList = new List<OrbTemplateWorldUI>();
    protected float paymentOrbSpeed;
    protected int orbIndex;

    protected bool playerInteracting;

    public event EventHandler OnOrbPaymentSuccess;
    public event EventHandler<OnSingleOrbFilledEventArgs> OnSingleOrbPaid;
    public static event EventHandler<OnSingleOrbFilledEventArgs> OnAnySingleOrbPaid;

    public class OnSingleOrbFilledEventArgs : EventArgs {
        public int orbIndex;
    }

    protected void Update() {
        if (playerInteracting) {

                if (!playerInteracting) return;
                // Build was canceled due do lack of resources

                
        }
    }

    public virtual void CancelOrbPayment() {
        foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIList) {
            if (orbTemplateWorldUI.GetOrbPaid()) {
                orbTemplateWorldUI.SetOrbPaid(false);
            }
            orbTemplateWorldUI.SetFillAmount(0);
        }

        orbIndex = 0;

        playerInteracting = false;
    }

    public void SetPlayerInteracting(bool isInteracting) {

        if (playerInteracting == isInteracting) return;

        Debug.Log("SetPlayerInteracting " + isInteracting);

        playerInteracting = isInteracting;
        orbIndex = 0;
        paymentOrbSpeed = initialPaymentOrbSpeed;

        if (!isInteracting) {
            foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIList) {
                orbTemplateWorldUI.SetOrbPaid(false);
            }

            PlayerCurrencies.Instance.CancelCurrencyPayment();
            UICurrencyManager.Instance.SetPayingOrbs(this, false);

        } else {
            UICurrencyManager.Instance.SetPayingOrbs(this, true);
        }
    }

    public void SetOrbTemplateUIList(List<OrbTemplateWorldUI> orbTemplateList) {
        foreach (OrbTemplateWorldUI orbTemplate in orbTemplateWorldUIList) {
            orbTemplate.OnOrbPaid -= OrbTemplate_OnOrbPaid;
        }

        orbTemplateWorldUIList = orbTemplateList;

        foreach(OrbTemplateWorldUI orbTemplate in orbTemplateWorldUIList) {
            orbTemplate.OnOrbPaid += OrbTemplate_OnOrbPaid;
        }
    }

    protected virtual void OrbTemplate_OnOrbPaid(object sender, EventArgs e) {
        // For sound
        OnAnySingleOrbPaid?.Invoke(this, new OnSingleOrbFilledEventArgs {
            orbIndex = orbIndex,
        });

        orbIndex++;
        if (orbIndex == orbTemplateWorldUIList.Count) {

            playerInteracting = false;
            PlayerCurrencies.Instance.FinalizeCurrencyPayment();

            OnOrbPaymentSuccess?.Invoke(this, EventArgs.Empty);

        }
        else {
            OnSingleOrbPaid?.Invoke(this, new OnSingleOrbFilledEventArgs {
                orbIndex = orbIndex,
            });
        }
    }

    public OrbTemplateWorldUI GetCurrentOrbTemplateWorldUI() {
        return orbTemplateWorldUIList[orbIndex];
    }

}
