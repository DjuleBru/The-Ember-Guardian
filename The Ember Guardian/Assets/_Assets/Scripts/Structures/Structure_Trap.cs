using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure_Trap : Structure
{
    [SerializeField] protected TrapSO trapSO;
    protected int trapDamage;
    protected int usesPerNight;
    protected int rearmPrice;
    protected int maxRearms;
    protected float trapCooldown;
    protected float trapActiveDuration;

    protected int currentUseIndex;
    protected int currentRearmIndex;

    protected float trapActiveTimer;
    protected float trapCooldownTimer;

    protected bool trapIsActive;
    protected bool triggeredEnded;
    protected bool trapCoolingDown;

    public event EventHandler OnTrapTriggered;
    public event EventHandler OnTrapTriggeredEnded;
    public event EventHandler OnTrapActiveEnded;
    public event EventHandler OnTrapRearmed;
    public event EventHandler OnTrapMaxRearmsReached;

    protected override void Start() {
        base.Start();
        MerchantItem.OnAnyMerchantItemBought += MerchantItem_OnAnyMerchantItemBought;

        RefreshTrapStats();
    }


    protected void Update() {

        if (trapIsActive) {
            trapActiveTimer -= Time.deltaTime;
            if(trapActiveTimer < 0) {
                trapIsActive = false;
                trapCoolingDown = true;

                OnTrapActiveEnded?.Invoke(this, EventArgs.Empty);
            }
            return;
        };    

        if(trapCoolingDown) {
            trapCooldownTimer -= Time.deltaTime;

            if (trapCooldownTimer < .2f && !triggeredEnded) {
                triggeredEnded = true;
                OnTrapTriggeredEnded?.Invoke(this, EventArgs.Empty);
            }

            if (trapCooldownTimer < 0) {
                trapCoolingDown = false;
            }
        }
    }

    public void TryTriggerTrap() {
        if (trapIsActive) return;
        if (trapCoolingDown) return;

        if (currentUseIndex < usesPerNight) {
            currentUseIndex++;
            OnTrapTriggered?.Invoke(this, EventArgs.Empty);

            trapActiveTimer = trapActiveDuration;
            trapCooldownTimer = trapCooldown;

            trapIsActive = true;
            triggeredEnded = false;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
    }

    private void MerchantItem_OnAnyMerchantItemBought(object sender, System.EventArgs e) {
        TrapItem trapItem = (TrapItem)sender;

        if (trapItem != null) {
            if(trapItem.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
                RefreshTrapStats();
            }
        }
    }

    protected override void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);
    }

    protected void RefreshTrapStats() {
        int trapDamageUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.damage);
        trapDamage = trapSO.trapDamage + trapDamageUpgrade;

        int usesPerNightUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.usesPerNight);
        usesPerNight = trapSO.trapUsesPerNight + usesPerNightUpgrade;

        int rearmPriceUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.priceToReload);
        rearmPrice = trapSO.trapPriceToReload - rearmPriceUpgrade;

        int maxRearmsUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.totalUses);
        rearmPrice = trapSO.trapReloadsBeforeBreaking - maxRearmsUpgrade;

        float trapCooldownUpgrade = TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.cooldown);
        trapCooldown = trapSO.trapCooldown - trapCooldownUpgrade;

        trapActiveDuration = trapSO.trapActiveDuration;
    }

    public bool GetHasUsesLeft() {
        return currentUseIndex < usesPerNight;
    }

    public TrapSO GetTrapSO() {
        return trapSO;
    }
    protected void OnDestroy() {
        MerchantItem.OnAnyMerchantItemBought -= MerchantItem_OnAnyMerchantItemBought;
    }
}
