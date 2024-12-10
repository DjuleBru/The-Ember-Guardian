using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;

    #region TUTORIAL
    public bool tutorialComplete { get; private set; }
    public bool hubLoadedOnce { get; private set; }
    #endregion

    #region HUB
    [SerializeField] private Portal defaultLastHUBPortalUsedByPlayer;
    public int lastHUBPortalUsedByPlayer { get; private set; }
    #endregion

    #region CURRENCIES
    public int greenGemAmountFromLastLevel { get; private set; }
    public int redGemAmountFromLastLevel { get; private set; }
    public bool gemsRewardedFromlastLevel { get; private set; }
    public List<Vector3> hubGreenGemPositions { get; private set; }
    public List<Vector3> hubRedGemPositions { get; private set; }
    #endregion

    #region METAMERCHANTS

    #endregion

    private void Awake() {
        Instance = this;

        tutorialComplete = ES3.Load("tutorialComplete", false);
        hubLoadedOnce = ES3.Load("hubLoadedOnce", false);

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB && hubLoadedOnce) {
            int lastPortal = defaultLastHUBPortalUsedByPlayer.GetPortalNumber();
            lastHUBPortalUsedByPlayer = ES3.Load("lastHUBPortalUsedByPlayer", lastPortal);
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.H)) {
            //SaveHubGems();
        }
    }

    #region SAVE TUTORIAL
    public void SetTutorialCompleted() {
        ES3.Save("tutorialComplete", true);
    }

    public void SetHubLoadedOnce() {
        ES3.Save("hubLoadedOnce", true);
    }
    #endregion

    #region SAVE HUB
    public void SetAsLastPortalUsedByPlayer(int portalNumber) {
        ES3.Save("lastHUBPortalUsedByPlayer", portalNumber);
    }

    public void SetPortalUnlocked(string portalName) {
        string key = portalName + "_Unlocked";
        ES3.Save(key, true);
    }

    public bool GetPortalUnlocked(string portalName) {
        string key = portalName + "_Unlocked";
        return ES3.Load(key, false);
    }
    #endregion

    #region SAVE/GET CURRENCIES

    public int GetGreenGemAmountFromLastLevel() {
        return ES3.Load("greenGemAmountFromLastLevel", 0);
    }
    public int GetRedGemAmountFromLastLevel() {
        return ES3.Load("redGemAmountFromLastLevel", 0);
    }

    public void SetGreenGemAmountFromLevel(int greenGemAmount) {
        Debug.Log("SetGreenGemAmountFromLevel " + greenGemAmount);
        ES3.Save("greenGemAmountFromLastLevel", greenGemAmount);
        ES3.Save("gemsRewardedFromlastLevel", false);
    }
    public void SetRedGemAmountFromLevel(int redGemAmount) {
        Debug.Log("SetRedGemAmountFromLevel " + redGemAmount);
        ES3.Save("redGemAmountFromLastLevel", redGemAmount);
        ES3.Save("gemsRewardedFromlastLevel", false);
    }

    public void SetGemsRewarded() {
        ES3.Save("gemsRewardedFromlastLevel", true);
    }

    public bool GetGemFromLastLevelRewarded() {
        return ES3.Load("gemsRewardedFromlastLevel", false);
    }

    public void SaveHubGems() {
        Debug.Log("save hub gems");

        List<Vector3> greenGemPositions = UICurrencyManager.Instance.GetCurrencyPositions(PlayerCurrencies.CurrencyType.greenGem);
        List<Vector3> redGemPositions = UICurrencyManager.Instance.GetCurrencyPositions(PlayerCurrencies.CurrencyType.redGem);

        ES3.Save("greenGemPositions", greenGemPositions);
        ES3.Save("redGemPositions", redGemPositions);
    }

    public List<Vector3> GetGreenGemPositions() {
        return ES3.Load("greenGemPositions", new List<Vector3>());
    }

    public List<Vector3> GetRedGemPositions() {
        return ES3.Load("redGemPositions", new List<Vector3>());
    }
    #endregion


}
