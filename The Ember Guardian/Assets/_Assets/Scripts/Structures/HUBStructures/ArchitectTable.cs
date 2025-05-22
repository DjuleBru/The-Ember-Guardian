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
    private int maxSniperTowerAmount;
    private int maxMachineGunTowerAmount;
    private int maxMortarPositionsAmount;

    private int initialMaxAmmoCrafterAmount = 1;
    private int initialMaxSecondaryFireAmount = 2;
    private int initialMaxTrapSlotsAmount = 6;
    private int initialMaxSniperTowerAmount = 1;
    private int initialMaxMachineGunTowerAmount = 1;
    private int initialMaxMortarPositionsAmount = 1;

    [SerializeField] private GameObject activeHubMerchantGameObject;
    [SerializeField] private GameObject inActiveHubMerchantGameObject;
    private HubMerchant architectTableHubMerchant;

    private void Awake() {
        Instance = this;

        architectTableHubMerchant= GetComponent<HubMerchant>();
        LoadStats();
    }

    private void LoadStats() {
        architectTableUnlocked = ES3.Load("architectTableUnlocked", false);

        maxAmmoCrafterAmount = ES3.Load("maxAmmoCrafterAmount", initialMaxAmmoCrafterAmount);
        maxSecondaryFireAmount = ES3.Load("maxSecondaryFireAmount", initialMaxSecondaryFireAmount);
        maxTrapSlotsAmount = ES3.Load("maxTrapSlotsAmount", initialMaxTrapSlotsAmount);
        maxSniperTowerAmount = ES3.Load("maxSniperTowerAmount", initialMaxSniperTowerAmount);
        maxMachineGunTowerAmount = ES3.Load("maxMachineGunTowerAmount", initialMaxMachineGunTowerAmount);
        maxMortarPositionsAmount = ES3.Load("maxMortarPositionsAmount", initialMaxMortarPositionsAmount);
    }

    public void SaveStats() {
        ES3.Save("architectTableUnlocked", architectTableUnlocked);
        ES3.Save("maxAmmoCrafterAmount", maxAmmoCrafterAmount);
        ES3.Save("maxSecondaryFireAmount", maxSecondaryFireAmount);
        ES3.Save("maxTrapSlotsAmount", maxTrapSlotsAmount);
        ES3.Save("maxSniperTowerAmount", maxSniperTowerAmount);
        ES3.Save("maxMachineGunTowerAmount", maxMachineGunTowerAmount);
        ES3.Save("maxMortarPositionsAmount", maxMortarPositionsAmount);

        CampEditManager.Instance.SaveCampLayout();

        if(architectTableUnlocked) {
            MetaProgressionManager.Instance.SetMerchantUnlocked(HubMerchant.HubMerchantType.ArchitectTable);
        }
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
    public void SetMaxSniperTowerAmountBuff(int amountBuff) {
        maxSniperTowerAmount = initialMaxSniperTowerAmount + amountBuff;
    }
    public void SetMaxMachineGunTowerAmountBuff(int amountBuff) {
        maxMachineGunTowerAmount = initialMaxMachineGunTowerAmount + amountBuff;
    }
    public void SetMaxMortarPositionsAmountBuff(int amountBuff) {
        maxMortarPositionsAmount = initialMaxMortarPositionsAmount + amountBuff;
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
    public int GetMaxSniperTowerAmount() {
        return maxSniperTowerAmount;
    }
    public int GetMachineGunTowerAmount() {
        return maxMachineGunTowerAmount;
    }
    public int GetMortarPositionsAmount() {
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
    public int GetInitialMaxSniperTowerAmount() {
        return initialMaxSniperTowerAmount;
    }
    public int GetInitialMachineGunTowerAmount() {
        return initialMaxMachineGunTowerAmount;
    }
    public int GetInitialMortarPositionsAmount() {
        return initialMaxMortarPositionsAmount;
    }
    #endregion
}
