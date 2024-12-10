using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager : MonoBehaviour
{
    public static HUBManager Instance;

    public bool DEBUGMODE;
    public int debugGreenGemAmount;
    public int debugRedGemAmount;

    [SerializeField] private Transform firstHubLoadPlayerSpawnPoint;
    [SerializeField] private Portal firstPortalUnlocked;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if(!MetaProgressionManager.Instance.hubLoadedOnce) {
            MetaProgressionManager.Instance.SetHubLoadedOnce();
            firstPortalUnlocked.UnlockPortal();
            Player.Instance.transform.position = firstHubLoadPlayerSpawnPoint.transform.position;
            RewardLastLevelGems();
        }

        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetRedGemPositions();
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGreenGemPositions();
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions);
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions);


        if(DEBUGMODE) {
            //UICurrencyManager.Instance.AddCurrencyAmount(PlayerCurrencies.CurrencyType.redGem, debugRedGemAmount);
            //UICurrencyManager.Instance.AddCurrencyAmount(PlayerCurrencies.CurrencyType.greenGem, debugGreenGemAmount);
        }
    }

    public void RewardLastLevelGems() {
        if (!MetaProgressionManager.Instance.GetGemFromLastLevelRewarded()) {
            int redGemAmount = MetaProgressionManager.Instance.GetRedGemAmountFromLastLevel();
            int greenGemAmount = MetaProgressionManager.Instance.GetGreenGemAmountFromLastLevel();

            List<PlayerCurrencies.CurrencyType> currencyTypes = new List<PlayerCurrencies.CurrencyType> {
                PlayerCurrencies.CurrencyType.greenGem,
                PlayerCurrencies.CurrencyType.redGem,
            };
            List<int> currencyTypesAmount = new List<int> {
                greenGemAmount,
                redGemAmount,
            };


            UICurrencyManager.Instance.AddMultipleCurrencies(currencyTypes, currencyTypesAmount);
            MetaProgressionManager.Instance.SetGemsRewarded();
        }
    }

    private void OnApplicationQuit() {
        MetaProgressionManager.Instance.SaveHubGems();
    }
}
