using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayOrbsUI : MonoBehaviour
{

    [SerializeField] protected Transform blueOrbPrefab;

    [SerializeField] protected float fillPaymentOrbTime = 1f;
    [SerializeField] protected float initialFillPaymentOrbRate = 1f;
    [SerializeField] protected float fillPaymentOrbRateIncrease = .3f;

    protected List<OrbTemplateWorldUI> orbTemplateWorldUIList;
    protected float fillPaymentOrbRate;
    protected int orbIndex;
    protected float fillPaymentOrbTimer;

    protected bool playerInteracting;

    public event EventHandler OnOrbPaymentCanceled;
    public event EventHandler OnOrbPaymentSuccess;

    protected void Update() {
        if (playerInteracting) {
            fillPaymentOrbTimer += Time.deltaTime * fillPaymentOrbRate;

            orbTemplateWorldUIList[orbIndex].SetFillAmount(fillPaymentOrbTimer / fillPaymentOrbTime);

            if (fillPaymentOrbTimer > fillPaymentOrbTime) {
                fillPaymentOrbTimer = 0;

                TryPayOrb(orbTemplateWorldUIList[orbIndex]);

                if (!playerInteracting) return;
                // Build was canceled due do lack of resources

                if (orbIndex == orbTemplateWorldUIList.Count) {
                    OnOrbPaymentSuccess?.Invoke(this, EventArgs.Empty);
                    playerInteracting = false;
                }
            }
        }
    }

    public virtual void CancelOrbPayment() {
        foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIList) {
            if (orbTemplateWorldUI.GetOrbPaid()) {
                orbTemplateWorldUI.SetOrbPaid(false);

                Collectible collectibleWorld = Instantiate(blueOrbPrefab, orbTemplateWorldUI.transform.position, Quaternion.identity).GetComponent<Collectible>();
                collectibleWorld.SetCollectibleUnInteractable(1f);
                collectibleWorld.ApplyRandomUpwardsForce(1, 5);
            }
            orbTemplateWorldUI.SetFillAmount(0);
        }

        fillPaymentOrbTimer = 0;
        orbIndex = 0;

        playerInteracting = false;
    }

    protected virtual void TryPayOrb(OrbTemplateWorldUI orbTemplateWorldUI) {
        if (PlayerCurrencies.Instance.GetCurrencyAmount(PlayerCurrencies.CurrencyType.blueOrb) >= 1) {
            orbTemplateWorldUI.SetOrbPaid(true);
            PlayerCurrencies.Instance.ChangeCurrencyAmount(PlayerCurrencies.CurrencyType.blueOrb, -1);
            fillPaymentOrbRate += fillPaymentOrbRateIncrease;
            orbIndex++;
        }
        else {
            CancelOrbPayment();
            OnOrbPaymentCanceled?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetPlayerInteracting(bool isInteracting) {
        playerInteracting = isInteracting;
        orbIndex = 0;
        fillPaymentOrbTimer = 0;
        fillPaymentOrbRate = initialFillPaymentOrbRate;

        if (!isInteracting) {
            foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIList) {
                orbTemplateWorldUI.SetOrbPaid(false);
                orbTemplateWorldUI.SetFillAmount(0);
            }
        }
    }

    public void SetOrbTemplateUIList(List<OrbTemplateWorldUI> orbTemplateList) {
        orbTemplateWorldUIList = orbTemplateList;
    }

    protected void TriggerOnPayOrbsCanceled() {
        OnOrbPaymentCanceled?.Invoke(this, EventArgs.Empty);
    }

}
