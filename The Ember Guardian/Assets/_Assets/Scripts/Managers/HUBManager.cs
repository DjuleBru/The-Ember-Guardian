using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager : MonoBehaviour
{
    public static HUBManager Instance;

    private bool DEBUGMODE;

    [SerializeField] private bool demoHUB;

    [SerializeField] private Transform firstHubLoadPlayerSpawnPoint;
    [SerializeField] private Transform firstHubLoadDogSpawnPoint;
    [SerializeField] private Transform DEBUGPlayerSpawnPoint;
    [SerializeField] private List<Portal> allPortalsInHub;
    [SerializeField] private Portal firstPortalArrival;
    [SerializeField] private TutorialCollider enterHubCollider;
    [SerializeField] private Fire hubFire;
    [SerializeField] private List<HubMerchant> hubMerchantList;
    [SerializeField] private HubMerchantTalkUI gemMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO gemMerchantIntroTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantOutroTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantComeBuyTextLines;

    [SerializeField] private GameObject gemMerchantIndicator;

    [SerializeField] private LevelSO level1SO;

    private float hubDelayToStartPlayingMusic = 3f;

    private int gemAmountDroppedInChest;
    private int initialRedGemsAfterTutorial = 3;
    private int initialGreenGemsAfterTutorial = 3;
    private int initialYellowGemsAfterTutorial = 3;
    private int totalGemsAfterTutorial;

    private bool firstHubEncounterRoutineOver;
    private bool nextArrivalThroughPortal;

    private bool playerInteractedWithMerchantOnce;
    private bool playerBoughtItem;
    private bool emberExtracted;
    private bool hubFireEmberExtractable;
    private bool extractEmberTooltipShown;
    private bool merchantEndedTalking;

    public event EventHandler OnHubSaved;

    private void Awake() {
        Instance = this;

        if(!demoHUB) {
            gemMerchantIndicator.gameObject.SetActive(false);
        }
        totalGemsAfterTutorial = initialGreenGemsAfterTutorial + initialYellowGemsAfterTutorial + initialRedGemsAfterTutorial;
    }

    private void Start() {
        DEBUGMODE = DebugManager.Instance.GetDebugMode_HUBManager();
        firstHubEncounterRoutineOver = ES3.Load("firstHubEncounterRoutineOver", firstHubEncounterRoutineOver);
        LoadPlayerInventory();
        LoadHubChestGems();

        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;

        if (!demoHUB && !firstHubEncounterRoutineOver && !DEBUGMODE) {
            // FIRST HUB ENCOUNTER

            StartCoroutine(FirstHUBSpawnCoroutine());
            enterHubCollider.gameObject.SetActive(true);

            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            hubFire.OnPlayerTriggeredIn += HubFire_OnPlayerTriggeredIn;
            hubFire.OnFireEmberExtractionStarted += HubFire_OnFireEmberExtractionStarted;
            HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
        }

        else {
            // Player loads game OR is coming back from level

            nextArrivalThroughPortal = MetaProgressionManager.Instance.GetNextHubArrivalThroughPortal();
            if (!nextArrivalThroughPortal) {
                // Player is not coming back from a level (ex. loading game)

                Vector3 playerPosition = MetaProgressionManager.Instance.GetPlayerHubPosition();
                Player.Instance.SetPosition(playerPosition);

            }

            nextArrivalThroughPortal = false;
            enterHubCollider.gameObject.SetActive(false);

            if(!demoHUB) {
                MusicManager.Instance.PlayMusicDelayed(hubDelayToStartPlayingMusic);
            }
        }

        if(DEBUGMODE) {
            Player.Instance.SetPosition(DEBUGPlayerSpawnPoint.position);
        }
    }


    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (firstHubEncounterRoutineOver) return;

        gemAmountDroppedInChest++;

        if (gemAmountDroppedInChest == (totalGemsAfterTutorial)) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_DropGems, LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_BuyUpgrade);
            StartCoroutine(StartGemMerchantTextLines(gemMerchantComeBuyTextLines));
            HubChest.Instance.SetInteractionTooltipShown();
        }
    }

    private void LoadPlayerInventory() {
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
    private void HubFire_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!hubFireEmberExtractable) return;

        if (!playerBoughtItem) return;
        if (emberExtracted) return;
        extractEmberTooltipShown = true;
        PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_extractEmber"), InputControlIcons.Control.Interact, 5f);
        
    }

    private void HubFire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        if (extractEmberTooltipShown) {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            extractEmberTooltipShown = false;
        }
    }

    private void HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        if (firstHubEncounterRoutineOver) return;

        if (sender is HubMerchantItem_GemMerchantItem) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_BuyUpgrade, LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber);
            gemMerchantIndicator.SetActive(false);
            HubFireVisualIndicator.Instance.SetIndicatorActive();
            hubFire.SetHubFireEmberExtractable(true);
            hubFireEmberExtractable = true;
            playerBoughtItem = true;
        }
    }

    private void HubMerchantTalkUI_OnAnyMerchantEndTalk(object sender, System.EventArgs e) {

        if (firstHubEncounterRoutineOver) {
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
            if (!emberExtracted || !playerInteractedWithMerchantOnce) return;
            if (merchantEndedTalking) return;

            merchantEndedTalking = true;

            List<LevelSO> level1SO_toList = new List<LevelSO> {
                level1SO
            };
            StartCoroutine(ActivateTeleportersCoroutine(level1SO_toList));
        }

    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (firstHubEncounterRoutineOver) return;

        if ((e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember)) {
            emberExtracted = true;
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
            StartCoroutine(StartGemMerchantTextLines(gemMerchantOutroTextLines));
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (firstHubEncounterRoutineOver) return;


        if(!playerInteractedWithMerchantOnce) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader, LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_DropGems);
            HubChest.Instance.SetCanOpenChest(true);
        }

        playerInteractedWithMerchantOnce = true;

        if (gemAmountDroppedInChest == totalGemsAfterTutorial && !playerBoughtItem) {
            gemMerchantIndicator.gameObject.SetActive(true);
            hubFire.SetHubFireEmberExtractable(false);
            return;
        };


    }

    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, EventArgs e) {
        if (playerBoughtItem) return;
        gemMerchantIndicator.SetActive(false);
    }

    private IEnumerator FirstHUBSpawnCoroutine() {
        CameraManager.Instance.SetCameraOrthographicSize(8f);

        if (!DEBUGMODE) {
            PauseMenuUI.Instance.SetCanSave(false);
        }

        yield return new WaitForEndOfFrame();
        HubChest.Instance.SetCanOpenChest(false);
        firstPortalArrival.TeleportPlayerOutInHubManually();
        yield return new WaitForSeconds(2f);

        if(!MetaProgressionManager.Instance.GetTutorialCompleted()) {
            // Tutorial has been skipped

            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.yellowGem, initialYellowGemsAfterTutorial);
            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.redGem, initialRedGemsAfterTutorial);
            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.greenGem, initialGreenGemsAfterTutorial);

            List<PlayerCurrencies.CurrencyType> tutorialSkippedGems = new List<PlayerCurrencies.CurrencyType> {
                PlayerCurrencies.CurrencyType.yellowGem,
                PlayerCurrencies.CurrencyType.redGem,
                PlayerCurrencies.CurrencyType.greenGem
            };
            List<int> tutorialSkippedGemAmount = new List<int> {
                initialYellowGemsAfterTutorial,initialRedGemsAfterTutorial,initialGreenGemsAfterTutorial,
            };
            UICurrencyManager.PlayerInventoryUI.AddMultipleCurrencies(tutorialSkippedGems, tutorialSkippedGemAmount);
        }

        yield return new WaitForSeconds(2f);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUB_HeadToFire);
        gemMerchantTalkUI.SetTextLinesSO(gemMerchantIntroTextLines);
    }

    private IEnumerator FirstHUBEnterCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(0f);
        CameraManager.Instance.ZoomOut(false, .8f, 2f);
        MusicManager.Instance.PlayMusicDelayed(3f);

        yield return new WaitForSeconds(2f);

        LevelUI_Locations.Instance.ShowLocationText(LocalizationManager.Instance.GetLocalizedText("TheEternalFlame"));

        yield return new WaitForSeconds(7f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUB_HeadToNewLevel);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveTypes = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader,
        };
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveTypes);
    }

    private IEnumerator StartGemMerchantTextLines(MerchantTextLinesSO textLines) {
        yield return new WaitForSeconds(.5f);
        gemMerchantTalkUI.SetTalkingWithMerchant(textLines);
    }

    private IEnumerator ActivateTeleportersCoroutine(List<LevelSO> levelSOList) {
        PauseMenuUI.Instance.SetCanSave(true);

        foreach (LevelSO levelSO in levelSOList) {
            MetaProgressionManager.Instance.SetLevelUnlocked(levelSO);

            Portal linkedPortal = GetLevelLinkedPortal(levelSO);
            CameraManager.Instance.ChangeCameraTarget(linkedPortal.transform);

            yield return new WaitForSeconds(3.5f);

            linkedPortal.UnlockOrActivatePortal();
            linkedPortal.SetPortalUnlockedInSave();
            linkedPortal.SetLinkedLevelSO(levelSO);

            yield return new WaitForSeconds(3f);
        }

        firstHubEncounterRoutineOver = true;
        ES3.Save("firstHubEncounterRoutineOver", true);
        CameraManager.Instance.ResetCameraTargetToPlayer();
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
        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber();
        MetaProgressionManager.Instance.SavePlayerHubPosition(Player.Instance.transform.position);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(nextArrivalThroughPortal);

        PlayerSave.Instance.SavePrimaryActiveGunSO(PlayerShoot.Instance.GetPrimaryGunSO());
        PlayerSave.Instance.SaveSecondaryActiveGunSO(PlayerShoot.Instance.GetSecondaryGunSO());
        PlayerSave.Instance.SavePlayerMetaStats();
        DogStats.Instance.SaveDogStats();
        WorkerStats.Instance.SaveWorkerValues();
        StructureStats.Instance.SaveStructureStats();
        ArchitectTable.Instance.SaveStats();

        bool holdingEmber = false;
        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.ember).Count != 0) {
            holdingEmber = true;
        }
        ES3.Save("holdingEmber", holdingEmber);

        foreach (HubMerchant hubMerchant in hubMerchantList) {
            hubMerchant.SaveMerchant();
        }

        foreach (Portal portal in allPortalsInHub) {
            MetaProgressionManager.Instance.SetPortalLinkedLevelSOIndex(portal.GetPortalNumber(), portal.GetLinkedLevelSOIndex());
        }

        OnHubSaved?.Invoke(this, EventArgs.Empty);
    }

    public bool GetIsDemo() {
        return demoHUB;
    }

    private void OnDestroy() {

        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
