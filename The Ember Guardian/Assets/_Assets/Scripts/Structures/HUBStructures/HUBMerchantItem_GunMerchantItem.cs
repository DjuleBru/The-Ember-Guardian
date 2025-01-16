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
        adaptiveFire,
        rifle,
        shotgun,
        sniper,
        smg,
        shootConeAngle,
        pelletsPerBullet,
        aimSight,
        overclock,
        focusedBlast,
        range,
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
        itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
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
                RefreshStatValues();
                InvokeItemMustRefreshDescriptionCard();
                return;
            }

            if(gunItemCategory == GunItemCategory.newGun) {

                if (gunItem.GetGunItemCategory() == GunItemCategory.statIncrease && gunItem.GetLinkedGunSO() == linkedGunSO) {
                    RefreshStatValues();
                    InvokeItemMustRefreshDescriptionCard();
                }

            }

        }
    }

    public override void BuyItem() {
        if(gunItemCategory == GunItemCategory.statIncrease) {
            SetNewStatIncreaseStats();
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
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();
    }

    private void SetNewStatIncreaseStats() {
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

        if (gunItem == GunItemType.range) {
            float modifiedBulletLifetime = linkedGunSO.bulletLifetime + linkedStatModifierSO.statModifierList[itemLevel];
            MetaProgressionManager.Instance.SetGunBulletLifetime(linkedGunSO, modifiedBulletLifetime);
        }
    }

    public override string GetItemType() {
        return gunItem.ToString() + " " + linkedGunSO.ToString();
    }

    private void RefreshStatValues() {
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
            statValues.Add(modifierCooldown.ToString("F2") + "s");

            // RELOAD TIME
            float initialReloadTime = linkedGunSO.reloadTime;
            float modifiedReloadTime = MetaProgressionManager.Instance.GetGunReloadTime(linkedGunSO);
            if (initialReloadTime != modifiedReloadTime) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedReloadTime.ToString("F2") + "s");

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

            // RANGE
            float initialRange = linkedGunSO.bulletLifetime * linkedGunSO.bulletSpeed;
            float modifiedRange = MetaProgressionManager.Instance.GetGunBulletLifetime(linkedGunSO) * linkedGunSO.bulletSpeed;
            if (initialRange != modifiedRange) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedRange + "m");

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
            string currentStatValue = "";
            string totalStatWithModifierPostfix = "";
            string relativeStatPostfix = "";
            string relativeStatPrefix = "";

            float initialStatValue = 0;
            float statValueModifierMultiplier = 1;
            float absoluteStatValueModifier = 0;
            float totalStatWithModifier = 0;
            float relativeDamageBulletModifier = 0;


            if (gunItem == GunItemType.critChance) {
                initialStatValue = linkedGunSO.critChance;
                currentStatValue = MetaProgressionManager.Instance.GetGunCritChance(linkedGunSO).ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.reloadTime) {
                initialStatValue = linkedGunSO.reloadTime;
                currentStatValue = MetaProgressionManager.Instance.GetGunReloadTime(linkedGunSO).ToString("F2");
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.cooldownTime) {
                initialStatValue = linkedGunSO.shootCooldownTime;
                currentStatValue = MetaProgressionManager.Instance.GetGunCooldown(linkedGunSO).ToString("F2");
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.maxAmmo) {
                initialStatValue = linkedGunSO.maxAmmo;
                currentStatValue = MetaProgressionManager.Instance.GetGunMaxAmmo(linkedGunSO).ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shotsPerClip) {
                initialStatValue = linkedGunSO.shotsPerClip;
                currentStatValue = MetaProgressionManager.Instance.GetGunShotsPerClip(linkedGunSO).ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.bulletDamage) {
                initialStatValue = linkedGunSO.damagePerBullet;
                currentStatValue = MetaProgressionManager.Instance.GetGunDamagePerBullet(linkedGunSO).ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.pelletsPerBullet) {
                initialStatValue = linkedGunSO.pelletsPerBullet;
                currentStatValue = MetaProgressionManager.Instance.GetGunPelletsPerBullet(linkedGunSO).ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shootConeAngle) {
                initialStatValue = linkedGunSO.shootConeAngle;
                currentStatValue = MetaProgressionManager.Instance.GetGunShootConeAnle(linkedGunSO).ToString();
                relativeStatPrefix = "";
                totalStatWithModifierPostfix = "\u00B0";
                relativeStatPostfix = "\u00B0";
            }

            if (gunItem == GunItemType.range) {
                initialStatValue = linkedGunSO.bulletLifetime * linkedGunSO.bulletSpeed;
                currentStatValue = (MetaProgressionManager.Instance.GetGunBulletLifetime(linkedGunSO) * linkedGunSO.bulletSpeed).ToString();
                relativeStatPrefix = "+";
                totalStatWithModifierPostfix = "m";
                relativeStatPostfix = "m";
            }

            if (itemLevel == maxItemLevel) {
                absoluteStatValueModifier = linkedStatModifierSO.statModifierList[itemLevel - 1];
                totalStatWithModifier = initialStatValue + absoluteStatValueModifier * statValueModifierMultiplier;
                totalStatValue = totalStatWithModifier.ToString();
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

            if(gunItem == GunItemType.cooldownTime || gunItem == GunItemType.reloadTime) {
                totalStatWithModifier = initialStatValue + (absoluteStatValueModifier * statValueModifierMultiplier)*initialStatValue;
                totalStatValue = totalStatWithModifier.ToString("F2");
            }


            if (itemLevel == maxItemLevel) {
                statValues.Add(totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
            else {
                statValues.Add(currentStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(false);

                statValues.Add(relativeStatPrefix + relativeDamageBulletModifier.ToString() + relativeStatPostfix);
                statModifiedBools.Add(true);

                statValues.Add("");
                statModifiedBools.Add(false);

                statValues.Add(totalStatValue + totalStatWithModifierPostfix);
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

            statDescriptionList.Add("Shots per clip ");
            statDescriptionList.Add("Max ammo clips ");
            statDescriptionList.Add("Cooldown ");
            statDescriptionList.Add("Reload time ");
            statDescriptionList.Add("Crit chance ");
            statDescriptionList.Add("Range ");
            statDescriptionList.Add("Spread ");
        }

        if (gunItem == GunItemType.bulletDamage) {
            if(itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current damage ");
                statDescriptionList.Add("Bullet damage ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New bullet damage ");
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current Pellets/shot ");
                statDescriptionList.Add("Pellets/shot ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New pellets/shot ");
        }

        if (gunItem == GunItemType.range) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current range ");
                statDescriptionList.Add("Range ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New range ");
        }

        if (gunItem == GunItemType.critChance) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current crit chance ");
                statDescriptionList.Add("Crit chance ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New crit chance ");
        }

        if (gunItem == GunItemType.reloadTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current reload time ");
                statDescriptionList.Add("Reload time ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New reload time ");
        }

        if (gunItem == GunItemType.cooldownTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current cooldown ");
                statDescriptionList.Add("Shot cooldown ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New cooldown ");
        }

        if (gunItem == GunItemType.shotsPerClip) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current bullets/clip ");
                statDescriptionList.Add("Bullets/clip ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New bullets/clip ");
        }

        if (gunItem == GunItemType.maxAmmo) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current max clips ");
                statDescriptionList.Add("Max clips ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New ammo clips ");
        }

        if (gunItem == GunItemType.shootConeAngle) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add("Current spread ");
                statDescriptionList.Add("Spread ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add("New spread ");
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
        
        if(isEquippedAtStart) {
            MetaProgressionManager.Instance.SetHubMerchantItemEquippedAtStart(GetItemType(), false);
        }
    }

    private void EquipGun() {
        itemEquipped = true;
        PlayerShoot.Instance.SetGun(linkedGunSO);
    }

    private void UnlockGunAbility() {
        MetaProgressionManager.Instance.SetGunSecondaryAbilityUnlocked(linkedGunSO);

        if(gunItem == GunItemType.overclock) {
            PlayerTooltipManager.Instance.PrepareGunSecondaryAbilityTooltipInstruction(linkedGunSO);
        }

        if (gunItem == GunItemType.aimSight) {
            PlayerTooltipManager.Instance.PrepareGunSecondaryAbilityTooltipInstruction(linkedGunSO);
        }

        if (gunItem == GunItemType.focusedBlast) {
            PlayerTooltipManager.Instance.PrepareGunSecondaryAbilityTooltipInstruction(linkedGunSO);
        }

        if (gunItem == GunItemType.adaptiveFire) {
            PlayerTooltipManager.Instance.PrepareGunSecondaryAbilityTooltipInstruction(linkedGunSO);
        }

        PlayerTooltipManager.Instance.SetGunSOAbilityPrepared(linkedGunSO);
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
