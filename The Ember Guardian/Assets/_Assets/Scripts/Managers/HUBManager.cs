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
    [SerializeField] private HubMerchant gemMerchant;
    [SerializeField] private HubMerchantTalkUI gemMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO gemMerchantIntroTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantOutroTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantComeBuyTextLines;

    [SerializeField] private GameObject gemMerchantIndicator;
    [SerializeField] private GameObject chestIndicator;
    [SerializeField] private GameObject fireIndicator;

    [SerializeField] private LevelSO level1SO;

    private float hubDelayToStartPlayingMusic = 3f;

    private int gemAmountDroppedInChest;
    private int initialRedGemsAfterTutorial = 5;
    private int initialGreenGemsAfterTutorial = 5;
    private int initialYellowGemsAfterTutorial = 5;
    private int initialPurpleGemsAfterTutorial = 1;
    private int totalGemsAfterTutorial;

    private bool firstHubEncounterRoutineOver;
    private bool nextArrivalThroughPortal;
    private bool hasShownParallelTextLinesSO;

    private bool playerInteractedWithMerchantOnce;
    private bool playerBoughtItem;
    private bool emberExtracted;
    private bool hubFireEmberExtractable;
    private bool extractEmberTooltipShown;
    private bool merchantEndedTalking;
    private bool fireIndicatorActive;
    private bool chestIndicatorActive;

    public event EventHandler OnHubSaved;

    private void Awake() {
        Instance = this;

        if (!demoHUB) {
            gemMerchantIndicator.gameObject.SetActive(false);
            chestIndicator.gameObject.SetActive(false);
            fireIndicator.gameObject.SetActive(false);
        }

        totalGemsAfterTutorial = initialGreenGemsAfterTutorial + initialYellowGemsAfterTutorial + initialRedGemsAfterTutorial + initialPurpleGemsAfterTutorial;

        string mainPath = "SaveFile.es3";
        string backupPath = "SaveFile_backup.es3";

        // Si le fichier principal est corrompu ou manquant mais qu’un backup existe
        if (!ES3.FileExists(mainPath) && ES3.FileExists(backupPath)) {
            ES3.CopyFile(backupPath, mainPath);
        }
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
            MetaProgressionManager.Instance.SetTutorialCompleted();

            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
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
            hubFire.RefreshHubFireEmberExtractable();
            RefreshPlayerHasGemsIndicators();
            if (!demoHUB) {
                MusicManager.Instance.PlayMusicDelayed(hubDelayToStartPlayingMusic);
            }
        }

        if(!demoHUB) {
            hubFire.OnPlayerTriggeredIn += HubFire_OnPlayerTriggeredIn;
            hubFire.OnPlayerTriggeredOut += HubFire_OnPlayerTriggeredOut;
            hubFire.OnFireEmberExtracted += HubFire_OnFireEmberExtracted;
            hubFire.OnFireEmberExtractionStarted += HubFire_OnFireEmberExtractionStarted;
            gemMerchant.OnPlayerTriggeredIn += GemMerchant_OnPlayerTriggeredIn;
            gemMerchant.OnPlayerTriggeredOut += GemMerchant_OnPlayerTriggeredOut;
            HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
            HubChest.Instance.OnChestClosed += HubChest_OnChestClosed;
        }

        if (DEBUGMODE) {
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
        List<Vector3> cyanGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.cyanGem, true);

        List<Quaternion> redGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.redGem, true);
        List<Quaternion> greenGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.greenGem, true);
        List<Quaternion> yellowGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.yellowGem, true);
        List<Quaternion> purpleGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.purpleGem, true);
        List<Quaternion> blueGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.blueGem, true);
        List<Quaternion> cyanGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.cyanGem, true);

        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions, redGemRotations);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions, greenGemRotations);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.yellowGem, yellowGemPositions, yellowGemRotations);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.purpleGem, purpleGemPositions, purpleGemRotations);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.blueGem, blueGemPositions, blueGemRotations);
        UICurrencyManager.PlayerInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.cyanGem, cyanGemPositions, cyanGemRotations);

    }

    private void LoadHubChestGems() {
        List<Vector3> redGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.redGem, false);
        List<Vector3> greenGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.greenGem, false);
        List<Vector3> yellowGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.yellowGem, false);
        List<Vector3> purpleGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.purpleGem, false);
        List<Vector3> blueGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.blueGem, false);
        List<Vector3> cyanGemPositions = MetaProgressionManager.Instance.GetGemPositions(PlayerCurrencies.CurrencyType.cyanGem, false);

        List<Quaternion> redGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.redGem, false);
        List<Quaternion> greenGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.greenGem, false);
        List<Quaternion> yellowGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.yellowGem, false);
        List<Quaternion> purpleGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.purpleGem, false);
        List<Quaternion> blueGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.blueGem, false);
        List<Quaternion> cyanGemRotations = MetaProgressionManager.Instance.GetGemRotations(PlayerCurrencies.CurrencyType.cyanGem, false);

        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.redGem, redGemPositions, redGemRotations);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.greenGem, greenGemPositions, greenGemRotations);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.yellowGem, yellowGemPositions, yellowGemRotations);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.purpleGem, purpleGemPositions, purpleGemRotations);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.blueGem, blueGemPositions, blueGemRotations);
        UICurrencyManager.HubInventoryUI.LoadCurrencies(PlayerCurrencies.CurrencyType.cyanGem, cyanGemPositions, cyanGemRotations);

    }

    public void PlayerEnteredHubFirstTime() {
        StartCoroutine(FirstHUBEnterCoroutine());
    }

    #region INDICATORS
    private void GemMerchant_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (firstHubEncounterRoutineOver) return;
        if (playerBoughtItem) return;
        if (totalGemsAfterTutorial != gemAmountDroppedInChest) return;

        gemMerchantIndicator.gameObject.SetActive(true);
    }

    private void GemMerchant_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (firstHubEncounterRoutineOver) return;

        gemMerchantIndicator.gameObject.SetActive(false);
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        if (!firstHubEncounterRoutineOver) return;
        RefreshPlayerHasGemsIndicators();

        if (!chestIndicatorActive) return;
        if (totalGemsAfterTutorial == gemAmountDroppedInChest) return;

        chestIndicator.gameObject.SetActive(true);
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        if (!chestIndicatorActive) return;

        chestIndicator.gameObject.SetActive(false);
    }

    private void HubFire_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!hubFireEmberExtractable) return;
        if (!firstHubEncounterRoutineOver) {
            if (emberExtracted) return;
        }
        else {
            if (!fireIndicatorActive) return;
        }


        fireIndicator.gameObject.SetActive(true);
    }

    private void HubFire_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!hubFireEmberExtractable) return;

        if (!firstHubEncounterRoutineOver) {
            if (emberExtracted) return;
            extractEmberTooltipShown = true;
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_extractEmber"), InputControlIcons.Control.Interact, 5f);
        }
        else {
            if (!fireIndicatorActive) return;
        }

        fireIndicator.gameObject.SetActive(false);
    }
    private void HubFire_OnFireEmberExtracted(object sender, System.EventArgs e) {
        if (hubFire.GetEmberExtracted()) {
            emberExtracted = true;
            fireIndicator.gameObject.SetActive(false);
            fireIndicatorActive = false;
        }
    }

    private void HubFire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        if (extractEmberTooltipShown) {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            extractEmberTooltipShown = false;
        }
    }

    #endregion

    #region FIRST HUB ENCOUNTER

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, EventArgs e) {
        if (firstHubEncounterRoutineOver) return;

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
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
                // Player just finished talking to Gem Merchant
                List<LevelSO> levelSOToUnlockList = MetaProgressionManager.Instance.GetPreviousLevelsUnlocked();
                LevelSO nextLevelSO = levelSOToUnlockList[0];

                if (nextLevelSO.isBranchingLevel && nextLevelSO.requiredLevelSO1 != null) {
                    // Parallel level : assign correct next textlinesSO
                    bool level1Completed = MetaProgressionManager.Instance.GetLevelCompleted(nextLevelSO.requiredLevelSO1);
                    bool level2Completed = MetaProgressionManager.Instance.GetLevelCompleted(nextLevelSO.requiredLevelSO2);
                    MerchantTextLinesSO nextTextLines = null;

                    if (hasShownParallelTextLinesSO) {
                        if (level1Completed && level2Completed) {
                            StartCoroutine(ActivateTeleportersCoroutine(levelSOToUnlockList));
                        }
                        return;
                    };

                    if (level1Completed && !level2Completed) {
                        nextTextLines = nextLevelSO.requiredLevelSO1.gemMerchantTextLinesAfterLevel.nextTextLineSO_LinkedLevelNotCompleted;
                    }

                    if (level2Completed && !level1Completed) {
                        nextTextLines = nextLevelSO.requiredLevelSO2.gemMerchantTextLinesAfterLevel.nextTextLineSO_LinkedLevelNotCompleted;
                    }

                    if (level1Completed && level2Completed) {
                        nextTextLines = nextLevelSO.requiredLevelSO1.gemMerchantTextLinesAfterLevel.nextTextLineSO_LinkedLevelCompleted;
                    }

                    gemMerchantTalkUI.SetTextLinesSO(nextTextLines);
                    gemMerchant.SetHasTalkLinesToShow(true);
                    hasShownParallelTextLinesSO = true;

                }
                else {
                    // No other parallel level : activate next level(s)

                    if (hasShownParallelTextLinesSO) return;
                    Debug.Log("levelSOToUnlockList " + levelSOToUnlockList.Count);
                    StartCoroutine(ActivateTeleportersCoroutine(levelSOToUnlockList));
                }
 
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
            gemMerchant.SetPlayerCanInteractWithMerchant(false);
        }

        playerInteractedWithMerchantOnce = true;

        if (gemAmountDroppedInChest == totalGemsAfterTutorial && !playerBoughtItem) {
            gemMerchantIndicator.gameObject.SetActive(true);
            hubFire.SetHubFireEmberExtractable(false);
            gemMerchant.SetPlayerCanInteractWithMerchant(true);
            return;
        };


    }

    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, EventArgs e) {
        if (playerBoughtItem) return;
        gemMerchantIndicator.SetActive(false);
    }

    private IEnumerator FirstHUBSpawnCoroutine() {
        yield return new WaitForEndOfFrame();

        CameraManager.Instance.SetCameraOrthographicSize(8f);

        if (!DEBUGMODE) {
            PauseMenuUI.Instance.SetCanSave(false);
        }

        yield return new WaitForEndOfFrame();
        HubChest.Instance.SetCanOpenChest(false);
        firstPortalArrival.TeleportPlayerOutInHubManually();
        yield return new WaitForSeconds(2f);

        if(MetaProgressionManager.Instance.GetTutorialSkipped()) {
            // Tutorial has been skipped

            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.yellowGem, initialYellowGemsAfterTutorial);
            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.redGem, initialRedGemsAfterTutorial);
            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.greenGem, initialGreenGemsAfterTutorial);
            MetaProgressionManager.Instance.SetGemAmountFromLevel(PlayerCurrencies.CurrencyType.purpleGem, initialPurpleGemsAfterTutorial);

            List<PlayerCurrencies.CurrencyType> tutorialSkippedGems = new List<PlayerCurrencies.CurrencyType> {
                PlayerCurrencies.CurrencyType.yellowGem,
                PlayerCurrencies.CurrencyType.redGem,
                PlayerCurrencies.CurrencyType.greenGem,
                PlayerCurrencies.CurrencyType.purpleGem
            };
            List<int> tutorialSkippedGemAmount = new List<int> {
                initialYellowGemsAfterTutorial,initialRedGemsAfterTutorial,initialGreenGemsAfterTutorial,initialPurpleGemsAfterTutorial
            };
            UICurrencyManager.PlayerInventoryUI.AddMultipleCurrencies(tutorialSkippedGems, tutorialSkippedGemAmount);
        }

        yield return new WaitForSeconds(2f);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUB_HeadToFire);
        gemMerchantTalkUI.SetTextLinesSO(gemMerchantIntroTextLines);

        yield return new WaitForSeconds(1.5f);

        DirectionIndicator.Instance.ShowDirection(1f);
    }

    private IEnumerator FirstHUBEnterCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(0f);
        CameraManager.Instance.ZoomOut(false, .8f, 2f);
        MusicManager.Instance.PlayMusicDelayed(3f);

        yield return new WaitForSeconds(2f);

        LevelUI_Locations.Instance.ShowLocationText(LocalizationManager.Instance.GetLocalizedText("TheEternalFlame"));
        gemMerchantIndicator.gameObject.SetActive(true);

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

        foreach (LevelSO levelSO in levelSOList) {
            MetaProgressionManager.Instance.SetLevelUnlocked(levelSO);

            Portal linkedPortal = GetLevelLinkedPortal(levelSO);
            CameraManager.Instance.ChangeCameraTarget(linkedPortal.transform);

            yield return new WaitForSeconds(3.5f);

            linkedPortal.UnlockOrActivatePortal();

            yield return new WaitForSeconds(3f);
        }
        CameraManager.Instance.ResetCameraTargetToPlayer();

        PauseMenuUI.Instance.SetCanSave(true);
        firstHubEncounterRoutineOver = true;
        ES3.Save("firstHubEncounterRoutineOver", true);
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

    public void SavePortalStatuses() {
        foreach (Portal portal in allPortalsInHub) {
            portal.SetPortalUnlockedInSave();
        }
    }

    private void RefreshPlayerHasGemsIndicators() {
        StartCoroutine(RefreshPlayerHasGemsIndicatorsAfterDelay());
    }

    private IEnumerator RefreshPlayerHasGemsIndicatorsAfterDelay() {
        yield return new WaitForSeconds(.1f);
        if (GetIsDemo()) yield break;

        bool playerHasGemsInInventory = (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory.gem).Count != 0);

        if (playerHasGemsInInventory) {

            chestIndicatorActive = true;
            chestIndicator.gameObject.SetActive(true);

            fireIndicatorActive = false;
            fireIndicator.gameObject.SetActive(false);
            hubFireEmberExtractable = false;

        }
        else {

            chestIndicatorActive = false;
            chestIndicator.gameObject.SetActive(false);

            if (!PlayerCurrencies.Instance.GetCarryingEmber()) {
                fireIndicatorActive = true;
                fireIndicator.gameObject.SetActive(true);
                hubFireEmberExtractable = true;
            }
            else {
                fireIndicatorActive = false;
                fireIndicator.gameObject.SetActive(false);
                hubFireEmberExtractable = false;
            };

        }
    }

    public void SaveHub() {
        SaveHubInstant();
    }

    private void SaveHubInstant() {
        string mainPath = "SaveFile.es3";
        string backupPath = "SaveFile_backup.es3";

        try {
            if (ES3.FileExists(mainPath)) {
                ES3.CopyFile(mainPath, backupPath);
            }

            MetaProgressionManager.Instance.SaveHubGemsBatch();
            MetaProgressionManager.Instance.SavePlayerHubPosition(Player.Instance.transform.position);
            MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber();
            MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(nextArrivalThroughPortal);
            PlayerSave.Instance.SavePrimaryActiveGunSO(PlayerShoot.Instance.GetPrimaryGunSO());
            PlayerSave.Instance.SaveSecondaryActiveGunSO(PlayerShoot.Instance.GetSecondaryGunSO());
            PlayerSave.Instance.SavePlayerMetaStats();
            PlayerSave.Instance.SaveNewUnlockedSkills();
            DogStats.Instance.SaveDogStats();
            WorkerStats.Instance.SaveWorkerValues();
            StructureStats.Instance.SaveStructureStats();
            TrapManager.Instance.SaveNewUnlockedTraps();

            SavePortalStatuses();

            if (ArchitectTable.Instance != null) {
                ArchitectTable.Instance.SaveStats();
            }

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
        catch (Exception ex) {
        }

    }
    public bool GetIsDemo() {
        return demoHUB;
    }

    private void OnDestroy() {

        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
    }

}
