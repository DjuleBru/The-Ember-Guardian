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

    private List<TrapItem.TrapType> trapTypesBoughtByPlayer = new List<TrapItem.TrapType>();

    private void Awake() {
        Instance = this;
        InitializeTraps(allTrapTypes);
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
            return TrapItem.TrapType.spikeEjector;
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
}
