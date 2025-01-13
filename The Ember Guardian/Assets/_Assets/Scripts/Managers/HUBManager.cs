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
    [SerializeField] private Portal firstPortalUnlocked;
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
        if(!MetaProgressionManager.Instance.GetLevelUnlocked(level1SO)) {
            // FIRST HUB ENCOUNTER

            StartCoroutine(FirstHUBSpawnCoroutine());
            enterHubCollider.gameObject.SetActive(true);
            Dog.Instance.SetPosition(firstHubLoadDogSpawnPoint.transform.position);
            Player.Instance.SetPosition(firstHubLoadPlayerSpawnPoint.transform.position);

            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;
            UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        }

        else {
            // Player loads game OR is coming back from level

            if(!MetaProgressionManager.Instance.GetNextHubArrivalThroughPortal()) {
                // Player is not coming back from a level (ex. loading game)
                Vector3 playerPosition = MetaProgressionManager.Instance.GetPlayerHubPosition();
                Player.Instance.SetPosition(playerPosition);
            }

            enterHubCollider.gameObject.SetActive(false);
            MusicManager.Instance.PlayMusicDelayed(hubDelayToStartPlayingMusic);
        }

        if(DEBUGMODE) {
            Player.Instance.SetPosition(DEBUGPlayerSpawnPoint.position);
        }

        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetRedGemPositions();
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGreenGemPositions();
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions);
        UICurrencyManager.Instance.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions);
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
        if (!playerExtractedEmber || !playerInteractedWithMerchantOnce) return;
        if (merchantEndedTalking) return;

        merchantEndedTalking = true;
        StartCoroutine(ActivateTeleporterCoroutine());
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if ((e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember)) {
            playerExtractedEmber = true;

            if(playerInteractedWithMerchantOnce) {
                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
                StartCoroutine(StartGemMerchantOpenGrassyAreaLines());
            }
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (playerInteractedWithMerchantOnce) return;
        playerInteractedWithMerchantOnce = true;

        hubFireExtractable = true;
        hubFire.SetHubFireEmberExtractable();
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
        gemMerchantTalkUI.SetTalkingWithMerchant(gemMerchantOutroTextLines, false);
    }

    private IEnumerator ActivateTeleporterCoroutine() {
        MetaProgressionManager.Instance.SetLevelUnlocked(level1SO);
        SaveHub();
        PauseMenuUI.Instance.SetCanSave(true);

        CameraManager.Instance.ChangeCameraTarget(firstPortalUnlocked.transform);
        Player.Instance.DisableControlInputs();
        firstPortalUnlocked.SetPortalUnlockedInSave();

        yield return new WaitForSeconds(3.5f);
        firstPortalUnlocked.UnlockPortal();
        yield return new WaitForSeconds(3.5f);
        CameraManager.Instance.ResetCameraTargetToPlayer();
        Player.Instance.EnableControlInputs();

    }

    #endregion

    public void SaveHub() {
        MetaProgressionManager.Instance.SetHubFireEmberExtractable(hubFireExtractable);
        MetaProgressionManager.Instance.SaveHubGems();
        MetaProgressionManager.Instance.SavePlayerHubPosition(Player.Instance.transform.position);
        MetaProgressionManager.Instance.SetGemsRewarded(lastLevelGemsRewarded);

        foreach (HubMerchant hubMerchant in hubMerchantList) {
            hubMerchant.SaveMerchant();
        }
    }

}
