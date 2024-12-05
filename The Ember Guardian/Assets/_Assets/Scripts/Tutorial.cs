using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    [SerializeField] private bool testing;
    [SerializeField] private TutorialCollider firstCreatureCollider;
    [SerializeField] private TutorialCollider blockingWorkersCollider;
    [SerializeField] private TutorialCollider endLevelAreaCollider;
    [SerializeField] private StructureLocation startFireLocation;
    [SerializeField] private StructureLocation ammoCrafterLocation;
    [SerializeField] private StructureLocation hunterShrineLocation;
    [SerializeField] private EndLevelArea endLevelArea;
    [SerializeField] private Light2D firstCreatureSpotLight;

    private bool moveTooltipShown;
    private bool moveTooltipHidden;
    private bool transferAmmoTooltipShown;
    private bool reloadTooltipShown;
    private bool reloadTooltipHidden;
    private bool saveAmmoTooltipShown;

    private bool shootTipShown;
    private bool shootTipHidden;
    private bool firstCreatureDied;

    private bool dropOrbShown;
    private int workerNumberRecruited;

    private bool healTooltipShown;
    private bool huntingFlagTooltipShown;
    private bool fireBuilt;
    private bool lightFireTooltipShown;
    private bool buildStructuresTooltipShown;
    private bool buildStructuresTooltipHidden;
    private bool ammoCrafted;
    private bool duskStarted;
    private bool dawnStarted;
    private bool emberExtracted;
    private bool animalDied;
    private bool workerDroppedOrb;
    private bool fireFuelled;
    private bool emberExtractionObjectiveStarted;
    private bool emberExtractionObjectiveEnded;

    private bool nightStarted;
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
        PlayerShoot.Instance.SetCanShoot(false);
        StartCoroutine(SetGunAmmoAfterDelay());

        UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
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
        CurrencyCrafter.OnAnyCurrencyCraftingStarted += CurrencyCrafter_OnAnyCurrencyCraftingStarted;
        startFireLocation.OnPlayerTriggeredIn += StartFireLocation_OnPlayerTriggeredIn;
        ammoCrafterLocation.OnPlayerTriggeredIn += AmmoCrafterLocation_OnPlayerTriggeredIn;
        hunterShrineLocation.OnPlayerTriggeredIn += HunterShrineLocation_OnPlayerTriggeredIn;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        Animal.OnAnyMobDied += Animal_OnAnyMobDied;
        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
        Fire.Instance.OnPlayerTriggeredIn += Fire_OnPlayerTriggeredIn;
        Fire.Instance.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        HuntingFlag_PlayerDefined.OnAnyPlayerTriggeredIn += HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn;
        endLevelArea.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
        endLevelArea.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;

        StartCoroutine(ShowMoveTooltipAfterDelay());
    }

    private void Update() {
        if(moveTooltipShown && !moveTooltipHidden) {
            if(GameInput.Instance.GetMovementFloatNormalized() != 0) {
                moveTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(1.5f));
            }
        }

        if(!reloadTooltipHidden) {
            HandleBlockingCollider(firstCreatureCollider.transform.position, "I should get my gun ready first");
        }

        if(reloadTooltipHidden && workerNumberRecruited < 4) {
            HandleBlockingCollider(blockingWorkersCollider.transform.position, "I should recruit the lost souls first");
        }

        if (reloadTooltipHidden && !dawnStarted) {
            HandleBlockingCollider(endLevelAreaCollider.transform.position, "This seems too dangerous for now");
        }

        if (dawnStarted && !emberExtracted) {
            HandleBlockingCollider(endLevelAreaCollider.transform.position, "I should extract an ember first");
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            StartCoroutine(StartGuardingWorkersObjective(0f));
        }

        //if (Input.GetKeyDown(KeyCode.B)) {
        //    StartCoroutine(StartSurviveTheNightObjective());
        //}
    }

    private void HandleBlockingCollider(Vector3 colliderPosition, string textToShow) {
        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) < 1.5f && !showingGetReady) {
            showingGetReady = true;
            PlayerUI_World.Instance.GetTooltipRight().ShowTooltip(textToShow, 3f);
        }

        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) > 1.5f && showingGetReady) {
            showingGetReady = false;
            PlayerUI_World.Instance.GetTooltipRight().HideTooltip();
        }
    }

    private void Fire_OnFireFuelled(object sender, EventArgs e) {
        fireFuelledNumber++;

        if (fireFuelledNumber == 1) {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FuelFire);
            PlayerUI_World.Instance.GetTooltipRight().ShowTooltip("The fire's light seems to weaken the creatures... ", 4f);

            if (barricadeNumberBuilt == 2) {
                StartCoroutine(StartSurviveTheNightObjective());
            }
            Fire.Instance.SetStructurePrimaryFunctionUnlocked(false);

        }

        if(fireFuelledNumber == 3) {
            Fire.Instance.SetStructurePrimaryFunctionUnlocked(false);
            Fire.Instance.SetStructureSecondaryFunctionUnlocked(true);
            HideTooltipAfterDelay(0f);
            StartCoroutine(ShowTooltipAfterDelay(1f, "Hold", "To extract en ember", InputControlIcons.Control.Interact));
        }

    }

    private void Worker_OnAnyWorkerRecruited(object sender, System.EventArgs e) {
        workerNumberRecruited++;

        if(workerNumberRecruited == 4) {
            if (testing) return;
            blockingWorkersCollider.SetColliderTrigger();
            StartCoroutine(HideTooltipAfterDelay(0f));
            StartCoroutine(StartGuardingWorkersObjective(.5f));
        }

        if(workerNumberRecruited == 6) {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.RecruitMoreEmberlings);
        }
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        if (testing) return;
        if (firstCreatureDied) return;
        StartCoroutine(TransitionToTutorialCameraCoroutine(3f));
        firstCreatureDied = true;
    }

    private IEnumerator SetGunAmmoAfterDelay() {
        yield return new WaitForSeconds(.05f);
        PlayerShoot.Instance.SetGunAmmo(PlayerShoot.Instance.GetHeldGunSO(), 0);
        PlayerShoot.Instance.SetCanShoot(true);
        PlayerUI_AmmoBar.Instance.RefreshAmmoBar();
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = (StructureLocation)sender;
        StructureSO structureSO = structureLocation.GetStructureSOToBuild();

        if (structureSO.structureType == StructureSO.StructureType.fire) {
            if (fireBuilt) return;

            Fire.Instance.SetStructureSecondaryFunctionUnlocked(false);

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

            StartCoroutine(ShowTooltipAfterDelay(.5f, "Hold", "To recruit trappers", InputControlIcons.Control.Interact));
            StartCoroutine(HideTooltipAfterDelay(3f));
        }

        if (structureSO.structureType == StructureSO.StructureType.ammoCrafter) {

            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.BuildAmmoCrafter, LevelUI_ObjectiveUI.SubObjectiveType.TurnOnAmmoCrafter);

            if (!buildStructuresTooltipHidden) {
                buildStructuresTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(0f));
            }

            StartCoroutine(ShowTooltipAfterDelay(.5f, "Hold", "To turn on crafter", InputControlIcons.Control.Interact));
            StartCoroutine(HideTooltipAfterDelay(3f));
        }

        if (structureSO.structureType == StructureSO.StructureType.barricade) {
            barricadeNumberBuilt++;

            if (barricadeNumberBuilt == 2) {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.Build2Barricades);
            }

            if(fireFuelled) {
                StartCoroutine(StartSurviveTheNightObjective());
            }
        }

    }

    private void Animal_OnAnyMobDied(object sender, EventArgs e) {
        if (animalDied || hunterNumberRecruited < 2) return;
        animalDied = true;

        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.WaitForHunt, LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

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
        DayNightManager.Instance.SetCyclePaused(false);
        DayNightManager.Instance.ChangeState(DayNightManager.State.Night);
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, EventArgs e) {
        if (workerDroppedOrb) return;
        workerDroppedOrb = true;

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbsFromHunters);
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (dawnStarted) return;
        dawnStarted = true;
        DayNightManager.Instance.SetCyclePaused(true);

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

        MusicManager.Instance.FadeInMusic(8f);

        yield return new WaitForSeconds(4f);

        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To run", InputControlIcons.Control.Run));
        StartCoroutine(HideTooltipAfterDelay(4f));
    }

    private IEnumerator StartDestroyNestObjective(float delay) {
        emberExtractionObjectiveStarted = true;
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(2f);

        yield return new WaitForSeconds(delay);

        //Fire.Instance.SetStructureSecondaryFunctionUnlocked(true);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindNest);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber };

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
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
    }

    private IEnumerator StartSetupCampObjectiveCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.Keep2WorkersAlive);

        yield return new WaitForSeconds(5f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.SetupCamp);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectivesTypesList = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.LightMainFire,
        };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectivesTypesList);
    }

    #endregion

    #region CONTROL TOOLTIPS

    private void Fire_OnFireEmberExtractionStarted(object sender, EventArgs e) {
        StartCoroutine(HideTooltipAfterDelay(1f));
    }

    private void Fire_OnPlayerTriggeredIn(object sender, EventArgs e) {
        if(emberExtractionObjectiveStarted && !emberExtractionObjectiveEnded && Fire.Instance.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.primaryFunction) {
            emberExtractionObjectiveEnded = true;

            StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To add fuel", InputControlIcons.Control.Interact));
        }
    }

    private void PlayerSHoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (!shootTipShown) return;
        if (shootTipHidden) return;

        StartCoroutine(HideTooltipAfterDelay(2f));
        shootTipHidden = true;
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (shootTipShown) return;

        StartCoroutine(ShowTooltipAfterDelay(.2f, "Press", "To shoot", InputControlIcons.Control.Shoot));
        shootTipShown = true;
    }


    private void PlayerShoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        if (reloadTooltipShown) return;
        reloadTooltipShown = true;
        StartCoroutine(SwapReloadInstructionsCoroutine(1.5f));
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!transferAmmoTooltipShown) {
            if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
                StartCoroutine(ShowTooltipAfterDelay(1f, "Hold", "To transfer ammo to rifle", InputControlIcons.Control.Reload));
                transferAmmoTooltipShown = true;
                return;
            }
        };

        if (!dropOrbShown) {
            if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                StartCoroutine(ShowTooltipAfterDelay(1f, "Press", "To recruit an emberling", InputControlIcons.Control.Interact));
                dropOrbShown = true;
                return;
            }
        }

        if(e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            emberExtracted = true;
            endLevelAreaCollider.SetColliderTrigger();
            LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber, LevelUI_ObjectiveUI.SubObjectiveType.FindNest);
        }

    }

    private void HunterShrineLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        if (buildStructuresTooltipShown) return;
        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To build structures", InputControlIcons.Control.Interact));
        buildStructuresTooltipShown = true;

    }

    private void AmmoCrafterLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        if (buildStructuresTooltipShown) return;
        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To build structures", InputControlIcons.Control.Interact));
        buildStructuresTooltipShown = true;

    }

    private void StartFireLocation_OnPlayerTriggeredIn(object sender, EventArgs e) {
        Debug.Log(lightFireTooltipShown);
        if (lightFireTooltipShown) return;
        lightFireTooltipShown = true;
        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To light fire", InputControlIcons.Control.Interact));
    }

    private IEnumerator SwapReloadInstructionsCoroutine(float delay) {
        Debug.Log("SwapReloadInstructionsCoroutine");
        yield return new WaitForSeconds(delay);

        PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();

        yield return new WaitForSeconds(3f);

        if (PlayerShoot.Instance.GetCurrentBullets() == 0) {
            StartCoroutine(ShowTooltipAfterDelay(0f, "Press", "To reload", InputControlIcons.Control.Reload));
        }
        else {
            reloadTooltipHidden = true;
        }

    }

    private IEnumerator ShowMoveTooltipAfterDelay() {
        StartCoroutine(ShowTooltipAfterDelay(2f, "Use", "To move", InputControlIcons.Control.Move));
        yield return new WaitForSeconds(2f);
        moveTooltipShown = true;
    }

    #endregion

    #region OTHER TOOLTIPS
    private void HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn(object sender, EventArgs e) {
        if(huntingFlagTooltipShown) return;
        huntingFlagTooltipShown = true;
        PlayerUI_World.Instance.GetTooltipRight().ShowTooltip("Trappers won't venture past this flag", 4f);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        if (reloadTooltipShown && !reloadTooltipHidden) {
            PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();
            reloadTooltipHidden = true;
            showingGetReady = false;
            firstCreatureCollider.SetColliderTrigger();
            return;
        }

        if (!saveAmmoTooltipShown) {
            if (PlayerShoot.Instance.GetCurrentBullets() != 0) {
                PlayerUI_World.Instance.GetTooltipRight().ShowTooltip("I should save my ammo ... ", 3f);
                saveAmmoTooltipShown = true;
            }
        }
    }
    private void Player_OnPlayerDamaged(object sender, EventArgs e) {
        if (!fireBuilt) return;
        if (healTooltipShown) return;
        healTooltipShown = true;
        PlayerUI_World.Instance.GetTooltipRight().ShowTooltip("I can heal at my tent", 3f);
    }
    public void ShowLightTip() {
        StartCoroutine(ShowLightTipCoroutine());
    }

    public IEnumerator ShowLightTipCoroutine() {
        StartCoroutine(ShowTooltipAfterDelay(0f, "Press", "To toggle your flashlight", InputControlIcons.Control.LightSwitch));
        yield return new WaitForSeconds(4f);
        PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();
    }
    #endregion

    private IEnumerator TransitionToTutorialCameraCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        TransitionToTutorialCamera();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay, string text1, string text2, InputControlIcons.Control control) {
        yield return new WaitForSeconds(delay);
        List<Sprite> spriteList = InputControlIcons.Instance.GetControlIconSprite(control);

        if (spriteList.Count == 1) {
            PlayerUI_World.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, spriteList[0]);
        }

        if (spriteList.Count == 2) {
            PlayerUI_World.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, spriteList[0], spriteList[1]);
        }

    }

    private IEnumerator HideTooltipAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();
    }

    public void ActivateCreatureSpotLight() {
        StartCoroutine(ActivateCreatureSpotLightCoroutine(3f));
    }

    public void TransitionToCombatCamera() {
        CameraManager.Instance.ZoomOut(false, .7f, 2f);
    }

    public void TransitionToTutorialCamera() {
        CameraManager.Instance.ZoomOut(false, 1.3f, 2f);
    }

    private IEnumerator ActivateCreatureSpotLightCoroutine(float delay) {
        yield return new WaitForSeconds(delay);

        firstCreatureSpotLight.enabled = true;
        OnAnySpotLightActivated?.Invoke(this, EventArgs.Empty);
    }


    private IEnumerator EndTutorialCorioutine() {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
        yield return new WaitForSeconds(6f);
        SceneLoader.Instance.LoadHub();
    }
}
