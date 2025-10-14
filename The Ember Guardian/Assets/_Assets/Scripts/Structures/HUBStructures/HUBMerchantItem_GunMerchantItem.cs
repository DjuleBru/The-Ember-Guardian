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
        surgeReloadBulletsBoosted,
        surgeReloadProbability,
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
        OnAnyHubMerchantItemBought += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped += HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh() {
        yield return new WaitForEndOfFrame(); // attend 1 frame
        RefreshStatValues();
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

    public override void SetItemBought() {
        base.SetItemBought();

        if (gunItemCategory == GunItemCategory.newGun) {
            UnlockGun();
        }
    }

    public override void UnlockItem() {
        base.UnlockItem();

        if(gunItemCategory == GunItemCategory.newGun && itemBought) {
            UnlockGun();
        }
    }

    public override void UpgradeItem() {
        if (gunItemCategory == GunItemCategory.statIncrease) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();
    }

    private void SetNewStatIncreaseStats() {
        if (gunItem == GunItemType.bulletDamage || gunItem == GunItemType.explosionDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetBulletDamage_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShotsPerClip_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.maxAmmo) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetMaxAmmo_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.cooldownTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCooldownTime_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.reloadTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetReloadTime_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.critChance) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCritChange_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.shootConeAngle) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShootConeAngle_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetPelletsPerBullet_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.range) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetGunBulletLifetime_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.surgeReloadBulletsBoosted) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeWindowBulletBoost_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.surgeReloadProbability) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeReloadProbability_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.explosionRadiusBuff) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetExplosionRadiusModified_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.spinUpTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSpinUpDuration_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.subExplosivesDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesDamage_StatModifierListLevel(itemLevel);
        }

        if (gunItem == GunItemType.subExplosivesAmount) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesAmount_StatModifierListLevel(itemLevel);
        }
    }

    private void ResetStats() {
        if (gunItem == GunItemType.bulletDamage || gunItem == GunItemType.explosionDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetBulletDamage_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.shotsPerClip) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShotsPerClip_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.maxAmmo) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetMaxAmmo_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.cooldownTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCooldownTime_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.reloadTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetReloadTime_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.critChance) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetCritChange_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.shootConeAngle) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetShootConeAngle_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.pelletsPerBullet) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetPelletsPerBullet_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.range) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetGunBulletLifetime_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.surgeReloadBulletsBoosted) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeWindowBulletBoost_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.surgeReloadProbability) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSurgeReloadProbability_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.explosionRadiusBuff) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetExplosionRadiusModified_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.spinUpTime) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSpinUpDuration_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.subExplosivesDamage) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesDamage_StatModifierListLevel(0);
        }

        if (gunItem == GunItemType.subExplosivesAmount) {
            PlayerShoot.Instance.GetGun(linkedGunSO).SetSubExplosivesAmount_StatModifierListLevel(0);
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

            if(itemLevel > maxItemLevel) {
                // Balancing security 
                itemLevel = maxItemLevel;
            }

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

            if (gunItem == GunItemType.surgeReloadBulletsBoosted) {
                initialStatValue = linkedGunSO.perfectQTEBulletAmountDamageBuffed;
                currentStatValue = (PlayerShoot.Instance.GetGun(linkedGunSO).GetSurgeWindowBulletAmountBuffed()).ToString();
                relativeStatPrefix = "+";
                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
            }

            if (gunItem == GunItemType.surgeReloadProbability) {
                initialStatValue = linkedGunSO.surgeReloadProbability;
                currentStatValue = (PlayerShoot.Instance.GetGun(linkedGunSO).GetSurgeReloadProbability()).ToString();
                relativeStatPrefix = "+";
                totalStatWithModifierPostfix = "%";
                relativeStatPostfix = "%";
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

        if (gunItem == GunItemType.surgeReloadBulletsBoosted) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSurgeWindowBoost") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_surgeWindowBoost") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSurgeWindowBoost") + " ");
        }

        if (gunItem == GunItemType.surgeReloadProbability) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentSurgeReloadProbability") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_surgeReloadProbability") + " ");
                statDescriptionList.Add("");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newSurgeReloadProbability") + " ");
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

    private void UnlockGun() {
        PlayerShoot.Instance.GetGun(linkedGunSO).SetGunUnlocked();
    }

    private void UnlockGunAbility() {
        PlayerShoot.Instance.GetGun(linkedGunSO).SetSecondaryAbilityUnlocked();
        PlayerTooltipManager.Instance.SetGunSOAbilityPrepared(linkedGunSO);
    }

    public GunItemCategory GetGunItemCategory() {
        return gunItemCategory;
    }
    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }

    public override void ResetGunItemStatus() {
        itemLevel = 0;
        itemStatusChanged = true;

        UpdateItemCost();

        if (gunItemCategory == GunItemCategory.newGun || gunItemCategory == GunItemCategory.gunAbility) {
            itemUnlocked = true;
            itemBought = true;
        } else {
            itemBought = false;
            itemUnlocked = false;
        }

        ResetStats();
        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
        InvokeOnItemLoaded();
    }

    private void OnDestroy() {
        OnAnyHubMerchantItemBought -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemUpgraded -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemBoughtOrUpgraded;
        OnAnyHubMerchantItemEquipped -= HUBMerchantItem_GunMerchantItem_OnAnyHubMerchantItemEquipped;
    }

}
