using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapItem : MerchantItem
{
    public enum TrapType {
        bladeTrap,
        bearTrap,
        smokeEjector,
        spikeEjectorSmall,
        shockerEjector,
        spikes,
        flameEjector,
        fireEjector,
    }

    public TrapType trapType;
    public TrapSO trapSO { get; private set; }
    public int maxLevel;

    public override void Initialize(ScriptableObject data) {
        TrapSO TrapDataSO = data as TrapSO;

        if (TrapDataSO != null) {
            trapSO = TrapDataSO;

            price = trapSO.buyTrapPrice;

            itemName = trapSO.TrapName;
            itemDescription = trapSO.Description;
            itemType = trapSO.itemType;
            trapType = trapSO.trapType;
            icon = trapSO.Icon;
            currencyTypeToPay = trapSO.currencyTypeToPay;

            if(itemType == MerchantItemType.Trap) {
                buyingLocksPurchasesUntilRefresh = false;
            } else {
                buyingLocksPurchasesUntilRefresh = true;
            }
        }
    }

    public override void Purchase() {
        base.Purchase();

        if (trapSO.itemType == MerchantItemType.TrapUpgrade) {
            TrapManager.Instance.SetTrapUpgradeLevel(trapType, trapSO.trapUpgradeSO.trapUpgradeType, currentLevel);
        }

        if (trapSO.itemType == MerchantItemType.Trap) {
            TrapManager.Instance.AddTrapTypeBought(trapType);
        }
    }

    public TrapSO GetTrapSO() {
        return trapSO;
    }
}
