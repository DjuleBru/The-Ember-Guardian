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
    [SerializeField] private VideoTipSO emberExtractionTip;
    [SerializeField] private VideoTipSO dayNightCycleTip;
    [SerializeField] private VideoTipSO hunterTip;
    [SerializeField] private VideoTipSO recruitEmberlingTip;
    [SerializeField] private VideoTipSO gunTip;
    [SerializeField] private VideoTipSO storeGemsTip;
    [SerializeField] private VideoTipSO swapWeaponTip;
    [SerializeField] private VideoTipSO huntingFlagTip;

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
    private bool setupDefensesTipShown;

    private bool dieTipShown;
    private bool setupEconomyTipShown;
    private bool dayNightCycleTipShown;
    private bool gunTipShown;
    private bool storeGemsTipShown;
    private bool swapWeaponTipShown;
    private bool huntingFlagTipShown;

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
            setupDefensesTipShown = true;
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
    }

    private void SubscribeToHubEvents() {
        HubMerchant.OnAnyPlayerTriggeredIn += HubMerchant_OnAnyPlayerTriggeredIn;
        HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
        HubChest.Instance.OnChestSetCanOpen += HubChest_OnChestSetCanOpen;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

    private void HubChest_OnChestSetCanOpen(object sender, EventArgs e) {
        if (storeGemsTipShown) return;

        VideoTipUI.Instance.PlayTipSO(storeGemsTip, 0f);

        storeGemsTipShown = true;
        ES3.Save("storeGemsTipShown", true);
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;
        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.HeroMerchant) {
            if (swapWeaponTipShown) return;
            if (!PlayerStats.Instance.GetCanHold2WeaponsUnlocked()) return;

            VideoTipUI.Instance.PlayTipSO(swapWeaponTip, 1f);

            swapWeaponTipShown = true;
            ES3.Save("swapWeaponTipShown", true);

        }

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) {
            if (huntingFlagTipShown) return;

            VideoTipUI.Instance.PlayTipSO(huntingFlagTip, 1f);

            huntingFlagTipShown = true;
            ES3.Save("huntingFlagTipShown", true);

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
        if (setupEconomyTipShown) return;

        VideoTipUI.Instance.PlayTipSO(setupEconomyTip, 1f);

        setupEconomyTipShown = true;
        ES3.Save("setupEconomyTipShown", true);
    }

    private void Structure_OnAnyPlayerTriggeredIn_Level(object sender, EventArgs e) {

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

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = (StructureLocation)sender;
        StructureSO structureSO = structureLocation.GetStructureSOToBuild();

        if(structureSO.structureType == StructureSO.StructureType.hunterShrine) {
            if (hunterTipShown) return;
            VideoTipUI.Instance.PlayTipSO(hunterTip, 0f);
            hunterTipShown = true;
        }

        //if (structureSO.structureType == StructureSO.StructureType.tower) {
        //    if (setupDefensesTipShown) return;
        //    VideoTipUI.Instance.PlayTipSO(setupDefensesTip, 1f);
        //    setupDefensesTipShown = true;
        //}
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
        VideoTipUI.Instance.PlayTipSO(critHitsTip, 3f);
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
        swapWeaponTipShown = ES3.Load("swapWeaponTipShown", false);
        huntingFlagTipShown = ES3.Load("huntingFlagTipShown", false);
    }

    private void OnDestroy() {

        if(isLevelScene) {
            Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
            Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Level;
            Fire.Instance.OnInitialFireActivated -= Fire_OnInitialFireActivated;
            Player.Instance.OnPlayerExitedCamp -= Player_OnPlayerExitedCamp;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        }

        if(isHubScene) {
            HubMerchant.OnAnyPlayerTriggeredIn -= HubMerchant_OnAnyPlayerTriggeredIn;
        }

        if(isTutorialScene) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
            Mob.OnAnyMobDied -= Creature_OnAnyMobDied;
            TutorialCollider.OnRollTipCollided -= TutorialCollider_OnRollTipCollided;
            TutorialCollider.OnRecruitWorkerTipCollided -= TutorialCollider_OnRecruitWorkerTipCollided;
            StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
            Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Tutorial;
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        }

    }
}
