using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager : MonoBehaviour
{
    public static HUBManager Instance;

    public bool DEBUGMODE;

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

    private bool hubFireExtractable;
    private bool playerInteractedWithMerchantOnce;
    private bool playerExtractedEmber;
    private bool merchantEndedTalking;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetRedGemPositions();
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGreenGemPositions();
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions);
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions);

        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;

        if (!MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) {
            // FIRST HUB ENCOUNTER

            StartCoroutine(FirstHUBSpawnCoroutine());
            enterHubCollider.gameObject.SetActive(true);
            Dog.Instance.SetPosition(firstHubLoadDogSpawnPoint.transform.position);
            Player.Instance.SetPosition(firstHubLoadPlayerSpawnPoint.transform.position);

            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        }

        else {
            // Player loads game OR is coming back from level

            if(!MetaProgressionManager.Instance.GetNextHubArrivalThroughPortal()) {
                // Player is not coming back from a level (ex. loading game)
                Vector3 playerPosition = MetaProgressionManager.Instance.GetPlayerHubPosition();
                Player.Instance.SetPosition(playerPosition);

            } else {

                // Player is coming back from a level
                SaveHub();

            }

            enterHubCollider.gameObject.SetActive(false);
            MusicManager.Instance.PlayMusicDelayed(hubDelayToStartPlayingMusic);
        }

        if(DEBUGMODE) {
            Player.Instance.SetPosition(DEBUGPlayerSpawnPoint.position);
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

            lastLevelGemsRewarded = true;
        }
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

        yield return new WaitForSeconds(2f);

        RewardLastLevelGems();

        yield return new WaitForSeconds(2f);
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

    public Portal GetLevelLinkedPortal(LevelSO levelSO) {
        foreach (Portal portal in allPortalsInHub) {
            if (portal.GetLevelSOIsInPortal(levelSO)) {
                return portal;
            }
        }

        return null;
    }

    public void SaveHub() {
        MetaProgressionManager.Instance.SaveHubGems();
        MetaProgressionManager.Instance.SavePlayerHubPosition(Player.Instance.transform.position);
        MetaProgressionManager.Instance.SetGemsRewarded(lastLevelGemsRewarded);

        PlayerSave.Instance.SetPrimaryActiveGunSO(PlayerShoot.Instance.GetPrimaryGunSO());

        foreach (HubMerchant hubMerchant in hubMerchantList) {
            hubMerchant.SaveMerchant();
        }
    }

    private void OnDestroy() {

        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
