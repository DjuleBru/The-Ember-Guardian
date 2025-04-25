using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureStats : MonoBehaviour
{

    public static StructureStats Instance;

    private float orbFuelValue = 10;
    private float initialOrbFuelValue = 10;
    private float fuelDepletionRate = 0.05f;
    private float initialFuelDepletionRate = 0.05f;
    private int maxFuelTreshold = 140;
    private int initialMaxFuelTreshold = 140;

    private int ammoCrafterBatchCapacity;
    private int initialAmmoCrafterBatchCapacity = 1;
    private int singleAmmoCraftDuration;
    private int initialSingleAmmoCraftDuration = 15;
    private int ammoCrafterMaxAmmoPerBatch;
    private int initialAmmoCrafterMaxAmmoPerBatch = 3;

    private int tentHealAmountPerSmallOrb;
    private int initialTentHealAmountPerSmallOrb = 1;

    private int barricadeHealthPerCrate;
    private int initialBarricadeHealthPerCrate = 8;

    private bool barricadesSpiked;
    private bool startWithAmmoCrafter;
    private bool startWithResearchTower;
    private bool startWithBarricadeLayer;

    private void Awake() {
        Instance = this;
        LoadStructureStats();
    }

    private void LoadStructureStats() {
        barricadesSpiked = ES3.Load("barricadesSpiked", false);
        startWithAmmoCrafter = ES3.Load("startWithAmmoCrafter", false);
        startWithResearchTower = ES3.Load("startWithResearchTower", false);
        startWithBarricadeLayer = ES3.Load("startWithBarricadeLayer", false);

        maxFuelTreshold = ES3.Load("maxFuelTreshold", initialMaxFuelTreshold);
        orbFuelValue = ES3.Load("orbFuelValue", initialOrbFuelValue);
        fuelDepletionRate = ES3.Load("fuelDepletionRate", initialFuelDepletionRate);

        ammoCrafterBatchCapacity = ES3.Load("ammoCrafterBatchCapacity", initialAmmoCrafterBatchCapacity);
        singleAmmoCraftDuration = ES3.Load("singleAmmoCraftDuration", initialSingleAmmoCraftDuration);
        ammoCrafterMaxAmmoPerBatch = ES3.Load("ammoCrafterMaxAmmoPerBatch", initialAmmoCrafterMaxAmmoPerBatch);

        barricadeHealthPerCrate = ES3.Load("barricadeHealthPerCrate", initialBarricadeHealthPerCrate);

        tentHealAmountPerSmallOrb = ES3.Load("tentHealAmountPerSmallOrb", initialTentHealAmountPerSmallOrb);
    }

    public void SaveStructureStats() {
        ES3.Save("barricadesSpiked", barricadesSpiked);
        ES3.Save("startWithAmmoCrafter", startWithAmmoCrafter);
        ES3.Save("startWithResearchTower", startWithResearchTower);
        ES3.Save("startWithBarricadeLayer", startWithBarricadeLayer);

        ES3.Save("maxFuelTreshold", maxFuelTreshold);
        ES3.Save("orbFuelValue", orbFuelValue);
        ES3.Save("fuelDepletionRate", fuelDepletionRate);

        ES3.Save("ammoCrafterBatchCapacity", ammoCrafterBatchCapacity);
        ES3.Save("singleAmmoCraftDuration", singleAmmoCraftDuration);
        ES3.Save("ammoCrafterMaxAmmoPerBatch", ammoCrafterMaxAmmoPerBatch);

        ES3.Save("tentHealAmountPerSmallOrb", tentHealAmountPerSmallOrb);

        ES3.Save("barricadeHealthPerCrate", barricadeHealthPerCrate);
    }

    #region FIRE

    public int GetMaxFuelTreshold() {
        return maxFuelTreshold;
    }
    public float GetOrbFuelValue() {
        return orbFuelValue;
    }
    public float GetFuelDepletionRate() {
        return fuelDepletionRate;
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
        maxFuelTreshold = initialMaxFuelTreshold + maxFuelTresholdBuff;
    }
    public void SetOrbFuelValueBuff(int orbFuelValueBuff) {
        orbFuelValue = initialOrbFuelValue + orbFuelValueBuff;
    }
    public void SetFuelDepletionRateBuff(float fuelDepletionRateBuff) {
        fuelDepletionRate = initialFuelDepletionRate + fuelDepletionRateBuff/60f;
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
}
