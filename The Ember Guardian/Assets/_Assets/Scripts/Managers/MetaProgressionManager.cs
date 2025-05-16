using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;
    [SerializeField] private bool destroySaveOnApplicationQuit;
    private bool flagCarry_Debug;

    public class OnGunChangedEventArgs : EventArgs {
        public GunSO.GunType gunTypeModified;
    }

    #region TUTORIAL
    public bool savedOnce { get; private set; }
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
    [SerializeField] private MerchantTextLinesSO defaultMerchantTextLinesSO;
    #endregion

    private void Awake() {
        Instance = this;

        // Create a new ES3Settings to enable encryption.
        //string saveFilePassword = "es3SavePass59463";
        //var settings = new ES3Settings(ES3.EncryptionType.AES, saveFilePassword);
        //var settingsDefault = new ES3Settings("SaveFile.es3", settings);
        //ES3.Save<Transform>("myTransform", this.transform, settingsDefault);

        tutorialComplete = ES3.Load("tutorialComplete", false);

        if (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            int lastPortal = defaultLastHUBPortalUsedByPlayer.GetPortalNumber();
            lastHUBPortalUsedByPlayer = ES3.Load("lastHUBPortalUsedByPlayer", lastPortal);
        }

        flagCarry_Debug = DebugManager.Instance.GetDebugMode_FlagCarry();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.H)) {
            //SaveHubGems();
            //HUBManager.Instance.SaveHub();
        }
    }

    #region GENERAL
    public bool GetSavedOnce() {
        return ES3.Load("savedOnce", false);
    }

    public void SetSavedOnce() {
        ES3.Save("savedOnce", true);
    }

    #endregion

    #region TUTORIAL
    public void SetTutorialCompleted() {
        ES3.Save("tutorialComplete", true);
    }

    public bool GetTutorialCompleted() {
        return ES3.Load("tutorialComplete", false);
    }

    #endregion

    #region HUB

    public void SetPortalLinkedLevelSOIndex(int portalNumber, int levelSOIndex) {
        string key = "portal_" + portalNumber.ToString() + "_linkedLevelSOIndex";
        ES3.Save(key, levelSOIndex);
    }

    public int GetPortalLinkedLevelSOIndex(int portalNumber) {
        string key = "portal_" + portalNumber.ToString() + "_linkedLevelSOIndex";
        return ES3.Load(key, 0);

    } 

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

    public void SavePlayerHubPosition(Vector3 position) {
        ES3.Save("_playerHUBPosition", position);
    }

    public Vector3 GetPlayerHubPosition() {
        return ES3.Load("_playerHUBPosition", Vector3.zero);
    }
    #endregion

    #region CURRENCIES
    public void SetGemAmountFromLevel(PlayerCurrencies.CurrencyType gemType, int gemAmount) {
        string key = gemType.ToString() + "_AmountFromLastLevel";

        ES3.Save(key, gemAmount);
    }

    public void SaveHubGems() {
        Debug.Log("save hub gems");

        List<Vector3> greenGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.greenGem);
        List<Vector3> redGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.redGem);
        List<Vector3> blueGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.blueGem);
        List<Vector3> purpleGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.purpleGem);
        List<Vector3> yellowGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.yellowGem);
        List<Vector3> cyanGemPositions = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.cyanGem);

        SaveGemPositions(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions, false);
        SaveGemPositions(PlayerCurrencies.CurrencyType.redGem, redGemPositions, false);
        SaveGemPositions(PlayerCurrencies.CurrencyType.blueGem, blueGemPositions, false);
        SaveGemPositions(PlayerCurrencies.CurrencyType.purpleGem, purpleGemPositions, false);
        SaveGemPositions(PlayerCurrencies.CurrencyType.yellowGem, yellowGemPositions, false);
        SaveGemPositions(PlayerCurrencies.CurrencyType.cyanGem, cyanGemPositions, false);
    }

    public void SaveLevelGemsAndHoldingEmber(float proportionToSave = 1) {
        List<Vector3> greenGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.greenGem);
        List<Vector3> redGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.redGem);
        List<Vector3> blueGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.blueGem);
        List<Vector3> purpleGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.purpleGem);
        List<Vector3> yellowGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.yellowGem);
        List<Vector3> cyanGemPositions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.cyanGem);

        int newgreenGemPositionsSize = (int)(greenGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver
        int newredGemPositionsSize = (int)(redGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver
        int newblueGemPositionsSize = (int)(blueGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver
        int newpurpleGemPositionsSize = (int)(purpleGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver
        int newyellowGemPositionsSize = (int)(yellowGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver
        int newcyanGemPositionsSize = (int)(cyanGemPositions.Count * (proportionToSave)); // Calcul du nombre d'éléments à conserver

        List<Vector3> truncatedGreenGemPositions = greenGemPositions.GetRange(0, newgreenGemPositionsSize); // Conserver les premiers éléments
        List<Vector3> truncatedRedGemPositions = redGemPositions.GetRange(0, newredGemPositionsSize); // Conserver les premiers éléments
        List<Vector3> truncatedBlueGemPositions = blueGemPositions.GetRange(0, newblueGemPositionsSize); // Conserver les premiers éléments
        List<Vector3> truncatedPurpleGemPositions = purpleGemPositions.GetRange(0, newpurpleGemPositionsSize); // Conserver les premiers éléments
        List<Vector3> truncatedYellowGemPositions = yellowGemPositions.GetRange(0, newyellowGemPositionsSize); // Conserver les premiers éléments
        List<Vector3> truncatedCyanGemPositions = cyanGemPositions.GetRange(0, newcyanGemPositionsSize); // Conserver les premiers éléments

        SaveGemPositions(PlayerCurrencies.CurrencyType.greenGem, truncatedGreenGemPositions, true);
        SaveGemPositions(PlayerCurrencies.CurrencyType.redGem, truncatedRedGemPositions, true);
        SaveGemPositions(PlayerCurrencies.CurrencyType.blueGem, truncatedBlueGemPositions, true);
        SaveGemPositions(PlayerCurrencies.CurrencyType.purpleGem, truncatedPurpleGemPositions, true);
        SaveGemPositions(PlayerCurrencies.CurrencyType.yellowGem, truncatedYellowGemPositions, true);
        SaveGemPositions(PlayerCurrencies.CurrencyType.cyanGem, truncatedCyanGemPositions, true);

        ES3.Save("holdingEmber", false);
    }

    public void SaveGemPositions(PlayerCurrencies.CurrencyType gemType, List<Vector3> positions, bool playerInventory) {
        string key = gemType.ToString() + "_positions_playerInventory_" + playerInventory;
        ES3.Save(key, positions);
    }

    public List<Vector3> GetGemPositions(PlayerCurrencies.CurrencyType gemType, bool playerInventory) {
        string key = gemType.ToString() + "_positions_playerInventory_" + playerInventory;
        return ES3.Load(key, new List<Vector3>());
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

    public void SetNextMerchantTalkLines(HubMerchant.HubMerchantType merchantType, MerchantTextLinesSO textLinesSO) {
        SetNextMerchantTalkLinesShowShopAfterDialog(merchantType, textLinesSO);

        string key = merchantType.ToString() + "_nextTextLinesSO";
        ES3.Save(key, textLinesSO.merchantTextLines);
    }

    public List<string> GetNextMerchantTextLines(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_nextTextLinesSO";
        return ES3.Load(key, defaultMerchantTextLinesSO.merchantTextLines);
    }

    public void SetNextMerchantTalkLinesShowShopAfterDialog(HubMerchant.HubMerchantType merchantType, MerchantTextLinesSO textLinesSO) {
        string key = merchantType.ToString() + "_nextTextLinesSOShowShopAfterDialog";
        ES3.Save(key, textLinesSO.showShopAfterDialog);
    }

    public bool GetNextMerchantTextLinesShowShopAfterDialog(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_nextTextLinesSOShowShopAfterDialog";
        return ES3.Load(key, defaultMerchantTextLinesSO.showShopAfterDialog);
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
        ES3.Save(key, unlocked);
    }

    public void SetHubMerchantItemBought(string merchantItemSaveString, bool bought) {
        string key = merchantItemSaveString + "_Bought";
        ES3.Save(key, bought);
    }

    public void SetHubMerchantItemLevel(string merchantItemType, int level) {
        string key = merchantItemType + "_Level_";
        ES3.Save(key, level);
    }

    #endregion

    #region LEVELS
    public bool GetLevelUnlocked(LevelSO levelSO) {
        return ES3.Load(levelSO.name, false);
    }

    public void SetLevelUnlocked(LevelSO levelSO) {
        ES3.Save(levelSO.name, true);
    }

    public List<LevelSO> GetPreviousLevelsUnlocked() {
        string key = "_previousLevelsUnlocked";
        return ES3.Load(key, new List<LevelSO>());
    }

    public void SetPreviousLevelsUnlocked(List<LevelSO> levelSOList) {
        string key = "_previousLevelsUnlocked";
        ES3.Save(key, levelSOList);
    }

    public bool GetLevelCompleted(LevelSO levelSO) {
        string key = levelSO.ToString() + "_Completed";
        return ES3.Load(key, false);
    }

    public void SetLevelCompleted(LevelSO levelSO) {
        string key = levelSO.ToString() + "_Completed";
        ES3.Save(key, true);
    }

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

    public void SetGunUnlocked(GunSO gunSO, bool unlocked) {
        string key = gunSO.name;
        ES3.Save(key, unlocked);

        if(gunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            ES3.Save("specialAmmoUnlocked", true);
        }
    }

    public bool GetGunUnlocked(GunSO gunSO) {
        string key = gunSO.name;

        if (gunSO.gunType == GunSO.GunType.Rifle) return true;

        return ES3.Load(key, false);
    }

    public bool GetSpecialAmmoUnlocked() {
        return ES3.Load("specialAmmoUnlocked", false);
    }

    public void SetInitialLevelAmmo(int initialLevelAmmo) {
        string key = "initialLevelAmmo";

        ES3.Save(key, initialLevelAmmo);
    }

    public int  GetInitialLevelAmmo() {
        string key = "initialLevelAmmo";

        return ES3.Load(key, 2);
    }

    public void SetGunSecondaryAbilityUnlocked(GunSO gunSO, bool secondaryAbilityUnlocked) {
        string key = gunSO.gunType + "_secondaryAbilityUnlocked";

        ES3.Save(key, secondaryAbilityUnlocked);
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
    }

    public int GetGunDamagePerBullet(GunSO gunSO) {
        string key = gunSO.gunType + "_damagePerBullet";
        int damagePerBullet = ES3.Load(key, gunSO.damagePerBullet);
        return damagePerBullet;
    }

    public float GetGunBulletKnockback(GunSO gunSO) {
        string key = gunSO.gunType + "_bulletKnockback";
        float bulletKnockback = ES3.Load(key, gunSO.bulletKnockback);
        return bulletKnockback;
    }

    public void SetGunMaxAmmo(GunSO gunSO, float maxAmmo) {
        int maxAmmoToSave = (int)maxAmmo;
        string key = gunSO.gunType + "_maxAmmo";

        ES3.Save(key, maxAmmoToSave);
    }

    public int GetGunMaxAmmo(GunSO gunSO) {
        string key = gunSO.gunType + "_maxAmmo";

        return ES3.Load(key, gunSO.maxAmmo);
    }
    public void SetGunShotsPerClip(GunSO gunSO, float shotsPerClip) {
        int shotsPerClipToSave = (int)shotsPerClip;
        string key = gunSO.gunType + "_shotsPerClip";

        ES3.Save(key, shotsPerClipToSave);
    }

    public int GetGunPelletsPerBullet(GunSO gunSO) {
        string key = gunSO.gunType + "_pelletsPerBullet";

        return ES3.Load(key, gunSO.pelletsPerBullet);
    }

    public void SetGunPelletsPerBullet(GunSO gunSO, float pelletsPerBullet) {
        int pelletsPerBulletToSave = (int)pelletsPerBullet;
        string key = gunSO.gunType + "_pelletsPerBullet";

        ES3.Save(key, pelletsPerBulletToSave);
    }

    public float GetGunBulletLifetime(GunSO gunSO) {
        string key = gunSO.gunType + "_bulletLifetime";

        return ES3.Load(key, gunSO.bulletLifetime);
    }

    public void SetGunBulletLifetime(GunSO gunSO, float bulletLitefime) {
        string key = gunSO.gunType + "_bulletLifetime";

        ES3.Save(key, bulletLitefime);
    }

    public float GetGunBulletSpeed(GunSO gunSO) {
        string key = gunSO.gunType + "_bulletSpeed";

        return ES3.Load(key, gunSO.bulletSpeed);
    }

    public void SetGunBulletSpeed(GunSO gunSO, float bulletSpeed) {
        string key = gunSO.gunType + "_bulletSpeed";

        ES3.Save(key, bulletSpeed);
    }

    public float GetGunReloadAccelerationFactor(GunSO gunSO) {
        string key = gunSO.gunType + "_reloadAccelerationFactor";

        return ES3.Load(key, gunSO.reloadAccelerationFactor);
    }
    public void SetGunReloadAccelerationFactor(GunSO gunSO, float reloadAccelerationFactor) {
        string key = gunSO.gunType + "_reloadAccelerationFactor";

        ES3.Save(key, reloadAccelerationFactor);
    }

    public float GetGunWeightAccelerationFactor(GunSO gunSO) {
        string key = gunSO.gunType + "_weightAccelerationFactor";

        return ES3.Load(key, gunSO.weightAccelerationFactor);
    }
    public void SetGunWeightAccelerationFactor(GunSO gunSO, float weightAccelerationFactor) {
        string key = gunSO.gunType + "_weightAccelerationFactor";

        ES3.Save(key, weightAccelerationFactor);
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
    public void SetGunHandsReloadTime(GunSO gunSO, float handsReloadTime) {
        string key = gunSO.gunType + "_handsReloadTime";

        ES3.Save(key, handsReloadTime);
    }
    public float GetGunReloadTime(GunSO gunSO) {
        string key = gunSO.gunType + "_reloadTime";

        return ES3.Load(key, gunSO.reloadTime);
    }
    public float GetHandsGunReloadTime(GunSO gunSO) {
        string key = gunSO.gunType + "_handsReloadTime";

        return ES3.Load(key, gunSO.handsReloadTime);
    }

    public void SetGunSwapToWeaponTimeMultiplier(GunSO gunSO, float swapToWeaponTimeMultiplier) {
        string key = gunSO.gunType + "_swapToWeaponTimeMultiplier";

        ES3.Save(key, swapToWeaponTimeMultiplier);
    }

    public float GetSwapToWeaponTimeMultiplier(GunSO gunSO) {
        string key = gunSO.gunType + "_swapToWeaponTimeMultiplier";

        return ES3.Load(key, gunSO.swapToWeaponTimeMultiplier);
    }

    public void SetGunShootConeAnle(GunSO gunSO, float shootConeAngle) {
        string key = gunSO.gunType + "_shootConeAngle";

        ES3.Save(key, shootConeAngle);
    }

    public float GetGunShootConeAnle(GunSO gunSO) {
        string key = gunSO.gunType + "_shootConeAngle";

        Debug.Log("GetGunShootConeAnle " + gunSO + " " + gunSO.shootConeAngle);
        return ES3.Load(key, gunSO.shootConeAngle);
    }
    #endregion

    #region OTHER
    public bool GetPlayerUnlockedFlagCarry() {
        return ES3.Load("playerUnlockedFlagCarry", flagCarry_Debug);
    }
    public void SetPlayerUnlockedFlagCarry(bool playerUnlockedFlagCarry) {
        ES3.Save("playerUnlockedFlagCarry", playerUnlockedFlagCarry);
    }
    #endregion


    private void OnApplicationQuit() {
        if(destroySaveOnApplicationQuit) {
            ES3.DeleteFile("SaveFile.es3");
        }
    }
}
