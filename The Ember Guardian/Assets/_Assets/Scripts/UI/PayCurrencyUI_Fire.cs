using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayCurrencyUI_Fire : PayCurrencyUI
{
    public override void ResetCurrencyPayment() {
        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in currencyTemplateWorldUIList) {
            if (orbTemplateWorldUI.GetCurrencyPaid()) {
                orbTemplateWorldUI.SetCurrencyPaid(false);
            }
        }

        currencyIndex = 0;

        playerInteracting = false;
    }

    protected override void OrbTemplate_OnOrbPaid(object sender, EventArgs e) {
        base.OrbTemplate_OnOrbPaid(sender, e);

        PayCurrencyTemplateWorldUI payCurrencyTemplateWorldUI = sender as PayCurrencyTemplateWorldUI;
        payCurrencyTemplateWorldUI.SetCurrencyPaid(false);

        currencyIndex = 0;

        Collectible collectibleWorld = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.bigBlueOrb), (sender as MonoBehaviour).transform.position, Quaternion.identity).GetComponent<Collectible>();
        collectibleWorld.SetCollectibleUnInteractable(1f);
    }
}
