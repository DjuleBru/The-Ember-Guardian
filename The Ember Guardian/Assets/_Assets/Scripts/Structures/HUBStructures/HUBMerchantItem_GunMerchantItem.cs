using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBMerchantItem_GunMerchantItem : HubMerchantItem
{
    public enum GunItemType {
        bulletDamage,
        critChance,
        maxAmmo,
        shotsPerClip,
        reloadTime,
        cooldownTime,
        laserSight,
        holdBreath,
        rifle,
        shotgun,
        sniper,
        smg,
    }

    public enum GunItemCategory {
        newGun,
        gunAbility,
        gunModule,
        statIncrease,
    }

    [SerializeField] private GunItemType gunItem;
    [SerializeField] private GunItemCategory gunItemCategory;
    [SerializeField] private GunSO linkedGunSO;
    [SerializeField] private GunStatModifierSO linkedStatModifierSO;

    private List<string> statValues = new List<string>();
    private List<bool> statModifiedBools = new List<bool>();
    
    protected void Awake() {
        RefreshStatValues();

        if(gunItemCategory == GunItemCategory.statIncrease) {
            maxItemLevel = linkedStatModifierSO.maxStatModifierLevel;
            greenGemCostList = linkedStatModifierSO.greenGemCostList;
            redGemCostList = linkedStatModifierSO.redGemCostList;
        }

        OnAnyHubMerchantItemBought += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBought;
    }

    private void HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBought(object sender, EventArgs e) {
        HubMerchantItem merchantItem = (HubMerchantItem)sender;

        if(merchantItem is HUBMerchantItem_GunMerchantItem) {
            HUBMerchantItem_GunMerchantItem gunItem = (HUBMerchantItem_GunMerchantItem)merchantItem;

            if(gunItem == this) {
                Debug.Log(linkedGunSO + " " + gunItem + " RefreshStatValues");
                RefreshStatValues();
                InvokeItemMustRefreshDescriptionCard();
                return;
            }

            if(gunItemCategory == GunItemCategory.newGun) {

                if (gunItem.GetGunItemCategory() == GunItemCategory.statIncrease && gunItem.GetLinkedGunSO() == linkedGunSO) {
                    Debug.Log(linkedGunSO + " " + gunItem + " RefreshStatValues");
                    RefreshStatValues();
                    InvokeItemMustRefreshDescriptionCard();
                }

            }

        }
    }

    public override void BuyItem() {
        if (gunItem == GunItemType.bulletDamage) {
            int modifiedDamage = linkedGunSO.damagePerBullet + linkedStatModifierSO.intStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunDamagePerBullet(linkedGunSO, modifiedDamage);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            int modifiedShotsPerClip = linkedGunSO.shotsPerClip + linkedStatModifierSO.intStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunShotsPerClip(linkedGunSO, modifiedShotsPerClip);
        }

        if (gunItem == GunItemType.maxAmmo) {
            int modifiedMaxAmmo = linkedGunSO.maxAmmo + linkedStatModifierSO.intStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunMaxAmmo(linkedGunSO, modifiedMaxAmmo);
        }

        if (gunItem == GunItemType.cooldownTime) {
            float modifiedCooldown = linkedGunSO.shootCooldownTime + linkedStatModifierSO.floatStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunCooldown(linkedGunSO, modifiedCooldown);
        }

        if (gunItem == GunItemType.reloadTime) {
            float modifiedReloadTime = linkedGunSO.reloadTime + linkedStatModifierSO.floatStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunReloadTime(linkedGunSO, modifiedReloadTime);
        }

        if (gunItem == GunItemType.critChance) {
            float modifiedCritChange = linkedGunSO.critChance + linkedStatModifierSO.floatStatModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunCritChance(linkedGunSO, modifiedCritChange);
        }

        base.BuyItem();
    }

    public override string GetItemType() {
        return gunItem.ToString() + " " + linkedGunSO.ToString();
    }

    private void RefreshStatValues() {
        if (gunItem == GunItemType.rifle) {

            // DAMAGE
            int initialDamagePerBullet = linkedGunSO.damagePerBullet;
            int modifiedDamagePerBuller = MetaProgressionManager.Instance.GetGunDamagePerBullet(linkedGunSO);

            Debug.Log("initialDamagePerBullet " + initialDamagePerBullet);
            Debug.Log("modifiedDamagePerBuller " + modifiedDamagePerBuller);

            if(initialDamagePerBullet != modifiedDamagePerBuller) {
                statModifiedBools.Add(true);
            } else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedDamagePerBuller.ToString());

            // CRIT CHANCE
            float initialCritChance = linkedGunSO.critChance;
            float modifierCritChance = MetaProgressionManager.Instance.GetGunCritChance(linkedGunSO);
            if (initialCritChance != modifierCritChance) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifierCritChance * 100 + "%");

            // SHOTS PER CLIP
            int initialShotsPerClip = linkedGunSO.shotsPerClip;
            int modifiedShotsPerClip = MetaProgressionManager.Instance.GetGunShotsPerClip(linkedGunSO);
            if (initialShotsPerClip != modifiedShotsPerClip) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedShotsPerClip.ToString());

            // MAX AMMO
            int initialMaxAmmo = linkedGunSO.maxAmmo;
            int modifiedMaxAmmo = MetaProgressionManager.Instance.GetGunMaxAmmo(linkedGunSO);
            if (initialMaxAmmo != modifiedMaxAmmo) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedMaxAmmo.ToString());

            // COOLDOWN
            float initialCooldown = linkedGunSO.shootCooldownTime;
            float modifierCooldown = MetaProgressionManager.Instance.GetGunCooldown(linkedGunSO);
            if (initialCooldown != modifierCooldown) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifierCooldown + "s");

            // RELOAD TIME
            float initialReloadTime = linkedGunSO.reloadTime;
            float modifiedReloadTime = MetaProgressionManager.Instance.GetGunReloadTime(linkedGunSO);
            if (initialReloadTime != modifiedReloadTime) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedReloadTime + "s");
        }
    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        if(gunItem == GunItemType.rifle) {
            statDescriptionList.Add("Bullet damage ");
            statDescriptionList.Add("Crit chance ");
            statDescriptionList.Add("Shots per clip ");
            statDescriptionList.Add("Max ammo clips ");
            statDescriptionList.Add("Cooldown ");
            statDescriptionList.Add("Reload time ");
        }

        return statDescriptionList;
    }

    public override List<string> GetStatValues() {
        return statValues;
    }

    public override List<bool> GetStatModifierBools() {
        return statModifiedBools;
    }

    public GunItemCategory GetGunItemCategory() {
        return gunItemCategory;
    }
    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }

}
