using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    private bool testing;
    [SerializeField] private Transform initialSpawnPoint;
    [SerializeField] private Transform debugSpawnPoint;
    [SerializeField] private Transform beforeFireRespawnPoint;
    [SerializeField] private TutorialCollider firstCreatureCollider;
    [SerializeField] private TutorialCollider blockingWorkersCollider;
    [SerializeField] private TutorialCollider endLevelAreaCollider;
    [SerializeField] private TutorialCollider extractEmberCollider;
    [SerializeField] private StructureLocation startFireLocation;
    [SerializeField] private StructureLocation ammoCrafterLocation;
    [SerializeField] private StructureLocation hunterShrineLocation;
    [SerializeField] private List<StructureLocation> defensiveStructureLocations;
    [SerializeField] private EndLevelArea endLevelArea;
    [SerializeField] private Light2D firstCreatureSpotLight;
    [SerializeField] private MobSpawner firstWorkerSpawner;
    [SerializeField] private Transform looseTutorialChest;
    [SerializeField] private Portal endTutorialPortal;
    [SerializeField] private Dog dog;

    [SerializeField] private GameObject fireLocationIndicator;
    [SerializeField] private GameObject ammoCrafterLocationIndicator;
    [SerializeField] private GameObject hunterShrineLocationIndicator;
    [SerializeField] private GameObject endLevelPortalIndicator;

    private float aimDirTooltip;

    private bool dogTipShown;
    private bool dogStateChanged;
    private bool moveTooltipShown;
    private bool aimTooltipShown;
    private bool moveTooltipHidden;
    private bool aimTooltipHidden;
    private bool transferAmmoTooltipShown;
    private bool rollTooltipShown;
    private bool reloadTooltipShown;
    private bool reloadTooltipHidden;
    private bool saveAmmoTooltipShown;
    private bool climbTowerTooltipShown;

    private bool shootTipShown;
    private bool shootTipHidden;
    private bool firstCreatureDied;

    private bool dropOrbShown;

    private bool healTooltipShown;
    private bool huntingFlagTooltipShown;
    private bool fireBuilt;
    private bool lightFireTooltipShown;
    private bool buildStructuresTooltipShown;
    private bool buildStructuresTooltipHidden;
    private bool ammoCrafted;
    private bool dawnStarted;
    private bool initialEmberGiven;
    private bool emberExtracted;
    private bool animalDied;
    private bool workerDroppedOrb;
    private bool fireFuelled;
    private bool emberExtractionObjectiveStarted;
    private bool emberExtractionObjectiveEnded;
    private bool setupDefensesObjectiveStarted;

    private bool nightStarted;
    private int workerNumberRecruited;
    private int towerNumberBuilt;
    private int workerNumberDied;
    private int barricadeNumberBuilt;
    private int hunterNumberRecruited;
    private int fireFuelledNumber;

    private bool showingGetReady;

    public static event EventHandler OnAnySpotLightActivated;

    private void Awake() {
        Instance = this;
        firstCreatureSpotLight.enabled = false;
    }

    private void Start() {
        testing = DebugManager.Instance.GetDebugMode_Tutorial();
        PlayerCamp.Instance.BlockStructureUnlocks();

        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        dog.OnIdleStateChanged += Dog_OnIdleStateChanged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        Creature.OnAnyMobDied += Creature_OnAnyMobDied;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        PlayerShoot.Instance.OnPlayerShot += PlayerSHoot_OnPlayerShot;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Shrine.OnAnyShrineActivated += Shrine_OnAnyShrineActivated;
        CurrencyCrafter.OnPlayerCollectedAnyCurrency += CurrencyCrafter_OnPlayerCollectedAnyCurrency;
        CurrencyCrafter.OnAnyCurrencyCraftingEnded += CurrencyCrafter_OnAnyCurrencyCraftingEnded;
        CurrencyCrafter.OnAnyNewCurrencyBatchCraftingStarted += CurrencyCrafter_OnAnyCurrencyCraftingStarted;
        Collectible.OnAnyCollectiblePickedUpByPlayer += Collectible_OnAnyCollectiblePickedUpByPlayer;
        startFireLocation.OnPlayerTriggeredIn += StartFireLocation_OnPlayerTriggeredIn;
        startFireLocation.OnPlayerTriggeredOut += StartFireLocation_OnPlayerTriggeredOut;
        ammoCrafterLocation.OnPlayerTriggeredIn += AmmoCrafterLocation_OnPlayerTriggeredIn;
        ammoCrafterLocation.OnPlayerTriggeredOut += AmmoCrafterLocation_OnPlayerTriggeredOut;
        hunterShrineLocation.OnPlayerTriggeredIn += HunterShrineLocation_OnPlayerTriggeredIn;
        hunterShrineLocation.OnPlayerTriggeredOut += HunterShrineLocation_OnPlayerTriggeredOut;

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        Animal.OnAnyMobDied += Animal_OnAnyMobDied;
        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
        Fire.Instance.OnPlayerTriggeredIn += Fire_OnPlayerTriggeredIn;
        Fire.Instance.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        WorkerManager.Instance.OnRecruitedWorkerDied += WorkerManager_OnRecruitedWorkerDied;
        HuntingFlag_PlayerDefined.OnAnyPlayerTriggeredIn += HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn;
        Tower.OnPlayerClimbedOnAnyTower += Tower_OnPlayerClimbedOnAnyTower;
        endLevelArea.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
        endLevelArea.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;

        if (endTutorialPortal != null) {
            endTutorialPortal.OnPlayerMovedOnTeleporter += EndLevelPortal_OnPlayerMovedOnTeleporter;
        }

        PlayerShoot.Instance.SetCanShoot(false);
        PlayerShoot.Instance.SetGunCanJam(false);
        StartCoroutine(SetGunAmmoAfterDelay());
        StartCoroutine(ShowMoveTooltipAfterDelay());

        if(testing) {
            reloadTooltipHidden = true;
            Player.Instance.SetPosition(debugSpawnPoint.position);
        } else {
            Player.Instance.SetPosition(initialSpawnPoint.position);
        }
    }

    private void LevelUI_OnObjectiveCompleted(object sender, EventArgs e) {
        Debug.Log("LevelUI_OnObjectiveCompleted " + LevelUI_ObjectiveUI.Instance.GetCurrentObjectiveType());
        if(LevelUI_ObjectiveUI.Instance.GetCurrentObjectiveType() == LevelUI_ObjectiveUI.ObjectiveType.SetupCamp && !setupDefensesObjectiveStarted) {
            StartCoroutine(EndCampSetupObjective());
            setupDefensesObjectiveStarted = true;
        }
    }

    private IEnumerator EndCampSetupObjective() {

        yield return new WaitForSeconds(3f);

        DayNightManager.Instance.ChangeState(DayNightManager.State.Dusk);

        yield return new WaitForSeconds(2f);

        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectivesUnlocked = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
                LevelUI_ObjectiveUI.SubObjectiveType.Build2Barricades,
                LevelUI_ObjectiveUI.SubObjectiveType.Build2Towers,
                LevelUI_ObjectiveUI.SubObjectiveType.FuelFire,
        };

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.PrepareForNight);
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectivesUnlocked);
        Fire.Instance.ManualSetFireCurrentMaxFuelTreshold(Fire.State.wild);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);
        UnlockDefensiveStructureLocations();
    }

    private void Update() {
        if(testing) {
            if (Input.GetKeyDown(KeyCode.M)) {
                Debug.Log("M");
                //LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
                //LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.RecruitMoreEmberlings);
                //LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire);
                StartCoroutine(StartGuardingWorkersObjective(0f));
                //StartSetupCampObjective();
            }

        }

        if (moveTooltipShown && !moveTooltipHidden) {
            if(GameInput.Instance.GetMovementFloatNormalized() != 0) {
                moveTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(.5f));
            }
        }

        if(aimTooltipShown && !aimTooltipHidden) {
            float currentAimDir = PlayerAim.Instance.GetAimDir().x;
            if (currentAimDir > 0) {
                currentAimDir = 1;
            }
            else {
                currentAimDir = -1;
            }

            if (aimDirTooltip != currentAimDir) {
                StartCoroutine(HideTooltipAfterDelay(.5f));
                aimTooltipHidden = true;
            }
        }

        if(!reloadTooltipHidden) {
            HandleBlockingCollider(firstCreatureCollider.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_getGunReady"));
        }

        if(reloadTooltipHidden && workerNumberRecruited < 4) {
            HandleBlockingCollider(blockingWorkersCollider.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_recruitFirst"));
        }

        if (reloadTooltipHidden && !dawnStarted && workerNumberRecruited >= 4) {
            HandleBlockingCollider(endLevelAreaCollider.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_tooDangerous"));
        }

        if (dawnStarted && !emberExtracted) {
            HandleBlockingCollider(extractEmberCollider.transform.position, LocalizationManager.Instance.GetLocalizedText("tooltip_ectractEmberFirst"));
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

    private void EndLevelPortal_OnPlayerMovedOnTeleporter(object sender, EventArgs e) {
        endLevelPortalIndicator.SetActive(false);
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub);
    }

    private void Collectible_OnAnyCollectiblePickedUpByPlayer(object sender, EventArgs e) {
        if (workerDroppedOrb) return;
        if (hunterNumberRecruited < 2) return;
        if (!animalDied) return;

        Collectible collectible = sender as Collectible;
        if (collectible.GetCurrencyType() != PlayerCurrencies.CurrencyType.bigBlueOrb) return;
        if (collectible.GetDroppedByPlayer()) return;

        workerDroppedOrb = true;

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }
    private void Fire_OnFireFuelled(object sender, EventArgs e) {
        fireFuelledNumber++;
        fireFuelled = true;

        if (fireFuelledNumber == 1) {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);
            PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_fireLightTip"), 4f);

            if (barricadeNumberBuilt == 2 && towerNumberBuilt == 2) {
                StartCoroutine(StartSurviveTheNightObjective());
            }
            Fire.Instance.SetStructurePrimaryFunctionUnlocked(false);

        }

        if(fireFuelledNumber == 3) {
            Fire.Instance.SetStructurePrimaryFunctionUnlocked(false);
            Fire.Instance.SetStructureSecondaryFunctionUnlocked(true);
            HideTooltipAfterDelay(0f);
            StartCoroutine(ShowTooltipAfterDelay(1f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_extractEmber"), InputControlIcons.Control.Interact));
        }

    }

    private void Worker_OnAnyWorkerRecruited(object sender, System.EventArgs e) {
        workerNumberRecruited++;

        if (workerNumberRecruited == 4) {
            blockingWorkersCollider.SetColliderTrigger();
            StartCoroutine(HideTooltipAfterDelay(0f));
            StartCoroutine(StartGuardingWorkersObjective(1.5f));
        }

        if(workerNumberRecruited == 6) {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.RecruitMoreEmberlings);
        }
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        if (testing) return;
        if (firstCreatureDied) return;
        StartCoroutine(TransitionToTutorialCameraCoroutine(1f));
        firstCreatureDied = true;
    }

    private IEnumerator SetGunAmmoAfterDelay() {
        yield return new WaitForSeconds(.05f);
        PlayerShoot.Instance.SetGunAmmo(PlayerShoot.Instance.GetHeldGunSO(), 0, 0);
        PlayerShoot.Instance.SetCanShoot(true);
        PlayerUI_AmmoBar.Instance.RefreshAmmoBar();
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = (StructureLocation)sender;
        StructureSO structureSO = structureLocation.GetStructureSOToBuild();

        if (structureSO.structureType == StructureSO.StructureType.fire) {
            if (fireBuilt) return;

            Fire.Instance.SetFireInteractionsUpdateLocked(true);
            Fire.Instance.SetStructureSecondaryFunctionUnlocked(false);
            ammoCrafterLocation.UnlockStructureLocation();
            hunterShrineLocation.UnlockStructureLocation();
            ammoCrafterLocationIndicator.SetActive(true);
            hunterShrineLocationIndicator.SetActive(true);
            fireBuilt = true;
            Fire.Instance.ManualSetFireCurrentMaxFuelTreshold(Fire.State.calm);

            StartCoroutine(HideTooltipAfterDelay(0f));

            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectivesUnlocked = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
                LevelUI_ObjectiveUI.SubObjectiveType.BuildAmmoCrafter,
                LevelUI_ObjectiveUI.SubObjectiveType.BuildHunterShrine,
                LevelUI_ObjectiveUI.SubObjectiveType.RecruitMoreEmberlings,
            };

            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire, subObjectivesUnlocked);
        }

        if (structureSO.structureType == StructureSO.StructureType.hunterShrine) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.BuildHunterShrine, LevelUI_ObjectiveUI.SubObjectiveType.Recruit2Hunters);

            if (!buildStructuresTooltipHidden) {
                buildStructuresTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(0f));
            }

        }

        if (structureSO.structureType == StructureSO.StructureType.ammoCrafter) {

            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.BuildAmmoCrafter, LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter);

            if (!buildStructuresTooltipHidden) {
                buildStructuresTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(0f));
            }

            StartCoroutine(ShowTooltipAfterDelay(.5f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("SubObj_TurnOnAmmoCrafter"), InputControlIcons.Control.Interact));
            StartCoroutine(HideTooltipAfterDelay(3f));
        }

        if (structureSO.structureType == StructureSO.StructureType.barricade) {
            barricadeNumberBuilt++;

            if (barricadeNumberBuilt == 2) {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.Build2Barricades);
            }

            if(fireFuelled && towerNumberBuilt == 2) {
                StartCoroutine(StartSurviveTheNightObjective());
            }
        }

        if (structureSO.structureType == StructureSO.StructureType.tower) {
            towerNumberBuilt++;

            if (towerNumberBuilt == 2) {
                //StartCoroutine(ShowTooltipAfterDelay(.5f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_hold"), InputControlIcons.Control.Interact));
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.Build2Towers);
            }

            if (fireFuelled && barricadeNumberBuilt == 2) {
                StartCoroutine(StartSurviveTheNightObjective());
            }
        }


    }

    private void Animal_OnAnyMobDied(object sender, EventArgs e) {
        if (animalDied || hunterNumberRecruited < 2) return;
        animalDied = true;

        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt, LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

    #region TUTORIAL LOOSE CONDITIONS
    private void WorkerManager_OnRecruitedWorkerDied(object sender, EventArgs e) {
        if (!fireBuilt) {
            workerNumberDied++;
            if (workerNumberDied == 3) {
                workerNumberDied = 0;
                Player.Instance.Die();
            }
        }
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e) {

    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (!fireBuilt) {
            RetryGuardWorkers();
        }
    }

    private void RetryGuardWorkers() {
        List<Worker> recruitedWorkers = WorkerManager.Instance.GetRecruitedWorkers();
        List<Worker> recruitedWorkersCoppy = new List<Worker>();

        foreach (Worker worker in recruitedWorkers) {
            recruitedWorkersCoppy.Add(worker);
        }
        foreach (Worker worker in recruitedWorkersCoppy) {
            worker.Die();
        }

        workerNumberRecruited = 0;
        firstWorkerSpawner.SpawnMobs(4);
        Transform chest = Instantiate(looseTutorialChest, looseTutorialChest.transform.position, Quaternion.identity);
        chest.gameObject.SetActive(true);
    }

    #endregion

    #region OBJECTIVES

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        StartCoroutine(EndTutorialCorioutine());
    }
    private void EndLevelArea_OnEndLevelAreaCleared(object sender, EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.ClearNest, LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
    }

    private void CurrencyCrafter_OnAnyCurrencyCraftingEnded(object sender, EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitCraftingAmmo, LevelUI_ObjectiveUI.SubObjectiveType.CollectCrafterAmmo);
    }

    private void CurrencyCrafter_OnAnyCurrencyCraftingStarted(object sender, EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter, LevelUI_ObjectiveUI.SubObjectiveType.WaitCraftingAmmo);
    }

    private IEnumerator StartSurviveTheNightObjective() {
        yield return new WaitForSeconds(2f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.Survive);
        CreaturesSpawnManager.Instance.SetTutorialWave();

        yield return new WaitForSeconds(4f);
        DayNightManager.Instance.SetCyclePausedTutorial(false);
        DayNightManager.Instance.ChangeState(DayNightManager.State.Night);
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, EventArgs e) {
        if (!animalDied) return;
        if (workerDroppedOrb) return;
        workerDroppedOrb = true;

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (dawnStarted) return;
        dawnStarted = true;

        DayNightManager.Instance.SetCyclePausedTutorial(true);
        extractEmberCollider.SetColliderSolid();

        StartCoroutine(StartDestroyNestObjective(1.5f));
    }

    private void CurrencyCrafter_OnPlayerCollectedAnyCurrency(object sender, EventArgs e) {
        if (ammoCrafted) return;

        ammoCrafted = true;
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectCrafterAmmo);
    }

    private void Shrine_OnAnyShrineActivated(object sender, EventArgs e) {
        hunterNumberRecruited++;
        if (hunterNumberRecruited == 2) {
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.Recruit2Hunters, LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt);
        }
    }

    public void StartSetupCampObjective() {
        StartCoroutine(StartSetupCampObjectiveCoroutine());
    }

    private IEnumerator StartGuardingWorkersObjective(float delay) {
        TransitionToCombatCamera();

        yield return new WaitForSeconds(delay);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.KeepWorkersAlive);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectivesTypesList = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.Keep2WorkersAlive,
        };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectivesTypesList);

        yield return new WaitForSeconds(2f);

        MusicManager.Instance.SetTargetVolumeToMainTrack();
        MusicManager.Instance.FadeInMusic(8f);

        yield return new WaitForSeconds(4f);

        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To run", InputControlIcons.Control.Run));
        StartCoroutine(HideTooltipAfterDelay(4f));
    }

    private IEnumerator StartDestroyNestObjective(float delay) {
        Debug.Log("StartDestroyNestObjective");
        emberExtractionObjectiveStarted = true;
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(2f);

        yield return new WaitForSeconds(delay);

        //Fire.Instance.SetStructureSecondaryFunctionUnlocked(true);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindNest);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmberTutorial };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
    }

    private IEnumerator DebugStartDestroyNestObjective(float delay) {
        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.Survive);

        yield return new WaitForSeconds(4f);

        emberExtractionObjectiveStarted = true;
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(2f);

        yield return new WaitForSeconds(delay);

        //Fire.Instance.SetStructureSecondaryFunctionUnlocked(true);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindNest);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmberTutorial };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
    }

    private IEnumerator StartSetupCampObjectiveCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.Keep2WorkersAlive);

        yield return new WaitForSeconds(3f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.SetupCamp);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectivesTypesList = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire,
        };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectivesTypesList);
        fireLocationIndicator.SetActive(true);
    }

    #endregion

    #region CONTROL TOOLTIPS

    private void Dog_OnIdleStateChanged(object sender, EventArgs e) {
        if (dogStateChanged) return;

        dogStateChanged = true;
        StartCoroutine(HideTooltipAfterDelay(0f));
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, EventArgs e) {
        StartCoroutine(HideTooltipAfterDelay(1f));
    }

    private void Fire_OnPlayerTriggeredIn(object sender, EventArgs e) {
        if(emberExtractionObjectiveStarted && !emberExtractionObjectiveEnded && Fire.Instance.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.primaryFunction) {
            emberExtractionObjectiveEnded = true;

            StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("SubObj_FuelFire"), InputControlIcons.Control.Interact));
        }
    }

    private void PlayerSHoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (shootTipShown) {
            if (!shootTipHidden) {
                StartCoroutine(HideTooltipAfterDelay(2f));
                shootTipHidden = true;
            };
        };
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (shootTipShown) return;

        StartCoroutine(ShowTooltipAfterDelay(.2f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_shoot"), InputControlIcons.Control.Shoot));
        shootTipShown = true;
    }


    private void PlayerShoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        if (reloadTooltipShown) return;
        reloadTooltipShown = true;
        StartCoroutine(SwapReloadInstructionsCoroutine(1.5f));
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        if (e.tipTypeShown == VideoTipSO.VideoTipType.Reloading && !transferAmmoTooltipShown) {
            StartCoroutine(ShowTooltipAfterDelay(1f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_ammoTip"), InputControlIcons.Control.Reload));
            transferAmmoTooltipShown = true;
            return;
        };

        if (e.tipTypeShown == VideoTipSO.VideoTipType.Roll && !rollTooltipShown) {
            ShowRollTip();
            rollTooltipShown = true;
            return;
        };

        if(e.tipTypeShown == VideoTipSO.VideoTipType.Hunters) {
            StartCoroutine(ShowTooltipAfterDelay(.5f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_hunterShrineTip"), InputControlIcons.Control.Interact));
            StartCoroutine(HideTooltipAfterDelay(3f));
        }
    }
    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        
        if (!dropOrbShown) {
            if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                StartCoroutine(ShowTooltipAfterDelay(1f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_recruitEmberling"), InputControlIcons.Control.Interact));
                dropOrbShown = true;
                return;
            }
        }

        if(e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            if(!initialEmberGiven) {
                initialEmberGiven = true;
                return;
            }

            Debug.Log("ember Collected : endLevelAreaCollider.SetColliderTrigger()");
            emberExtracted = true;
            endLevelAreaCollider.SetColliderTrigger();
            extractEmberCollider.SetColliderTrigger();

            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmberTutorial, LevelUI_ObjectiveUI.SubObjectiveType.FindNest);
        }

    }

    private void HunterShrineLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        hunterShrineLocationIndicator.gameObject.SetActive(false);
        if (buildStructuresTooltipShown) return;
        StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_buildStructureTip"), InputControlIcons.Control.Interact));
        buildStructuresTooltipShown = true;
    }

    private void AmmoCrafterLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        ammoCrafterLocationIndicator.gameObject.SetActive(false);
        if (buildStructuresTooltipShown) return;
        StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_buildStructureTip"), InputControlIcons.Control.Interact));
        buildStructuresTooltipShown = true;
    }

    private void StartFireLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        fireLocationIndicator.gameObject.SetActive(false);
        if (lightFireTooltipShown) return;
        lightFireTooltipShown = true;
        StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_lightFire"), InputControlIcons.Control.Interact));
    }

    private void HunterShrineLocation_OnPlayerTriggeredOut(object sender, EventArgs e) {
        hunterShrineLocation.gameObject.SetActive(true);
    }

    private void AmmoCrafterLocation_OnPlayerTriggeredOut(object sender, EventArgs e) {
        ammoCrafterLocationIndicator.gameObject.SetActive(true);
    }

    private void StartFireLocation_OnPlayerTriggeredOut(object sender, EventArgs e) {
        fireLocationIndicator.gameObject.SetActive(true);
    }

    private IEnumerator SwapReloadInstructionsCoroutine(float delay) {
        yield return new WaitForSeconds(delay);

        PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();

        yield return new WaitForSeconds(3f);

        if (PlayerShoot.Instance.GetCurrentBullets() == 0) {
            StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_reload"), InputControlIcons.Control.Reload));
        }
        else {
            reloadTooltipHidden = true;
        }

    }

    private IEnumerator ShowMoveTooltipAfterDelay() {
        StartCoroutine(ShowTooltipAfterDelay(2f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_moveLeftRight"), InputControlIcons.Control.Move));
        yield return new WaitForSeconds(2f);
        moveTooltipShown = true;
    }

    #endregion

    #region OTHER TOOLTIPS

    private void Tower_OnPlayerClimbedOnAnyTower(object sender, EventArgs e) {
        if (climbTowerTooltipShown) return;

        climbTowerTooltipShown = true;
        StartCoroutine(HideTooltipAfterDelay(.2f));
        PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_towerTip"), 4f);
    }

    private void HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn(object sender, EventArgs e) {
        if(huntingFlagTooltipShown) return;
        huntingFlagTooltipShown = true;
        PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_hunterFlagTip"), 4f);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        if (reloadTooltipShown && !reloadTooltipHidden) {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            reloadTooltipHidden = true;
            showingGetReady = false;
            firstCreatureCollider.SetColliderTrigger();
            return;
        }

        if (!saveAmmoTooltipShown) {
            if (PlayerShoot.Instance.GetCurrentBullets() != 0) {
                PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_saveAmmoTip"), 3f);
                saveAmmoTooltipShown = true;
            }
        }
    }
    private void Player_OnPlayerDamaged(object sender, EventArgs e) {
        if (!fireBuilt) return;
        if (healTooltipShown) return;
        healTooltipShown = true;
        PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_healTentTip"), 3f);
    }
    public void ShowLightTip() {
        StartCoroutine(ShowLightTipCoroutine());
    }

    public void ShowDogTip() {
        dogTipShown = true;
        StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_callDoggo"), InputControlIcons.Control.SwitchDog));
    }

    public IEnumerator ShowLightTipCoroutine() {
        StartCoroutine(ShowTooltipAfterDelay(0f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_torchOnOff"), InputControlIcons.Control.LightSwitch));
        yield return new WaitForSeconds(4f);
        PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
    }

    public void ShowRollTip() {
        StartCoroutine(ShowRollTipCoroutine());
    }

    public IEnumerator ShowRollTipCoroutine() {
        StartCoroutine(ShowTooltipAfterDelay(1f, LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("menu_roll"), InputControlIcons.Control.Roll));
        yield return new WaitForSeconds(4f);
        PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
    }
    #endregion

    private IEnumerator TransitionToTutorialCameraCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        TransitionToTutorialCamera();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay, string text1, string text2, InputControlIcons.Control control) {
        yield return new WaitForSeconds(delay);

        PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, control);
     
    }

    private IEnumerator HideTooltipAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
    }

    public void ActivateCreatureSpotLight() {
        StartCoroutine(ActivateCreatureSpotLightCoroutine(1.5f));
    }

    public void TransitionToCombatCamera() {
        CameraManager.Instance.ZoomOut(false, .7f, 2f);
    }

    public void TransitionToTutorialCamera() {
        CameraManager.Instance.ZoomOut(false, 1f, 2f);
    }

    private IEnumerator ActivateCreatureSpotLightCoroutine(float delay) {
        yield return new WaitForSeconds(delay);

        firstCreatureSpotLight.enabled = true;
        OnAnySpotLightActivated?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetRespawnPosition() {
        if(fireBuilt) {
            return Tent.Instance.transform.position;
        } else {
            return beforeFireRespawnPoint.position;
        }
    }

    public void UnlockDefensiveStructureLocations() {
        foreach(StructureLocation structureLocation in defensiveStructureLocations) {
            structureLocation.UnlockStructureLocation();
        }
    }

    private IEnumerator EndTutorialCorioutine() {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
        MetaProgressionManager.Instance.SetTutorialCompleted(); 
        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber();
        
        yield return new WaitForSeconds(4f); 
        endTutorialPortal.gameObject.SetActive(true);
        endLevelPortalIndicator.SetActive(true);
        yield return new WaitForSeconds(1f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.ReturnToHub);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveUIList = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub,
            };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveUIList);
    }

    private void OnDestroy() {
        VideoTipUI.Instance.OnVideoTipPanelClosed -= VideoTipUI_OnVideoTipPanelClosed;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
        dog.OnIdleStateChanged -= Dog_OnIdleStateChanged;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
        PlayerShoot.Instance.OnPlayerAmmoRefilled -= PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerReload -= PlayerShoot_OnPlayerReload;
        CreatureAI.OnAnyCreatureAggro -= CreatureAI_OnAnyCreatureAggro;
        Creature.OnAnyMobDied -= Creature_OnAnyMobDied;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        PlayerShoot.Instance.OnPlayerShot -= PlayerSHoot_OnPlayerShot;
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        Shrine.OnAnyShrineActivated -= Shrine_OnAnyShrineActivated;
        CurrencyCrafter.OnPlayerCollectedAnyCurrency -= CurrencyCrafter_OnPlayerCollectedAnyCurrency;
        CurrencyCrafter.OnAnyCurrencyCraftingEnded -= CurrencyCrafter_OnAnyCurrencyCraftingEnded;
        CurrencyCrafter.OnAnyNewCurrencyBatchCraftingStarted -= CurrencyCrafter_OnAnyCurrencyCraftingStarted;
        Collectible.OnAnyCollectiblePickedUpByPlayer -= Collectible_OnAnyCollectiblePickedUpByPlayer;
        startFireLocation.OnPlayerTriggeredIn -= StartFireLocation_OnPlayerTriggeredIn;
        ammoCrafterLocation.OnPlayerTriggeredIn -= AmmoCrafterLocation_OnPlayerTriggeredIn;
        hunterShrineLocation.OnPlayerTriggeredIn -= HunterShrineLocation_OnPlayerTriggeredIn;
        Player.Instance.OnPlayerDamaged -= Player_OnPlayerDamaged;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        Animal.OnAnyMobDied -= Animal_OnAnyMobDied;
        Fire.Instance.OnFireFuelled -= Fire_OnFireFuelled;
        Fire.Instance.OnPlayerTriggeredIn -= Fire_OnPlayerTriggeredIn;
        Fire.Instance.OnFireEmberExtractionStarted -= Fire_OnFireEmberExtractionStarted;
        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        WorkerManager.Instance.OnRecruitedWorkerDied -= WorkerManager_OnRecruitedWorkerDied;
        HuntingFlag_PlayerDefined.OnAnyPlayerTriggeredIn -= HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn;
        Tower.OnPlayerClimbedOnAnyTower -= Tower_OnPlayerClimbedOnAnyTower;
        endLevelArea.OnEndLevelAreaCleared -= EndLevelArea_OnEndLevelAreaCleared;
        endLevelArea.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
    }

}
