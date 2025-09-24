using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureStats : MonoBehaviour
{

    public static StructureStats Instance;

    private float orbFuelValue = 10;
    private float initialOrbFuelValue = 10;
    private float mainFireFuelDepletionRate = 0.05f;
    private float initialFuelDepletionRate = 0.05f;
    private int mainFireMaxFuelTreshold = 50;
    private int initialMaxFuelTreshold = 50;
    private float secondaryFireFuelDepletionRate = 0.05f;
    private float initialSecondaryFireFuelDepletionRate = 0.05f;
    private int secondaryFireMaxFuelTreshold;
    private int initialSecondaryFireMaxFuelTreshold = 20;

    private int ammoCrafterBatchCapacity = 1;
    private int initialAmmoCrafterBatchCapacity = 1;
    private int singleAmmoCraftDuration = 15;
    private int initialSingleAmmoCraftDuration = 15;
    private int ammoCrafterMaxAmmoPerBatch = 3;
    private int initialAmmoCrafterMaxAmmoPerBatch = 3;

    private int orbProcessorBatchCapacity = 1;
    private int initialOrbProcessorBatchCapacity = 1;
    private int singleOrbCraftDuration = 45;
    private int initialSingleOrbCraftDuration = 45;
    private int orbProcessorMaxOrbsPerBatch = 2;
    private int initialOrbProcessorMaxOrbsPerBatch = 2;

    private int tentHealAmountPerSmallOrb = 1;
    private int initialTentHealAmountPerSmallOrb = 1;

    private int barricadeHealthPerCrate = 8;
    private int initialBarricadeHealthPerCrate = 8;

    private int skillMerchantMaxActiveSkillsDisplayed = 1;
    private int initialSkillMerchantMaxActiveSkillsDisplayed = 1;
    private int skillMerchantMaxPassiveSkillsDisplayed = 2;
    private int initialSkillMerchantMaxPassiveSkillsDisplayed = 2;
    private int trapMerchantMaxTrapsDisplayed = 1;
    private int initialTrapMerchantMaxTrapsDisplayed = 1;
    private int trapMerchantMaxTrapUpgradesDisplayed = 2;
    private int initialTrapMerchantMaxTrapUpgradeDisplayed = 2;

    private int startWithRandomTrapAmount;
    private int initialStartWithRandomTrapAmount = 0;

    private float engineerContainerSizeBuff;

    private bool barricadesSpiked;
    private bool startWithAmmoCrafter;
    private bool startWithResearchTower;
    private bool startWithBarricadeLayer;

    private bool observationTowerEnemyTypesDetectionUnlocked;
    private bool observationTowerEnemyAmountDetectionUnlocked;

    private void Awake() {
        Instance = this;
        LoadStructureStats();
    }


    private void LoadStructureStats() {
        if (!ES3.KeyExists("StructureStats"))
            return;

        var structureData = ES3.Load<Dictionary<string, object>>("StructureStats");

        // Structures de base
        barricadesSpiked = GetValue(structureData, "barricadesSpiked", false);
        startWithAmmoCrafter = GetValue(structureData, "startWithAmmoCrafter", false);
        startWithResearchTower = GetValue(structureData, "startWithResearchTower", false);
        startWithBarricadeLayer = GetValue(structureData, "startWithBarricadeLayer", false);

        // Feu principal et secondaire
        orbFuelValue = GetValue(structureData, "orbFuelValue", initialOrbFuelValue);
        mainFireMaxFuelTreshold = GetValue(structureData, "mainFireMaxFuelTreshold", initialMaxFuelTreshold);
        mainFireFuelDepletionRate = GetValue(structureData, "mainFireFuelDepletionRate", initialFuelDepletionRate);
        secondaryFireMaxFuelTreshold = GetValue(structureData, "secondaryFireMaxFuelTreshold", initialSecondaryFireMaxFuelTreshold);
        secondaryFireFuelDepletionRate = GetValue(structureData, "secondaryFireFuelDepletionRate", initialSecondaryFireFuelDepletionRate);

        // Crafter munitions
        ammoCrafterBatchCapacity = GetValue(structureData, "ammoCrafterBatchCapacity", initialAmmoCrafterBatchCapacity);
        singleAmmoCraftDuration = GetValue(structureData, "singleAmmoCraftDuration", initialSingleAmmoCraftDuration);
        ammoCrafterMaxAmmoPerBatch = GetValue(structureData, "ammoCrafterMaxAmmoPerBatch", initialAmmoCrafterMaxAmmoPerBatch);

        // Crafter orbes
        orbProcessorBatchCapacity = GetValue(structureData, "orbProcessorBatchCapacity", initialOrbProcessorBatchCapacity);
        singleOrbCraftDuration = GetValue(structureData, "singleOrbCraftDuration", initialSingleOrbCraftDuration);
        orbProcessorMaxOrbsPerBatch = GetValue(structureData, "orbProcessorMaxOrbsPerBatch", initialOrbProcessorMaxOrbsPerBatch);

        // Autres structures
        tentHealAmountPerSmallOrb = GetValue(structureData, "tentHealAmountPerSmallOrb", initialTentHealAmountPerSmallOrb);
        barricadeHealthPerCrate = GetValue(structureData, "barricadeHealthPerCrate", initialBarricadeHealthPerCrate);

        // Marchands
        skillMerchantMaxActiveSkillsDisplayed = GetValue(structureData, "skillMerchantMaxActiveSkillsDisplayed", initialSkillMerchantMaxActiveSkillsDisplayed);
        skillMerchantMaxPassiveSkillsDisplayed = GetValue(structureData, "skillMerchantMaxPassiveSkillsDisplayed", initialSkillMerchantMaxPassiveSkillsDisplayed);
        trapMerchantMaxTrapsDisplayed = GetValue(structureData, "trapMerchantMaxTrapsDisplayed", initialTrapMerchantMaxTrapsDisplayed);
        trapMerchantMaxTrapUpgradesDisplayed = GetValue(structureData, "trapMerchantMaxTrapUpgradesDisplayed", initialTrapMerchantMaxTrapUpgradeDisplayed);
        startWithRandomTrapAmount = GetValue(structureData, "startWithRandomTrapAmount", initialStartWithRandomTrapAmount);

        // Buffs
        engineerContainerSizeBuff = GetValue(structureData, "engineerContainerSizeBuff", 0f);

        // Tour de recherche
        observationTowerEnemyTypesDetectionUnlocked = GetValue(structureData, "researchTowerEnemyTypesDetectionUnlocked", false);
        observationTowerEnemyAmountDetectionUnlocked = GetValue(structureData, "researchTowerEnemyAmountDetectionUnlocked", false);
    }

    public void SaveStructureStats() {
        var structureData = new Dictionary<string, object>();

        // Structures de base
        structureData["barricadesSpiked"] = barricadesSpiked;
        structureData["startWithAmmoCrafter"] = startWithAmmoCrafter;
        structureData["startWithResearchTower"] = startWithResearchTower;
        structureData["startWithBarricadeLayer"] = startWithBarricadeLayer;

        // Feu principal et secondaire
        structureData["orbFuelValue"] = orbFuelValue;
        structureData["mainFireMaxFuelTreshold"] = mainFireMaxFuelTreshold;
        structureData["mainFireFuelDepletionRate"] = mainFireFuelDepletionRate;
        structureData["secondaryFireMaxFuelTreshold"] = secondaryFireMaxFuelTreshold;
        structureData["secondaryFireFuelDepletionRate"] = secondaryFireFuelDepletionRate;

        // Crafter munitions
        structureData["ammoCrafterBatchCapacity"] = ammoCrafterBatchCapacity;
        structureData["singleAmmoCraftDuration"] = singleAmmoCraftDuration;
        structureData["ammoCrafterMaxAmmoPerBatch"] = ammoCrafterMaxAmmoPerBatch;

        // Crafter orbes
        structureData["orbProcessorBatchCapacity"] = orbProcessorBatchCapacity;
        structureData["singleOrbCraftDuration"] = singleOrbCraftDuration;
        structureData["orbProcessorMaxOrbsPerBatch"] = orbProcessorMaxOrbsPerBatch;

        // Autres structures
        structureData["tentHealAmountPerSmallOrb"] = tentHealAmountPerSmallOrb;
        structureData["barricadeHealthPerCrate"] = barricadeHealthPerCrate;

        // Marchands
        structureData["skillMerchantMaxActiveSkillsDisplayed"] = skillMerchantMaxActiveSkillsDisplayed;
        structureData["skillMerchantMaxPassiveSkillsDisplayed"] = skillMerchantMaxPassiveSkillsDisplayed;
        structureData["trapMerchantMaxTrapsDisplayed"] = trapMerchantMaxTrapsDisplayed;
        structureData["trapMerchantMaxTrapUpgradesDisplayed"] = trapMerchantMaxTrapUpgradesDisplayed;
        structureData["startWithRandomTrapAmount"] = startWithRandomTrapAmount;

        // Buffs
        structureData["engineerContainerSizeBuff"] = engineerContainerSizeBuff;

        // Tour de recherche
        structureData["researchTowerEnemyTypesDetectionUnlocked"] = observationTowerEnemyTypesDetectionUnlocked;
        structureData["researchTowerEnemyAmountDetectionUnlocked"] = observationTowerEnemyAmountDetectionUnlocked;

        ES3.Save("StructureStats", structureData);
    }


    private T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue) {
        if (dict.ContainsKey(key)) {
            try {
                return (T)Convert.ChangeType(dict[key], typeof(T));
            }
            catch {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    #region FIRE

    public int GetMainFireMaxFuelTreshold() {
        return mainFireMaxFuelTreshold;
    }
    public int GetSecondaryFireMaxFuelTreshold() {
        return secondaryFireMaxFuelTreshold;
    }
    public float GetOrbFuelValue() {
        return orbFuelValue;
    }
    public float GetMainFireFuelDepletionRate() {
        return mainFireFuelDepletionRate;
    }
    public float GetSecondaryFireFuelDepletionRate() {
        return secondaryFireFuelDepletionRate;
    }
    public int GetInitialMaxFuelTreshold() {
        return initialMaxFuelTreshold;
    }
    public float GetInitialOrbFuelValue() {
        return initialOrbFuelValue;
    }
    public float GetInitialFuelDepletionRate() {
        return initialFuelDepletionRate;
    }

    public void SetMaxFuelTresholdBuff(int maxFuelTresholdBuff) {
        mainFireMaxFuelTreshold = initialMaxFuelTreshold + maxFuelTresholdBuff;
    }
    public void SetOrbFuelValueBuff(int orbFuelValueBuff) {
        orbFuelValue = initialOrbFuelValue + orbFuelValueBuff;
    }
    public void SetFuelDepletionRateBuff(float fuelDepletionRateBuff) {
        mainFireFuelDepletionRate = initialFuelDepletionRate + fuelDepletionRateBuff/60f;
    }


    #endregion

    #region CAMP
    public int GetInitialTentHealAmountPerSmallOrb() {
        return initialTentHealAmountPerSmallOrb;
    }
    public int GetTentHealAmountPerSmallOrb() {
        return tentHealAmountPerSmallOrb;
    }
    public void SetTentHealAmountPerSmallOrbBuff(int buffValue) {
        tentHealAmountPerSmallOrb = initialTentHealAmountPerSmallOrb + buffValue;
    }

    public void SetBarricadesSpiked() {
        barricadesSpiked = true;
    }
    public void SetStartWithAmmoCrafter() {
        startWithAmmoCrafter = true;
    }
    public void SetStartWithResearchTower() {
        startWithResearchTower = true;
    }
    public void SetStartWithBarricades() {
        startWithBarricadeLayer = true;
    }
    public bool GetStartWithAmmoCrafter() {
        return startWithAmmoCrafter;
    }
    public bool GetStartWithResearchTower() {
        return startWithResearchTower;
    }
    public bool GetStartWithBarricades() {
        return startWithBarricadeLayer;
    }

    #endregion

    #region BARRICADES
    public int GetBarricadeHealthPerCrate() {
        return barricadeHealthPerCrate;
    }
    public int GetInitialBarricadeHealthPerCrate() {
        return initialBarricadeHealthPerCrate;
    }

    public void SetBarricadeHealthPerCrateBuff(int barricadeHealthPerCrateBuff) {
        barricadeHealthPerCrate = initialBarricadeHealthPerCrate + barricadeHealthPerCrateBuff;
    }

    public bool GetBarricadesSpiked() {
        return barricadesSpiked;
    }

    #endregion

    #region AMMO CRAFTER

    public int GetAmmoCrafterBatchCapacity() {
        return ammoCrafterBatchCapacity;
    }
    public int GetInitialAmmoCrafterBatchCapacity() {
        return initialAmmoCrafterBatchCapacity;
    }
    public void SetAmmoCrafterBatchCapacityBuff(int batchCapacityBuff) {
        ammoCrafterBatchCapacity = initialAmmoCrafterBatchCapacity + batchCapacityBuff;
    }

    public int GetAmmoCrafterSingleAmmoCraftDuration() {
        return singleAmmoCraftDuration;
    }
    public int GetInitialSingleAmmoCraftDuration() {
        return initialSingleAmmoCraftDuration;
    }
    public void SetSingleAmmoCraftDurationBuff(int craftingSpeedBuff) {
        singleAmmoCraftDuration = initialSingleAmmoCraftDuration + craftingSpeedBuff;
    }

    public int GetAmmoCrafterMaxAmmoPerBatch() {
        return ammoCrafterMaxAmmoPerBatch;
    }
    public int GetInitialAmmoMaxAmmoPerBatch() {
        return initialAmmoCrafterMaxAmmoPerBatch;
    }
    public void SetAmmoCrafterMaxAmmoPerBatchBuff(int maxAmmoPerBatchBuff) {
        ammoCrafterMaxAmmoPerBatch = initialAmmoCrafterMaxAmmoPerBatch + maxAmmoPerBatchBuff;
    }

    #endregion

    #region ORB PROCESSOR
    public int GetOrbProcessorBatchCapacity() {
        return orbProcessorBatchCapacity;
    }
    public int GetInitialOrbProcessorBatchCapacity() {
        return initialOrbProcessorBatchCapacity;
    }
    public void SetOrbProcessorBatchCapacityBuff(int batchCapacityBuff) {
        orbProcessorBatchCapacity = initialOrbProcessorBatchCapacity + batchCapacityBuff;
    }

    public int GetOrbProcessorSingleOrbCraftDuration() {
        return singleOrbCraftDuration;
    }
    public int GetInitialSingleOrbCraftDuration() {
        return initialSingleOrbCraftDuration;
    }
    public void SetSingleOrbCraftDurationBuff(int craftingSpeedBuff) {
        singleOrbCraftDuration = initialSingleOrbCraftDuration + craftingSpeedBuff;
    }

    public int GetOrbProcessorMaxOrbsPerBatch() {
        return orbProcessorMaxOrbsPerBatch;
    }
    public int GetInitialOrbProcessorMaxOrbsPerBatch() {
        return initialOrbProcessorMaxOrbsPerBatch;
    }
    public void SetOrbProcessorMaxOrbsPerBatchBuff(int maxAmmoPerBatchBuff) {
        orbProcessorMaxOrbsPerBatch = initialOrbProcessorMaxOrbsPerBatch + maxAmmoPerBatchBuff;
    }

    #endregion

    #region OTHER
    public bool GetObservationTowerEnemyTypesDetectionUnlocked() {
        return observationTowerEnemyTypesDetectionUnlocked;
    }
    public bool GetObservationTowerEnemyAmountDetectionUnlocked() {
        return observationTowerEnemyAmountDetectionUnlocked;
    }

    public void SetObservationTowerEnemyTypesDetectionUnlocked() {
        observationTowerEnemyTypesDetectionUnlocked = true;
    }
    public void SetObservationTowerEnemyAmountDetectionUnlocked() {
        observationTowerEnemyAmountDetectionUnlocked = true;
    }

    public float GetEngineerContainerSizeBuff() {
        return engineerContainerSizeBuff;
    }
    public void SetEngineerContainerSizeBuff(float buff) {
        engineerContainerSizeBuff = buff;
    }
    #endregion

    [Button]
    public void UnlockArchitectTable() {
        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.ArchitectTable.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.ArchitectTable.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.StructuresMerchant, true);
    }

    #region LEVELMERCHANTS
    public int GetSkillMerchantMaxActiveSkillsDisplayed() {
        return skillMerchantMaxActiveSkillsDisplayed;
    }
    public int GetInitialSkillMerchantMaxActiveSkillsDisplayed() {
        return initialSkillMerchantMaxActiveSkillsDisplayed;
    }
    public int GetSkillMerchantMaxPassiveSkillsDisplayed() {
        return skillMerchantMaxPassiveSkillsDisplayed;
    }
    public int GetInitialSkillMerchantMaxPassiveSkillsDisplayed() {
        return initialSkillMerchantMaxPassiveSkillsDisplayed;
    }
    public int GetTrapMerchantMaxTrapsDisplayed() {
        return trapMerchantMaxTrapsDisplayed;
    }
    public int GetInitialTrapMerchantMaxTrapsDisplayed() {
        return initialTrapMerchantMaxTrapsDisplayed;
    }
    public int GetTrapMerchantMaxTrapUpgradesDisplayed() {
        return trapMerchantMaxTrapUpgradesDisplayed;
    }
    public int GetInitialTrapMerchantMaxTrapUpgradesDisplayed() {
        return initialTrapMerchantMaxTrapUpgradeDisplayed;
    }
    public int GetStartWithRandomTrapAmount() {
        return startWithRandomTrapAmount;
    }
    public int GetInitialStartWithRandomTrapAmount() {
        return initialStartWithRandomTrapAmount;
    }

    public void SetSkillsMerchantMaxActiveSkillsDisplayed(int skillsDisplayBuff) {
        skillMerchantMaxActiveSkillsDisplayed = initialSkillMerchantMaxActiveSkillsDisplayed + skillsDisplayBuff;
    }
    public void SetSkillsMerchantMaxPassiveSkillsDisplayed(int skillsDisplayBuff) {
        skillMerchantMaxPassiveSkillsDisplayed = initialSkillMerchantMaxPassiveSkillsDisplayed + skillsDisplayBuff;
    }
    public void SetTrapsMerchantMaxTrapsDisplayed(int trapsDisplayBuff) {
        trapMerchantMaxTrapsDisplayed = initialTrapMerchantMaxTrapsDisplayed + trapsDisplayBuff;
    }
    public void SetTrapsMerchantMaxTrapUpgradesDisplayed(int skillsDisplayBuff) {
        trapMerchantMaxTrapUpgradesDisplayed = initialTrapMerchantMaxTrapUpgradeDisplayed + skillsDisplayBuff;
    }
    public void SetStartWithRandomTrapAmount(int trapAmountBuff) {
        startWithRandomTrapAmount = initialStartWithRandomTrapAmount + trapAmountBuff;
    }

    #endregion
}
