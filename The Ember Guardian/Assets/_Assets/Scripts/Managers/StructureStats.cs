using Sirenix.OdinInspector;
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
    private int mainFireMaxFuelTreshold;
    private int initialMaxFuelTreshold = 50;
    private float secondaryFireFuelDepletionRate = 0.05f;
    private float initialSecondaryFireFuelDepletionRate = 0.05f;
    private int secondaryFireMaxFuelTreshold;
    private int initialSecondaryFireMaxFuelTreshold = 20;

    private int ammoCrafterBatchCapacity;
    private int initialAmmoCrafterBatchCapacity = 1;
    private int singleAmmoCraftDuration;
    private int initialSingleAmmoCraftDuration = 15;
    private int ammoCrafterMaxAmmoPerBatch;
    private int initialAmmoCrafterMaxAmmoPerBatch = 3;

    private int orbProcessorBatchCapacity;
    private int initialOrbProcessorBatchCapacity = 1;
    private int singleOrbCraftDuration;
    private int initialSingleOrbCraftDuration = 45;
    private int orbProcessorMaxOrbsPerBatch;
    private int initialOrbProcessorMaxOrbsPerBatch = 2;

    private int tentHealAmountPerSmallOrb;
    private int initialTentHealAmountPerSmallOrb = 1;

    private int barricadeHealthPerCrate;
    private int initialBarricadeHealthPerCrate = 8;

    private int skillMerchantMaxActiveSkillsDisplayed;
    private int initialSkillMerchantMaxActiveSkillsDisplayed = 1;
    private int skillMerchantMaxPassiveSkillsDisplayed;
    private int initialSkillMerchantMaxPassiveSkillsDisplayed = 2;
    private int trapMerchantMaxTrapsDisplayed;
    private int initialTrapMerchantMaxTrapsDisplayed = 1;
    private int trapMerchantMaxTrapUpgradesDisplayed;
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
        barricadesSpiked = ES3.Load("barricadesSpiked", false);
        startWithAmmoCrafter = ES3.Load("startWithAmmoCrafter", false);
        startWithResearchTower = ES3.Load("startWithResearchTower", false);
        startWithBarricadeLayer = ES3.Load("startWithBarricadeLayer", false);

        orbFuelValue = ES3.Load("orbFuelValue", initialOrbFuelValue);
        mainFireMaxFuelTreshold = ES3.Load("maxFuelTreshold", initialMaxFuelTreshold);
        mainFireFuelDepletionRate = ES3.Load("fuelDepletionRate", initialFuelDepletionRate);
        secondaryFireMaxFuelTreshold = ES3.Load("secondaryFireMaxFuelTreshold", initialSecondaryFireMaxFuelTreshold);
        secondaryFireFuelDepletionRate = ES3.Load("secondaryFireFuelDepletionRate", initialSecondaryFireFuelDepletionRate);

        ammoCrafterBatchCapacity = ES3.Load("ammoCrafterBatchCapacity", initialAmmoCrafterBatchCapacity);
        singleAmmoCraftDuration = ES3.Load("singleAmmoCraftDuration", initialSingleAmmoCraftDuration);
        ammoCrafterMaxAmmoPerBatch = ES3.Load("ammoCrafterMaxAmmoPerBatch", initialAmmoCrafterMaxAmmoPerBatch);

        orbProcessorBatchCapacity = ES3.Load("orbProcessorBatchCapacity", initialOrbProcessorBatchCapacity);
        singleOrbCraftDuration = ES3.Load("singleOrbCraftDuration", initialSingleOrbCraftDuration);
        orbProcessorMaxOrbsPerBatch = ES3.Load("orbProcessorMaxOrbsPerBatch", initialOrbProcessorMaxOrbsPerBatch);

        barricadeHealthPerCrate = ES3.Load("barricadeHealthPerCrate", initialBarricadeHealthPerCrate);

        tentHealAmountPerSmallOrb = ES3.Load("tentHealAmountPerSmallOrb", initialTentHealAmountPerSmallOrb);

        skillMerchantMaxActiveSkillsDisplayed = ES3.Load("skillMerchantMaxActiveSkillsDisplayed", initialSkillMerchantMaxActiveSkillsDisplayed);
        skillMerchantMaxPassiveSkillsDisplayed = ES3.Load("skillMerchantMaxPassiveSkillsDisplayed", initialSkillMerchantMaxPassiveSkillsDisplayed);
        trapMerchantMaxTrapsDisplayed = ES3.Load("trapMerchantMaxTrapsDisplayed", initialTrapMerchantMaxTrapsDisplayed);
        trapMerchantMaxTrapUpgradesDisplayed = ES3.Load("trapMerchantMaxTrapUpgradesDisplayed", initialTrapMerchantMaxTrapUpgradeDisplayed);
        startWithRandomTrapAmount = ES3.Load("startWithRandomTrapAmount", initialStartWithRandomTrapAmount);
        engineerContainerSizeBuff = ES3.Load("engineerContainerSizeBuff", 0f);

        observationTowerEnemyTypesDetectionUnlocked = ES3.Load("researchTowerEnemyTypesDetectionUnlocked", false);
        observationTowerEnemyAmountDetectionUnlocked = ES3.Load("researchTowerEnemyAmountDetectionUnlocked", false);
    }

    public void SaveStructureStats() {
        ES3.Save("barricadesSpiked", barricadesSpiked);
        ES3.Save("startWithAmmoCrafter", startWithAmmoCrafter);
        ES3.Save("startWithResearchTower", startWithResearchTower);
        ES3.Save("startWithBarricadeLayer", startWithBarricadeLayer);

        ES3.Save("orbFuelValue", orbFuelValue);
        ES3.Save("mainFireMaxFuelTreshold", mainFireMaxFuelTreshold);
        ES3.Save("mainFireFuelDepletionRate", mainFireFuelDepletionRate);
        ES3.Save("secondaryFireFuelDepletionRate", secondaryFireFuelDepletionRate);
        ES3.Save("secondaryFireMaxFuelTreshold", secondaryFireMaxFuelTreshold);

        ES3.Save("ammoCrafterBatchCapacity", ammoCrafterBatchCapacity);
        ES3.Save("singleAmmoCraftDuration", singleAmmoCraftDuration);
        ES3.Save("ammoCrafterMaxAmmoPerBatch", ammoCrafterMaxAmmoPerBatch);

        ES3.Save("orbProcessorBatchCapacity", orbProcessorBatchCapacity);
        ES3.Save("singleOrbCraftDuration", singleOrbCraftDuration);
        ES3.Save("orbProcessorMaxOrbsPerBatch", orbProcessorMaxOrbsPerBatch);

        ES3.Save("tentHealAmountPerSmallOrb", tentHealAmountPerSmallOrb);

        ES3.Save("barricadeHealthPerCrate", barricadeHealthPerCrate);

        ES3.Save("skillMerchantMaxActiveSkillsDisplayed", skillMerchantMaxActiveSkillsDisplayed);
        ES3.Save("skillMerchantMaxPassiveSkillsDisplayed", skillMerchantMaxPassiveSkillsDisplayed);
        ES3.Save("trapMerchantMaxTrapsDisplayed", trapMerchantMaxTrapsDisplayed);
        ES3.Save("trapMerchantMaxTrapUpgradesDisplayed", trapMerchantMaxTrapUpgradesDisplayed);
        ES3.Save("startWithRandomTrapAmount", startWithRandomTrapAmount);

        ES3.Save("engineerContainerSizeBuff", engineerContainerSizeBuff);

        ES3.Save("researchTowerEnemyTypesDetectionUnlocked", observationTowerEnemyTypesDetectionUnlocked);
        ES3.Save("researchTowerEnemyAmountDetectionUnlocked", observationTowerEnemyAmountDetectionUnlocked);
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
        return singleAmmoCraftDuration;
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
