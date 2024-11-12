using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrenciesManager : MonoBehaviour
{
    public static CurrenciesManager Instance;

    [SerializeField] private Transform blueOrbPrefab;

    private void Awake() {
        Instance = this;
    }

    public Transform GetCurrencyPrefab(PlayerCurrencies.CurrencyType currencyType) {
        if(currencyType == PlayerCurrencies.CurrencyType.blueOrb) {
            return blueOrbPrefab;
        }
        return blueOrbPrefab;
    }
}
