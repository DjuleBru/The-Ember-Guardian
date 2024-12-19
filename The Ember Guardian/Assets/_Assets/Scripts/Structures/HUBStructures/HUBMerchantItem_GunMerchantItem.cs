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
        shootConeAngle,
        pelletsPerBullet,
        aimSight,
        overclock,
        focusedBlast,
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
            maxItemLevel = linkedStatModifierSO.statModifierList.Count;
            greenGemCostList = linkedStatModifierSO.greenGemCostList;
            redGemCostList = linkedStatModifierSO.redGemCostList;
        }

        OnAnyHubMerchantItemBought += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
    }

    private void HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped(object sender, EventArgs e) {
        HubMerchantItem merchantItem = (HubMerchantItem)sender;

        if (merchantItem is HUBMerchantItem_GunMerchantItem) {
            HUBMerchantItem_GunMerchantItem gunItem = (HUBMerchantItem_GunMerchantItem)merchantItem;

            if(gunItem.GetGunItemCategory() == GunItemCategory.newGun && gunItemCategory == GunItemCategory.newGun && gunItem.GetLinkedGunSO() != linkedGunSO) {
                UnequipItem();
            }
        }
    }

    private void HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded(object sender, EventArgs e) {
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
                    Debug.Log("new gun " + linkedGunSO + " " + gunItem + " RefreshStatValues");
                    RefreshStatValues();
                    InvokeItemMustRefreshDescriptionCard();
                }

            }

        }
    }

    public override void BuyItem() {
        if(gunItemCategory == GunItemCategory.statIncrease) {
            SetNewGunStats();
        }

        if (gunItemCategory == GunItemCategory.newGun) {
            EquipOrUnequipItem();
        }

        if (gunItemCategory == GunItemCategory.gunAbility) {
            UnlockGunAbility();
        }

        if (gunItemCategory == GunItemCategory.gunModule) {
            EquipOrUnequipItem();
            UnlockGunModule();
        }

        base.BuyItem();

    }

    public override void UpgradeItem() {
        if (gunItemCategory == GunItemCategory.statIncrease) {
            SetNewGunStats();
        }

        base.UpgradeItem();
    }

    private void SetNewGunStats() {
        if (gunItem == GunItemType.bulletDamage) {
            float modifiedDamage = linkedGunSO.damagePerBullet + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunDamagePerBullet(linkedGunSO, modifiedDamage);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            float modifiedShotsPerClip = linkedGunSO.shotsPerClip + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunShotsPerClip(linkedGunSO, modifiedShotsPerClip);
        }

        if (gunItem == GunItemType.maxAmmo) {
            float modifiedMaxAmmo = linkedGunSO.maxAmmo + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunMaxAmmo(linkedGunSO, modifiedMaxAmmo);
        }

        if (gunItem == GunItemType.cooldownTime) {
            float modifiedCooldown = linkedGunSO.shootCooldownTime + linkedGunSO.shootCooldownTime * linkedStatModifierSO.statModifierList[itemLevel] * 0.01f;
            MetaProgressionManager.Instance.SetGunCooldown(linkedGunSO, modifiedCooldown);
        }

        if (gunItem == GunItemType.reloadTime) {
            float modifiedReloadTime = linkedGunSO.reloadTime + linkedGunSO.reloadTime * linkedStatModifierSO.statModifierList[itemLevel] * 0.01f;
            MetaProgressionManager.Instance.SetGunReloadTime(linkedGunSO, modifiedReloadTime);
        }

        if (gunItem == GunItemType.critChance) {
            float modifiedCritChange = linkedGunSO.critChance + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunCritChance(linkedGunSO, modifiedCritChange);
        }

        if (gunItem == GunItemType.shootConeAngle) {
            float modifiedShootAngle = linkedGunSO.shootConeAngle + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunShootConeAnle(linkedGunSO, modifiedShootAngle);
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            float modifiedPelletsPerBullet = linkedGunSO.pelletsPerBullet + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunPelletsPerBullet(linkedGunSO, modifiedPelletsPerBullet);
        }
    }

    public override string GetItemType() {
        return gunItem.ToString() + " " + linkedGunSO.ToString();
    }

    private void RefreshStatValues() {
        itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
        statModifiedBools.Clear();
        statValues.Clear();

        if (gunItemCategory == GunItemCategory.newGun) {

            // DAMAGE
            int initialDamagePerBullet = linkedGunSO.damagePerBullet;
            int modifiedDamagePerBuller = MetaProgressionManager.Instance.GetGunDamagePerBullet(linkedGunSO);

            if(initialDamagePerBullet != modifiedDamagePerBuller) {
                statModifiedBools.Add(true);
            } else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedDamagePerBuller.ToString());

            if (gunItem == GunItemType.shotgun) {
                // PELLETS PER BULLET
                float initialPelletsPerBullet = linkedGunSO.pelletsPerBullet;
                float modifiedPelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(linkedGunSO);
                if (initialPelletsPerBullet != modifiedPelletsPerBullet) {
                    statModifiedBools.Add(true);
                }
                else {
                    statModifiedBools.Add(false);
                }
                statValues.Add(modifiedPelletsPerBullet.ToString());
            }

            // CRIT CHANCE
            float initialCritChance = linkedGunSO.critChance;
            float modifierCritChance = MetaProgressionManager.Instance.GetGunCritChance(linkedGunSO);
            if (initialCritChance != modifierCritChance) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifierCritChance + "%");

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

            // SHOOT CONE
            float initialShootCone = linkedGunSO.shootConeAngle;
            float modifiedShootCone = MetaProgressionManager.Instance.GetGunShootConeAnle(linkedGunSO);
            if (initialShootCone != modifiedShootCone) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedShootCone + "\u00B0");
        }

        if(gunItemCategory == GunItemCategory.statIncrease) {
            maxItemLevel = linkedStatModifierSO.statModifierList.Count;

            string totalStatValue = "";
            string totalStatWithModifierPostfix = "";
            string totalStatWithModifierPrefix = "";
            string relativeStatPostfix = "";
            string relativeStatPrefix = "";

            float initialStatValue = 0;
            float statValueModifierMultiplier = 1;
            float absoluteStatValueModifier = 0;
            float totalStatWithModifier = 0;
            float relativeDamageBulletModifier = 0;


            if (gunItem == GunItemType.critChance) {
                initialStatValue = linkedGunSO.critChance;
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.reloadTime) {
                initialStatValue = linkedGunSO.reloadTime;
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.cooldownTime) {
                initialStatValue = linkedGunSO.shootCooldownTime;
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.maxAmmo) {
                initialStatValue = linkedGunSO.maxAmmo;
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shotsPerClip) {
                initialStatValue = linkedGunSO.shotsPerClip;
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.bulletDamage) {
                initialStatValue = linkedGunSO.damagePerBullet;
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.pelletsPerBullet) {
                initialStatValue = linkedGunSO.pelletsPerBullet;
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shootConeAngle) {
                initialStatValue = linkedGunSO.shootConeAngle;
                relativeStatPrefix = "";
                totalStatWithModifierPostfix = "\u00B0";
                relativeStatPostfix = "\u00B0";
            }

            if (itemLevel == maxItemLevel) {
                absoluteStatValueModifier = linkedStatModifierSO.statModifierList[itemLevel - 1];
                totalStatWithModifier = initialStatValue + absoluteStatValueModifier * statValueModifierMultiplier;
            }
            else {
                absoluteStatValueModifier = linkedStatModifierSO.statModifierList[itemLevel];
                totalStatWithModifier = initialStatValue + absoluteStatValueModifier * statValueModifierMultiplier;

                totalStatValue = totalStatWithModifier.ToString();
                relativeDamageBulletModifier = linkedStatModifierSO.statModifierList[itemLevel];

                if (itemLevel > 0) {
                    relativeDamageBulletModifier = linkedStatModifierSO.statModifierList[itemLevel] - linkedStatModifierSO.statModifierList[itemLevel - 1];
                }
            }


            if (itemLevel == maxItemLevel) {
                statValues.Add(totalStatWithModifierPrefix + totalStatWithModifier.ToString() + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
            else {
                statValues.Add(relativeStatPrefix + relativeDamageBulletModifier.ToString() + relativeStatPostfix);
                statModifiedBools.Add(false);

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
        }

    }

    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        if(GetConstantUnlockDescription()) {
            statDescriptionList.Add(unlockDescription);
        }

        if(gunItemCategory == GunItemCategory.newGun) {
            statDescriptionList.Add("Bullet damage ");

            if (gunItem == GunItemType.shotgun) {
                statDescriptionList.Add("Pellets per bullet ");
            }

            statDescriptionList.Add("Crit chance ");
            statDescriptionList.Add("Shots per clip ");
            statDescriptionList.Add("Max ammo clips ");
            statDescriptionList.Add("Cooldown ");
            statDescriptionList.Add("Reload time ");
            statDescriptionList.Add("Spread ");
        }

        if (gunItem == GunItemType.bulletDamage) {
            if(itemLevel < maxItemLevel) {
                statDescriptionList.Add("Bullet damage ");
            }
            statDescriptionList.Add("Total bullet damage ");
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Pellets/shot ");
            }
            statDescriptionList.Add("Total pellets/shot ");
        }

        if (gunItem == GunItemType.critChance) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Crit shot chance ");
            }
            statDescriptionList.Add("Total crit chance ");
        }

        if (gunItem == GunItemType.reloadTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Reload time ");
            }
            statDescriptionList.Add("Total reload time ");
        }

        if (gunItem == GunItemType.cooldownTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Shoot cooldown ");
            }
            statDescriptionList.Add("Total cooldown ");
        }

        if (gunItem == GunItemType.shotsPerClip) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Shots per clip ");
            }
            statDescriptionList.Add("Total shots per clip ");
        }

        if (gunItem == GunItemType.maxAmmo) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Max clips ");
            }
            statDescriptionList.Add("Total ammo clips ");
        }

        if (gunItem == GunItemType.shootConeAngle) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Spread ");
            }
            statDescriptionList.Add("Final spread ");
        }

        return statDescriptionList;
    }

    public override List<string> GetStatValues() {
        return statValues;
    }

    public override List<bool> GetStatModifierBools() {
        return statModifiedBools;
    }
    public override bool GetConstantUnlockDescription() {
        bool constantUnlockDescription = true;

        if(gunItemCategory == GunItemCategory.statIncrease || gunItemCategory == GunItemCategory.newGun) {
            constantUnlockDescription = false;
        }

        return constantUnlockDescription;
    }

    public override void EquipOrUnequipItem() {
        if(gunItemCategory == GunItemCategory.newGun) {
            if (!itemEquipped) {
                itemEquipped = true;
                EquipGun();
                InvokeOnItemEquipped();
            }
        }

        if (gunItemCategory == GunItemCategory.gunModule) {
            if (!itemEquipped) {
                itemEquipped = true;
                InvokeOnItemEquipped();
            } else {
                itemEquipped = false;
                InvokeOnItemUnequipped();
            }
        }
    }

    public override void UnequipItem() {
        itemEquipped = false;
        InvokeOnItemUnequipped();
        MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), false);
    }

    private void EquipGun() {
        PlayerShoot.Instance.SetGun(linkedGunSO);
        MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), true);
        PlayerSave.Instance.SetPrimaryActiveGunSO(linkedGunSO);
    }

    private void UnlockGunAbility() {
        MetaProgressionManager.Instance.SetGunSecondaryAbilityUnlocked(linkedGunSO);
    }

    private void UnlockGunModule() {
        MetaProgressionManager.Instance.SetGunModuleEquipped(linkedGunSO,gunItem);
    }

    public GunItemCategory GetGunItemCategory() {
        return gunItemCategory;
    }
    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }

}
