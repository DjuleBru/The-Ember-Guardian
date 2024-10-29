using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayOrbsUI_Fire : PayOrbsUI
{
    public override void CancelOrbPayment() {
        foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIList) {
            if (orbTemplateWorldUI.GetOrbPaid()) {
                orbTemplateWorldUI.SetOrbPaid(false);

                Collectible collectibleWorld = Instantiate(blueOrbPrefab, orbTemplateWorldUI.transform.position, Quaternion.identity).GetComponent<Collectible>();
                collectibleWorld.SetCollectibleUnInteractable(1f);
                collectibleWorld.ApplyRandomSidewardsForce(1, 5);
            }
            orbTemplateWorldUI.SetFillAmount(0);
        }

        fillPaymentOrbTimer = 0;
        orbIndex = 0;

        playerInteracting = false;
    }

    protected override void TryPayOrb(OrbTemplateWorldUI orbTemplateWorldUI) {
        if (PlayerCurrencies.Instance.GetCurrencyAmount(PlayerCurrencies.CurrencyType.blueOrb) >= 1) {

            Collectible collectibleWorld = Instantiate(blueOrbPrefab, orbTemplateWorldUI.transform.position, Quaternion.identity).GetComponent<Collectible>();
            collectibleWorld.SetCollectibleUnInteractable(1f);

            orbTemplateWorldUI.SetOrbPaid(true);
            PlayerCurrencies.Instance.ChangeCurrencyAmount(PlayerCurrencies.CurrencyType.blueOrb, -1);
            initialFillPaymentOrbRate += fillPaymentOrbRateIncrease;
            orbIndex++;
        }
        else {
            CancelOrbPayment();
            TriggerOnPayOrbsCanceled();
        }
    }
}
