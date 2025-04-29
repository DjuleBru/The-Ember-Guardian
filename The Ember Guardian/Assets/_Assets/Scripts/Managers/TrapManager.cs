using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance;

    [SerializeField] private List<TrapSO> allTrapSOList;
    [SerializeField] private List<TrapItem.TrapType> allTrapTypes;
    [SerializeField] private List<TrapUpgradeSO> allTrapUpgradeSOs; // Liste de tous les ScriptableObjects d'upgrades
    private Dictionary<TrapItem.TrapType, Dictionary<TrapUpgradeSO.TrapUpgradeType, int>> trapUpgradesLevels = new();

    [SerializeField] private List<TrapSO> trapSOList;
    [SerializeField] private List<TrapSO> initialTrapsUnlockedList;
    private List<TrapSO> newTrapsUnlockedList = new List<TrapSO>();
    private List<TrapSO> trapsUnlockedList = new List<TrapSO>();
    private List<TrapSO> trapsAndUpgradesUnlockedList = new List<TrapSO>();

    private List<TrapItem.TrapType> trapTypesBoughtByPlayer = new List<TrapItem.TrapType>();

    private void Awake() {
        Instance = this;
        InitializeTraps(allTrapTypes);
        LoadUnlockedTraps();
    }


    public void InitializeTraps(List<TrapItem.TrapType> allTrapTypes) {
        foreach (var trapType in allTrapTypes) {
            trapUpgradesLevels[trapType] = new Dictionary<TrapUpgradeSO.TrapUpgradeType, int>();
            foreach (TrapUpgradeSO.TrapUpgradeType upgradeType in Enum.GetValues(typeof(TrapUpgradeSO.TrapUpgradeType))) {
                trapUpgradesLevels[trapType][upgradeType] = 0; // Niveau initial par défaut
            }
        }
    }

    public void SetTrapUpgradeLevel(TrapItem.TrapType trapType, TrapUpgradeSO.TrapUpgradeType upgradeType, int level) {
        if (trapUpgradesLevels.ContainsKey(trapType) && trapUpgradesLevels[trapType].ContainsKey(upgradeType)) {
            trapUpgradesLevels[trapType][upgradeType] = level;
        }
    }

    public int GetTrapUpgradeLevel(TrapItem.TrapType trapType, TrapUpgradeSO.TrapUpgradeType upgradeType) {
        if (trapUpgradesLevels.TryGetValue(trapType, out var upgradeDict) && upgradeDict.TryGetValue(upgradeType, out var level)) {
            return level;
        }
        return 0; // Retourne un niveau par défaut si non trouvé
    }

    public float GetCurrentUpgradeValue(TrapItem.TrapType trapType, TrapUpgradeSO.TrapUpgradeType upgradeType) {
        if (trapUpgradesLevels.TryGetValue(trapType, out var upgradeDict) &&
            upgradeDict.TryGetValue(upgradeType, out var level)) {
            // Trouver l'UpgradeSO correspondant
            TrapUpgradeSO upgradeSO = allTrapUpgradeSOs.Find(upg => upg.linkedTrapType == trapType && upg.trapUpgradeType == upgradeType);
            if (upgradeSO != null) {
                return upgradeSO.GetValueAtLevel(level);
            }
        }
        return 0f; // Retourne 0 si l'amélioration ou le niveau est invalide
    }

    public List<TrapSO> GetAllTrapSOList() {
        return allTrapSOList;
    }

    public TrapItem.TrapType GetTrapType(PlayerCurrencies.CurrencyType currencyType) {
        if (currencyType == PlayerCurrencies.CurrencyType.bearTrap) {
            return TrapItem.TrapType.bearTrap;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.bladeTrap) {
            return TrapItem.TrapType.bladeTrap;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.shockerEjector) {
            return TrapItem.TrapType.shockerEjector;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.smokeEjector) {
            return TrapItem.TrapType.smokeEjector;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.spikeEjector) {
            return TrapItem.TrapType.spikeEjectorSmall;
        }
        return TrapItem.TrapType.bearTrap;
    }

    public TrapSO GetTrapSO(TrapItem.TrapType trapType) {
        foreach (TrapSO trapSO in allTrapSOList) {
            if (trapSO.trapType == trapType) return trapSO;
        }
        return allTrapSOList[0];
    }

    public List<TrapUpgradeSO> GetTrapUpgradeSOList() {
        return allTrapUpgradeSOs;
    }

    public void AddTrapTypeBought(TrapItem.TrapType trapType) {
        if(!trapTypesBoughtByPlayer.Contains(trapType)) {
            trapTypesBoughtByPlayer.Add(trapType);
        }
    }

    public bool TrapTypeBoughtByPlayer(TrapItem.TrapType trapType) {
        return trapTypesBoughtByPlayer.Contains(trapType);
    }


    #region SAVE_LOAD HUB
    public void HUBUnlockNewTrap(TrapSO trapSO) {
        newTrapsUnlockedList.Add(trapSO);
    }

    public void SaveNewUnlockedTraps() {
        foreach (TrapSO trapSO in newTrapsUnlockedList) {
            string key = trapSO.name + "_unlocked";
            ES3.Save(key, true);
        }
    }

    public void LoadUnlockedTraps() {
        // Load Traps
        foreach (TrapSO trapSO in initialTrapsUnlockedList) {
            trapsUnlockedList.Add(trapSO);
            trapsAndUpgradesUnlockedList.Add(trapSO);
        }

        foreach (TrapSO trapSO in trapSOList) {
            string key = trapSO.name + "_unlocked";
            bool unlocked = ES3.Load(key, false);

            if (unlocked) {
                trapsUnlockedList.Add(trapSO);
                trapsAndUpgradesUnlockedList.Add(trapSO);
            }
        }


        // Add Trap Upgrades linked to unlocked traps
        List<TrapSO> trapUpgradesUnlocked = new List<TrapSO>();
        foreach (TrapSO trapSOUpgrade in allTrapSOList) {
            foreach (TrapSO trapSO in trapsUnlockedList) {
                if (trapSO == trapSOUpgrade) continue;

                if (trapSOUpgrade.linkedTrapSO == trapSO) {
                    trapUpgradesUnlocked.Add(trapSOUpgrade);
                }
            }
        }

        foreach(TrapSO trapSO in trapUpgradesUnlocked) {
            trapsAndUpgradesUnlockedList.Add(trapSO);
        }
    }

    public List<TrapSO> GetUnlockedTraps() {
        return trapsUnlockedList;
    }
    public List<TrapSO> GetUnlockedTrapsAndTheirUpgrades() {
        return trapsAndUpgradesUnlockedList;
    }

    #endregion
}
