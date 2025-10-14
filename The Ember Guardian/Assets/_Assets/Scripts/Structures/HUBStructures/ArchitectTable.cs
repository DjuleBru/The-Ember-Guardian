using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchitectTable : MonoBehaviour
{
    public static ArchitectTable Instance;
    private bool architectTableUnlocked;

    private int maxAmmoCrafterAmount;
    private int maxSecondaryFireAmount;
    private int maxTrapSlotsAmount;
    private int maxTowerAmount;
    private int maxSniperTowerAmount;
    private int maxMachineGunTowerAmount;
    private int maxMortarPositionsAmount;
    private int maxFastTravelTP;

    private int initialMaxAmmoCrafterAmount = 1;
    private int initialMaxSecondaryFireAmount = 2;
    private int initialMaxTrapSlotsAmount = 6;
    private int initialMaxTowerAmount = 6;
    private int initialMaxSniperTowerAmount = 1;
    private int initialMaxMachineGunTowerAmount = 1;
    private int initialMaxMortarPositionsAmount = 1;
    private int initialMaxFastTravelTP = 1;

    [SerializeField] private GameObject activeHubMerchantGameObject;
    [SerializeField] private GameObject inActiveHubMerchantGameObject;
    [SerializeField] private GameObject allStructureBlueprintsParent;
    private HubMerchant architectTableHubMerchant;

    private void Awake() {
        Instance = this;

        architectTableHubMerchant= GetComponent<HubMerchant>();
        LoadStats();

        AddStructureBlueprint[] allStructureBlueprints = allStructureBlueprintsParent.GetComponentsInChildren<AddStructureBlueprint>();
        foreach(AddStructureBlueprint blueprint in allStructureBlueprints) {
            blueprint.SubscribeToNewItemsEvents();
        }
    }

    public void SaveStats() {
        // Dictionnaire de toutes les valeurs à sauver
        var statsToSave = new Dictionary<string, object>()
        {
        { "architectTableUnlocked", architectTableUnlocked },
        { "maxAmmoCrafterAmount", maxAmmoCrafterAmount },
        { "maxSecondaryFireAmount", maxSecondaryFireAmount },
        { "maxTrapSlotsAmount", maxTrapSlotsAmount },
        { "maxSniperTowerAmount", maxSniperTowerAmount },
        { "maxMachineGunTowerAmount", maxMachineGunTowerAmount },
        { "maxMortarPositionsAmount", maxMortarPositionsAmount },
        { "maxTowerAmount", maxTowerAmount },
        { "maxFastTravelTP", maxFastTravelTP }
    };

        // Sauvegarde batch
        ES3.Save("ArchitectTableStats", statsToSave);

        // Sauvegarde du layout du camp séparément
        CampEditManager.Instance.SaveCampLayout();

        if (architectTableUnlocked) {
            MetaProgressionManager.Instance.SetMerchantUnlocked(HubMerchant.HubMerchantType.ArchitectTable);
        }
    }

    private void LoadStats() {
        //Debug.Log("ARCHITECT TABLE LOAD STATS");
        if (!ES3.KeyExists("ArchitectTableStats")) {
            // Si aucune sauvegarde, on garde les valeurs initiales
            maxAmmoCrafterAmount = initialMaxAmmoCrafterAmount;
            maxSecondaryFireAmount = initialMaxSecondaryFireAmount;
            maxTrapSlotsAmount = initialMaxTrapSlotsAmount;
            maxSniperTowerAmount = initialMaxSniperTowerAmount;
            maxMachineGunTowerAmount = initialMaxMachineGunTowerAmount;
            maxMortarPositionsAmount = initialMaxMortarPositionsAmount;
            maxTowerAmount = initialMaxTowerAmount;
            maxFastTravelTP = initialMaxFastTravelTP;
            return;
        }

        var loadedStats = ES3.Load<Dictionary<string, object>>("ArchitectTableStats");

        architectTableUnlocked = loadedStats.ContainsKey("architectTableUnlocked") ? Convert.ToBoolean(loadedStats["architectTableUnlocked"]) : false;
        maxAmmoCrafterAmount = loadedStats.ContainsKey("maxAmmoCrafterAmount") ? Convert.ToInt32(loadedStats["maxAmmoCrafterAmount"]) : initialMaxAmmoCrafterAmount;
        maxSecondaryFireAmount = loadedStats.ContainsKey("maxSecondaryFireAmount") ? Convert.ToInt32(loadedStats["maxSecondaryFireAmount"]) : initialMaxSecondaryFireAmount;
        maxTrapSlotsAmount = loadedStats.ContainsKey("maxTrapSlotsAmount") ? Convert.ToInt32(loadedStats["maxTrapSlotsAmount"]) : initialMaxTrapSlotsAmount;
        maxSniperTowerAmount = loadedStats.ContainsKey("maxSniperTowerAmount") ? Convert.ToInt32(loadedStats["maxSniperTowerAmount"]) : initialMaxSniperTowerAmount;
        maxMachineGunTowerAmount = loadedStats.ContainsKey("maxMachineGunTowerAmount") ? Convert.ToInt32(loadedStats["maxMachineGunTowerAmount"]) : initialMaxMachineGunTowerAmount;
        maxMortarPositionsAmount = loadedStats.ContainsKey("maxMortarPositionsAmount") ? Convert.ToInt32(loadedStats["maxMortarPositionsAmount"]) : initialMaxMortarPositionsAmount;
        maxTowerAmount = loadedStats.ContainsKey("maxTowerAmount") ? Convert.ToInt32(loadedStats["maxTowerAmount"]) : initialMaxTowerAmount;
        maxFastTravelTP = loadedStats.ContainsKey("maxFastTravelTP") ? Convert.ToInt32(loadedStats["maxFastTravelTP"]) : initialMaxFastTravelTP;
    }

    public HubMerchant GetArchitectTableHubMerchant() {
        return architectTableHubMerchant;
    }

    public void SetArchitectTableUnlocked() {
        architectTableUnlocked = true;
        architectTableHubMerchant.SetMerchantUnlocked();
        activeHubMerchantGameObject.SetActive(true);
        inActiveHubMerchantGameObject.SetActive(false);

    }

    #region SET MAX STRUCTURE AMOUNTS
    public void SetMaxAmmoCrafterAmountBuff(int amountBuff) {
        maxAmmoCrafterAmount = initialMaxAmmoCrafterAmount + amountBuff;
    }
    public void SetSecondaryFireAmountBuff(int amountBuff) {
        maxSecondaryFireAmount = initialMaxSecondaryFireAmount + amountBuff;
    }
    public void SetMaxTrapSlotsAmountBuff(int amountBuff) {
        maxTrapSlotsAmount = initialMaxTrapSlotsAmount + amountBuff;
    }
    public void SetMaxTowerAmountBuff(int amountBuff) {
        maxTowerAmount = initialMaxTowerAmount + amountBuff;
    }
    public void SetMaxSniperTowerAmountBuff(int amountBuff) {
        maxSniperTowerAmount = initialMaxSniperTowerAmount + amountBuff;
    }
    public void SetMaxMachineGunTowerAmountBuff(int amountBuff) {
        maxMachineGunTowerAmount = initialMaxMachineGunTowerAmount + amountBuff;
    }
    public void SetMaxMortarPositionsAmountBuff(int amountBuff) {
        maxMortarPositionsAmount = initialMaxMortarPositionsAmount + amountBuff;
    }
    public void SetMaxFastTravelTPAmountBuff(int amountBuff) {
        maxFastTravelTP = initialMaxFastTravelTP + amountBuff;
    }
    #endregion

    #region GET MAX STRUCTURE AMOUNTS
    public int GetMaxAmmoCrafterAmount() {
        return maxAmmoCrafterAmount;
    }
    public int GetMaxSecondaryFireAmount() {
        return maxSecondaryFireAmount;
    }
    public int GetMaxTrapSlotsAmount() {
        return maxTrapSlotsAmount;
    }
    public int GetMaxTowerAmount() {
        return maxTowerAmount;
    }
    public int GetMaxSniperTowerAmount() {
        return maxSniperTowerAmount;
    }
    public int GetMachineGunTowerAmount() {
        return maxMachineGunTowerAmount;
    }
    public int GetMortarPositionsAmount() {
        return maxMortarPositionsAmount;
    }

    public int GetMaxFastTravelTPAmount() {
        return maxMortarPositionsAmount;
    }

    #endregion

    #region GET INITIAL MAX STRUCTURE AMOUNTS
    public int GetInitialMaxAmmoCrafterAmount() {
        return initialMaxAmmoCrafterAmount;
    }
    public int GetInitialMaxSecondaryFireAmount() {
        return initialMaxSecondaryFireAmount;
    }
    public int GetInitialMaxTrapSlotsAmount() {
        return initialMaxTrapSlotsAmount;
    }
    public int GetInitialMaxTowerAmount() {
        return initialMaxTowerAmount;
    }
    public int GetInitialMaxSniperTowerAmount() {
        return initialMaxSniperTowerAmount;
    }
    public int GetInitialMachineGunTowerAmount() {
        return initialMaxMachineGunTowerAmount;
    }
    public int GetInitialMortarPositionsAmount() {
        return initialMaxMortarPositionsAmount;
    }
    public int GetInitialMaxFastTravelTPAmount() {
        return initialMaxFastTravelTP;
    }
    #endregion
}
