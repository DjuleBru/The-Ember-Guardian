using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager_Demo : MonoBehaviour
{
    private bool firstDemoHubEncounter;
    private bool demoFirstLevelCompleted;
    private bool demoMainLevelEncountered;
    private int demoLevelLostAmount;

    [SerializeField] private List<HubMerchant> functionalDemoHubMerchantList;
    [SerializeField] private List<HubMerchant> decorationalDemoHubMerchantList;
    [SerializeField] private HubMerchant gemMerchant;
    [SerializeField] private LevelNPCGemReward gemMerchantReward;
    [SerializeField] private HubMerchantTalkUI armorerTalkUI;
    [SerializeField] private HubMerchantTalkUI trainerTalkUI;
    [SerializeField] private HubMerchantTalkUI gemMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO gemMerchantOpenTeleporterTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantComeBuyTextLines;
    [SerializeField] private MerchantTextLinesSO gemMerchantLevelLostOnceTextLinesSO;
    [SerializeField] private MerchantTextLinesSO gemMerchantLevelLostWithWidowTextLinesSO;
    [SerializeField] private MerchantTextLinesSO gemMerchantLevelLostAgainTextLinesSO;
    [SerializeField] private MerchantTextLinesSO gemMerchantLevelCompletedTextLinesSO;
    [SerializeField] private MerchantTextLinesSO armorerIntroTextLinesSO;
    [SerializeField] private MerchantTextLinesSO trainerIntroTextLinesSO;
    [SerializeField] private Fire hubFire;
    [SerializeField] private Portal grassyAreaPortal;
    [SerializeField] private LevelSO demoMainLevelSO;
    [SerializeField] private LevelSO demoFirstLevelSO;
    [SerializeField] private VideoTipSO endDemoTipSO;
    [SerializeField] private VideoTipSO gunMerchantTip;

    [SerializeField] private GameObject chestIndicator;
    [SerializeField] private GameObject fireIndicator;
    [SerializeField] private GameObject gemMerchantIndicator;

    private int gemMerchantStoppedInteractingCount;
    private int gemsToDropInChest = 7;
    private int gemAmountDroppedInChest;
    private bool playerBoughtItem;
    private bool hubFireEmberExtractable;
    private bool emberExtractionTalkLineShown;
    private bool emberExtracted;
    private bool playerHasGemsInInventory;
    private bool fireIndicatorActive;
    private bool chestIndicatorActive;
    private bool demoMainLevelCompleted;
    private bool firstHubEnterWithDemoLevelCompleted;
    private bool functionalMerchantsShopsUnlocked;
    private bool gunTipShown;
    private bool handleAnyLevelDefeatHubEvolutionDone;
    private bool playerDiedWithWidowTextShown;
    private bool merchantsUnlocked;

    private void Awake() {
        // First demo hub encounter becomes true when player moves on teleporter
        firstDemoHubEncounter = ES3.Load("firstDemoHubEncounter", true);
        demoFirstLevelCompleted = ES3.Load("demoFirstLevelCompleted", false);
        demoMainLevelEncountered = ES3.Load("demoMainLevelEncountered", false);
        demoLevelLostAmount = ES3.Load("demoLevelLostAmount", 0);
        demoMainLevelCompleted = ES3.Load("demoMainLevelCompleted", false);
        firstHubEnterWithDemoLevelCompleted = ES3.Load("firstHubEnterWithDemoLevelCompleted", true);
        gunTipShown = ES3.Load("gunTipShown", false);
        merchantsUnlocked = ES3.Load("merchantsUnlocked", false);

        Debug.Log("firstDemoHubEncounter " + firstDemoHubEncounter);
        Debug.Log("demoFirstLevelCompleted " + demoFirstLevelCompleted);
        Debug.Log("demoMainLevelEncountered " + demoMainLevelEncountered);
        Debug.Log("demoLevelLostAmount " + demoLevelLostAmount);
        Debug.Log("demoMainLevelCompleted " + demoMainLevelCompleted);
        Debug.Log("firstHubEnterWithDemoLevelCompleted " + firstHubEnterWithDemoLevelCompleted);
        Debug.Log("merchantsUnlocked " + merchantsUnlocked);

        chestIndicator.gameObject.SetActive(false);
        fireIndicator.gameObject.SetActive(false);
        gemMerchantIndicator.gameObject.SetActive(false);
    }

    void Start() {
        if(firstDemoHubEncounter) {

            StartCoroutine(FirstHUBEnterCoroutine());

        } else {
            MusicManager.Instance.PlayMusicDelayed(3f);

            if(demoLevelLostAmount == 0 && !demoFirstLevelCompleted) {
                StartCoroutine(HandleBackFromFirstRun());
                grassyAreaPortal.SetLinkedLevelSO(demoFirstLevelSO);
            }

            if (demoFirstLevelCompleted) {
                if(!demoMainLevelEncountered && !merchantsUnlocked) {
                    // Player lost level once 
                    StartCoroutine(HandleFirstLevelCompletedHubEvolution());
                    grassyAreaPortal.SetLinkedLevelSO(demoMainLevelSO);

                }
                else {
                    // Player returned to hub without loosing the level
                    StartCoroutine(HandleAnyLevelDefeatHubEvolution());
                    grassyAreaPortal.SetLinkedLevelSO(demoMainLevelSO);
                    RefreshPlayerHasGemsIndicators();
                }
            }

            if(demoLevelLostAmount >= 1) {
                StartCoroutine(HandleAnyLevelDefeatHubEvolution());
                grassyAreaPortal.SetLinkedLevelSO(demoMainLevelSO);
            }

            if (demoMainLevelCompleted && firstHubEnterWithDemoLevelCompleted) {
                StartCoroutine(HandleLevelSuccessHubEvolution());
                grassyAreaPortal.SetLinkedLevelSO(demoMainLevelSO);
            }

            RefreshPlayerHasGemsIndicators();
        }

        UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;
        
        HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        HubMerchant.OnAnyPlayerTriggeredIn += HubMerchant_OnAnyPlayerTriggeredIn;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        gemMerchant.OnPlayerStoppedInteractingWithHubMerchant += GemMerchant_OnPlayerStoppedInteractingWithHubMerchant;
        grassyAreaPortal.OnPlayerMovedOnTeleporter += GrassyAreaPortal_OnPlayerMovedOnTeleporter;

        hubFire.OnPlayerTriggeredIn += HubFire_OnPlayerTriggeredIn;
        hubFire.OnPlayerTriggeredOut += HubFire_OnPlayerTriggeredOut;
        hubFire.OnFireEmberExtracted += HubFire_OnFireEmberExtracted;
        gemMerchant.OnPlayerTriggeredIn += GemMerchant_OnPlayerTriggeredIn;
        gemMerchant.OnPlayerTriggeredOut += GemMerchant_OnPlayerTriggeredOut;
        HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
        HubChest.Instance.OnChestClosed += HubChest_OnChestClosed;

    }

    private IEnumerator HandleFirstLevelCompletedHubEvolution() {
        hubFireEmberExtractable = true;

        yield return new WaitForSeconds(.5f);
        Debug.Log("HandleFirstLevelCompletedHubEvolution");
        gemMerchantTalkUI.SetTextLinesSO(gemMerchantLevelLostOnceTextLinesSO);

        foreach (HubMerchant hubMerchant in functionalDemoHubMerchantList) {
            hubMerchant.SetHasTalkLinesToShow(false, false);
            hubMerchant.SetDemoMerchantUnlocked();
        }

        foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
            decorationalHubMerchant.SetHasTalkLinesToShow(true, false);
            decorationalHubMerchant.SetDemoMerchantUnlocked();
        }

        gemMerchant.SetHasTalkLinesToShow(true, true);
        gemMerchantReward.DisableReward();
    }

    private IEnumerator HandleBackFromFirstRun() {
        hubFireEmberExtractable = true;

        yield return new WaitForSeconds(.5f);
        Debug.Log("HandleBackFromFirstRun");
        gemMerchant.SetHasTalkLinesToShow(false, false);

        foreach (HubMerchant hubMerchant in functionalDemoHubMerchantList) {
            hubMerchant.SetHasTalkLinesToShow(false, false);
            hubMerchant.SetDemoMerchantUnlocked();
        }

        foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
            decorationalHubMerchant.SetHasTalkLinesToShow(true, false);
            decorationalHubMerchant.SetDemoMerchantUnlocked();
        }

        gemMerchantReward.DisableReward();
    }

    private IEnumerator HandleAnyLevelDefeatHubEvolution() {
        if (handleAnyLevelDefeatHubEvolutionDone) yield break;
        Debug.Log("HandleAnyLevelDefeatHubEvolution");
        handleAnyLevelDefeatHubEvolutionDone = true;
        hubFireEmberExtractable = true;

        yield return new WaitForSeconds(.5f);
        
        foreach (HubMerchant hubMerchant in functionalDemoHubMerchantList) {
            hubMerchant.SetDemoMerchantUnlocked();
            hubMerchant.SetDemoMerchantFunctional();
            hubMerchant.SetHasTalkLinesToShow(false, false);
        }

        foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
            decorationalHubMerchant.SetHasTalkLinesToShow(true, false);
            decorationalHubMerchant.SetDemoMerchantUnlocked();
        }

        bool playerDiedWithWidow = ES3.Load("playerDiedWithWidow", false);
        bool playerDiedWithWidowTextShown = ES3.Load("playerDiedWithWidowTextShown", false);
        Debug.Log("playerDiedWithWidow " + playerDiedWithWidow);
        Debug.Log("playerDiedWithWidowTextShown " + playerDiedWithWidowTextShown);

        if (playerDiedWithWidow && !playerDiedWithWidowTextShown) {
            playerDiedWithWidowTextShown = true;
            gemMerchantTalkUI.SetTextLinesSO(gemMerchantLevelLostWithWidowTextLinesSO);
            gemMerchant.SetHasTalkLinesToShow(true, true);
            SetWidowGemMerchantReward();
        }
        else {

            gemMerchantTalkUI.SetTextLinesSO(gemMerchantLevelLostAgainTextLinesSO);
            gemMerchant.SetHasTalkLinesToShow(true, false);
            gemMerchantReward.DisableReward();

        }
        functionalMerchantsShopsUnlocked = true;

    }

    private void SetWidowGemMerchantReward() {
        List<PlayerCurrencies.CurrencyType> currencyTypesList = new List<PlayerCurrencies.CurrencyType>() {
                PlayerCurrencies.CurrencyType.greenGem,
                PlayerCurrencies.CurrencyType.redGem,
            };
        int greenGemAmountInBag = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int greenGemAmountInChest = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int totalGreenGemAmount = greenGemAmountInBag + greenGemAmountInChest;
        int redGemAmountInBag = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
        int redGemAmountInChest = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
        int totalRedGemAmount = redGemAmountInBag + redGemAmountInChest;

        int greenGemsToReward = 3 - totalGreenGemAmount;
        int redGemsToReward = 3 - totalRedGemAmount;
        bool enableReward = false;

        if (greenGemsToReward < 0) {
            greenGemsToReward = 0;
        } else {
            enableReward = true;
        }
        if (redGemsToReward < 0) {
            redGemsToReward = 0;
        } else {
            enableReward = true;
        }

        List<int> gemsToReward = new List<int>() {
                greenGemsToReward,
                redGemsToReward,
            };
        gemMerchantReward.SetReward(currencyTypesList, gemsToReward);

        if (!enableReward) {
            gemMerchantReward.DisableReward();
        }
    }

    private IEnumerator HandleLevelSuccessHubEvolution() {
        yield return new WaitForSeconds(.5f);
        Debug.Log("HandleLevelSuccessHubEvolution");

        gemMerchantTalkUI.SetTextLinesSO(gemMerchantLevelCompletedTextLinesSO);
        gemMerchant.SetHasTalkLinesToShow(true, true);
        gemMerchantReward.DisableReward();

        hubFire.SetStructureSecondaryFunctionUnlocked(false);
        fireIndicator.gameObject.SetActive(false);
        fireIndicatorActive = false;

        hubFireEmberExtractable = false; 
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        ES3.Save("firstDemoHubEncounter", false);
        ES3.Save("playerDiedWithWidowTextShown", playerDiedWithWidowTextShown);
    }

    private void HubMerchant_OnAnyPlayerTriggeredIn(object sender, System.EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;
        if (!hubMerchant.GetMerchantUnlocked()) return;
        if (!hubMerchant.GetMerchantIsFunctionalDemoMerchant()) return;

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {
            if (gunTipShown) return;

            VideoTipUI.Instance.PlayTipSO(gunMerchantTip, 0f);

            gunTipShown = true;
            ES3.Save("gunTipShown", true);
        }
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {

        if ((e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember)) {

            if (firstDemoHubEncounter) {

                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);

                StartCoroutine(ActivateTeleporterCoroutine());

            }
        }
       
    }

    private IEnumerator StartGemMerchantLines(MerchantTextLinesSO textLines) {
        yield return new WaitForSeconds(.5f);
        gemMerchantTalkUI.SetTalkingWithMerchant(textLines);
    }

    private void HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;

        if (sender is HubMerchantItem_GemMerchantItem) {
            playerBoughtItem = true;
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_BuyUpgrade, LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber);
        }
    }

    private IEnumerator FirstHUBEnterCoroutine() {
        UICurrencyManager.HubInventoryUI.RemoveAllCurrenciesFromBag();

        yield return new WaitForSeconds(5f);
        gemMerchant.ResetAllItemStatuses();

        LevelUI_Locations.Instance.ShowLocationText(LocalizationManager.Instance.GetLocalizedText("TheEternalFlame"));
        MusicManager.Instance.PlayMusicDelayed(2f);

        yield return new WaitForSeconds(6.5f);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.HUBDemo_PrepareToReturn);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveTypes = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader,
        };
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveTypes);
    }

    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!firstDemoHubEncounter) return;

        gemAmountDroppedInChest++;

        if(gemAmountDroppedInChest == gemsToDropInChest) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_DropGems, LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_BuyUpgrade);
            StartCoroutine(StartGemMerchantLines(gemMerchantComeBuyTextLines));
            chestIndicator.gameObject.SetActive(false);
            chestIndicatorActive = false;
            HubChest.Instance.SetInteractionTooltipShown();
        }

    }

    private void HubMerchantTalkUI_OnAnyMerchantEndTalk(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;

        HubMerchantTalkUI hubMerchantTalkUI = sender as HubMerchantTalkUI;
        HubMerchant hubMerchant = hubMerchantTalkUI.GetHubMerchant();


        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GemMerchant) {

            gemMerchantStoppedInteractingCount++;

            if(gemMerchantStoppedInteractingCount == 1) {
                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader, LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_DropGems);
                chestIndicator.gameObject.SetActive(true);
                chestIndicatorActive = true;

                foreach (HubMerchant functionalHubMerchant in functionalDemoHubMerchantList) {
                    functionalHubMerchant.SetHasTalkLinesToShow(true, false);
                    functionalHubMerchant.SetDemoMerchantUnlocked();
                }

                foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
                    decorationalHubMerchant.SetHasTalkLinesToShow(true, false);
                    decorationalHubMerchant.SetDemoMerchantUnlocked();
                }

            }

            if(gemMerchantStoppedInteractingCount == 2) {
                gemMerchantIndicator.gameObject.SetActive(true);
            }
            if (gemMerchantStoppedInteractingCount == 3) {
                fireIndicator.gameObject.SetActive(true);
                fireIndicatorActive = true;
            }
        }

    }

    private void GemMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        if (playerBoughtItem) {
            if (!emberExtractionTalkLineShown) {
                StartCoroutine(StartGemMerchantLines(gemMerchantOpenTeleporterTextLines));
                emberExtractionTalkLineShown = true;
                hubFireEmberExtractable = true;
                hubFire.SetHubFireEmberExtractable();
            };
        };

        if(demoFirstLevelCompleted && !functionalMerchantsShopsUnlocked) {
            functionalMerchantsShopsUnlocked = true;
            MetaProgressionManager.Instance.SetLevelUnlocked(demoMainLevelSO);

            foreach (HubMerchant hubMerchant in functionalDemoHubMerchantList) {
                hubMerchant.SetDemoMerchantFunctional();
                hubMerchant.SetHasTalkLinesToShow(true, true);
            }

            armorerTalkUI.SetTextLinesSO(armorerIntroTextLinesSO);
            trainerTalkUI.SetTextLinesSO(trainerIntroTextLinesSO);

            merchantsUnlocked = true;
            ES3.Save("merchantsUnlocked", true);
        }


        if(demoMainLevelCompleted && firstHubEnterWithDemoLevelCompleted) {

            firstHubEnterWithDemoLevelCompleted = false;
            ES3.Save("firstHubEnterWithDemoLevelCompleted", false);

            fireIndicator.gameObject.SetActive(true);
            fireIndicatorActive = true;
            hubFireEmberExtractable = true;
            hubFire.SetHubFireEmberExtractable();

            VideoTipUI.Instance.PlayTipSO(endDemoTipSO);
            VideoTipUI.Instance.SetEndDemoTip();

            Debug.Log("firstHubEnterWithDemoLevelCompleted false");
        }
    }

    private IEnumerator ActivateTeleporterCoroutine() {
        yield return new WaitForSeconds(.5f);

        CameraManager.Instance.ChangeCameraTarget(grassyAreaPortal.transform);

        yield return new WaitForSeconds(2f);

        grassyAreaPortal.UnlockOrActivatePortal();
        grassyAreaPortal.SetPortalUnlockedInSave();
        grassyAreaPortal.SetLinkedLevelSO(demoFirstLevelSO);
        MetaProgressionManager.Instance.SetLevelUnlocked(demoFirstLevelSO);

        yield return new WaitForSeconds(3f);
        CameraManager.Instance.ResetCameraTargetToPlayer();

    }

    private void HubFire_OnFireEmberExtracted(object sender, System.EventArgs e) {
        if (hubFire.GetEmberExtracted()) {
            emberExtracted = true;
            fireIndicator.gameObject.SetActive(false);
            fireIndicatorActive = false;
        }
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        if (firstDemoHubEncounter) return;
        RefreshPlayerHasGemsIndicators();

        if (!chestIndicatorActive) return;
        if (gemsToDropInChest == gemAmountDroppedInChest) return;

        chestIndicator.gameObject.SetActive(true);
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        if (!chestIndicatorActive) return;

        chestIndicator.gameObject.SetActive(false);
    }

    private void GemMerchant_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;
        if (playerBoughtItem) return;
        if (gemsToDropInChest != gemAmountDroppedInChest) return;

        gemMerchantIndicator.gameObject.SetActive(true);
    }

    private void GemMerchant_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;

        gemMerchantIndicator.gameObject.SetActive(false);
    }

    private void HubFire_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!hubFireEmberExtractable) return;
        if (firstDemoHubEncounter) {
            if (!emberExtractionTalkLineShown) return;
            if (emberExtracted) return;
        } else {
            if (!fireIndicatorActive) return;
        }


        fireIndicator.gameObject.SetActive(true);
    }

    private void HubFire_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!hubFireEmberExtractable) return;
        if (firstDemoHubEncounter) {
            if (!emberExtractionTalkLineShown) return;
            if (emberExtracted) return;
        }
        else {
            if (!fireIndicatorActive) return;
        }

        fireIndicator.gameObject.SetActive(false);
    }

    private void GrassyAreaPortal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
    }

    private void RefreshPlayerHasGemsIndicators() {
        playerHasGemsInInventory = (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory.gem).Count != 0);

        if (playerHasGemsInInventory) {

            chestIndicatorActive = true;
            chestIndicator.gameObject.SetActive(true);

            fireIndicatorActive = false;
            fireIndicator.gameObject.SetActive(false);

        } else {

            chestIndicatorActive = false;
            chestIndicator.gameObject.SetActive(false);

            hubFire.SetHubFireEmberExtractable();
            fireIndicatorActive = true;
            fireIndicator.gameObject.SetActive(true);
        }
    }

    private void OnDestroy() {
        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        HubMerchant.OnAnyPlayerTriggeredIn -= HubMerchant_OnAnyPlayerTriggeredIn;
    }

}
