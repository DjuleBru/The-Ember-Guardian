using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoTipManager : MonoBehaviour
{
    public static VideoTipManager Instance;

    [SerializeField] private VideoTipSO reloadingTip;
    [SerializeField] private VideoTipSO fireManagementTip;
    [SerializeField] private VideoTipSO dieTip;
    [SerializeField] private VideoTipSO critHitsTip;
    [SerializeField] private VideoTipSO rollTip;
    [SerializeField] private VideoTipSO healTentTip;
    [SerializeField] private VideoTipSO setupEconomyTip;
    [SerializeField] private VideoTipSO setupDefensesTip;
    [SerializeField] private VideoTipSO trapTip;
    [SerializeField] private VideoTipSO emberExtractionTip;
    [SerializeField] private VideoTipSO dayNightCycleTip;
    [SerializeField] private VideoTipSO hunterTip;
    [SerializeField] private VideoTipSO recruitEmberlingTip;
    [SerializeField] private VideoTipSO gunTip;
    [SerializeField] private VideoTipSO storeGemsTip;
    [SerializeField] private VideoTipSO huntingFlagTip;
    [SerializeField] private VideoTipSO scavengersTip;
    [SerializeField] private VideoTipSO engineersTip_basic;
    [SerializeField] private VideoTipSO engineersTip_advanced;
    [SerializeField] private VideoTipSO guardTips;
    [SerializeField] private VideoTipSO worldPortalTip;
    [SerializeField] private VideoTipSO scavengableObstacleTip;
    [SerializeField] private VideoTipSO architectTableTip;
    [SerializeField] private VideoTipSO mineTip;
    [SerializeField] private VideoTipSO watcherArtifactTip;
    [SerializeField] private VideoTipSO windTip;
    [SerializeField] private VideoTipSO specialAmmoTip;
    [SerializeField] private VideoTipSO surgeWindowTip;
    [SerializeField] private VideoTipSO gunManagementTip;

    private bool isLevelScene;
    private bool isTutorialScene;
    private bool isHubScene;
    [SerializeField] private bool isDemoTutorial;

    private bool reloadingTipShown;
    private bool critHitsTipShown;
    private bool rollTipShown;
    private bool fireManagementTipShown;
    private bool hunterTipShown;
    private bool recruitEmberlingTipShown;
    private bool emberExtractionTipShown;
    private bool healTentTipShown;

    private bool dieTipShown;
    private bool setupEconomyTipShown;
    private bool dayNightCycleTipShown;
    private bool gunTipShown;
    private bool storeGemsTipShown;
    private bool huntingFlagTipShown;
    private bool scavengablesTipShown;
    private bool worldPortalTipShown;
    private bool scavengableObstacleTipShown;
    private bool architectTableTipShown;
    private bool engineerTipBasicShown;
    private bool engineerTipAdvancedShown;
    private bool mineTipShown;
    private bool guardTipShown;
    private bool watcherArtifactTipShown;
    private bool windTipShown;
    private bool specialAmmoTipShown;
    private bool trapTipShown;
    private bool surgeWindowTipShown;
    private bool gunManagementTipShown;

    private bool showGunManagementTip;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadTooltipsShown();

        if(isDemoTutorial) {
            SubscribeToDemoTutorialEvents();
            return;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            isLevelScene = true;
            SubscribeToLevelEvents();
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            isHubScene = true;
            SubscribeToHubEvents();
        }

        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        Gun.OnAnyGunJammed += Gun_OnAnyGunJammed;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            isTutorialScene = true;
            SubscribeToTutorialEvents();

        } else {
            reloadingTipShown = true;
            critHitsTipShown = true;
            rollTipShown = true;
            fireManagementTipShown = true;
            hunterTipShown = true;
            recruitEmberlingTipShown = true;
            healTentTipShown = true;
            emberExtractionTipShown = true;
        }
    }


    private void SubscribeToDemoTutorialEvents() {
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
    }

    private void SubscribeToTutorialEvents() {
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        Mob.OnAnyMobDied += Creature_OnAnyMobDied;
        TutorialCollider.OnRollTipCollided += TutorialCollider_OnRollTipCollided;
        TutorialCollider.OnRecruitWorkerTipCollided += TutorialCollider_OnRecruitWorkerTipCollided;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn_Tutorial;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    private void SubscribeToLevelEvents() {
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn_Level;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Scavengable.OnAnyScavengableMarkedToScavenge += Scavengable_OnAnyScavengableMarkedToScavenge;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
    }

    private void Gun_OnAnyGunJammed(object sender, EventArgs e) {
        if (surgeWindowTipShown) return;
        VideoTipUI.Instance.PlayTipSO(surgeWindowTip, .7f);

        surgeWindowTipShown = true;
        ES3.Save("surgeWindowTipShown", true);
    }

    private void WindManager_OnWindStrengthChanged(object sender, EventArgs e) {
        if (windTipShown) return;

        if(WindManager.Instance.GetWindStrength() != WindManager.WindStrength.none) {
            VideoTipUI.Instance.PlayTipSO(windTip, 1f);

            windTipShown = true;
            ES3.Save("windTipShown", true);
        }
    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (trapTipShown) return;

        if (e.currencyUIDropped.GetCurrencyCategory() == PlayerCurrencies.CurrencyCategory.trap) {
            VideoTipUI.Instance.PlayTipSO(trapTip, 1f);

            trapTipShown = true;
            ES3.Save("trapTipShown", true);
        }
    }


    private void Scavengable_OnAnyScavengableMarkedToScavenge(object sender, EventArgs e) {
        Scavengable scavengable = sender as Scavengable;
        if(scavengable.GetIsMine()) {
            if (mineTipShown) return;
            mineTipShown = true;
            ES3.Save("mineTipShown", true);

            VideoTipUI.Instance.PlayTipSO(mineTip, .5f);
        }
    }
    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (specialAmmoTipShown) return;

        GunSO gunSO = PlayerShoot.Instance.GetHeldGunSO();

        if(gunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            VideoTipUI.Instance.PlayTipSO(specialAmmoTip, 1f);

            specialAmmoTipShown = true;
            ES3.Save("specialAmmoTipShown", true);
        }
    }

    private void Worker_OnAnyWorkerRecruited(object sender, EventArgs e) {
        if (guardTipShown) return;

        Worker worker = (Worker)sender;

        if (worker.GetWildJobType() == WorkerAI.JobTypes.guard) {
            VideoTipUI.Instance.PlayTipSO(guardTips, 1f);

            guardTipShown = true;
            ES3.Save("guardTipShown", true);
        }
    }

    private void SubscribeToHubEvents() {
        HubMerchant.OnAnyPlayerTriggeredIn += HubMerchant_OnAnyPlayerTriggeredIn;
        HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
        HubChest.Instance.OnChestSetCanOpen += HubChest_OnChestSetCanOpen;
        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }
    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, EventArgs e) {
        if (gunManagementTipShown) return;
        if (sender is HUBMerchantItem_GunMerchantItem) {
            HUBMerchantItem_GunMerchantItem gunItem = sender as HUBMerchantItem_GunMerchantItem;
            if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.newGun) {
                showGunManagementTip = true;
            }
        }

        if (sender is HubMerchantItem_TrainerMerchantItem) {
            HubMerchantItem_TrainerMerchantItem trainerItem = sender as HubMerchantItem_TrainerMerchantItem;
            if (trainerItem.GetTrainerItemType() == HubMerchantItem_TrainerMerchantItem.TrainerItemType.Hold2Weapons) {
                showGunManagementTip = true;
            }
        }
    }

    private void HubChest_OnChestSetCanOpen(object sender, EventArgs e) {
        if (storeGemsTipShown) return;

        VideoTipUI.Instance.PlayTipSO(storeGemsTip, 0f);

        storeGemsTipShown = true;
        ES3.Save("storeGemsTipShown", true);
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;

        if (showGunManagementTip) {
            VideoTipUI.Instance.PlayTipSO(gunManagementTip, 1f);
            gunManagementTipShown = true;
            ES3.Save("gunManagementTipShown", true);
        }

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) {
            if (huntingFlagTipShown) return;

            VideoTipUI.Instance.PlayTipSO(huntingFlagTip, 1f);

            huntingFlagTipShown = true;
            ES3.Save("huntingFlagTipShown", true);

        }

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.StructuresMerchant) {

            if (!isLevelScene) return;
            if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindArchitectTable) {
                if (scavengableObstacleTipShown) return;

                VideoTipUI.Instance.PlayTipSO(scavengableObstacleTip, 1f);

                scavengableObstacleTipShown = true;
                ES3.Save("scavengableObstacleTipShown", true);

            }; 
            
            if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
                if(engineerTipBasicShown) return;

                engineerTipBasicShown = true;
                ES3.Save("engineerTipBasicShown", true);
                VideoTipUI.Instance.PlayTipSO(engineersTip_basic);

            };

        }
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = (StructureLocation)sender;
        StructureSO structureSO = structureLocation.GetStructureSOToBuild();

        if (structureSO.structureType == StructureSO.StructureType.hunterShrine) {
            if (hunterTipShown) return;
            VideoTipUI.Instance.PlayTipSO(hunterTip, 0f);
            hunterTipShown = true;
        }

        if (structureSO.structureType == StructureSO.StructureType.currencyStorage_Objective) {
            if (watcherArtifactTipShown) return;
            VideoTipUI.Instance.PlayTipSO(watcherArtifactTip, 1f);
            watcherArtifactTipShown = true;

            ES3.Save("watcherArtifactTipShown", true);
        }

        if (structureSO.structureType == StructureSO.StructureType.currencyStorage_Ammo || structureSO.structureType == StructureSO.StructureType.currencyStorage_SpecialAmmo || structureSO.structureType == StructureSO.StructureType.currencyStorage_BigOrb || structureSO.structureType == StructureSO.StructureType.currencyStorage_SmallOrb) {
            if (engineerTipAdvancedShown) return;

            VideoTipUI.Instance.PlayTipSO(engineersTip_advanced, 0f);
            engineerTipAdvancedShown = true;

            ES3.Save("engineerTipAdvancedShown", true);
        }
    }

    #region HUB ONLY

    private void HubChest_OnChestOpened(object sender, EventArgs e) {
       
    }

    private void HubMerchant_OnAnyPlayerTriggeredIn(object sender, EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;
        if (!hubMerchant.GetMerchantUnlocked()) return;

        if(hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {
            if (gunTipShown) return;

            VideoTipUI.Instance.PlayTipSO(gunTip, 0f);

            gunTipShown = true;
            ES3.Save("gunTipShown", true);
        }

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.ArchitectTable) {
            if (architectTableTipShown) return;

            VideoTipUI.Instance.PlayTipSO(architectTableTip, 0f);

            architectTableTipShown = true;
            ES3.Save("architectTableTipShown", architectTableTipShown);
        }
    }

    #endregion

    #region LEVEL ONLY

    private void Player_OnPlayerExitedCamp(object sender, EventArgs e) {
        if (dieTipShown) return;

        VideoTipUI.Instance.PlayTipSO(dieTip, 0f);

        dieTipShown = true;
        ES3.Save("dieTipShown", true);
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        if (!setupEconomyTipShown) {
            VideoTipUI.Instance.PlayTipSO(setupEconomyTip, 1f);

            setupEconomyTipShown = true;
            ES3.Save("setupEconomyTipShown", true);
        };

        if(!scavengablesTipShown && LevelManager.Instance.GetLevelSO().levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.ExploreCorruptedCity) {
            VideoTipUI.Instance.PlayTipSO(scavengersTip, 1f);

            scavengablesTipShown = true;
            ES3.Save("scavengablesTipShown", true);
        }

    }

    private void Structure_OnAnyPlayerTriggeredIn_Level(object sender, EventArgs e) {
        Structure structure = sender as Structure;
        if(structure.GetStructureSO().structureType == StructureSO.StructureType.fastTravelTeleporter) {
            if (worldPortalTipShown) return;

            worldPortalTipShown = true;
            ES3.Save("worldPortalTipShown", true);
            VideoTipUI.Instance.PlayTipSO(worldPortalTip);
        }
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        if (dayNightCycleTipShown) return;

        VideoTipUI.Instance.PlayTipSO(dayNightCycleTip, 1f);

        dayNightCycleTipShown = true;
        ES3.Save("dayNightCycleTipShown", true);
    }

    #endregion

    #region TUTORIAL ONLY

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (emberExtractionTipShown) return;
        VideoTipUI.Instance.PlayTipSO(emberExtractionTip, 7f);
        emberExtractionTipShown = true;
    }

    private void Structure_OnAnyPlayerTriggeredIn_Tutorial(object sender, EventArgs e) {
        Structure structure = (Structure)sender;

        if (structure.GetStructureSO().structureType == StructureSO.StructureType.tent) {
            if (healTentTipShown) return;

            VideoTipUI.Instance.PlayTipSO(healTentTip, .3f);
            healTentTipShown = true;
        }

        if (structure.GetStructureSO().structureType == StructureSO.StructureType.fire) {
            if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Dusk) return;
            if (fireManagementTipShown) return;

            VideoTipUI.Instance.PlayTipSO(fireManagementTip, 0f);
            fireManagementTipShown = true;
        }
    }


    private void TutorialCollider_OnRecruitWorkerTipCollided(object sender, EventArgs e) {
        if (recruitEmberlingTipShown) return;
        VideoTipUI.Instance.PlayTipSO(recruitEmberlingTip, 0f);
        recruitEmberlingTipShown = true;
    }

    private void TutorialCollider_OnRollTipCollided(object sender, EventArgs e) {
        if (rollTipShown) return;
        VideoTipUI.Instance.PlayTipSO(rollTip, 0f);
        rollTipShown = true;
    }

    private void Creature_OnAnyMobDied(object sender, EventArgs e) {
        if (critHitsTipShown) return;
        VideoTipUI.Instance.PlayTipSO(critHitsTip, 2f);
        critHitsTipShown = true;
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyType = e.currencyUIDropped.GetCurrencyType();

        if (currencyType == PlayerCurrencies.CurrencyType.ammo && !reloadingTipShown) {
            VideoTipUI.Instance.PlayTipSO(reloadingTip, 1f);
            reloadingTipShown = true;
        }
    }

    #endregion

    [Button] 
    public void TestTip(VideoTipSO tipSO) {
        VideoTipUI.Instance.PlayTipSO(tipSO, 0f);
    }


    private void LoadTooltipsShown() {
        dieTipShown = ES3.Load("dieTipShown", false);
        setupEconomyTipShown = ES3.Load("setupEconomyTipShown", false);
        dayNightCycleTipShown = ES3.Load("dayNightCycleTipShown", false);
        gunTipShown = ES3.Load("gunTipShown", false);
        storeGemsTipShown = ES3.Load("storeGemsTipShown", false);
        huntingFlagTipShown = ES3.Load("huntingFlagTipShown", false);
        scavengablesTipShown = ES3.Load("scavengablesTipShown", false);
        worldPortalTipShown = ES3.Load("worldPortalTipShown", false);
        scavengableObstacleTipShown = ES3.Load("scavengableObstacleTipShown", false);
        architectTableTipShown = ES3.Load("architectTableTipShown", false);
        engineerTipBasicShown = ES3.Load("engineerTipBasicShown", false);
        engineerTipAdvancedShown = ES3.Load("engineerTipAdvancedShown", false);
        guardTipShown = ES3.Load("guardTipShown", false);
        mineTipShown = ES3.Load("mineTipShown", false);
        watcherArtifactTipShown = ES3.Load("watcherArtifactTipShown", false);
        windTipShown = ES3.Load("windTipShown", false);
        specialAmmoTipShown = ES3.Load("specialAmmoTipShown", false);
        trapTipShown = ES3.Load("trapTipShown", false);
        surgeWindowTipShown = ES3.Load("surgeWindowTipShown", false);
    }

    private void OnDestroy() {

        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Level;
        Player.Instance.OnPlayerExitedCamp -= Player_OnPlayerExitedCamp;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchant.OnAnyPlayerTriggeredIn -= HubMerchant_OnAnyPlayerTriggeredIn;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Gun.OnAnyGunJammed -= Gun_OnAnyGunJammed;

        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
        Mob.OnAnyMobDied -= Creature_OnAnyMobDied;
        TutorialCollider.OnRollTipCollided -= TutorialCollider_OnRollTipCollided;
        TutorialCollider.OnRecruitWorkerTipCollided -= TutorialCollider_OnRecruitWorkerTipCollided;
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Tutorial;

        if(isLevelScene || isTutorialScene || isDemoTutorial) {
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
            Fire.Instance.OnInitialFireActivated -= Fire_OnInitialFireActivated;
        }
    }
}
