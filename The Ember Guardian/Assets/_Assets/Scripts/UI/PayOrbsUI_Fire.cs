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

        //paymentOrbTimer = 0;
        orbIndex = 0;

        playerInteracting = false;
    }

    protected override void OrbTemplate_OnOrbPaid(object sender, EventArgs e) {
        base.OrbTemplate_OnOrbPaid(sender, e);

        Collectible collectibleWorld = Instantiate(blueOrbPrefab, (sender as MonoBehaviour).transform.position, Quaternion.identity).GetComponent<Collectible>();
        collectibleWorld.SetCollectibleUnInteractable(1f);
    }
}
