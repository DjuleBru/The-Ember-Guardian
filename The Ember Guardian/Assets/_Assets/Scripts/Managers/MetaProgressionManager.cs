using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;
    [SerializeField] private bool destroySaveOnApplicationQuit;

    public event EventHandler<OnGunChangedEventArgs> OnGunStatChanged;
    public event EventHandler<OnGunChangedEventArgs> OnGunSecondaryAbilityUnlocked;
    public class OnGunChangedEventArgs : EventArgs {
        public GunSO.GunType gunTypeModified;
    }

    #region TUTORIAL
    public bool tutorialComplete { get; private set; }
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

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
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

    public bool GetLevelUnlocked(LevelSO levelSO) {
        return ES3.Load(levelSO.name, false);
    }

    public void SetLevelUnlocked(LevelSO levelSO) {
        ES3.Save(levelSO.name, true);
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

    public void SetHubFireEmberExtractable(bool hubFireExtractable) {
        string key = "SetHubFireEmberExtractable";
        ES3.Save(key, hubFireExtractable);
    }

    public void SavePlayerHubPosition(Vector3 position) {
        ES3.Save("_playerHUBPosition", position);
    }

    public Vector3 GetPlayerHubPosition() {
        return ES3.Load("_playerHUBPosition", Vector3.zero);
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

    public void SetGemsRewarded(bool rewarded) {
        ES3.Save("gemsRewardedFromlastLevel", rewarded);
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
        string key = merchantType.ToString() + "_TalkLinesToShow";
        ES3.Save(key, hasTalkLinesToShow);
    }

    public bool GetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_TalkLinesToShow";
        return ES3.Load(key, true);
    }

    public void SetMerchantJustArrivedInHub(HubMerchant.HubMerchantType merchantType, bool justArrived) {
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


    public bool GetMerchantItemEquipped(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Equipped";
        return ES3.Load(key, false);
    }

    public void SetHubMerchantItemEquippedAtStart(string merchantItemSaveString, bool equipped) {
        string key = merchantItemSaveString + "_EquippedAtStart";
        ES3.Save(key, equipped);
    }

    public bool GetMerchantItemEquippedAtStart(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_EquippedAtStart";
        return ES3.Load(key, true);
    }

    public void SetHubMerchantItemEquipped(string merchantItemSaveString, bool equipped) {
        string key = merchantItemSaveString + "_Equipped";
        ES3.Save(key, equipped);
    }

    public int GetHubMerchantItemLevel(string merchantItemSaveString) {
        string key = merchantItemSaveString + "_Level_";
        int level = ES3.Load(key, 0);

        return level;
    }

    public void SetHubMerchantItemUnlocked(string merchantItemSaveString, bool unlocked) {
        string key = merchantItemSaveString + "_Unlocked";
        ES3.Save(key, true);
    }

    public void SetHubMerchantItemBought(string merchantItemSaveString, bool bought) {
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

    public void SetInitialLevelAmmo(int initialLevelAmmo) {
        string key = "initialLevelAmmo";

        ES3.Save(key, initialLevelAmmo);
    }

    public int  GetInitialLevelAmmo() {
        string key = "initialLevelAmmo";

        return ES3.Load(key, 2);
    }
    public void SetGunSecondaryAbilityUnlocked(GunSO gunSO) {
        string key = gunSO.gunType + "_secondaryAbilityUnlocked";

        ES3.Save(key, true);
        OnGunSecondaryAbilityUnlocked?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public bool GetGunSecondaryAbilityUnlocked(GunSO gunSO) {
        string key = gunSO.gunType + "_secondaryAbilityUnlocked";

        return ES3.Load(key, false);
    }

    public void SetGunModuleEquipped(GunSO gunSO, HUBMerchantItem_GunMerchantItem.GunItemType module) {
        string key = gunSO.gunType + "_moduleEquipped" + module;

        ES3.Save(key, true);
    }

    public bool GetGunModuleEquipped(GunSO gunSO, HUBMerchantItem_GunMerchantItem.GunItemType module) {
        string key = gunSO.gunType + "_moduleEquipped" + module;

        return ES3.Load(key, false);
    }

    public void SetGunDamagePerBullet(GunSO gunSO, float damage) {
        int damageToSave = (int)damage;
        string key = gunSO.gunType + "_damagePerBullet";

        ES3.Save(key, damageToSave);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public int GetGunDamagePerBullet(GunSO gunSO) {
        string key = gunSO.gunType + "_damagePerBullet";
        int damagePerBullet = ES3.Load(key, gunSO.damagePerBullet);
        return damagePerBullet;
    }

    public void SetGunMaxAmmo(GunSO gunSO, float maxAmmo) {
        int maxAmmoToSave = (int)maxAmmo;
        string key = gunSO.gunType + "_maxAmmo";

        ES3.Save(key, maxAmmoToSave);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public int GetGunMaxAmmo(GunSO gunSO) {
        string key = gunSO.gunType + "_maxAmmo";

        return ES3.Load(key, gunSO.maxAmmo);
    }
    public void SetGunShotsPerClip(GunSO gunSO, float shotsPerClip) {
        int shotsPerClipToSave = (int)shotsPerClip;
        string key = gunSO.gunType + "_shotsPerClip";

        ES3.Save(key, shotsPerClipToSave);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public int GetGunPelletsPerBullet(GunSO gunSO) {
        string key = gunSO.gunType + "_pelletsPerBullet";

        return ES3.Load(key, gunSO.pelletsPerBullet);
    }

    public void SetGunPelletsPerBullet(GunSO gunSO, float pelletsPerBullet) {
        int pelletsPerBulletToSave = (int)pelletsPerBullet;
        string key = gunSO.gunType + "_pelletsPerBullet";

        ES3.Save(key, pelletsPerBulletToSave);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunBulletLifetime(GunSO gunSO) {
        string key = gunSO.gunType + "_bulletLifetime";

        return ES3.Load(key, gunSO.bulletLifetime);
    }

    public void SetGunBulletLifetime(GunSO gunSO, float bulletLitefime) {
        string key = gunSO.gunType + "_bulletLifetime";

        ES3.Save(key, bulletLitefime);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunBulletSpeed(GunSO gunSO) {
        string key = gunSO.gunType + "_bulletSpeed";

        return ES3.Load(key, gunSO.bulletSpeed);
    }

    public void SetGunBulletSpeed(GunSO gunSO, float bulletSpeed) {
        string key = gunSO.gunType + "_bulletSpeed";

        ES3.Save(key, bulletSpeed);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }
    public float GetGunReloadAccelerationFactor(GunSO gunSO) {
        string key = gunSO.gunType + "_reloadAccelerationFactor";

        return ES3.Load(key, gunSO.reloadAccelerationFactor);
    }
    public void SetGunReloadAccelerationFactor(GunSO gunSO, float reloadAccelerationFactor) {
        string key = gunSO.gunType + "_reloadAccelerationFactor";

        ES3.Save(key, reloadAccelerationFactor);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public int GetGunShotsPerClip(GunSO gunSO) {
        string key = gunSO.gunType + "_shotsPerClip";

        return ES3.Load(key, gunSO.shotsPerClip);
    }

    public void SetGunCooldown(GunSO gunSO, float cooldown) {
        string key = gunSO.gunType + "_cooldown";
        ES3.Save(key, cooldown);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunCooldown(GunSO gunSO) {
        string key = gunSO.gunType + "_cooldown";

        return ES3.Load(key, gunSO.shootCooldownTime);
    }

    public void SetGunCritChance(GunSO gunSO, float critChance) {
        string key = gunSO.gunType + "_critChance";

        ES3.Save(key, critChance);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunCritChance(GunSO gunSO) {
        string key = gunSO.gunType + "_critChance";

        return ES3.Load(key, gunSO.critChance);
    }

    public void SetGunReloadTime(GunSO gunSO, float reloadTime) {
        string key = gunSO.gunType + "_reloadTime";

        ES3.Save(key, reloadTime);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunReloadTime(GunSO gunSO) {
        string key = gunSO.gunType + "_reloadTime";

        return ES3.Load(key, gunSO.reloadTime);
    }
    public float GetHandsGunReloadTime(GunSO gunSO) {
        string key = gunSO.gunType + "_handsReloadTime";

        return ES3.Load(key, gunSO.handsReloadTime);
    }
    public void SetGunShootConeAnle(GunSO gunSO, float shootConeAngle) {
        string key = gunSO.gunType + "_shootConeAngle";

        ES3.Save(key, shootConeAngle);
        OnGunStatChanged?.Invoke(this, new OnGunChangedEventArgs {
            gunTypeModified = gunSO.gunType,
        });
    }

    public float GetGunShootConeAnle(GunSO gunSO) {
        string key = gunSO.gunType + "_shootConeAngle";

        return ES3.Load(key, gunSO.shootConeAngle);
    }
    #endregion

    private void OnApplicationQuit() {
        if(destroySaveOnApplicationQuit) {
            ES3.DeleteFile("SaveFile.es3");
        }
    }
}
