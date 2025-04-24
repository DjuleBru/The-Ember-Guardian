using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_ArchitectMerchantItem : HubMerchantItem {
    public enum ArchitectItemType {
        ArchitectTable,
        AmmoCrafterMaxAmount,
        TowerMaxAmount,
        SecondaryFireMaxAmount,
        TrapSlotsMaxAmount,
        SniperTowerMaxAmount,
        MachineGunTowerMaxAmount,
        MortarPositionsMaxAmount,
        FireFuelConsumption,
        FireOrbConversionRate,
        MaxFuelCapacity,
        TentHealAmountPerSmallOrb,
        StartWithAmmoCrafter,
        StartWithResearchTower,
        StartWithFirstBarricadeLayer,
        BarricadeHealth,
        BarricadeSpiked,
        AmmoCrafterCraftingSpeed,
        AmmoCrafterBatchCapacity,
        AmmoCrafterMaxAmmoPerBatch,

    }

    public enum ArchitectItemCategory {
        architectTableUpgrades,
        startWithStructure,
        structureUpgrade,
    }

    [SerializeField] private ArchitectItemType architectItemType;
    [SerializeField] private ArchitectItemCategory architectItemCategory;

    public override string GetItemType() {
        return architectItemType.ToString();
    }
}
