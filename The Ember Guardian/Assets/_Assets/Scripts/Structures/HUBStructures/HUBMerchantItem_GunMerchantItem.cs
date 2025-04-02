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
        revolver,
        lmg,
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
    
    protected override void Awake() {
        base.Awake();

        RefreshStatValues();

        OnAnyHubMerchantItemBought += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
    }

    protected override void LoadItemEquipped() {
        itemEquipped = (PlayerShoot.Instance.GetHeldGunSO() == linkedGunSO);
    }

    public override string GetItemType() {
        return gunItem.ToString() + " " + linkedGunSO.ToString();
    }

    private void HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped(object sender, EventArgs e) {
        HubMerchantItem merchantItem = (HubMerchantItem)sender;

        if (merchantItem is HUBMerchantItem_GunMerchantItem) {
            HUBMerchantItem_GunMerchantItem gunItem = (HUBMerchantItem_GunMerchantItem)merchantItem;


            if (gunItem.GetGunItemCategory() == GunItemCategory.newGun && gunItemCategory == GunItemCategory.newGun && gunItem.GetLinkedGunSO() != linkedGunSO) {
                // Another gun has been equipped

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
            UnlockGun();
            EquipOrUnequipItem();
        }

        if (gunItemCategory == GunItemCategory.gunAbility) {
            UnlockGunAbility();
        }

        if (gunItemCategory == GunItemCategory.gunModule) {
            //EquipOrUnequipItem();
            //UnlockGunModule();
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
            PlayerShoot.Instance.GetGun(linkedGunSO).SetBulletDamage_Meta((int)modifiedDamage);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            float modifiedShotsPerClip = linkedGunSO.shotsPerClip + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShotsPerClip_Meta((int)modifiedShotsPerClip);
        }

        if (gunItem == GunItemType.maxAmmo) {
            float modifiedMaxAmmo = linkedGunSO.maxAmmo + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetMaxAmmo_Meta((int)modifiedMaxAmmo);
        }

        if (gunItem == GunItemType.cooldownTime) {
            float modifiedCooldown = linkedGunSO.shootCooldownTime + linkedGunSO.shootCooldownTime * linkedStatModifierSO.statModifierList[itemLevel] * 0.01f;
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCooldownTime_Meta(modifiedCooldown);
        }

        if (gunItem == GunItemType.reloadTime) {
            float modifiedReloadTime = linkedGunSO.reloadTime + linkedGunSO.reloadTime * linkedStatModifierSO.statModifierList[itemLevel] * 0.01f;
            PlayerShoot.Instance.GetGun(linkedGunSO).SetReloadTime_Meta(modifiedReloadTime);
        }

        if (gunItem == GunItemType.critChance) {
            float modifiedCritChange = linkedGunSO.critChance + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCritChange_Meta((int)modifiedCritChange);
        }

        if (gunItem == GunItemType.shootConeAngle) {
            float modifiedShootAngle = linkedGunSO.shootConeAngle + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShootConeAngle_Meta(modifiedShootAngle);
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            float modifiedPelletsPerBullet = linkedGunSO.pelletsPerBullet + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetPelletsPerBullet_Meta((int)modifiedPelletsPerBullet);
        }

        if (gunItem == GunItemType.range) {
            float modifiedBulletLifetime = linkedGunSO.bulletLifetime + linkedStatModifierSO.statModifierList[itemLevel] / linkedGunSO.bulletSpeed;
            PlayerShoot.Instance.GetGun(linkedGunSO).SetGunBulletLifetime_Meta(modifiedBulletLifetime);
        }
    }
   
    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();

        if (gunItemCategory == GunItemCategory.newGun) {

            // DAMAGE
            int initialDamagePerBullet = linkedGunSO.damagePerBullet;
            int modifiedDamagePerBuller = PlayerShoot.Instance.GetGun(linkedGunSO).GetDamagePerBullet();

            if(initialDamagePerBullet != modifiedDamagePerBuller) {
                statModifiedBools.Add(true);
            } else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedDamagePerBuller.ToString());

            if (gunItem == GunItemType.shotgun) {
                // PELLETS PER BULLET
                float initialPelletsPerBullet = linkedGunSO.pelletsPerBullet;
                float modifiedPelletsPerBullet = PlayerShoot.Instance.GetGun(linkedGunSO).GetPelletsPerBullet();
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
            int modifiedShotsPerClip = PlayerShoot.Instance.GetGun(linkedGunSO).GetShotsPerClip();
            if (initialShotsPerClip != modifiedShotsPerClip) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedShotsPerClip.ToString());

            // MAX AMMO
            int initialMaxAmmo = linkedGunSO.maxAmmo;
            int modifiedMaxAmmo = PlayerShoot.Instance.GetGun(linkedGunSO).GetMaxAmmo();
            if (initialMaxAmmo != modifiedMaxAmmo) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedMaxAmmo.ToString());

            // COOLDOWN
            float initialCooldown = linkedGunSO.shootCooldownTime;
            float modifierCooldown = PlayerShoot.Instance.GetGun(linkedGunSO).GetCooldownTime();
            if (initialCooldown != modifierCooldown) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifierCooldown.ToString("F2") + "s");

            // RELOAD TIME
            float initialReloadTime = linkedGunSO.reloadTime;
            float modifiedReloadTime = PlayerShoot.Instance.GetGun(linkedGunSO).GetReloadTime();
            if (initialReloadTime != modifiedReloadTime) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifiedReloadTime.ToString("F2") + "s");

            // CRIT CHANCE
            float initialCritChance = linkedGunSO.critChance;
            float modifierCritChance = PlayerShoot.Instance.GetGun(linkedGunSO).GetCritChance();
            if (initialCritChance != modifierCritChance) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }
            statValues.Add(modifierCritChance + "%");

            // RANGE
            float initialRange = linkedGunSO.bulletLifetime * linkedGunSO.bulletSpeed;
            float modifiedRange = PlayerShoot.Instance.GetGun(linkedGunSO).GetBulletLifetime() * linkedGunSO.bulletSpeed;
            if (initialRange != modifiedRange) {
                statModifiedBools.Add(true);
            }
            else {
                statModifiedBools.Add(false);
            }

            statValues.Add(modifiedRange + "m");

            // SHOOT CONE
            float initialShootCone = linkedGunSO.shootConeAngle;
            float modifiedShootCone = PlayerShoot.Instance.GetGun(linkedGunSO).GetDefaultShootAngle();
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
            float relativeStatModifier = 0;


            if (gunItem == GunItemType.critChance) {
                initialStatValue = linkedGunSO.critChance;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetCritChance().ToString();
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.reloadTime) {
                initialStatValue = linkedGunSO.reloadTime;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetReloadTime().ToString("F2");
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.cooldownTime) {
                initialStatValue = linkedGunSO.shootCooldownTime;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetCooldownTime().ToString("F2");
                relativeStatPostfix = "%";
                totalStatWithModifierPostfix = "s";
                statValueModifierMultiplier = 0.01f;
            }

            if (gunItem == GunItemType.maxAmmo) {
                initialStatValue = linkedGunSO.maxAmmo;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetMaxAmmo().ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shotsPerClip) {
                initialStatValue = linkedGunSO.shotsPerClip;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetShotsPerClip().ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.bulletDamage) {
                initialStatValue = linkedGunSO.damagePerBullet;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetDamagePerBullet().ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.pelletsPerBullet) {
                initialStatValue = linkedGunSO.pelletsPerBullet;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetPelletsPerBullet().ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.shootConeAngle) {
                initialStatValue = linkedGunSO.shootConeAngle;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetDefaultShootAngle().ToString();
                relativeStatPrefix = "";
                totalStatWithModifierPostfix = "\u00B0";
                relativeStatPostfix = "\u00B0";
            }

            if (gunItem == GunItemType.range) {
                initialStatValue = linkedGunSO.bulletLifetime * linkedGunSO.bulletSpeed;
                currentStatValue = (PlayerShoot.Instance.GetGun(linkedGunSO).GetBulletLifetime() * linkedGunSO.bulletSpeed).ToString();
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
                relativeStatModifier = linkedStatModifierSO.statModifierList[itemLevel];

                if (itemLevel > 0) {
                    relativeStatModifier = linkedStatModifierSO.statModifierList[itemLevel] - linkedStatModifierSO.statModifierList[itemLevel - 1];
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

                statValues.Add(relativeStatPrefix + relativeStatModifier.ToString() + relativeStatPostfix);
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
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        }

        if(gunItemCategory == GunItemCategory.newGun) {
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_bulletDamage") + " ");

            if (gunItem == GunItemType.shotgun) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_pelletsPerBullet") + " ");
            }

            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_shotsPerClip") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxAmmoClips") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_cooldown") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_reloadTime") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_critChance") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_range") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_spread") + " ");
        }

        if (gunItem == GunItemType.bulletDamage) {
            if(itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_bulletDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBulletDamage") + " ");
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentPelletsPerShot") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_pelletsPerBullet") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newPelletsPerShot") + " ");
        }

        if (gunItem == GunItemType.range) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentRange") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_range") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newRange") + " ");
        }

        if (gunItem == GunItemType.critChance) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentCritChance") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_critChance") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newCritChance") + " ");
        }

        if (gunItem == GunItemType.reloadTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentReloadTime") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_reloadTime") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newReloadTime") + " ");
        }

        if (gunItem == GunItemType.cooldownTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentCooldown") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_shotCooldown") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newCooldown") + " ");
        }

        if (gunItem == GunItemType.shotsPerClip) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentBulletsPerClip") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_shotsPerClip") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBulletsPerClip") + " ");
        }

        if (gunItem == GunItemType.maxAmmo) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxClips") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxClips") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newAmmoClips") + " ");
        }

        if (gunItem == GunItemType.shootConeAngle) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSpread") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_spread") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSpread") + " ");
        }

        return statDescriptionList;
    }

    public override bool GetConstantUnlockDescription() {
        bool constantUnlockDescription = true;

        if(gunItemCategory == GunItemCategory.statIncrease || gunItemCategory == GunItemCategory.newGun) {
            constantUnlockDescription = false;
        }

        return constantUnlockDescription;
    }

    public override void EquipOrUnequipItem() {
        Debug.Log("Switching equip function to pause menu");
        return;

        if(gunItemCategory == GunItemCategory.newGun) {

            if (!itemEquipped) {

                itemEquipped = true;

                if (PlayerStats.Instance.GetHold2WeaponsUnlocked()) {
                    // Player can hold 2 weapons

                    if (PlayerShoot.Instance.GetSecondaryGunSO() == null) {
                        // Player doesn't have a secondary gun yet

                        EquipGun(false);

                    } else {

                    }

                }

                else {
                    // Player can't hold 2 weapons
                }

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
    }

    private void EquipGun(bool primaryGun) {
        itemEquipped = true;

        if(primaryGun) {
            PlayerShoot.Instance.SetActiveGun(linkedGunSO, true);
        } else {
            PlayerShoot.Instance.SetActiveGun(linkedGunSO, false);
        }
    }

    private void UnlockGun() {
        PlayerShoot.Instance.GetGun(linkedGunSO).SetGunUnlocked();
    }

    private void UnlockGunAbility() {
        PlayerShoot.Instance.GetGun(linkedGunSO).SetSecondaryAbilityUnlocked();
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

    private void OnDestroy() {
        OnAnyHubMerchantItemBought -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
    }

}
