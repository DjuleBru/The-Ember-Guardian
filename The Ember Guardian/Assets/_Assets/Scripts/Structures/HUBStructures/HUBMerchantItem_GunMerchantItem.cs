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
        surgeWindowBoost,
        surgeWindowHitAmount,
        explosionRadiusBuff,
        explosionDamage,
        grenadeLauncher,
        subExplosivesAmount,
        subExplosivesDamage,
        spinUpTime,
        minigun,
        AAgun,
        pistol,
        rocketLauncher,
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
        if (gunItem == GunItemType.bulletDamage || gunItem == GunItemType.explosionDamage) {
            int modifiedDamage = linkedGunSO.damagePerBullet + (int)linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetBulletDamage_Meta(modifiedDamage);
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

        if (gunItem == GunItemType.surgeWindowBoost) {
            int modifiedBulletsAmount = linkedGunSO.perfectQTEBulletAmountDamageBuffed + (int)linkedStatModifierSO.statModifierList[itemLevel];
            Debug.Log("modifiedBulletsAmount " + modifiedBulletsAmount);
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeWindowBulletBoost(modifiedBulletsAmount);
        }

        if (gunItem == GunItemType.surgeWindowHitAmount) {
            float modifiedJamRepairHitAmount = linkedGunSO.jamRepairHitAmount + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetJamRepairHitAmount((int)modifiedJamRepairHitAmount);
        }

        if (gunItem == GunItemType.explosionRadiusBuff) {
            float modifiedExplosionRadius = 100 + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetExplosionRadiusModified((float)modifiedExplosionRadius/100);
        }

        if (gunItem == GunItemType.spinUpTime) {
            float modifiedSpinUpTime = linkedGunSO.spinUpDuration + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSpinUpDuration((float)modifiedSpinUpTime);
        }

        if (gunItem == GunItemType.subExplosivesDamage) {
            float modifiedSubExplosivesDamage = linkedGunSO.subExplosivesDamage + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesDamage((int)modifiedSubExplosivesDamage);
        }

        if (gunItem == GunItemType.subExplosivesAmount) {
            float modifiedSubExplosivesAmount = linkedGunSO.subExplosivesAmount + linkedStatModifierSO.statModifierList[itemLevel];
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesAmount((int)modifiedSubExplosivesAmount);
        }
    }

    private void ResetStats() {
        if (gunItem == GunItemType.bulletDamage || gunItem == GunItemType.explosionDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetBulletDamage_Meta(linkedGunSO.damagePerBullet);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShotsPerClip_Meta((int)linkedGunSO.shotsPerClip);
        }

        if (gunItem == GunItemType.maxAmmo) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetMaxAmmo_Meta((int)linkedGunSO.maxAmmo);
        }

        if (gunItem == GunItemType.cooldownTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCooldownTime_Meta(linkedGunSO.shootCooldownTime);
        }

        if (gunItem == GunItemType.reloadTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetReloadTime_Meta(linkedGunSO.reloadTime);
        }

        if (gunItem == GunItemType.critChance) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCritChange_Meta((int)linkedGunSO.critChance);
        }

        if (gunItem == GunItemType.shootConeAngle) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShootConeAngle_Meta(linkedGunSO.shootConeAngle);
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetPelletsPerBullet_Meta((int)linkedGunSO.pelletsPerBullet);
        }

        if (gunItem == GunItemType.range) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetGunBulletLifetime_Meta(linkedGunSO.bulletLifetime);
        }

        if (gunItem == GunItemType.surgeWindowBoost) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeWindowBulletBoost(linkedGunSO.perfectQTEBulletAmountDamageBuffed);
        }

        if (gunItem == GunItemType.surgeWindowHitAmount) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetJamRepairHitAmount((int)linkedGunSO.jamRepairHitAmount);
        }

        if (gunItem == GunItemType.explosionRadiusBuff) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetExplosionRadiusModified(1);
        }

        if (gunItem == GunItemType.spinUpTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSpinUpDuration(linkedGunSO.spinUpDuration);
        }

        if (gunItem == GunItemType.subExplosivesDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesDamage((int)linkedGunSO.subExplosivesDamage);
        }

        if (gunItem == GunItemType.subExplosivesAmount) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesAmount((int)linkedGunSO.subExplosivesAmount);
        }
    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();

        if (gunItemCategory == GunItemCategory.newGun) {

            // DAMAGE
            int initialDamagePerBullet = linkedGunSO.damagePerBullet;
            int modifiedDamagePerBuller = PlayerShoot.Instance.GetGun(linkedGunSO).GetDamagePerBullet();

            if (initialDamagePerBullet != modifiedDamagePerBuller) {
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

            if (gunItem == GunItemType.grenadeLauncher) {
                // EXPLOSION RADIUS
                float initialExplosionRadius = 1;
                float modifiedExplosionRadius = PlayerShoot.Instance.GetGun(linkedGunSO).GetExplosionRadiusMultiplier();
                if (initialExplosionRadius != modifiedExplosionRadius) {
                    statModifiedBools.Add(true);
                }
                else {
                    statModifiedBools.Add(false);
                }
                statValues.Add(((int)(modifiedExplosionRadius*100f)).ToString() + "%");
            }

            if (gunItem == GunItemType.AAgun) {
                // SUB EXPLOSIVES AMOUNT
                float subExplosivesAmount = linkedGunSO.subExplosivesAmount;
                float modifiedSubExplosivesAmount = PlayerShoot.Instance.GetGun(linkedGunSO).GetSubExplosivesAmount();
                if (subExplosivesAmount != modifiedSubExplosivesAmount) {
                    statModifiedBools.Add(true);
                }
                else {
                    statModifiedBools.Add(false);
                }
                statValues.Add(((int)(modifiedSubExplosivesAmount)).ToString());

                // SUB EXPLOSIVES Damage
                float subExplosivesDamage = linkedGunSO.subExplosivesDamage;
                float modifiedSubExplosivesDamage= PlayerShoot.Instance.GetGun(linkedGunSO).GetSubExplosivesDamage();
                if (subExplosivesDamage != modifiedSubExplosivesDamage) {
                    statModifiedBools.Add(true);
                }
                else {
                    statModifiedBools.Add(false);
                }
                statValues.Add(((int)(modifiedSubExplosivesDamage)).ToString());
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

            // SPIN UP TIME
            if (gunItem == GunItemType.minigun) {
                float initialSpinUpTime = linkedGunSO.spinUpDuration;
                float modifiedSpinUpTime = PlayerShoot.Instance.GetGun(linkedGunSO).GetSpinUpDuration();
                if (initialSpinUpTime != modifiedSpinUpTime) {
                    statModifiedBools.Add(true);
                }
                else {
                    statModifiedBools.Add(false);
                }
                statValues.Add((modifiedSpinUpTime).ToString() + "s");
            }

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

            if (gunItem == GunItemType.bulletDamage || gunItem == GunItemType.explosionDamage) {
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

            if (gunItem == GunItemType.surgeWindowBoost) {
                initialStatValue = linkedGunSO.perfectQTEBulletAmountDamageBuffed;
                currentStatValue = (PlayerShoot.Instance.GetGun(linkedGunSO).GetSurgeWindowBulletAmountBuffed()).ToString();
                relativeStatPrefix = "+";
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
            }

            if (gunItem == GunItemType.surgeWindowHitAmount) {
                initialStatValue = linkedGunSO.jamRepairHitAmount;
                currentStatValue = (PlayerShoot.Instance.GetGun(linkedGunSO).GetJamRepairHitAmount()).ToString();
                relativeStatPrefix = "";
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
            }

            if (gunItem == GunItemType.explosionRadiusBuff) {
                initialStatValue = 100;
                currentStatValue = ((int)(PlayerShoot.Instance.GetGun(linkedGunSO).GetExplosionRadiusMultiplier()*100)).ToString();
                relativeStatPrefix = "+";
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
            }

            if (gunItem == GunItemType.spinUpTime) {
                initialStatValue = linkedGunSO.spinUpDuration;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetSpinUpDuration().ToString();
                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
            }

            if (gunItem == GunItemType.subExplosivesDamage) {
                initialStatValue = linkedGunSO.subExplosivesDamage;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetSubExplosivesDamage().ToString();
                relativeStatPrefix = "+";
            }

            if (gunItem == GunItemType.subExplosivesAmount) {
                initialStatValue = linkedGunSO.subExplosivesAmount;
                currentStatValue = PlayerShoot.Instance.GetGun(linkedGunSO).GetSubExplosivesAmount().ToString();
                relativeStatPrefix = "+";
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

            if(gunItem != GunItemType.grenadeLauncher) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_bulletDamage") + " ");
            } else {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_explosionDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_explosionRadiusMultiplier") + " ");
            }

            if (gunItem == GunItemType.shotgun) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_pelletsPerBullet") + " ");
            }

            if (gunItem == GunItemType.AAgun) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_subExplosivesAmount") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_subExplosivesDamage") + " ");
            }

            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_shotsPerClip") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxAmmoClips") + " ");
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_cooldown") + " "); 
            
            if (gunItem == GunItemType.minigun) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_spinUpDuration") + " ");
            }

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

        if (gunItem == GunItemType.explosionDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentExplosionDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_explosionDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newExplosionDamage") + " ");
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

        if (gunItem == GunItemType.surgeWindowBoost) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSurgeWindowBoost") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_surgeWindowBoost") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSurgeWindowBoost") + " ");
        }

        if (gunItem == GunItemType.surgeWindowHitAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentJamRepairHitAmount") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_jamRepairHitAmount") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newJamRepairHitAmount") + " ");
        }

        if (gunItem == GunItemType.explosionRadiusBuff) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentExplosionRadiusMultiplier") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_explosionRadiusMultiplier") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newExplosionRadiusMultiplier") + " ");
        }

        if (gunItem == GunItemType.spinUpTime) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSpinUpDuration") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_spinUpDuration") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSpinUpDuration") + " ");
        }

        if (gunItem == GunItemType.subExplosivesAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSubExplosivesAmount") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_subExplosivesAmount") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSubExplosivesAmount") + " ");
        }

        if (gunItem == GunItemType.subExplosivesDamage) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSubExplosivesDamage") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_subExplosivesDamage") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSubExplosivesDamage") + " ");
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
            PlayerShoot.Instance.SetActiveGun(linkedGunSO.gunType, true);
        } else {
            PlayerShoot.Instance.SetActiveGun(linkedGunSO.gunType, false);
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

    public override void ResetGunItemStatus() {
        base.ResetGunItemStatus();

        ResetStats();
        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void OnDestroy() {
        OnAnyHubMerchantItemBought -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
    }

}
