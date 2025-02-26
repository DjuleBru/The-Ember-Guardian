using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrenciesManager : MonoBehaviour
{
    public static CurrenciesManager Instance;

    [SerializeField] private Transform bigBlueOrbPrefab;
    [SerializeField] private Transform smallBlueOrbPrefab;
    [SerializeField] private Transform bigRedOrbPrefab;
    [SerializeField] private Transform smallRedOrbPrefab;
    [SerializeField] private Transform greenGemPrefab;
    [SerializeField] private Transform redGemPrefab;
    [SerializeField] private Transform blueGemPrefab;
    [SerializeField] private Transform yellowGemPrefab;
    [SerializeField] private Transform purpleGemPrefab;
    [SerializeField] private Transform ammoPrefab;
    [SerializeField] private Transform emberPrefab;
    [SerializeField] private Transform bearTrapPrefab;
    [SerializeField] private Transform bladeTrapPrefab;
    [SerializeField] private Transform smokeEjectorTrapPrefab;
    [SerializeField] private Transform spikeEjectorTrapPrefab;
    [SerializeField] private Transform shockEjectorTrapPrefab;

    private void Awake() {
        Instance = this;
    }

    public Transform GetCurrencyPrefab(PlayerCurrencies.CurrencyType currencyType) {
        if(currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            return bigBlueOrbPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            return smallBlueOrbPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.bigRedOrb) {
            return bigRedOrbPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.smallRedOrb) {
            return smallRedOrbPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.redGem) {
            return redGemPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.greenGem) {
            return greenGemPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.blueGem) {
            return blueGemPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.yellowGem) {
            return yellowGemPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.purpleGem) {
            return purpleGemPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            return ammoPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.ember) {
            return emberPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.bearTrap) {
            return bearTrapPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.bladeTrap) {
            return bladeTrapPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.shockerEjector) {
            return shockEjectorTrapPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.spikeEjector) {
            return spikeEjectorTrapPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.smokeEjector) {
            return smokeEjectorTrapPrefab;
        }
        return bigBlueOrbPrefab;
    }

    public Transform GetCurrencyPrefab(TrapItem.TrapType trapType) {
        if (trapType == TrapItem.TrapType.bearTrap) {
            return bearTrapPrefab;
        }
        if (trapType == TrapItem.TrapType.bladeTrap) {
            return bladeTrapPrefab;
        }
        if (trapType == TrapItem.TrapType.smokeEjector) {
            return smokeEjectorTrapPrefab;
        }
        if (trapType == TrapItem.TrapType.spikeEjector) {
            return spikeEjectorTrapPrefab;
        }
        if (trapType == TrapItem.TrapType.shockerEjector) {
            return shockEjectorTrapPrefab;
        }
        return bearTrapPrefab;
    }

    public PlayerCurrencies.CurrencyCategory GetCurrencyCategory(PlayerCurrencies.CurrencyType currencyType) {
        PlayerCurrencies.CurrencyCategory category = PlayerCurrencies.CurrencyCategory.orb;

        if(currencyType == PlayerCurrencies.CurrencyType.ammo) {
            category = PlayerCurrencies.CurrencyCategory.ammo;
        }

        if(currencyType == PlayerCurrencies.CurrencyType.greenGem || currencyType == PlayerCurrencies.CurrencyType.redGem || currencyType == PlayerCurrencies.CurrencyType.blueGem || currencyType == PlayerCurrencies.CurrencyType.yellowGem || currencyType == PlayerCurrencies.CurrencyType.purpleGem) {
            category = PlayerCurrencies.CurrencyCategory.gem;
        }

        return category;
    }

    public List<PlayerCurrencies.CurrencyType> GetCurrencyTypesInCategory(PlayerCurrencies.CurrencyCategory currencyCategory) {

        List<PlayerCurrencies.CurrencyType> currencyTypesInCategory = new List<PlayerCurrencies.CurrencyType>();

        if (currencyCategory == PlayerCurrencies.CurrencyCategory.orb) {
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.smallBlueOrb);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.bigRedOrb);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.smallRedOrb);
        }

        if (currencyCategory == PlayerCurrencies.CurrencyCategory.gem) {
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.purpleGem);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.yellowGem);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.blueGem);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.greenGem);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.redGem);
        }

        if (currencyCategory == PlayerCurrencies.CurrencyCategory.trap) {
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.bearTrap);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.bladeTrap);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.shockerEjector);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.smokeEjector);
            currencyTypesInCategory.Add(PlayerCurrencies.CurrencyType.spikeEjector);
        }

        return currencyTypesInCategory;
    }
}
