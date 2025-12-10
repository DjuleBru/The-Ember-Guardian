using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;
    [SerializeField] private bool destroySaveOnApplicationQuit;
    [SerializeField] private List<LevelSO> allLevelSOList;
    private bool flagCarry_Debug;

    private List<string> unlockedVideoTipList = new List<string>();
    private List<string> newlyUnlockedVideoTipList = new List<string>();

    public event EventHandler<OnLevelSOUnlockedEventArgs> OnLevelSOUnlocked;
    public class OnLevelSOUnlockedEventArgs : EventArgs {
        public LevelSO levelSOUnlocked;
    }

    public event EventHandler<OnCreatureSOUnlockedEventArgs> OnCreatureSOUnlocked;
    public class OnCreatureSOUnlockedEventArgs : EventArgs {
        public CreatureSO creatureSOUnlocked;
    }
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
    public bool dropRedOrbsUnlocked;
    #endregion

    #region METAMERCHANTS
    [SerializeField] private MerchantTextLinesSO defaultMerchantTextLinesSO;
    #endregion

    private void Awake() {
        Instance = this;
        tutorialComplete = ES3.Load("tutorialComplete", false);

        if (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            int lastPortal = defaultLastHUBPortalUsedByPlayer.GetPortalNumber();
            lastHUBPortalUsedByPlayer = ES3.Load("lastHUBPortalUsedByPlayer", lastPortal);
        }

        flagCarry_Debug = DebugManager.Instance.GetDebugMode_FlagCarry();
        dropRedOrbsUnlocked = ES3.Load("dropRedOrbsUnlocked", false);
        unlockedVideoTipList = ES3.Load("unlockedVideoTipList", new List<string>());
        newlyUnlockedVideoTipList = ES3.Load("newlyUnlockedVideoTipList", new List<string>());
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.H)) {
            //SaveHubGems();
            //HUBManager.Instance.SaveHub();
        }
    }

    #region GENERAL


    public void DeleteLevelSaveFile() {
        ES3.DeleteFile("LevelSave.es3");
    }

    public bool GetSavedOnce() {
        return ES3.Load("savedOnce", false);
    }

    public void SetSavedOnce() {
        ES3.Save("savedOnce", true);
    }

    public bool GetDropRedOrbsUnlocked() {
        return dropRedOrbsUnlocked;
    }

    public void SetDropRedOrbsUnlocked() {
        dropRedOrbsUnlocked = true;
        ES3.Save("dropRedOrbsUnlocked", true);
    }

    public bool GetPlayerLeftInLevel() {
        return ES3.Load("playerLeftInLevel", false);
    }

    public void SetPlayerLeftFromLevel(bool leftInLevel) {
        ES3.Save("playerLeftInLevel", leftInLevel);
    }

    public void SetLastLevel(string sceneName) {
        ES3.Save("lastLevel", sceneName);
    }

    #endregion

    #region TUTORIAL
    public void SetTutorialCompleted() {
        ES3.Save("tutorialComplete", true);
    }

    public bool GetTutorialCompletedOrSkipped() {
        bool tutorialCompleted = ES3.Load("tutorialComplete", false);
        bool tutorialSkipped = ES3.Load("tutorialSkipped", false);
        return tutorialCompleted || tutorialSkipped;
    }
    public void SetTutorialSkipped() {
        ES3.Save("tutorialSkipped", true);
    }
    public bool GetTutorialSkipped() {
        return ES3.Load("tutorialSkipped", false);
    }

    public void SetTipUnlocked(VideoTipSO videoTipSO) {
        if (unlockedVideoTipList.Contains(videoTipSO.tipNameLocalizationKey)) return;

        unlockedVideoTipList.Add(videoTipSO.tipNameLocalizationKey);

        ES3.Save("unlockedVideoTipList", unlockedVideoTipList);
    }

    public bool GetTipUnlocked(VideoTipSO videoTipSO) {
        return unlockedVideoTipList.Contains(videoTipSO.tipNameLocalizationKey);
    }
    public List<string> GetTipUnlockedList() {
        return unlockedVideoTipList;;
    }


    public void SetTipNewlyUnlocked(VideoTipSO videoTipSO, bool newlyUnlocked) {
        string key = videoTipSO.tipNameLocalizationKey;

        if(newlyUnlocked) {

            if(newlyUnlockedVideoTipList.Contains(key)) return;
            newlyUnlockedVideoTipList.Add(key);

        } else {

            if (!newlyUnlockedVideoTipList.Contains(key)) return;
            newlyUnlockedVideoTipList.Remove(key);

        }
        ES3.Save("newlyUnlockedVideoTipList", newlyUnlockedVideoTipList);
    }

    public bool GetTipNewlyUnlocked(VideoTipSO videoTipSO) {
        return newlyUnlockedVideoTipList.Contains(videoTipSO.tipNameLocalizationKey);
    }

    #endregion

    #region HUB

    public void SetPortalLinkedLevelSOIndex(int portalNumber, int levelSOIndex, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);
        string key = "portal_" + portalNumber.ToString() + "_linkedLevelSOIndex";
        ES3.Save(key, levelSOIndex, settings);
    }

    public int GetPortalLinkedLevelSOIndex(int portalNumber) {
        string key = "portal_" + portalNumber.ToString() + "_linkedLevelSOIndex";
        return ES3.Load(key, 0);

    } 

    public void SetNextHubArrivalThroughPortal(bool arrivalThroughPortal, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);
        ES3.Save("nextHubArrivalThroughPortal", arrivalThroughPortal, settings);
    }

    public bool GetNextHubArrivalThroughPortal() {
        string key = "nextHubArrivalThroughPortal";
        return ES3.Load(key, false);
    }

    public void SetAsLastPortalUsedByPlayer(int portalNumber) {
        ES3.Save("lastHUBPortalUsedByPlayer", portalNumber);
    }

    public void SetPortalUnlocked(string portalName, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);
        string key = portalName + "_Unlocked";
        ES3.Save(key, true, settings);
    }

    public bool GetPortalUnlocked(string portalName) {
        string key = portalName + "_Unlocked";
        return ES3.Load(key, false);
    }

    public void SavePlayerHubPosition(Vector3 position, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);
        ES3.Save("_playerHUBPosition", position, settings);
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

    public void SaveHubGemsBatch(string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);

        var gemData = new Dictionary<string, List<Vector3>>();
        var gemRotationData = new Dictionary<string, List<Quaternion>>();

        gemData["greenGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.greenGem);
        gemData["redGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.redGem);
        gemData["blueGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.blueGem);
        gemData["purpleGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.purpleGem);
        gemData["yellowGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.yellowGem);
        gemData["cyanGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyPositions(PlayerCurrencies.CurrencyType.cyanGem);

        gemRotationData["greenGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.greenGem);
        gemRotationData["redGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.redGem);
        gemRotationData["blueGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.blueGem);
        gemRotationData["purpleGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.purpleGem);
        gemRotationData["yellowGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.yellowGem);
        gemRotationData["cyanGem"] = UICurrencyManager.HubInventoryUI.GetCurrencyRotations(PlayerCurrencies.CurrencyType.cyanGem);

        ES3.Save("HubGems", gemData, settings);
        ES3.Save("HubGemRotations", gemRotationData, settings);
    }

    public void SaveLevelGemsAndHoldingEmber(float proportionToSave = 1f, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);

        var gemData = new Dictionary<string, List<Vector3>>();
        var gemDataRotations = new Dictionary<string, List<Quaternion>>();

        foreach (PlayerCurrencies.CurrencyType gemType in Enum.GetValues(typeof(PlayerCurrencies.CurrencyType))) {
            // Récupérer toutes les positions du type de gemme
            List<Vector3> positions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(gemType);
            List<Quaternion> rotations = UICurrencyManager.PlayerInventoryUI.GetCurrencyRotations(gemType);

            // Calcul du nombre d’éléments à conserver
            int newSize = (int)(positions.Count * proportionToSave);

            // Tronquer la liste
            List<Vector3> truncatedPositions = positions.GetRange(0, newSize);
            List<Quaternion> truncatedRotations = rotations.GetRange(0, newSize);

            // Ajouter au dictionnaire
            gemData[gemType.ToString()] = truncatedPositions;
            gemDataRotations[gemType.ToString()] = truncatedRotations;
        }

        // Sauvegarde batch des gemmes (uniquement l’inventaire joueur ici)
        ES3.Save("PlayerGems", gemData, settings);
        ES3.Save("PlayerGemRotations", gemDataRotations, settings);

        // Sauvegarde séparée pour l’Ember porté
        ES3.Save("holdingEmber", PlayerCurrencies.Instance.GetCarryingEmber(), settings);
    }

    public void SaveGemPositions(PlayerCurrencies.CurrencyType gemType, List<Vector3> positions, bool playerInventory) {
        string key = gemType.ToString() + "_positions_playerInventory_" + playerInventory;
        ES3.Save(key, positions);
    }

    public List<Vector3> GetGemPositions(PlayerCurrencies.CurrencyType gemType, bool playerInventory) {
        string key = playerInventory ? "PlayerGems" : "HubGems";

        if (!ES3.KeyExists(key))
            return new List<Vector3>();

        var gemData = ES3.Load<Dictionary<string, List<Vector3>>>(key);

        string gemKey = gemType.ToString();
        if (gemData.ContainsKey(gemKey))
            return gemData[gemKey];

        return new List<Vector3>();
    }

    public List<Quaternion> GetGemRotations(PlayerCurrencies.CurrencyType gemType, bool playerInventory) {
        string key = playerInventory ? "PlayerGemRotations" : "HubGemRotations";

        if (!ES3.KeyExists(key))
            return new List<Quaternion>();

        var gemData = ES3.Load<Dictionary<string, List<Quaternion>>>(key);

        string gemKey = gemType.ToString();
        if (gemData.ContainsKey(gemKey))
            return gemData[gemKey];

        return new List<Quaternion>();
    }


    #endregion

    #region HUB MERCHANTS
    public void SetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType merchantType, bool hasTalkLinesToShow, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);

        string key = merchantType.ToString() + "_TalkLinesToShow";
        ES3.Save(key, hasTalkLinesToShow, settings);
    }

    public bool GetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_TalkLinesToShow";
        return ES3.Load(key, true);
    }

    public void SetNextMerchantTalkLines(HubMerchant.HubMerchantType merchantType, MerchantTextLinesSO textLinesSO) {
        SetNextMerchantTalkLinesShowShopAfterDialog(merchantType, textLinesSO);

        string key = merchantType.ToString() + "_nextTextLinesSO";
        ES3.Save(key, textLinesSO.merchantTextLinesLocalizationKeys);
    }

    public List<string> GetNextMerchantTextLines(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_nextTextLinesSO";
        return ES3.Load(key, defaultMerchantTextLinesSO.merchantTextLinesLocalizationKeys);
    }

    public void SetNextMerchantTalkLinesShowShopAfterDialog(HubMerchant.HubMerchantType merchantType, MerchantTextLinesSO textLinesSO) {
        string key = merchantType.ToString() + "_nextTextLinesSOShowShopAfterDialog";
        ES3.Save(key, textLinesSO.showShopAfterDialog);
    }

    public bool GetNextMerchantTextLinesShowShopAfterDialog(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_nextTextLinesSOShowShopAfterDialog";
        return ES3.Load(key, defaultMerchantTextLinesSO.showShopAfterDialog);
    }

    public void SetMerchantJustArrivedInHub(HubMerchant.HubMerchantType merchantType, bool justArrived, string savePath = "SaveFile.es3") {
        ES3Settings settings = new ES3Settings(savePath);
        string key = merchantType.ToString() + "_JustArrivedInHub";
        ES3.Save(key, justArrived, settings);
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

    public bool GetMerchantItemBought(string saveString) {
        var data = LoadHubMerchantItemData(saveString);
        return Convert.ToBoolean(data["Bought"]);
    }

    public bool GetMerchantItemUnlocked(string saveString) {
        var data = LoadHubMerchantItemData(saveString);
        return Convert.ToBoolean(data["Unlocked"]);
    }

    public bool GetHubMerchantItemNewlyUnlocked(string saveString) {
        var data = LoadHubMerchantItemData(saveString);
        return Convert.ToBoolean(data["NewlyUnlocked"]);
    }

    public int GetHubMerchantItemLevel(string saveString) {
        var data = LoadHubMerchantItemData(saveString);
        return Convert.ToInt32(data["Level"]);
    }

    public void SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType merchantType, bool newItemsToSale) {
        string key = merchantType.ToString() + "_NewItemsToSale";
        ES3.Save(key, newItemsToSale);
    }

    public bool GetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType merchantType) {
        string key = merchantType.ToString() + "_NewItemsToSale";
        return ES3.Load(key, false);
    }

    public void SetHubMerchantItemBought(string saveString, bool value) {
        var data = LoadHubMerchantItemData(saveString);
        data["Bought"] = value;
        SaveHubMerchantItemData(saveString, data);
    }

    public void SetHubMerchantItemUnlocked(string saveString, bool value) {
        var data = LoadHubMerchantItemData(saveString);
        data["Unlocked"] = value;
        SaveHubMerchantItemData(saveString, data);
    }

    public void SetHubMerchantItemNewlyUnlocked(string saveString, bool value) {
        var data = LoadHubMerchantItemData(saveString);
        data["NewlyUnlocked"] = value;
        SaveHubMerchantItemData(saveString, data);
    }

    public void SetHubMerchantItemLevel(string saveString, int level) {
        var data = LoadHubMerchantItemData(saveString);
        data["Level"] = level;
        SaveHubMerchantItemData(saveString, data);
    }

    private Dictionary<string, object> LoadHubMerchantItemData(string saveString) {
        string key = saveString + "_Data";

        if (!ES3.KeyExists(key))
            return new Dictionary<string, object> {
                ["Bought"] = false,
                ["Unlocked"] = false,
                ["NewlyUnlocked"] = false,
                ["Level"] = 0
            };

        return ES3.Load<Dictionary<string, object>>(key);
    }
    private void SaveHubMerchantItemData(string saveString, Dictionary<string, object> data) {
        string key = saveString + "_Data";
        ES3.Save(key, data);
    }

    #endregion

    #region LEVELS
    public bool GetLevelUnlocked(LevelSO levelSO) {
        return ES3.Load(levelSO.name, false);
    }

    public void SetLevelUnlocked(LevelSO levelSO) {
        ES3.Save(levelSO.name, true);

        OnLevelSOUnlocked?.Invoke(this, new OnLevelSOUnlockedEventArgs {
            levelSOUnlocked = levelSO
        });
    }

    public List<LevelSO> GetPreviousLevelsUnlocked() {
        string key = "_previousLevelsUnlocked";
        List<string> levelNames = ES3.Load(key, new List<string>());

        List<LevelSO> previousLevelsUnlocked = new List<LevelSO>();
        foreach(string name in levelNames) {
            foreach(LevelSO levelSO in allLevelSOList) {
                if(levelSO.levelNameLocalizationKey == name) {
                    previousLevelsUnlocked.Add(levelSO);
                }
            }
        }

        return previousLevelsUnlocked;
    }

    public bool GetFinalLevelCompleted() {
        return GetLevelCompleted(allLevelSOList[allLevelSOList.Count]);
    }

    public void SetPreviousLevelsUnlocked(List<LevelSO> levelSOList) {
        string key = "_previousLevelsUnlocked";

        List<string> levelNames = new List<string>();
        foreach(LevelSO levelSO in levelSOList) {
            levelNames.Add(levelSO.levelNameLocalizationKey);
        }

        ES3.Save(key, levelNames);
    }

    public bool GetLevelCompleted(LevelSO levelSO) {
        string key = levelSO.ToString() + "_Completed";
        return ES3.Load(key, false);
    }

    public void SetLevelCompleted(LevelSO levelSO) {
        Debug.Log("SetLevelCompleted " + levelSO);
        SetLastLevelCompleted(levelSO);
        string key = levelSO.ToString() + "_Completed";
        ES3.Save(key, true);

        ProgressInHordeModeThroughMainGame(levelSO);
    }

    private void ProgressInHordeModeThroughMainGame(LevelSO levelSO) {
        if (levelSO != null && levelSO.linkedHordeUnlock != HordeModeProgressionManager.HordeModeUnlockables.None) {
            HordeModeProgressionManager.Instance.EnsureUnlockReached_MainGame(levelSO.linkedHordeUnlock);
        }
    }

    public void SetLastLevelCompleted(LevelSO levelSO) {
        string key =  "LastLevelCompleted_";
        ES3.Save(key, levelSO.ToString());
    }
    public string GetLastLevelCompletedString() {
        string key = "LastLevelCompleted_";
        if (!ES3.KeyExists(key)) return "";
        return ES3.Load<string>(key);
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

   

    #endregion

    #region OTHER
    public bool GetPlayerUnlockedFlagCarry() {
        return ES3.Load("playerUnlockedFlagCarry", flagCarry_Debug);
    }
    public void SetPlayerUnlockedFlagCarry(bool playerUnlockedFlagCarry) {
        ES3.Save("playerUnlockedFlagCarry", playerUnlockedFlagCarry);
    }
    #endregion

    #region CREATURES

    public void SetCreatureUnlocked(CreatureSO creatureSO) {
        string key = creatureSO.enemyName + "_unlocked";
        ES3.Save(key, true);

        OnCreatureSOUnlocked?.Invoke(this, new OnCreatureSOUnlockedEventArgs {
            creatureSOUnlocked = creatureSO
        });
    }

    public bool GetCreatureUnlocked(CreatureSO creatureSO) {
        string key = creatureSO.enemyName + "_unlocked";
        return ES3.Load(key, false);
    }

    #endregion



    private void OnApplicationQuit() {

        if (destroySaveOnApplicationQuit) {
            ES3.DeleteFile("SaveFile.es3");
        }
    }
}
