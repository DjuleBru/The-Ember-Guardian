using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;
    [SerializeField] private bool destroySaveOnApplicationQuit;

    #region TUTORIAL
    public bool tutorialComplete { get; private set; }
    public bool hubLoadedOnce { get; private set; }
    #endregion

    #region HUB
    [SerializeField] private Portal defaultLastHUBPortalUsedByPlayer;
    public int lastHUBPortalUsedByPlayer { get; private set; }
    public bool nextHubArrivalThroughPortal { get; private set; }
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

    #region TUTORIAL
    public void SetTutorialCompleted() {
        ES3.Save("tutorialComplete", true);
    }

    public void SetHubLoadedOnce() {
        ES3.Save("hubLoadedOnce", true);
    }
    #endregion

    #region HUB
    public void SetNextHubArrivalThroughPortal(bool arrivalThroughPortal) {
        ES3.Save("nextHubArrivalThroughPortal", arrivalThroughPortal);
    }

    public bool GetNextHubArrivalThroughPortal() {
        string key = "nextHubArrivalThroughPortal";
        return ES3.Load(key, false);
    }

    public void SetAsLastPortalUsedByPlayer(int portalNumber) {
        ES3.Save("lastHUBPortalUsedByPlayer", portalNumber);
    }

    public void SetPortalUnlocked(string portalName) {
        Debug.Log("Set Portal Unlocked");
        string key = portalName + "_Unlocked";
        ES3.Save(key, true);
    }

    public bool GetPortalUnlocked(string portalName) {
        string key = portalName + "_Unlocked";
        return ES3.Load(key, false);
    }

    public bool GetHubFireEmberExtractable() {
        string key = "SetHubFireEmberExtractable";
        return ES3.Load(key, false);
    }

    public void SetHubFireEmberExtractable() {
        Debug.Log("SetHubFireEmberExtractable");
        string key = "SetHubFireEmberExtractable";
        ES3.Save(key, true);
    }
    #endregion

    #region CURRENCIES

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

    #region HUB MERCHANTS
    public void SetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType merchantType, bool hasTalkLinesToShow) {
        Debug.Log("SetMerchantHasTalkLinesToShow");
        string key = merchantType.ToString() + "_TalkLinesToShow";
        ES3.Save(key, hasTalkLinesToShow);
    }

    public bool GetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_TalkLinesToShow";
        return ES3.Load(key, true);
    }
    public void SetMerchantJustArrivedInHub(HubMerchant.HubMerchantType merchantType, bool justArrived) {
        Debug.Log("SetMerchantJustArrivedInHub");
        string key = merchantType.ToString() + "_JustArrivedInHub";
        ES3.Save(key, justArrived);
    }

    public bool GetMerchantJustArrivedInHub(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_JustArrivedInHub";
        return ES3.Load(key, true);
    }
    public bool GetMerchantUnlocked(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_Unlocked";
        return ES3.Load(key, false);
    }

    public void SetMerchantUnlocked(HubMerchant.HubMerchantType merchantType) {
        Debug.Log("SetMerchantUnlocked");
        string key = merchantType.ToString() + "_Unlocked";
        ES3.Save(key, true);
    }

    public bool GetMerchantItemUnlocked(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Unlocked";
        return ES3.Load(key, false);
    }

    public bool GetMerchantItemBought(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Bought";
        return ES3.Load(key, false);
    }

    public int GetHubMerchantItemLevel(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Level_";
        int level = ES3.Load(key, 0);

        Debug.Log("Key " + key + " " + level);

        return level;
    }

    public void SetHubMerchantItemUnlocked(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Unlocked";
        ES3.Save(key, true);
    }

    public void SetHubMerchantItemBought(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Bought";
        ES3.Save(key, true);
    }

    public void SetHubMerchantItemLevel(string merchantItemType, int level) {
        string key = merchantItemType + "_Level_";
        ES3.Save(key, level);
    }

    #endregion

    #region LEVELS
    public bool GetLevelRegionUnlocked(LevelSO.LevelEnvironment environmentType) {
        string key = environmentType.ToString() + "_Unlocked";
        return ES3.Load(key, false);
    }

    public void SetLevelRegionUnlocked(LevelSO.LevelEnvironment environmentType) {
        string key = environmentType.ToString() + "_Unlocked";
        ES3.Save(key, true);
    }
    #endregion

    #region GUNS
    public void SetGunSecondaryAbilityUnlocked(GunSO gunSO) {
        string key = gunSO.gunType + "_secondaryAbilityUnlocked";

        ES3.Save(key, true);
    }

    public bool GetGunSecondaryAbilityUnlocked(GunSO gunSO) {
        string key = gunSO.gunType + "_secondaryAbilityUnlocked";

        return ES3.Load(key, false);
    }

    public void SetGunDamagePerBullet(GunSO gunSO, int damage) {
        string key = gunSO.gunType + "_damagePerBullet";

        Debug.Log(gunSO + " SetGunDamagePerBullet " + damage);
        ES3.Save(key, damage);
    }

    public int GetGunDamagePerBullet(GunSO gunSO) {
        string key = gunSO.gunType + "_damagePerBullet";

        return ES3.Load(key, gunSO.damagePerBullet);
    }

    public void SetGunMaxAmmo(GunSO gunSO, int maxAmmo) {
        string key = gunSO.gunType + "_maxAmmo";

        ES3.Save(key, maxAmmo);
    }

    public int GetGunMaxAmmo(GunSO gunSO) {
        string key = gunSO.gunType + "_maxAmmo";

        return ES3.Load(key, gunSO.maxAmmo);
    }
    public void SetGunShotsPerClip(GunSO gunSO, int shotsPerClip) {
        string key = gunSO.gunType + "_shotsPerClip";

        ES3.Save(key, shotsPerClip);
    }

    public int GetGunShotsPerClip(GunSO gunSO) {
        string key = gunSO.gunType + "_shotsPerClip";

        return ES3.Load(key, gunSO.shotsPerClip);
    }

    public void SetGunCooldown(GunSO gunSO, float cooldown) {
        string key = gunSO.gunType + "_cooldown";

        ES3.Save(key, cooldown);
    }

    public float GetGunCooldown(GunSO gunSO) {
        string key = gunSO.gunType + "_cooldown";

        return ES3.Load(key, gunSO.shootCooldownTime);
    }

    public void SetGunCritChance(GunSO gunSO, float critChance) {
        string key = gunSO.gunType + "_critChance";

        ES3.Save(key, critChance);
    }

    public float GetGunCritChance(GunSO gunSO) {
        string key = gunSO.gunType + "_critChance";

        return ES3.Load(key, gunSO.critChance);
    }

    public void SetGunReloadTime(GunSO gunSO, float reloadTime) {
        string key = gunSO.gunType + "_reloadTime";

        ES3.Save(key, reloadTime);
    }

    public float GetGunReloadTime(GunSO gunSO) {
        string key = gunSO.gunType + "_reloadTime";

        return ES3.Load(key, gunSO.reloadTime);
    }

    #endregion


    private void OnApplicationQuit() {
        if(destroySaveOnApplicationQuit) {
            ES3.DeleteFile("SaveFile.es3");
        }
    }
}
