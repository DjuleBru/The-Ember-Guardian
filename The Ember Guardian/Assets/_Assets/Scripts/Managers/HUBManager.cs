using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager : MonoBehaviour
{
    public static HUBManager Instance;

    private bool DEBUGMODE;

    [SerializeField] private HubChest firstHubChest;
    [SerializeField] private Transform firstHubLoadPlayerSpawnPoint;
    [SerializeField] private Transform firstHubLoadDogSpawnPoint;
    [SerializeField] private Transform DEBUGPlayerSpawnPoint;
    [SerializeField] private List<Portal> allPortalsInHub;
    [SerializeField] private TutorialCollider enterHubCollider;
    [SerializeField] private HubMerchantTalkUI gemMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO gemMerchantOutroTextLines;
    [SerializeField] private Fire hubFire;
    [SerializeField] private List<HubMerchant> hubMerchantList;

    [SerializeField] private LevelSO level1SO;

    private float hubDelayToStartPlayingMusic = 3f;

    private bool lastLevelGemsRewarded;
    private bool nextArrivalThroughPortal;

    private bool hubFireExtractable;
    private bool playerInteractedWithMerchantOnce;
    private bool playerExtractedEmber;
    private bool merchantEndedTalking;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DEBUGMODE = DebugManager.Instance.GetDebugMode_HUBManager();
        LoadBackpackGems();
        LoadHubChestGems();

        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;

        if (!MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) {
            // FIRST HUB ENCOUNTER

            StartCoroutine(FirstHUBSpawnCoroutine());
            enterHubCollider.gameObject.SetActive(true);
            Dog.Instance.SetPosition(firstHubLoadDogSpawnPoint.transform.position);
            Player.Instance.SetPosition(firstHubLoadPlayerSpawnPoint.transform.position);

            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        }

        else {
            // Player loads game OR is coming back from level

            firstHubChest.gameObject.SetActive(false);
            nextArrivalThroughPortal = MetaProgressionManager.Instance.GetNextHubArrivalThroughPortal();
            if (!nextArrivalThroughPortal) {
                // Player is not coming back from a level (ex. loading game)

                Vector3 playerPosition = MetaProgressionManager.Instance.GetPlayerHubPosition();
                Player.Instance.SetPosition(playerPosition);

            } else {

                // Player is coming back from a level
                //SaveHub();

            }

            lastLevelGemsRewarded = MetaProgressionManager.Instance.GetGemFromLastLevelRewarded();

            nextArrivalThroughPortal = false;
            enterHubCollider.gameObject.SetActive(false);
            MusicManager.Instance.PlayMusicDelayed(hubDelayToStartPlayingMusic);
        }

        if(DEBUGMODE) {
            Player.Instance.SetPosition(DEBUGPlayerSpawnPoint.position);
        }
    }

    private void LoadBackpackGems() {
        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.redGem, true);
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.greenGem, true);
        List<Vector3> yellowGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.yellowGem, true);
        List<Vector3> purpleGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.purpleGem, true);
        List<Vector3> blueGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.blueGem, true);

        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.yellowGem, yellowGemPositions);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.purpleGem, purpleGemPositions);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.blueGem, blueGemPositions);
    }

    private void LoadHubChestGems() {
        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.redGem, false);
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.greenGem, false);
        List<Vector3> yellowGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.yellowGem, false);
        List<Vector3> purpleGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.purpleGem, false);
        List<Vector3> blueGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.blueGem, false);

        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.yellowGem, yellowGemPositions);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.purpleGem, purpleGemPositions);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.blueGem, blueGemPositions);

    }

    public void PlayerEnteredHubFirstTime() {
        StartCoroutine(FirstHUBEnterCoroutine());
    }

    #region FIRST HUB ENCOUNTER

    private void HubMerchantTalkUI_OnAnyMerchantEndTalk(object sender, System.EventArgs e) {

        if (MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) {
            // Not first hub encounter

            HubMerchantTalkUI hubMerchantTalkUI = (HubMerchantTalkUI)sender;
            HubMerchant.HubMerchantType hubMerchantType = hubMerchantTalkUI.GetHubMerchantType();

            if(hubMerchantType == HubMerchant.HubMerchantType.GemMerchant) {
                // Player just finished talking to Gem Merchant : activate next level(s)

                List<LevelSO> levelSOToUnlockList = MetaProgressionManager.Instance.GetPreviousLevelsUnlocked();
                Debug.Log("levelSOToUnlockList " + levelSOToUnlockList.Count);
                StartCoroutine(ActivateTeleportersCoroutine(levelSOToUnlockList));
            }

        } else {

            // FIRST HUB ENCOUNTER
            if (!playerExtractedEmber || !playerInteractedWithMerchantOnce) return;
            if (merchantEndedTalking) return;

            merchantEndedTalking = true;

            List<LevelSO> level1SO_toList = new List<LevelSO> {
                level1SO
            };
            StartCoroutine(ActivateTeleportersCoroutine(level1SO_toList));
        }

    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) return;

        if ((e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember)) {
            playerExtractedEmber = true;

            if(playerInteractedWithMerchantOnce) {
                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
                StartCoroutine(StartGemMerchantOpenGrassyAreaLines());
            }
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) return;

        if (playerInteractedWithMerchantOnce) return;
        playerInteractedWithMerchantOnce = true;

        hubFireExtractable = true;
        hubFire.SetHubFireEmberExtractable();
        MetaProgressionManager.Instance.SetHubFireEmberExtractable(hubFireExtractable);
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader, LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber);
    }

    private IEnumerator FirstHUBSpawnCoroutine() {
        CameraManager.Instance.SetCameraOrthographicSize(8f);
        PauseMenuUI.Instance.SetCanSave(false);

        yield return new WaitForSeconds(4f);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUB_HeadToFire);
    }

    private IEnumerator FirstHUBEnterCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(0f);
        CameraManager.Instance.ZoomOut(false, .8f, 2f);
        MusicManager.Instance.PlayMusicDelayed(3f);

        yield return new WaitForSeconds(2f);

        LevelUI_Locations.Instance.ShowLocationText("The Eternal Flame");

        yield return new WaitForSeconds(7f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUB_HeadToNewLevel);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveTypes = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader,
        };
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveTypes);
    }

    private IEnumerator StartGemMerchantOpenGrassyAreaLines() {
        yield return new WaitForSeconds(.5f);
        Player.Instance.DisableControlInputs();
        gemMerchantTalkUI.SetTalkingWithMerchant(gemMerchantOutroTextLines);
    }

    private IEnumerator ActivateTeleportersCoroutine(List<LevelSO> levelSOList) {
        PauseMenuUI.Instance.SetCanSave(true);

        foreach (LevelSO levelSO in levelSOList) {
            MetaProgressionManager.Instance.SetLevelUnlocked(levelSO);

            Portal linkedPortal = GetLevelLinkedPortal(levelSO);
            CameraManager.Instance.ChangeCameraTarget(linkedPortal.transform);
            Player.Instance.DisableControlInputs();

            yield return new WaitForSeconds(3.5f);

            linkedPortal.UnlockOrActivatePortal();
            linkedPortal.SetPortalUnlockedInSave();
            linkedPortal.SetLinkedLevelSO(levelSO);

            yield return new WaitForSeconds(3f);
        }

        CameraManager.Instance.ResetCameraTargetToPlayer();
        Player.Instance.EnableControlInputs();

        SaveHub();
    }

    #endregion

    public bool GetLastLevelGemsRewarded() {
        return lastLevelGemsRewarded;
    }

    public void SetLastLevelGemsRewarded(bool lastLevelGemsRewarded) {
        this.lastLevelGemsRewarded = lastLevelGemsRewarded;
    }

    public Portal GetLevelLinkedPortal(LevelSO levelSO) {
        foreach (Portal portal in allPortalsInHub) {
            if (portal.GetLevelSOIsInPortal(levelSO)) {
                return portal;
            }
        }

        return null;
    }

    public void SaveHub() {
        Debug.Log("SaveHub nextArrivalThroughPortal" + nextArrivalThroughPortal);

        MetaProgressionManager.Instance.SaveHubGems();
        MetaProgressionManager.Instance.SavePlayerHubPosition(Player.Instance.transform.position);
        MetaProgressionManager.Instance.SetGemsRewarded(lastLevelGemsRewarded);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(nextArrivalThroughPortal);

        PlayerSave.Instance.SavePrimaryActiveGunSO(PlayerShoot.Instance.GetPrimaryGunSO());
        PlayerSave.Instance.SavePlayerMetaStats();
        DogStats.Instance.SaveDogStats();

        foreach (HubMerchant hubMerchant in hubMerchantList) {
            hubMerchant.SaveMerchant();
        }

        foreach (Portal portal in allPortalsInHub) {
            MetaProgressionManager.Instance.SetPortalLinkedLevelSOIndex(portal.GetPortalNumber(), portal.GetLinkedLevelSOIndex());
        }
    }

    private void OnDestroy() {

        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
