using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemDropManager : MonoBehaviour
{
    public static GemDropManager Instance;

    [SerializeField] private List<HubMerchant> allHubMerchantsInHub;

    private int baseGemDropPool;

    private int totalChestGems;
    private int totalGemsDropPoolPerRun;
    private int currentGemDropPool;

    private bool isHubScene;
    private bool isLevelScene;
    private int totalRedGemCosts;
    private int totalGreenGemCosts;
    private int totalYellowGemCosts;
    private int totalBlueGemCosts;
    private int totalCyanGemCosts;
    private int totalPurpleGemCosts;

    private float redGemCostsMultiplier = .5f;

    private List<PlayerCurrencies.CurrencyType> gemTypesDroppedUnlocked;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        gemTypesDroppedUnlocked = ES3.Load("gemTypesDroppedUnlocked", new List<PlayerCurrencies.CurrencyType>());
        isHubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;
        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;


        if (isLevelScene) {
            baseGemDropPool = LevelManager.Instance.GetLevelSO().gemDropPool;
            if (LevelManager.Instance.GetLevelSO().unlocksNewGemType) {
                List<PlayerCurrencies.CurrencyType> newGemTypeList = LevelManager.Instance.GetLevelSO().newGemTypeUnlockedByLevelList;

                foreach (PlayerCurrencies.CurrencyType newGemType in newGemTypeList) {

                    if (!gemTypesDroppedUnlocked.Contains(newGemType)) {
                        gemTypesDroppedUnlocked.Add(newGemType);
                        ES3.Save("gemTypesDroppedUnlocked", gemTypesDroppedUnlocked);
                    }

                }
            }

            totalRedGemCosts = ES3.Load("totalRedGemCosts", 0);
            totalBlueGemCosts = ES3.Load("totalBlueGemCosts", 0);
            totalGreenGemCosts = ES3.Load("totalGreenGemCosts", 0);
            totalYellowGemCosts = ES3.Load("totalYellowGemCosts", 0);
            totalPurpleGemCosts = ES3.Load("totalPurpleGemCosts", 0);
            totalCyanGemCosts = ES3.Load("totalCyanGemCosts", 0);

            StartCoroutine(UpdateLevelGems());
        }

        if(isHubScene) {
            StartCoroutine(UpdateTotalGemTypes());
        }
    }

    private IEnumerator UpdateLevelGems() {
        yield return new WaitForSeconds(1f);
        totalGemsDropPoolPerRun = Mathf.RoundToInt(baseGemDropPool - totalChestGems);
        currentGemDropPool = totalGemsDropPoolPerRun;
    }

    private IEnumerator UpdateTotalGemTypes() {
        yield return new WaitForSeconds(1f);
        foreach (HubMerchant merchant in allHubMerchantsInHub) {
            totalRedGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.redGem);
            totalBlueGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.blueGem);
            totalGreenGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.greenGem);
            totalYellowGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.yellowGem);
            totalPurpleGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.purpleGem);
            totalCyanGemCosts += merchant.CountAllGemCosts(PlayerCurrencies.CurrencyType.cyanGem);
        }

        ES3.Save("totalRedGemCosts", totalRedGemCosts);
        ES3.Save("totalBlueGemCosts", totalBlueGemCosts);
        ES3.Save("totalGreenGemCosts", totalGreenGemCosts);
        ES3.Save("totalYellowGemCosts", totalYellowGemCosts);
        ES3.Save("totalPurpleGemCosts", totalPurpleGemCosts);
        ES3.Save("totalCyanGemCosts", totalCyanGemCosts);

    }

    public bool TrySpendFromDropPool(int requestedAmount, out int finalDropAmount) {
        if (currentGemDropPool <= 0) {
            finalDropAmount = 0;
            return false;
        }

        finalDropAmount = Mathf.Min(requestedAmount, currentGemDropPool);
        currentGemDropPool -= finalDropAmount;
        return finalDropAmount > 0;
    }

    public PlayerCurrencies.CurrencyType GetBalancedGemType() {
        Dictionary<PlayerCurrencies.CurrencyType, int> demandPerType = new Dictionary<PlayerCurrencies.CurrencyType, int> {
        { PlayerCurrencies.CurrencyType.redGem, (int)(totalRedGemCosts*redGemCostsMultiplier) },
        { PlayerCurrencies.CurrencyType.blueGem, totalBlueGemCosts },
        { PlayerCurrencies.CurrencyType.greenGem, totalGreenGemCosts },
        { PlayerCurrencies.CurrencyType.yellowGem, totalYellowGemCosts },
        { PlayerCurrencies.CurrencyType.purpleGem, totalPurpleGemCosts },
        { PlayerCurrencies.CurrencyType.cyanGem, totalCyanGemCosts },
    };

        // Filtrer selon les types débloqués dans ce niveau
        Dictionary<PlayerCurrencies.CurrencyType, int> filtered = new Dictionary<PlayerCurrencies.CurrencyType, int>();
        foreach (var kvp in demandPerType) {
            if (gemTypesDroppedUnlocked.Contains(kvp.Key)) {
                filtered[kvp.Key] = kvp.Value;
            }
        }

        // Si aucun type valide (cas rare), fallback
        if (filtered.Count == 0) {
            return PlayerCurrencies.CurrencyType.greenGem;
        }

        // Pondérer le choix en fonction de la demande relative
        int total = 0;
        foreach (var val in filtered.Values) total += val;

        float r = UnityEngine.Random.value;
        float cumulative = 0f;

        foreach (var kvp in filtered) {
            float weight = (float)kvp.Value / total;
            cumulative += weight;
            if (r <= cumulative) {
                return kvp.Key;
            }
        }

        // Sécurité
        return filtered.Keys.First();
    }

    public void RecordChestGems(int gemAmount) {
        totalChestGems += gemAmount;
    }
}
