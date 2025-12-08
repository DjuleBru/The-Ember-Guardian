using System;
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
        FireFuelDepletion,
        FireOrbConversionRate,
        MaxFuelCapacity,
        TentHealAmountPerSmallOrb,
        StartWithAmmoCrafter,
        StartWithResearchTower,
        StartWithFirstBarricadeLayer,
        BarricadeHealth,
        BarricadeSpiked,
        SingleAmmoCraftDuration,
        AmmoCrafterBatchCapacity,
        AmmoCrafterMaxAmmoPerBatch,
        SingleOrbCraftDuration,
        OrbProcessorBatchCapacity,
        OrbProcessorMaxOrbsPerBatch,
        ObservationTowerEnemyTypes,
        ObservationTowerEnemyAmounts,
        FastTravelTPMaxAmount,
    }

    public enum ArchitectItemCategory {
        architectTableUpgrades,
        startWithStructure,
        structureUpgrade,
    }

    [SerializeField] private ArchitectItemType architectItemType;
    [SerializeField] private ArchitectItemCategory architectItemCategory;
    [SerializeField] private StructureSO linkedStructureSO;

    protected override void Awake() {
        base.Awake();
        RefreshStatValues();
    }

    public override void BuyItem() {
        if (itemUpgradeable) {

            SetNewStatIncreaseStats();

        }
        else {

            if(architectItemType == ArchitectItemType.ArchitectTable) {
                ArchitectTable.Instance.SetArchitectTableUnlocked();
            }
            if (architectItemType == ArchitectItemType.StartWithAmmoCrafter) {
                StructureStats.Instance.SetStartWithAmmoCrafter();
            }
            if (architectItemType == ArchitectItemType.StartWithFirstBarricadeLayer) {
                StructureStats.Instance.SetStartWithBarricades();
            }
            if (architectItemType == ArchitectItemType.StartWithResearchTower) {
                StructureStats.Instance.SetStartWithResearchTower();
            }
            if (architectItemType == ArchitectItemType.BarricadeSpiked) {
                StructureStats.Instance.SetBarricadesSpiked();
            }
            if (architectItemType == ArchitectItemType.ObservationTowerEnemyAmounts) {
                StructureStats.Instance.SetObservationTowerEnemyAmountDetectionUnlocked();
            }
            if (architectItemType == ArchitectItemType.ObservationTowerEnemyTypes) {
                StructureStats.Instance.SetObservationTowerEnemyTypesDetectionUnlocked();
            }
        }

        base.BuyItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }
    public override void UpgradeItem() {
        if (architectItemCategory != ArchitectItemCategory.startWithStructure && architectItemType != ArchitectItemType.ArchitectTable && architectItemType != ArchitectItemType.BarricadeSpiked) {
            SetNewStatIncreaseStats();
        }

        base.UpgradeItem();

        RefreshStatValues();
        InvokeItemMustRefreshDescriptionCard();
    }

    private void SetNewStatIncreaseStats() {
        float buff = linkedStatModifierSO.statModifierList[itemLevel];

        if (architectItemType == ArchitectItemType.AmmoCrafterMaxAmount) {
            ArchitectTable.Instance.SetMaxAmmoCrafterAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.SecondaryFireMaxAmount) {
            ArchitectTable.Instance.SetSecondaryFireAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.SniperTowerMaxAmount) {
            ArchitectTable.Instance.SetMaxSniperTowerAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.TowerMaxAmount) {
            ArchitectTable.Instance.SetMaxTowerAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.MortarPositionsMaxAmount) {
            ArchitectTable.Instance.SetMaxMortarPositionsAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.MachineGunTowerMaxAmount) {
            ArchitectTable.Instance.SetMaxMachineGunTowerAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.TrapSlotsMaxAmount) {
            ArchitectTable.Instance.SetMaxTrapSlotsAmountBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.FastTravelTPMaxAmount) {
            ArchitectTable.Instance.SetMaxFastTravelTPAmountBuff((int)buff);
        }

        if (architectItemType == ArchitectItemType.FireFuelDepletion) {
            StructureStats.Instance.SetFuelDepletionRateBuff(buff);
        }
        if (architectItemType == ArchitectItemType.FireOrbConversionRate) {
            StructureStats.Instance.SetOrbFuelValueBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.MaxFuelCapacity) {
            StructureStats.Instance.SetMaxFuelTresholdBuff((int)buff);
        }

        if (architectItemType == ArchitectItemType.BarricadeHealth) {
            StructureStats.Instance.SetBarricadeHealthPerCrateBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.TentHealAmountPerSmallOrb) {
            StructureStats.Instance.SetTentHealAmountPerSmallOrbBuff((int)buff);
        }

        if (architectItemType == ArchitectItemType.AmmoCrafterBatchCapacity) {
            StructureStats.Instance.SetAmmoCrafterBatchCapacityBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.AmmoCrafterMaxAmmoPerBatch) {
            StructureStats.Instance.SetAmmoCrafterMaxAmmoPerBatchBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.SingleAmmoCraftDuration) {
            StructureStats.Instance.SetSingleAmmoCraftDurationBuff((int)buff);
        }

        if (architectItemType == ArchitectItemType.SingleOrbCraftDuration) {
            StructureStats.Instance.SetSingleOrbCraftDurationBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.OrbProcessorBatchCapacity) {
            StructureStats.Instance.SetOrbProcessorBatchCapacityBuff((int)buff);
        }
        if (architectItemType == ArchitectItemType.OrbProcessorMaxOrbsPerBatch) {
            StructureStats.Instance.SetOrbProcessorMaxOrbsPerBatchBuff((int)buff);
        }

    }

    private void RefreshStatValues() {
        statModifiedBools.Clear();
        statValues.Clear();
        statModifiedBools.Add(false);
        statValues.Add("");
        statModifiedBools.Add(false);
        statValues.Add("");

        if (itemUpgradeable) {

            maxItemLevel = linkedStatModifierSO.statModifierList.Count;

            string totalStatValue = "";
            string currentStatValue = "";
            string initialStatPrefix = "";
            string totalStatWithModifierPrefix = "";
            string totalStatWithModifierPostfix = "";
            string relativeStatPostfix = "";
            string relativeStatPrefix = "";

            float initialStatValue = 0;
            float statValueModifierMultiplier = 1;
            float absoluteStatValueModifier = 0;
            float totalStatWithModifier = 0;
            float relativeStatModifier = 0;


            if (architectItemCategory == ArchitectItemCategory.architectTableUpgrades) {
                if(architectItemType == ArchitectItemType.AmmoCrafterMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxAmmoCrafterAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxAmmoCrafterAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.SniperTowerMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxSniperTowerAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxSniperTowerAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.FastTravelTPMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxFastTravelTPAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxFastTravelTPAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.TowerMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxTowerAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxTowerAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.MachineGunTowerMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMachineGunTowerAmount();
                    currentStatValue = ArchitectTable.Instance.GetMachineGunTowerAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.MortarPositionsMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMortarPositionsAmount();
                    currentStatValue = ArchitectTable.Instance.GetMortarPositionsAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.SecondaryFireMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxSecondaryFireAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxSecondaryFireAmount().ToString();
                }
                if (architectItemType == ArchitectItemType.TrapSlotsMaxAmount) {
                    initialStatValue = ArchitectTable.Instance.GetInitialMaxTrapSlotsAmount();
                    currentStatValue = ArchitectTable.Instance.GetMaxTrapSlotsAmount().ToString();
                }

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if(architectItemType == ArchitectItemType.FireFuelDepletion) {
                initialStatValue = StructureStats.Instance.GetInitialFuelDepletionRate()*60f;
                currentStatValue = (StructureStats.Instance.GetMainFireFuelDepletionRate()*60f).ToString("F1");

                totalStatWithModifierPostfix = "/min";
                relativeStatPostfix = "/min";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (architectItemType == ArchitectItemType.MaxFuelCapacity) {
                initialStatValue = StructureStats.Instance.GetInitialMaxFuelTreshold();
                currentStatValue = StructureStats.Instance.GetMainFireMaxFuelTreshold().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }
            if (architectItemType == ArchitectItemType.FireOrbConversionRate) {
                initialStatValue = StructureStats.Instance.GetInitialOrbFuelValue();
                currentStatValue = StructureStats.Instance.GetOrbFuelValue().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.TentHealAmountPerSmallOrb) {
                initialStatValue = StructureStats.Instance.GetInitialTentHealAmountPerSmallOrb();
                currentStatValue = StructureStats.Instance.GetTentHealAmountPerSmallOrb().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.BarricadeHealth) {
                initialStatValue = StructureStats.Instance.GetInitialBarricadeHealthPerCrate();
                currentStatValue = StructureStats.Instance.GetBarricadeHealthPerCrate().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.SingleAmmoCraftDuration) {
                initialStatValue = StructureStats.Instance.GetInitialSingleAmmoCraftDuration();
                currentStatValue = StructureStats.Instance.GetAmmoCrafterSingleAmmoCraftDuration().ToString();

                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.AmmoCrafterMaxAmmoPerBatch) {
                initialStatValue = StructureStats.Instance.GetInitialAmmoMaxAmmoPerBatch();
                currentStatValue = StructureStats.Instance.GetAmmoCrafterMaxAmmoPerBatch().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.AmmoCrafterBatchCapacity) {
                initialStatValue = StructureStats.Instance.GetInitialAmmoCrafterBatchCapacity();
                currentStatValue = StructureStats.Instance.GetAmmoCrafterBatchCapacity().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.SingleOrbCraftDuration) {
                initialStatValue = StructureStats.Instance.GetInitialSingleOrbCraftDuration();
                currentStatValue = StructureStats.Instance.GetOrbProcessorSingleOrbCraftDuration().ToString();

                totalStatWithModifierPostfix = "s";
                relativeStatPostfix = "s";
                relativeStatPrefix = "";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.OrbProcessorMaxOrbsPerBatch) {
                initialStatValue = StructureStats.Instance.GetInitialOrbProcessorMaxOrbsPerBatch();
                currentStatValue = StructureStats.Instance.GetOrbProcessorMaxOrbsPerBatch().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
            }

            if (architectItemType == ArchitectItemType.OrbProcessorBatchCapacity) {
                initialStatValue = StructureStats.Instance.GetInitialOrbProcessorBatchCapacity();
                currentStatValue = StructureStats.Instance.GetOrbProcessorBatchCapacity().ToString();

                totalStatWithModifierPostfix = "";
                relativeStatPostfix = "";
                relativeStatPrefix = "+";
                initialStatPrefix = "";
                totalStatWithModifierPrefix = "";
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

            if (itemLevel == maxItemLevel) {
                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
            else {
                statValues.Add(initialStatPrefix + currentStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(false);

                statValues.Add(relativeStatPrefix + relativeStatModifier.ToString() + relativeStatPostfix);
                statModifiedBools.Add(true);

                statValues.Add(totalStatWithModifierPrefix + totalStatValue + totalStatWithModifierPostfix);
                statModifiedBools.Add(true);
            }
        }
    }
    public override List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string>();

        statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription"));
        statDescriptionList.Add("");

        if (architectItemType == ArchitectItemType.AmmoCrafterMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxAmmoCrafter") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxAmmoCrafter") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxAmmoCrafter") + " ");
        }
        if (architectItemType == ArchitectItemType.TrapSlotsMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxTraps") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxTraps") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxTraps") + " ");
        }
        if (architectItemType == ArchitectItemType.TowerMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxTower") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxTower") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxTower") + " ");
        }
        if (architectItemType == ArchitectItemType.SniperTowerMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxSniperTower") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxSniperTower") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxSniperTower") + " ");
        }
        if (architectItemType == ArchitectItemType.FastTravelTPMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxFastTravelTP") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxFastTravelTP") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxFastTravelTP") + " ");
        }
        if (architectItemType == ArchitectItemType.MortarPositionsMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxMortarPositions") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxMortarPositions") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxMortarPositions") + " ");
        }
        if (architectItemType == ArchitectItemType.MachineGunTowerMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxMachineGunTower") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxMachineGunTower") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxMachineGunTower") + " ");
        }
        if (architectItemType == ArchitectItemType.SecondaryFireMaxAmount) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxSecondaryFire") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxSecondaryFire") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxSecondaryFire") + " ");
        }

        if (architectItemType == ArchitectItemType.FireFuelDepletion) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentFireFuelDepletion") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_fireFuelDepletion") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newFireFuelDepletion") + " ");
        }
        if (architectItemType == ArchitectItemType.FireOrbConversionRate) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentOrbConversionRate") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_orbConversionRate") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newOrbConversionRate") + " ");
        }
        if (architectItemType == ArchitectItemType.MaxFuelCapacity) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentMaxFuelCapacity") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_maxFuelCapacity") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newMaxFuelCapacity") + " ");
        }

        if (architectItemType == ArchitectItemType.TentHealAmountPerSmallOrb) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentTentHealAmountPerOrb") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_tentHealAmountPerOrb") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newTentHealAmountPerOrb") + " ");
        }

        if (architectItemType == ArchitectItemType.BarricadeHealth) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentBarricadeCrateHealth") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_barricadeCrateHealth") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newBarricadeCrateHealth") + " ");
        }

        if (architectItemType == ArchitectItemType.SingleAmmoCraftDuration) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentAmmoCraftDuration") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_ammoCraftDuration") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newAmmoCraftDuration") + " ");
        }
        if (architectItemType == ArchitectItemType.AmmoCrafterBatchCapacity) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentAmmoCrafterBatchCapacity") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_ammoCrafterBatchCapacity") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newAmmoCrafterBatchCapacity") + " ");
        }
        if (architectItemType == ArchitectItemType.AmmoCrafterMaxAmmoPerBatch) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentAmmoCrafterMaxAmmoPerBatch") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_ammoCrafterMaxAmmoPerBatch") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newAmmoCrafterMaxAmmoPerBatch") + " ");
        }

        if (architectItemType == ArchitectItemType.SingleOrbCraftDuration) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentOrbCraftDuration") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_orbCraftDuration") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newOrbCraftDuration") + " ");
        }
        if (architectItemType == ArchitectItemType.OrbProcessorBatchCapacity) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentOrbProcessorBatchCapacity") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_orbProcessorBatchCapacity") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newOrbProcessorBatchCapacity") + " ");
        }
        if (architectItemType == ArchitectItemType.OrbProcessorMaxOrbsPerBatch) {
            if (itemLevel < maxItemLevel) {
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_currentOrbProcessorMaxOrbsPerBatch") + " ");
                statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_orbProcessorMaxOrbsPerBatch") + " ");
            }
            statDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_newOrbProcessorMaxOrbsPerBatch") + " ");
        }

        return statDescriptionList;
    }
    public override bool GetConstantUnlockDescription() {
        return false;
    }
    public override void LoadItemStatus_HodeMode() {

        if (architectItemType == ArchitectItemType.SingleOrbCraftDuration) {
            if (HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.OrbProcessor)) {
                itemUnlocked = true;
            }
        }

        if (architectItemType == ArchitectItemType.ObservationTowerEnemyTypes) {
            if (HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.ObservationTower)) {
                itemUnlocked = true;
            }
        }

        base.LoadItemStatus_HodeMode();
    }
    public override string GetItemType() {
        return architectItemType.ToString();
    }

    public ArchitectItemType GetArchitectItemType() {
        return architectItemType;
    }
    public ArchitectItemCategory GetArchitectItemCategory() {
        return architectItemCategory;
    }
}
