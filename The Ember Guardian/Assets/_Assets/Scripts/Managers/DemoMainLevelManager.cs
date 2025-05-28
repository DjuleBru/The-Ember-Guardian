using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoMainLevelManager : MonoBehaviour
{
    public static DemoMainLevelManager Instance;

    public enum DemoLevelType {
        FirstLevel,
        MainLevel
    }

    [SerializeField] private DemoLevelType demoLevelType;
    [SerializeField] private GameObject fireLocationIndicator;
    [SerializeField] private GameObject endLevelPortalIndicator;
    [SerializeField] private TutorialCollider fireBlockingCollider1;
    [SerializeField] private TutorialCollider fireBlockingCollider2;
    [SerializeField] private TutorialCollider fireFuelledBlockingCollider1;
    [SerializeField] private TutorialCollider fireFuelledBlockingCollider2;
    [SerializeField] private Portal endLevelPortal;
    private GameObject hunterShrineIndicator;
    private GameObject ammoCrafterIndicator;

    [SerializeField] private StructureLocation mainFireLocation;
    [SerializeField] private List<StructureLocation> defensiveStructureLocations;
    [SerializeField] private GameObject firstLevelLeftSpawners;
    [SerializeField] private GameObject levelLeftSpawnerGroups;
    [SerializeField] private GameObject firstLevelRightSpawners;
    [SerializeField] private GameObject levelRightSpawnerGroups;
    [SerializeField] private Transform levelRightTeleporterPosition;
    [SerializeField] private Transform levelLeftTeleporterPosition;

    [SerializeField] private PropFadeOut leftPropFadeOut;
    [SerializeField] private PropFadeOut rightPropFadeOut;
    [SerializeField] private Chest leftChest;
    [SerializeField] private Chest rightChest;
    private bool showingGetReady;
    private bool mainFireLit;
    private bool leftPropCollected;
    private bool rightPropCollected;

    private bool ammoCraftStarted;
    private bool animalDied;
    private bool orbCollectedByPlayer;
    private bool ammoCraftEnded;
    private bool ammoCraftCollected;
    private bool recruitWorkerTooltipShown;
    private bool tabMenuTooltipShown;

    private int demoLevelLostAmount;
    private bool demoMainLevelTutorialCompleted;
    private bool demoMainLevelEncountered;
    private bool demoMainLevelCompleted;
    private bool demoFirstLevelCompleted;

    private int emberlingAmountRecruited;
    private int hunterAmountRecruited;

    private void Awake() {
        Instance = this;

        fireLocationIndicator.gameObject.SetActive(false);

        demoMainLevelTutorialCompleted = ES3.Load("demoMainLevelTutorialCompleted", false);
        demoMainLevelCompleted = ES3.Load("demoMainLevelCompleted", false);
        demoMainLevelEncountered = ES3.Load("demoMainLevelEncountered", false);
        demoFirstLevelCompleted = ES3.Load("demoFirstLevelCompleted", false);
        demoLevelLostAmount = ES3.Load("demoLevelLostAmount", 0);
        recruitWorkerTooltipShown = ES3.Load("recruitWorkerTooltipShown", false);
        tabMenuTooltipShown = ES3.Load("tabMenuTooltipShown", false);

        Debug.Log("demoMainLevelTutorialCompleted " + demoMainLevelTutorialCompleted);
        Debug.Log("demoFirstLevelCompleted " + demoFirstLevelCompleted);
        Debug.Log("demoMainLevelEncountered " + demoMainLevelEncountered);
        Debug.Log("demoMainLevelCompleted " + demoMainLevelCompleted);
        Debug.Log("demoLevelLostAmount " + demoLevelLostAmount);
        Debug.Log("recruitWorkerTooltipShown " + recruitWorkerTooltipShown);

        if(GetIsMainDemoLevel()) {
            InitializeSpawners(demoMainLevelEncountered);
        }
    }

    private void Start() {
        LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.Instance.OnLevelSuccess += LevelManager_OnLevelSuccess;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        Animal.OnAnyMobDied += Animal_OnAnyMobDied;
        Collectible.OnAnyCollectiblePickedUpByPlayer += Collectible_OnAnyCollectiblePickedUpByPlayer;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        Obstacle.OnAnyPlayerTriggeredIn += Obstacle_OnAnyPlayerTriggeredIn;
        Obstacle.OnAnyPlayerTriggeredOut += Obstacle_OnAnyPlayerTriggeredOut;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        mainFireLocation.OnPlayerTriggeredIn += MainFireLocation_OnPlayerTriggeredIn;
        mainFireLocation.OnPlayerTriggeredOut += MainFireLocation_OnPlayerTriggeredOut;

        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        GameInput.Instance.OnPlayerSwapGunPerformed += GameInput_OnPlayerSwapGunPerformed;
        GameInput.Instance.OnPlayerSecondaryGunSelected += GameInput_OnPlayerSecondaryGunSelected;

        if(rightChest != null) {
            rightChest.OnChestOpened += RightChest_OnChestOpened;
        }
        if(leftChest != null) {
            leftChest.OnChestOpened += LeftChest_OnChestOpened;
        }

        if(endLevelPortal != null) {
            endLevelPortal.OnPlayerMovedOnTeleporter += EndLevelPortal_OnPlayerMovedOnTeleporter;
        }

        if (demoLevelType == DemoLevelType.FirstLevel) {
            WindManager.Instance.DisableWind();
            CreaturesSpawnManager.Instance.SetGrowthFactor(2.2f);
            CreaturesSpawnManager.Instance.SetMinMaxDifficultyGrowthFactor(1.8f);
            CreaturesSpawnManager.Instance.SetMaxRemainingSubwaveCreaturesForNextSubwave(2);
            CreaturesSpawnManager.Instance.SetCanSpawnElite(false);
            CreaturesSpawnManager.Instance.SetSpawnEquallyFromBothSides(true);

            if (!demoMainLevelTutorialCompleted) {
                StartCoroutine(SetDemoTutorialObjective());
                fireBlockingCollider1.SetColliderSolid();
                fireBlockingCollider2.SetColliderSolid();
                Fire.Instance.SetFireInteractionsUpdateLocked(true);

                foreach (StructureLocation structureLocation in defensiveStructureLocations) {
                    structureLocation.gameObject.SetActive(false);
                }
            } else {

                fireBlockingCollider1.SetColliderTrigger();
                fireBlockingCollider2.SetColliderTrigger();
                fireFuelledBlockingCollider1.SetColliderTrigger();
                fireFuelledBlockingCollider2.SetColliderTrigger();
            }
        }

        if(!demoMainLevelEncountered && demoFirstLevelCompleted) {
            demoMainLevelEncountered = true;
            ES3.Save("demoMainLevelEncountered", true);
        }

        if(demoMainLevelCompleted) {
            StartCoroutine(SetNightsToSurviveAfterDelay());
        }
    }

    private void GameInput_OnPlayerSecondaryGunSelected(object sender, EventArgs e) {
        if(leftPropCollected || rightPropCollected) {
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltip(LocalizationManager.Instance.GetLocalizedText("tooltip_cannotSwapWeapon"), 3f);
        }
    }

    private void GameInput_OnPlayerSwapGunPerformed(object sender, EventArgs e) {
        if (leftPropCollected || rightPropCollected) {
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltip(LocalizationManager.Instance.GetLocalizedText("tooltip_cannotSwapWeapon"), 3f);
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, EventArgs e) {
        if (!tabMenuTooltipShown) {
            tabMenuTooltipShown = true;
            ES3.Save("tabMenuTooltipShown", true);
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
        }
    }

    private void Obstacle_OnAnyPlayerTriggeredOut(object sender, EventArgs e) {
        if(!tabMenuTooltipShown) {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
        }
    }

    private void Obstacle_OnAnyPlayerTriggeredIn(object sender, EventArgs e) {
        if(!tabMenuTooltipShown) {
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_openBackpackTip"), InputControlIcons.Control.OpenPlayerMenu, 99f);
        }
    }

    private void Update() {
        if(!demoMainLevelTutorialCompleted) {
            
            HandleBlockingCollider(fireFuelledBlockingCollider1.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_fuelFireBlocking"));
            HandleBlockingCollider(fireFuelledBlockingCollider2.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_fuelFireBlocking"));
           
            if (!mainFireLit) {
                HandleBlockingCollider(fireBlockingCollider1.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_lightFireBlocking"));
                HandleBlockingCollider(fireBlockingCollider2.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_lightFireBlocking"));
            };
           
        }
    }

    private void EndLevelPortal_OnPlayerMovedOnTeleporter(object sender, EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub);
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, EventArgs e) {
        if(!demoFirstLevelCompleted) {
            ES3.Save("demoFirstLevelCompleted", true);
        }
        endLevelPortalIndicator.gameObject.SetActive(false);
    }

    private IEnumerator EnableEndLevelPortal(float delayToEnable) {
        yield return new WaitForSeconds(delayToEnable);
        endLevelPortal.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.ReturnToHub);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveUIList = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub,
            };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveUIList);
    }

    private void RightChest_OnChestOpened(object sender, EventArgs e) {
        rightPropFadeOut.FadeOut();
        rightPropCollected = true;
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FindTrainerStockpile);

        if (leftPropCollected) {
            endLevelPortal.transform.position = levelRightTeleporterPosition.position;
            StartCoroutine(EnableEndLevelPortal(4f));
        }
    }

    private void LeftChest_OnChestOpened(object sender, EventArgs e) {
        leftPropFadeOut.FadeOut();
        leftPropCollected = true;
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FindArmorerStockpile);

        if (rightPropCollected) {
            endLevelPortal.transform.position = levelLeftTeleporterPosition.position;
            StartCoroutine(EnableEndLevelPortal(4f));
        }
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        int dayNumber = DayNightManager.Instance.GetCurrentDay();

        if (dayNumber == 4) {
            CreaturesSpawnManager.Instance.SetSetDifficultyAnimationCurve();
        }
    }

    private void InitializeSpawners(bool demoMainLevelEncountered) {

        if (firstLevelLeftSpawners == null || firstLevelRightSpawners == null || levelRightSpawnerGroups == null || levelLeftSpawnerGroups == null) return;
        foreach (MobSpawner daySpawner in firstLevelLeftSpawners.GetComponentsInChildren<MobSpawner>()) {
            daySpawner.gameObject.SetActive(!demoMainLevelEncountered);
        }
        foreach (MobSpawner daySpawner in firstLevelRightSpawners.GetComponentsInChildren<MobSpawner>()) {
            daySpawner.gameObject.SetActive(!demoMainLevelEncountered);
        }
        foreach (DayCreatureSpawnerGroup daySpawnerGroup in levelRightSpawnerGroups.GetComponentsInChildren<DayCreatureSpawnerGroup>()) {
            daySpawnerGroup.gameObject.SetActive(demoMainLevelEncountered);
        }
        foreach (DayCreatureSpawnerGroup daySpawnerGroup in levelLeftSpawnerGroups.GetComponentsInChildren<DayCreatureSpawnerGroup>()) {
            daySpawnerGroup.gameObject.SetActive(demoMainLevelEncountered);
        }

    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, StructureLocation.OnAnyStructureBuiltEventArgs e) {
        if (demoMainLevelTutorialCompleted) return;

        if (e.structureBuilt.GetStructureSO().structureType == StructureSO.StructureType.ammoCrafter) {
            CurrencyCrafter ammoCrafter = e.structureBuilt as CurrencyCrafter;

            ammoCrafter.OnPlayerTriggeredIn += AmmoCrafter_OnPlayerTriggeredIn;
            ammoCrafter.OnPlayerTriggeredOut += AmmoCrafter_OnPlayerTriggeredOut;
            ammoCrafter.OnNewCurrencyBatchCraftingStarted += AmmoCrafter_OnCurrencyCraftingStarted;
            ammoCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnCurrencyCraftingEnded;
            ammoCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;

            ammoCrafterIndicator = ammoCrafter.GetVisualIndicator();
            StartCoroutine(ActivateIndicatorAfterDelay(ammoCrafterIndicator, 4f));
        }
        if (e.structureBuilt.GetStructureSO().structureType == StructureSO.StructureType.hunterShrine) {
            Structure hunterShrine = e.structureBuilt;

            hunterShrine.OnPlayerTriggeredIn += HunterShrine_OnPlayerTriggeredIn;
            hunterShrine.OnPlayerTriggeredOut += HunterShrine_OnPlayerTriggeredOut;

            hunterShrineIndicator = hunterShrine.GetVisualIndicator();
        }
    }

    private IEnumerator SetNightsToSurviveAfterDelay() {
        yield return new WaitForSeconds(.1f);
        LevelObjectives.Instance.SetNightsToSurvive(99);
    }

    private void LevelManager_OnLevelSuccess(object sender, EventArgs e) {
        if (!demoFirstLevelCompleted) return;
        ES3.Save("demoMainLevelCompleted", true);
    }

    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        demoLevelLostAmount++;
        ES3.Save("demoLevelLostAmount", demoLevelLostAmount);

        if(CreaturesSpawnManager.Instance.GetCurrentWaveNumber() == 2 || CreaturesSpawnManager.Instance.GetCurrentWaveNumber() == 5) {
            // Died because of widow
            ES3.Save("playerDiedWithWidow", true);
        } else {
            ES3.Save("playerDiedWithWidow", false);
        }
    }

    public void AddLevelLostAmount() {
        if (demoLevelLostAmount == 0) return;

        demoLevelLostAmount++;
        ES3.Save("demoLevelLostAmount", demoLevelLostAmount);
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        if (e.tipTypeShown == VideoTipSO.VideoTipType.FireManagement) {
            if (demoMainLevelTutorialCompleted) return;

            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);

            fireFuelledBlockingCollider1.gameObject.SetActive(false);
            fireFuelledBlockingCollider2.gameObject.SetActive(false);

            StartCoroutine(StartFindStockpilesObjectiveAfterDelay(false, 4f));
            DayNightManager.Instance.SetCyclePaused(false, true);
        };

        if (e.tipTypeShown == VideoTipSO.VideoTipType.Hunters) {
            DayNightManager.Instance.SetCyclePaused(true, true);
        };
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        mainFireLit = true;
        Fire.Instance.DisableEmberExtraction();
        Debug.Log("demoFirstLevelCompleted " + demoFirstLevelCompleted);

        if(demoLevelType == DemoLevelType.FirstLevel) {
            if (!demoMainLevelTutorialCompleted) {
                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveUIList = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter,
                LevelUI_ObjectiveUI.SubObjectiveType.RecruitEmberlings,
            };

                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveUIList);
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire);
                fireBlockingCollider1.SetColliderTrigger();
                fireBlockingCollider2.SetColliderTrigger();
                StartCoroutine(PauseDayNightCycleAfterDelay());

                return;
            }
            else {
                StartCoroutine(StartFindStockpilesObjectiveAfterDelay(true, 2f));
            }
        }
    }
    private IEnumerator PauseDayNightCycleAfterDelay() {
        yield return new WaitForSeconds(.1f);
        DayNightManager.Instance.SetCyclePaused(true);
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

    private void Collectible_OnAnyCollectiblePickedUpByPlayer(object sender, EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (orbCollectedByPlayer) return;
        if (hunterAmountRecruited < 2) return;
        if (!animalDied) return;

        Collectible collectible = sender as Collectible;
        if (collectible.GetCurrencyType() != PlayerCurrencies.CurrencyType.bigBlueOrb) return;
        if (collectible.GetDroppedByPlayer()) return;

        orbCollectedByPlayer = true;

        if (ammoCraftCollected) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters, LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);
            UnlockFireInteractions();
        }
        else {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
        }
    }

    private void Animal_OnAnyMobDied(object sender, EventArgs e) {
        Mob mob = (Mob)sender;
        if (!(mob is Animal)) return;
        animalDied = true;

        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt, LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (orbCollectedByPlayer) return;
        if (!animalDied) return;

        orbCollectedByPlayer = true;

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
            TryShowRecruitWorkerTooltip(false);
            recruitWorkerTooltipShown = true;
        }
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftCollected) return;

        ammoCraftCollected = true;
        if (orbCollectedByPlayer) {
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
        Fire.Instance.SetStructureSecondaryFunctionUnlocked(false);
        Fire.Instance.DisableEmberExtraction();
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
        ammoCrafterIndicator.SetActive(false);
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter, LevelUI_ObjectiveUI.SubObjectiveType.WaitCraftingAmmo);
    }

    private void AmmoCrafter_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (demoMainLevelTutorialCompleted) return;
        if (ammoCraftStarted) {
            ammoCrafterIndicator.SetActive(false);
        } else {
            ammoCrafterIndicator.SetActive(true);
        }
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

    private IEnumerator StartFindStockpilesObjectiveAfterDelay(bool firestObjective, float delay) {
        yield return new WaitForSeconds(delay);

        if(firestObjective) {
            LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindStockpiles);
        } else {
            LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindStockpiles);
        }
        
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.FindArmorerStockpile,
                    LevelUI_ObjectiveUI.SubObjectiveType.FindTrainerStockpile,
                };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
        
        foreach (StructureLocation structureLocation in defensiveStructureLocations) {
            structureLocation.gameObject.SetActive(true);
        }

        demoMainLevelTutorialCompleted = true;
        ES3.Save("demoMainLevelTutorialCompleted", true);
        ES3.Save("recruitWorkerTooltipShown", true);
    }

    public bool GetDemoMainLevelTutorialCompleted() {
        return demoMainLevelTutorialCompleted;
    }
    public bool GetDemoFirstLevelCompleted() {
        return demoFirstLevelCompleted;
    }

    public bool GetDemoLevelLostOnce() {
        return demoLevelLostAmount == 1;
    }
    public int GetDemoLevelLostAmount() {
        return demoLevelLostAmount;
    }

    public void TryShowRecruitWorkerTooltip(bool show) {
        if (recruitWorkerTooltipShown) return;

        if (show) {

            if (!UICurrencyManager.PlayerInventoryUI.GetHasBigOrb()) return;
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_recruitEmberling"), InputControlIcons.Control.Interact, 999);

        } else {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
        }
    }

    private void HandleBlockingCollider(Vector3 colliderPosition, string textToShow) {
        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) < 1.5f && !showingGetReady) {
            showingGetReady = true;
            PlayerTooltipManager.Instance.GetTooltipRight().ShowTooltip(textToShow, 3f);
        }

        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) > 1.5f && showingGetReady) {
            showingGetReady = false;
            PlayerTooltipManager.Instance.GetTooltipRight().HideTooltip();
        }
    }

    public bool GetIsMainDemoLevel() {
        return demoLevelType == DemoLevelType.MainLevel;
    }

    private void OnDestroy() {
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        Collectible.OnAnyCollectiblePickedUpByPlayer -= Collectible_OnAnyCollectiblePickedUpByPlayer;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        Animal.OnAnyMobDied -= Animal_OnAnyMobDied;
        Obstacle.OnAnyPlayerTriggeredIn -= Obstacle_OnAnyPlayerTriggeredIn;
        Obstacle.OnAnyPlayerTriggeredOut -= Obstacle_OnAnyPlayerTriggeredOut;
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        GameInput.Instance.OnPlayerSwapGunPerformed -= GameInput_OnPlayerSwapGunPerformed;
        GameInput.Instance.OnPlayerSecondaryGunSelected -= GameInput_OnPlayerSecondaryGunSelected;
    }
}
