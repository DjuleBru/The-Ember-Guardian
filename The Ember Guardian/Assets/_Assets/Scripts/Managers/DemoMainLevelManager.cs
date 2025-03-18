using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoMainLevelManager : MonoBehaviour
{
    public static DemoMainLevelManager Instance;

    [SerializeField] private GameObject fireLocationIndicator;
    [SerializeField] private GameObject hunterShrineIndicator;
    [SerializeField] private GameObject ammoCrafterIndicator;

    [SerializeField] private StructureLocation mainFireLocation;
    [SerializeField] private List<StructureLocation> defensiveStructureLocations;
    [SerializeField] private Structure hunterShrine;
    [SerializeField] private CurrencyCrafter ammoCrafter;

    private bool ammoCraftStarted;
    private bool animalDied;
    private bool orbPickedUpByWorker;
    private bool orbDroppedByWorker;
    private bool ammoCraftEnded;
    private bool ammoCraftCollected;

    private int demoLevelLostAmount;
    private bool demoMainLevelTutorialCompleted;
    private bool demoLevelCompleted;

    private int emberlingAmountRecruited;
    private int hunterAmountRecruited;

    private void Awake() {
        Instance = this;

        fireLocationIndicator.gameObject.SetActive(false);
        hunterShrineIndicator.gameObject.SetActive(false);
        ammoCrafterIndicator.gameObject.SetActive(false);

        demoMainLevelTutorialCompleted = ES3.Load("demoMainLevelTutorialCompleted", false);
        demoLevelCompleted = ES3.Load("demoLevelCompleted", false);
        demoLevelLostAmount = ES3.Load("demoLevelLostAmount", 0);

    }

    private void Start() {
        LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.Instance.OnLevelSuccess += LevelManager_OnLevelSuccess;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        Animal.OnAnyMobDied += Animal_OnAnyMobDied;
        Collectible.OnAnyCollectiblePickedUpByWorker += Collectible_OnAnyCollectiblePickedUpByWorker;
        mainFireLocation.OnPlayerTriggeredIn += MainFireLocation_OnPlayerTriggeredIn;
        mainFireLocation.OnPlayerTriggeredOut += MainFireLocation_OnPlayerTriggeredOut;

        hunterShrine.OnPlayerTriggeredIn += HunterShrine_OnPlayerTriggeredIn;
        hunterShrine.OnPlayerTriggeredOut += HunterShrine_OnPlayerTriggeredOut;

        ammoCrafter.OnPlayerTriggeredIn += AmmoCrafter_OnPlayerTriggeredIn;
        ammoCrafter.OnPlayerTriggeredOut += AmmoCrafter_OnPlayerTriggeredOut;
        ammoCrafter.OnCurrencyCraftingStarted += AmmoCrafter_OnCurrencyCraftingStarted;
        ammoCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnCurrencyCraftingEnded;
        ammoCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;

        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;


        if (!demoMainLevelTutorialCompleted) {
            StartCoroutine(SetDemoTutorialObjective());
            Fire.Instance.SetFireInteractionsUpdateLocked(true);

            foreach(StructureLocation structureLocation in defensiveStructureLocations) {
                structureLocation.gameObject.SetActive(false);
            }
        }

        if(demoLevelCompleted) {
            StartCoroutine(SetNightsToSurviveAfterDelay());
        }
    }

    private IEnumerator SetNightsToSurviveAfterDelay() {
        yield return new WaitForSeconds(.1f);
        LevelObjectives.Instance.SetNightsToSurvive(99);
    }

    private void LevelManager_OnLevelSuccess(object sender, EventArgs e) {
        Debug.Log("LevelManager_OnLevelSuccess");
        ES3.Save("demoLevelCompleted", true);
    }

    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        demoLevelLostAmount++;
        ES3.Save("demoLevelLostAmount", demoLevelLostAmount);
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        if (e.tipTypeShown == VideoTipSO.VideoTipType.FireManagement) {
            if (demoMainLevelTutorialCompleted) return;

            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);

            StartCoroutine(StartSurviveNightsObjectiveAfterDelay());

            demoMainLevelTutorialCompleted = true;
            ES3.Save("demoMainLevelTutorialCompleted", true);
        };

        if (e.tipTypeShown == VideoTipSO.VideoTipType.Hunters) {
            DayNightManager.Instance.SetCyclePaused(true, true);
        };
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;

        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveUIList = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter,
                LevelUI_ObjectiveUI.SubObjectiveType.RecruitEmberlings,
            };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveUIList);
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire);
        StartCoroutine(ActivateIndicatorAfterDelay(ammoCrafterIndicator, 4f));
    }

    private IEnumerator ActivateIndicatorAfterDelay(GameObject indicator, float delay) {
        yield return new WaitForSeconds(delay);
        indicator.gameObject.SetActive(true);
    }

    private IEnumerator SetDemoTutorialObjective() {
        yield return new WaitForSeconds(6f);
        fireLocationIndicator.gameObject.SetActive(true);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.SetupCamp);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveUIList = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire,
            };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveUIList);
    }

    private void Collectible_OnAnyCollectiblePickedUpByWorker(object sender, EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (hunterAmountRecruited < 2) return;
        if (orbPickedUpByWorker) return;
        if (!animalDied) return;

        orbPickedUpByWorker = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt, LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

    private void Animal_OnAnyMobDied(object sender, EventArgs e) {
        animalDied = true;
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (orbDroppedByWorker) return;

        orbDroppedByWorker = true;

        if(ammoCraftCollected) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters, LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);
            UnlockFireInteractions();
        } else {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
        }
    }

    private void Worker_OnAnyWorkerAssignedHunter(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        hunterAmountRecruited++;

        if(hunterAmountRecruited == 2) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.Recruit2Hunters, LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt);
        }
    }

    private void Worker_OnAnyWorkerRecruited(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        emberlingAmountRecruited++;

        if(emberlingAmountRecruited == 2) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.RecruitEmberlings, LevelUI_ObjectiveUI.SubObjectiveType.Recruit2Hunters);
            hunterShrineIndicator.gameObject.SetActive(true);
        }
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftCollected) return;

        ammoCraftCollected = true;
        if (orbDroppedByWorker) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.CollectCrafterAmmo, LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);
            UnlockFireInteractions();
        }
        else {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectCrafterAmmo);
        }
    }

    private void UnlockFireInteractions() {
        Fire.Instance.SetFireInteractionsUpdateLocked(false);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);
    }

    private void AmmoCrafter_OnCurrencyCraftingEnded(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftEnded) return;

        ammoCraftEnded = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitCraftingAmmo, LevelUI_ObjectiveUI.SubObjectiveType.CollectCrafterAmmo);
    }

    private void AmmoCrafter_OnCurrencyCraftingStarted(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftStarted) return;

        ammoCraftStarted = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter, LevelUI_ObjectiveUI.SubObjectiveType.WaitCraftingAmmo);
    }

    private void AmmoCrafter_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftStarted) return;
        ammoCrafterIndicator.SetActive(true);
    }

    private void AmmoCrafter_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftStarted) return;
        ammoCrafterIndicator.SetActive(false);

    }

    private void HunterShrine_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (hunterAmountRecruited >= 2) return;
        if (emberlingAmountRecruited < 2) return;
        hunterShrineIndicator.SetActive(true);

    }

    private void HunterShrine_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        hunterShrineIndicator.SetActive(false);
    }

    private void MainFireLocation_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!demoMainLevelTutorialCompleted) return;
        fireLocationIndicator.SetActive(true);
    }

    private void MainFireLocation_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        fireLocationIndicator.SetActive(false);


    }

    private IEnumerator StartSurviveNightsObjectiveAfterDelay() {
        yield return new WaitForSeconds(4f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.SurviveNights);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.SurviveNights
                };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
        
        foreach (StructureLocation structureLocation in defensiveStructureLocations) {
            structureLocation.gameObject.SetActive(false);
        }

    }

    public bool GetDemoMainLevelTutorialCompleted() {
        return demoMainLevelTutorialCompleted;
    }

    public bool GetDemoLevelLostOnce() {
        return demoLevelLostAmount == 1;
    }

    private void OnDestroy() {

        Collectible.OnAnyCollectiblePickedUpByWorker -= Collectible_OnAnyCollectiblePickedUpByWorker;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        Animal.OnAnyMobDied -= Animal_OnAnyMobDied;
    }
}
