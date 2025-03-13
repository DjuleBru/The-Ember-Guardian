using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager_Demo : MonoBehaviour
{
    private bool firstDemoHubEncounter;
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
    [SerializeField] private MerchantTextLinesSO armorerIntroTextLinesSO;
    [SerializeField] private MerchantTextLinesSO trainerIntroTextLinesSO;
    [SerializeField] private Fire hubFire;
    [SerializeField] private Portal grassyAreaPortal;
    [SerializeField] private LevelSO demoMainLevelSO;

    [SerializeField] private GameObject chestIndicator;
    [SerializeField] private GameObject fireIndicator;
    [SerializeField] private GameObject gemMerchantIndicator;

    private int gemMerchantStoppedInteractingCount;
    private int gemsToDropInChest = 5;
    private int gemAmountDroppedInChest;
    private bool playerBoughtItem;
    private bool emberExtractionTalkLineShown;
    private bool emberExtracted;

    private void Awake() {
        // First demo hub encounter becomes true when player moves on teleporter
        firstDemoHubEncounter = ES3.Load("firstDemoHubEncounter", true);
        Debug.Log("firstDemoHubEncounter " + firstDemoHubEncounter);
        demoLevelLostAmount = ES3.Load("demoLevelLostAmount", 0);

        chestIndicator.gameObject.SetActive(false);
        fireIndicator.gameObject.SetActive(false);
        gemMerchantIndicator.gameObject.SetActive(false);
    }

    void Start() {
        if(firstDemoHubEncounter) {
            StartCoroutine(FirstHUBEnterCoroutine());
        } else {
            MusicManager.Instance.PlayMusicDelayed(3f);

            if(demoLevelLostAmount == 1) {
                StartCoroutine(HandleFirstLevelDefeatHubEvolution());
            }
        }

        UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;
        
        HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
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

    private IEnumerator HandleFirstLevelDefeatHubEvolution() {
        yield return new WaitForSeconds(.5f);
        Debug.Log("HandleFirstLevelDefeatHubEvolution");
        gemMerchantTalkUI.SetTextLinesSO(gemMerchantLevelLostOnceTextLinesSO);

        foreach(HubMerchant hubMerchant in functionalDemoHubMerchantList) {
            hubMerchant.SetHasTalkLinesToShow(true);
            hubMerchant.SetDemoMerchantUnlocked();
            hubMerchant.SetDemoMerchantFunctional();
        }

        armorerTalkUI.SetTextLinesSO(armorerIntroTextLinesSO);
        trainerTalkUI.SetTextLinesSO(trainerIntroTextLinesSO);

        fireIndicator.gameObject.SetActive(true);
        gemMerchantReward.enabled = false;
        hubFire.SetHubFireEmberExtractable();
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        ES3.Save("firstDemoHubEncounter", false);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!firstDemoHubEncounter) return;

        if ((e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember)) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);

            StartCoroutine(ActivateTeleporterCoroutine());
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

        yield return new WaitForSeconds(5f);

        LevelUI_Locations.Instance.ShowLocationText("The Eternal Flame");
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
        }

    }

    private void HubMerchantTalkUI_OnAnyMerchantEndTalk(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;

        HubMerchantTalkUI hubMerchantTalkUI = sender as HubMerchantTalkUI;
        HubMerchant hubMerchant = hubMerchantTalkUI.GetHubMerchant();


        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GemMerchant) {

            ES3.Save("firstDemoHubEncounter", false);
            gemMerchantStoppedInteractingCount++;

            if(gemMerchantStoppedInteractingCount == 1) {
                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.HUB_TalkToTrader, LevelUI_ObjectiveUI.SubObjectiveType.HUBDemo_DropGems);
                chestIndicator.gameObject.SetActive(true);

                foreach (HubMerchant functionalHubMerchant in functionalDemoHubMerchantList) {
                    functionalHubMerchant.SetHasTalkLinesToShow(false);
                    functionalHubMerchant.SetDemoMerchantUnlocked();
                }

                foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
                    decorationalHubMerchant.SetHasTalkLinesToShow(false);
                    decorationalHubMerchant.SetDemoMerchantUnlocked();
                }
            }

            if(gemMerchantStoppedInteractingCount == 2) {
                gemMerchantIndicator.gameObject.SetActive(true);
            }
            if (gemMerchantStoppedInteractingCount == 3) {
                fireIndicator.gameObject.SetActive(true);
            }
        }

    }

    private void GemMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        if (!playerBoughtItem) return;
        if (emberExtractionTalkLineShown) return;

        StartCoroutine(StartGemMerchantLines(gemMerchantOpenTeleporterTextLines));
        emberExtractionTalkLineShown = true;
        hubFire.SetHubFireEmberExtractable();
    }

    private IEnumerator ActivateTeleporterCoroutine() {
        yield return new WaitForSeconds(.5f);

        CameraManager.Instance.ChangeCameraTarget(grassyAreaPortal.transform);

        yield return new WaitForSeconds(2f);

        grassyAreaPortal.UnlockOrActivatePortal();
        grassyAreaPortal.SetPortalUnlockedInSave();
        MetaProgressionManager.Instance.SetLevelUnlocked(demoMainLevelSO);

        yield return new WaitForSeconds(3f);
        CameraManager.Instance.ResetCameraTargetToPlayer();

    }

    private void HubFire_OnFireEmberExtracted(object sender, System.EventArgs e) {
        if (hubFire.GetEmberExtracted()) {
            emberExtracted = true;
            fireIndicator.gameObject.SetActive(false);
        }
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;
        if (gemsToDropInChest == gemAmountDroppedInChest) return;

        chestIndicator.gameObject.SetActive(true);
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;

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
        if (!firstDemoHubEncounter) return;
        if (!emberExtractionTalkLineShown) return;
        if (emberExtracted) return;

        fireIndicator.gameObject.SetActive(true);
    }

    private void HubFire_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!firstDemoHubEncounter) return;
        if (!emberExtractionTalkLineShown) return;

        fireIndicator.gameObject.SetActive(false);
    }

    private void GrassyAreaPortal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.HUB_HeadToTeleporter);
    }


    private void OnDestroy() {
        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
        HubMerchantItem_GemMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_GemMerchantItem_OnAnyHubMerchantItemBought;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
    }

}
