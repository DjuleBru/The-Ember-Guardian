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
    [SerializeField] private Transform ammoPrefab;
    [SerializeField] private Transform emberPrefab;

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
        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            return ammoPrefab;
        }
        if (currencyType == PlayerCurrencies.CurrencyType.ember) {
            return emberPrefab;
        }
        return bigBlueOrbPrefab;
    }
}
