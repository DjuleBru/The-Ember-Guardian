using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundRefsSO soundRefsSO;
    [SerializeField] private AudioSource gunPoweringUpAudioSource;
    
    private AudioSource audioSource2D;
    private float sfxVolume;

    private bool initialCampBackgroundBuilt;
    private bool initialEmberGiven;
    private bool initialGunEquipped;

    private bool criticalFireTickJustRemoved;
    private float criticalFireTickRemovedTimer;
    private float criticalFireTickRemovedMinDelay = .3f;

    private void Awake() {
        Instance = this;
        audioSource2D = GetComponent<AudioSource>();
        audioSource2D.spatialBlend = 0;
        audioSource2D.ignoreListenerPause = true;
        AudioListener.volume = 1;
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        SceneLoader.Instance.OnSceneFadeOut += SceneLoader_OnSceneFadeOut;
        SceneLoader.Instance.OnSceneFadeIn += SceneLoader_OnSceneFadeIn;

        if (Player.Instance != null) {
            Player.Instance.OnPlayerBackToTentToRespawn += Player_OnPlayerBackToTentToRespawn;
            PlayerShoot.Instance.OnPlayerStartedShot += PlayerShoot_OnPlayerStartedShot;
            PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoor_OnPlayerCooldownSFXTrigger;
            PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
            PlayerShoot.Instance.OnPlayerTryShoot_GunJammed += PlayerShoot_OnPlayerTryShoot_GunJammed;
            PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
            PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;
            PlayerShoot.Instance.OnPlayerAimedSightStarted += PlayerShoot_OnPlayerAimedSightStarted;
            PlayerShoot.Instance.OnPlayerAimedSightEnded += PlayerShoot_OnPlayerAimedSightEnded;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStopped += PlayerSHoot_OnPlayerOverclockedSMGStopped;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStarted += PlayerShoot_OnPlayerOverclockedSMGStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerShoot_OnPlayerFocusBlastStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStopped += Player_OnPlayerFocusBlastStopped;
            PlayerShoot.Instance.OnPlayerSetupLMGStarted += PlayerSHoot_OnPlayerSetupLMGStarted;
            PlayerShoot.Instance.OnPlayerSetupLMGStopped += PlayerSHoot_OnPlayerSetupLMGStopped;
            PlayerShoot.Instance.OnPlayerEmptyRevolverMagEnd += PlayerSHoot_OnPlayerEmptyRevolverMagEnd;

            PlayerSkills.Instance.OnActiveSkillReady += PlayerSkills_OnActiveSkillReady;
            PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
            PlayerSkills.Instance.OnPassiveSkillAdded += PlayerSkills_OnPassiveSkillAdded;

            PassiveShield.OnAnyPassiveShieldActivated += PassiveShield_OnAnyPassiveShieldActivated;
            PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;
            ActiveTeleportation.Instance.OnPlayerTeleported += ActiveTeleportation_OnPlayerTeleported;
            PlayerUI_AmmoBar.Instance.OnAmmoTickAdded += PlayerUI_AmmoBar_OnAmmoTickAdded;
            PlayerUI_HPBar.Instance.OnHPTickAdded += PlayerUI_HPBar_OnHPTickAdded;
            PlayerWorldUITooltip.OnTooltipHidden += PlayerWorldUITooltip_OnTooltipHidden;
            PlayerWorldUITooltip.OnTooltipShown += PlayerWorldUITooltup_OnTooltipShown;

            if(UICurrencyManager.PlayerInventoryUI != null) {
                UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
            }

            if(Dog.Instance != null) {
                Dog.Instance.OnPlayerCalledDog += Dog_OnPlayerCalledDog;
            }

            PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
        }

        if (LevelUI_ObjectiveUI.Instance != null) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveUIShown += LevelUI_ObjectiveUI_OnObjectiveUIShown;
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUICompleted += LevelUI_OnSubObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUIProgressed += LevelUI_OnSubObjectiveUIProgressed;
        }
        if (LevelUI_Locations.Instance != null) {
            LevelUI_Locations.Instance.OnLocationTextShown += LevelUI_OnLocationTextShown;
        }
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            WorkerFollowPlayerHandler.Instance.OnHoveredFollowingWorkerChanged += WorkerFollowPlayerHandler_OnHoveredFollowingWorkerChanged;
            LevelUI_DayCountUI.Instance.OnDayUIShown += LevelUI_OnDayUIShown;
        }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
            HubChest.Instance.OnChestClosed += HubChest_OnChestClosed;
            HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
        }

        if(VideoTipUI.Instance != null) {
            VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTipUI_OnVideoTipPanelOpened;
            VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        }

        if(CampEditManager.Instance != null) {
            CampEditManager.Instance.OnLayoutResetToDefault += CampEditManager_OnLayoutResetToDefault;
            CampEditManager.Instance.OnLayoutSaved += CampEditManager_OnLayoutSaved;
            CampEditManager.Instance.OnStructureAdded += CampEditManager_OnStructureAdded;
            CampEditManager.Instance.OnStructureRemovedIndividually += CampEditManager_OnStructureRemoved;
            CampEditManager.Instance.OnStructurePickedUp += CampEditManager_OnStructurePickedUp;
            CampEditManager.Instance.OnStructureDroppedMoving += CampEditManager_OnStructureDropped;
            CampEditManager.Instance.OnAllStructuresRemoved += CampEditManager_OnAllStructuresRemoved;
        }

        StructureBlueprint.OnAnyBlueprintWithStructureHovered += StructureBlueprint_OnAnyBlueprintWithStructureHovered;
        GridVisualUnit.OnAnyGridWithoutStructureHovered += GridVisualUnit_OnAnyGidWithoutStructureHovered;
        GridVisualUnit.OnAnyGridHoveredWhileMovingBlueprint += GridVisualUnit_OnAnyGridHoveredWhileMovingBlueprint;

        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        StructureLocation.OnAnyStructureSOToBuildChanged += StructureLocation_OnAnyStructureSOToBuildChanged;

        Structure.OnAnyStructureUpgraded += Structure_OnAnyStructureUpgraded;
        Structure.OnAnyStructurePrimaryFunctionUsed += Structure_OnAnyStructurePrimaryFunctionUsed;
        Obstacle.OnAnyObstacleBuilt += Obstacle_OnAnyObstacleBuilt;

        StructureUI_Fire.OnMainFireTickRemoved += StructureUI_Fire_OnFireTickRemoved;
        StructureUI_Fire.OnMainCricitalFireTickRemoved += StructureUI_Fire_OnCricitalFireTickRemoved;
        MenuButton.OnAnyMenuButtonHovered += MenuButton_OnAnyMenuButtonHovered;
        MenuButton.OnAnyMenuButtonPressed += MenuButton_OnAnyMenuButtonPressed;

        ButtonUI.OnAnyButtonPressed += ButtonUI_OnAnyButtonPressed;
        ItemButtonUI.OnAnyButtonSelected += ItemButtonUI_OnAnyButtonSelected;
        ItemButtonUI.OnAnyButtonHovered += ItemButtonUI_OnAnyButtonHovered;
        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemUpgraded += HubMerchantItem_OnAnyHubMerchantItemUpgraded;
        ItemButtonUI.OnAnyHubMerchantItemFailedBuy += ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
        ItemButtonUI_Visual.OnAnyGemPSTriggered += ItemButtonUI_Visual_OnAnyGemPSTriggered;
        ItemButtonUI.OnAnyLockedButtonTryPress += ItemButtonUI_OnAnyLockedButtonTryPress;
        ItemButtonUI.OnAnyHubMerchantItemTryBuyMaxedItem += ItemButtonUI_OnAnyHubMerchantItemTryBuyMaxedItem;

        ParticleCollision.OnAnyBulletHitEnemy += ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitGround += ParticleCollision_OnAnyBulletHitGround;
        ParticleCollision.OnAnyPlayerBulletHitEnemyCrit += ParticleCollision_OnAnyBulletHitEnemyCrit;
        ParticleCollision.OnAnyPlayerBulletHitEnemy += ParticleCollision_OnAnyPlayerBulletHitEnemy;
        ParticleCollision.OnAnyPlayerBulletHitGround += ParticleCollision_OnAnyPlayerBulletHitGround;
        ParticleCollision.OnAnyParticleBouncedOff += ParticleCollision_OnParticleBouncedOff;

        Collectible.OnAnyCollectibleTouchedFloor += Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByWorker += Collectible_OnAnyCollectiblePickedUpByWorker;
        Collectible.OnAnyCollectiblePlouffed += Collectible_OnAnyCollectiblePlouffed;
        LevelNPCGemReward.OnAnyCurrencyDropped += LevelNPCGemReward_OnAnyCurrencyDropped;
        Chest.OnAnyChestSpawnedCollectible += Chest_OnAnyChestSpawnedCollectible;
        Scavengable.OnAnyScavengableMarkedToScavenge += Scavengable_OnAnyScavengableMarkedToScavenge;
        ScavengableObstacle.OnAnyScavengableMarkedToScavenge += Scavengable_OnAnyScavengableMarkedToScavenge;
        CurrencyStorage.OnAnyCurrencySpawned += CurrencyStorage_OnAnyCurrencySpawned;

        Mob.OnAnyMobDamageTaken += Mob_OnAnyMobDamageTaken;
        WorkerAI.OnAnyWorkerFollowPlayerStarted += WorkerAI_OnAnyWorkerFollowPlayerStarted;
        WorkerAI.OnAnyWorkerFollowPlayerStopped += WorkerAI_OnAnyWorkerFollowPlayerStopped;
        WorkerFollowPlayerHandler.Instance.OnAllFollowingWorkersRemoved += WorkerFollowPlayerHandler_OnAllFollowingWorkersRemoved;
        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned += EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        HuntingFlag.OnAnyHuntingFlagReset += HuntingFlag_OnAnyHuntingFlagReset;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagNewPositionSet += HuntingFlag_PlayerDefined_OnAnyHuntingFlagNewPositionSet;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagPickedUp += HuntingFlag_PlayerDefined_OnAnyHuntingFlagPickedUp;
        GunSpotLight.OnAnyLightSwitched += GunSpotLight_OnAnyLightSwitched;
        Gun.OnAnyGunJammed += Gun_OnAnyGunJammed;
        Gun.OnAnyGunJamRepaired += Gun_OnAnyGunJamRepaired;

        Tutorial.OnAnySpotLightActivated += Tutorial_OnAnySpotLightActivated;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchantTalkUI.OnAnyMerchantShowNewTalkLine += HubMerchantTalkUI_OnAnyMerchantShowNewTalkLine;

        DogReplaceButton.OnDogSwapped += DogReplaceButton_OnDogSwapped;
    }

    private void Update() {
        if(criticalFireTickJustRemoved) {
            criticalFireTickRemovedTimer -= Time.deltaTime;
            if(criticalFireTickRemovedTimer < 0) {
                criticalFireTickJustRemoved = false;
            }
        }
    }

    private void SceneLoader_OnSceneFadeIn(object sender, System.EventArgs e) {
        StartCoroutine(FadeInVolume(1f));
    }

    private void SceneLoader_OnSceneFadeOut(object sender, SceneLoader.OnSceneFadeOutEventArgs e) {
        StartCoroutine(FadeOutAllSounds(e.fadeOutTime));
    }

    private IEnumerator FadeOutAllSounds(float fadeDuration) {
        float timer = 0f;
        float initialVolume = AudioListener.volume;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            AudioListener.volume = Mathf.Lerp(initialVolume, 0f, alpha); // Diminue le volume
            yield return null;
        }
    }

    private IEnumerator FadeInVolume(float duration) {
        float timer = 0f;
        AudioListener.volume = 0f; // Assure que le volume commence à 0

        while (timer < duration) {
            timer += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(0f, 1, timer / duration);
            yield return null;
        }

        AudioListener.volume = 1; // S'assure que le volume est bien à 1
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }


    #region UI

    private void ButtonUI_OnAnyButtonPressed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.pressMenuButton, .3f);
    }
    private void MenuButton_OnAnyMenuButtonPressed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.pressMenuButton, 1);
    }

    private void MenuButton_OnAnyMenuButtonHovered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hoverOrSelectMenuButton, 1);
    }

    private void LevelUI_OnLocationTextShown(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.locationRevealed, .5f);
    }
    private void LevelUI_OnDayUIShown(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.dayCountShown, .5f);
    }

    private void ItemButtonUI_OnAnyLockedButtonTryPress(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tryBuyLockedHubMerchantItem, .5f);
    }
    private void ItemButtonUI_Visual_OnAnyGemPSTriggered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.gemPSExplosion, .5f);
    }
    private void ItemButtonUI_OnAnyHubMerchantItemFailedBuy(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.failBuyHubMerchantItem);
    }

    private void ItemButtonUI_OnAnyHubMerchantItemTryBuyMaxedItem(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tryBuyMaxedHubMerchantItem);
    }
    private void CampEditManager_OnStructureDropped(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_StructureDropped, .75f);
    }

    private void StructureBlueprint_OnAnyBlueprintWithStructureHovered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_GridHoveredWithStructure);
    }

    private void GridVisualUnit_OnAnyGidWithoutStructureHovered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_GridHovered, .3f);
    }
    private void GridVisualUnit_OnAnyGridHoveredWhileMovingBlueprint(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_GridHoveredWhileMovingBlueprint, .2f);
    }

    private void CampEditManager_OnStructurePickedUp(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_StructurePickedUp, .75f);
    }

    private void CampEditManager_OnStructureRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_StructureRemoved, .3f);
    }

    private void CampEditManager_OnStructureAdded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_StructureAdded, .5f);
    }

    private void CampEditManager_OnLayoutSaved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_LayoutSaved, .15f);
    }
    private void CampEditManager_OnAllStructuresRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_AllStructuresRemoved, .15f);
    }

    private void CampEditManager_OnLayoutResetToDefault(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.campEdit_LayoutReset, .15f);
    }


    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.buyHubMerchantItem);
    }

    private void HubMerchantItem_OnAnyHubMerchantItemUpgraded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.upgradeHubMerchantItem, .5f);
    }

    private void ItemButtonUI_OnAnyButtonHovered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hoverOrSelectHubMerchantItem, .3f);
    }

    private void ItemButtonUI_OnAnyButtonSelected(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hoverOrSelectHubMerchantItem, .3f);
    }

    private void LevelUI_ObjectiveUI_OnObjectiveUIShown(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.objectiveShown, .5f);
    }
    private void LevelUI_OnSubObjectiveUICompleted(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.subObjectiveCompleted, .35f);
    }

    private void LevelUI_OnSubObjectiveUIProgressed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.subObjectiveProgressed, .35f);
    }

    private void LevelUI_OnObjectiveUICompleted(object sender, System.EventArgs e) {
        //PlaySound2D(soundRefsSO.objectiveCompleted, .5f);
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tooltipHidden, .7f);
    }

    private void VideoTipUI_OnVideoTipPanelOpened(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tooltipShown, .7f);
    }

    private void PlayerWorldUITooltup_OnTooltipShown(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tooltipShown, .7f);
    }

    private void PlayerWorldUITooltip_OnTooltipHidden(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.tooltipHidden, .7f);
    }
    private void PlayerUI_HPBar_OnHPTickAdded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hpTickAdded,.7f);
    }

    private void PlayerUI_AmmoBar_OnAmmoTickAdded(object sender, System.EventArgs e) {
        if(PlayerShoot.Instance.GetHeldGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound2D(soundRefsSO.ammoTickAdded, .7f);
        } else {
            PlaySound2D(soundRefsSO.ammoSpecialTickAdded, .7f);
        }
    }

    private void StructureUI_Fire_OnFireTickRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.fireTickRemoved, 1);
    }

    private void StructureUI_Fire_OnCricitalFireTickRemoved(object sender, System.EventArgs e) {
        if (criticalFireTickJustRemoved) return;

        criticalFireTickRemovedTimer = criticalFireTickRemovedMinDelay;
        criticalFireTickJustRemoved = true;

        PlaySound2D(soundRefsSO.criticalFireTickRemoved);
    }

    #endregion

    #region WORKERS

    private void WorkerFollowPlayerHandler_OnHoveredFollowingWorkerChanged(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hoveredFollowingWorkerChanged, 1f);
    }

    private void WorkerAI_OnAnyWorkerFollowPlayerStopped(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.workerStoppedFollowing, 1f);
    }
    private void WorkerFollowPlayerHandler_OnAllFollowingWorkersRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.workerStoppedFollowing, 1f);
    }

    private void WorkerAI_OnAnyWorkerFollowPlayerStarted(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.workerStartedFollowing, 1f);
    }
    private void Worker_OnAnyOrbDroppedByWorker(object sender, System.EventArgs e) {
        PlaySound3D(soundRefsSO.orbDroppedUpByWorker, (sender as MonoBehaviour).transform.position, .5f);
    }

    private void Worker_OnAnyWorkerRecruited(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.workerRecruited, .5f);
    }
    private void Worker_OnAnyWorkerAssignedHunter(object sender, System.EventArgs e) {
        //PlaySound2D(soundRefsSO.workerHunterJobAssigned);
    }

    private void Worker_OnAnyWorkerDied(object sender, System.EventArgs e) {
        PlaySound3D(soundRefsSO.workerDied, (sender as MonoBehaviour).transform.position);
    }

    #endregion

    #region CURRENCIES

    private void Collectible_OnAnyCollectibleTouchedFloor(object sender, System.EventArgs e) {
        Collectible collectible = (Collectible)sender;

        if(collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            PlaySound3D(soundRefsSO.bigBlueOrbTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            PlaySound3D(soundRefsSO.smallBlueOrbTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigRedOrb) {
            PlaySound3D(soundRefsSO.bigRedOrbTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallRedOrb) {
            PlaySound3D(soundRefsSO.smallRedOrbTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyCategory() == PlayerCurrencies.CurrencyCategory.gem) {
            PlaySound3D(soundRefsSO.gemTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyCategory() == PlayerCurrencies.CurrencyCategory.trap) {
            PlaySound3D(soundRefsSO.trapTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo || collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo_special) {
            PlaySound3D(soundRefsSO.ammoTouchedFloor, (sender as MonoBehaviour).transform.position, .7f);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            PlaySound3D(soundRefsSO.emberTouchedFloor, (sender as MonoBehaviour).transform.position, 1f);
        }
    }

    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyTypeCollected = e.currencyUIDropped.GetCurrencyType();
        SetCorrectCurrencySound(currencyTypeCollected);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyTypeCollected = e.currencyUIDropped.GetCurrencyType();
        SetCorrectCurrencySound(currencyTypeCollected);
    }

    private AudioClip[] SetCorrectCurrencySpawnSound(PlayerCurrencies.CurrencyType currencyTypeSpawned) {
        if (CurrenciesManager.Instance.GetCurrencyCategory(currencyTypeSpawned) == PlayerCurrencies.CurrencyCategory.gem) {
            return soundRefsSO.gemDropped;
        }
        if (CurrenciesManager.Instance.GetCurrencyCategory(currencyTypeSpawned) == PlayerCurrencies.CurrencyCategory.trap) {
            return soundRefsSO.trapPickedUpByPlayer;
        }
        if (currencyTypeSpawned == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            return soundRefsSO.bigBlueOrbDropped;
        }
        if (currencyTypeSpawned == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            return soundRefsSO.smallBlueOrbDropped;
        }
        if (currencyTypeSpawned == PlayerCurrencies.CurrencyType.bigRedOrb) {
            return soundRefsSO.bigRedOrbDropped;
        }
        if (currencyTypeSpawned == PlayerCurrencies.CurrencyType.smallRedOrb) {
            return soundRefsSO.smallRedOrbDropped;
        }

        if (currencyTypeSpawned == PlayerCurrencies.CurrencyType.ammo || currencyTypeSpawned == PlayerCurrencies.CurrencyType.ammo_special) {
            return soundRefsSO.ammoDropped;
        }

        return null;
    }

    private void SetCorrectCurrencySound(PlayerCurrencies.CurrencyType currencyTypeCollected) {

        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            PlaySound2D(soundRefsSO.bigBlueOrbPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            PlaySound2D(soundRefsSO.smallBlueOrbPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.bigRedOrb) {
            PlaySound2D(soundRefsSO.bigRedOrbPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.smallRedOrb) {
            PlaySound2D(soundRefsSO.smallRedOrbPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.cyanGem || currencyTypeCollected == PlayerCurrencies.CurrencyType.blueGem || currencyTypeCollected == PlayerCurrencies.CurrencyType.purpleGem) {
            PlaySound2D(soundRefsSO.greenGemPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.redGem || currencyTypeCollected == PlayerCurrencies.CurrencyType.yellowGem || currencyTypeCollected == PlayerCurrencies.CurrencyType.greenGem) {
            PlaySound2D(soundRefsSO.redGemPickedUpByPlayer, .7f);
        }
        if (CurrenciesManager.Instance.GetCurrencyCategory(currencyTypeCollected) == PlayerCurrencies.CurrencyCategory.trap) {
            PlaySound2D(soundRefsSO.trapPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound2D(soundRefsSO.ammoPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.ammo_special) {
            PlaySound2D(soundRefsSO.ammoSpecialPickedUpByPlayer);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.ember) {
            if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB && !initialEmberGiven) {
                initialEmberGiven = true;
                return;
            }

            PlaySound2D(soundRefsSO.emberPickedUpByPlayer, .7f);
        }
    }

    private void CurrencyStorage_OnAnyCurrencySpawned(object sender, CurrencyStorage.OnAnyCurrencySpawnedEventArgs e) {
        PlaySound3D(SetCorrectCurrencySpawnSound(e.currencyType), (sender as MonoBehaviour).transform.position);
    }

    private void LevelNPCGemReward_OnAnyCurrencyDropped(object sender, LevelNPCGemReward.OnAnyCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyTypeCollected = e.currencyType;
        SetCorrectCurrencySound(currencyTypeCollected);
    }

    private void Chest_OnAnyChestSpawnedCollectible(object sender, Chest.OnAnyChestSpawnedCollectibleEventArgs e) {
        PlaySound3D(SetCorrectCurrencySpawnSound(e.currencyType), (sender as MonoBehaviour).transform.position);
    }

    private void Collectible_OnAnyCollectiblePickedUpByWorker(object sender, System.EventArgs e) {
        PlaySound3D(soundRefsSO.orbPickedUpByWorker, (sender as MonoBehaviour).transform.position);
    }

    private void PlayerCurrencies_OnBlueOrbDroppedOnTheFloor(object sender, PlayerCurrencies.OnBlueOrbDroppedOnTheFloorEventArgs e) {
        PlaySound3D(soundRefsSO.bigBlueOrbDropped, (sender as MonoBehaviour).transform.position);
    }

    private void Collectible_OnAnyCollectiblePlouffed(object sender, Collectible.OnAnyCollectiblePouffedEventArgs e) {
        bool isBigCollectible = false;

        if (e.currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            isBigCollectible = true;
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.bigRedOrb) {
            isBigCollectible = true;
        }

        if(isBigCollectible) {
            PlaySound3D(soundRefsSO.bigCollectiblePlouf, (sender as MonoBehaviour).transform.position);
        } else {
            PlaySound3D(soundRefsSO.smallCollectiblePlouf, (sender as MonoBehaviour).transform.position);
        }

    }

    #endregion

    #region SHOOTING

    private void Mob_OnAnyMobDamageTaken(object sender, System.EventArgs e) {
        //Mob mob = sender as Mob;
        //Creature creature = mob as Creature;
        //if (creature == null) return;

        //PlaySound2D(creature.GetCreatureSO().bulletHitAudioClips, creature.GetCreatureSO().bulletHitVolumeMultiplier);
    }

    private void ParticleCollision_OnAnyPlayerBulletHitGround(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitGroundSound;
        PlaySound2D(audioClipArray, .5f);
    }
    private void ParticleCollision_OnParticleBouncedOff(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = soundRefsSO.bulletBoucedOff;
        PlaySound2D(audioClipArray, .5f);
    }

    private void ParticleCollision_OnAnyPlayerBulletHitEnemy(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        Creature creatureHit = e.mobHit as Creature;

        if (creatureHit == null) return;

        AudioClip[] audioClipArray = creatureHit.GetCreatureSO().bulletHitAudioClips;
        PlaySound2D(audioClipArray, creatureHit.GetCreatureSO().bulletHitVolumeMultiplier);
    }

    private void ParticleCollision_OnAnyBulletHitEnemyCrit(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitEnemyCritSound;
        PlaySound2D(audioClipArray, 1f);
    }


    private void ParticleCollision_OnAnyBulletHitGround(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitGroundSound;
        PlaySound3D(audioClipArray, e.bulletHitPosition, .5f);
    }

    private void ParticleCollision_OnAnyBulletHitEnemy(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        Creature creatureHit = e.mobHit as Creature;

        if (creatureHit == null) return;

        AudioClip[] audioClipArray = creatureHit.GetCreatureSO().bulletHitAudioClips;
        PlaySound3D(audioClipArray, e.bulletHitPosition, creatureHit.GetCreatureSO().bulletHitVolumeMultiplier);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if(!initialGunEquipped) {
            initialGunEquipped = true;
            return;
        }
        AudioClip audioClipArray = PlayerShoot.Instance.GetHeldGunSO().swapToWeaponSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().swapToWeaponVolumeMultiplier);
    }

    private void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().outOfAmmoSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().outOfAmmoVolumeMultiplier);
    }

    private void PlayerShoot_OnPlayerTryShoot_GunJammed(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().tryShootGunJammedSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().tryShootGunJammedVolumeMultiplier);
    }

    private void Gun_OnAnyGunJammed(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().gunJammedSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().gunJammedVolumeMultiplier);
    }
    private void Gun_OnAnyGunJamRepaired(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().gunJamRepairedSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().gunJamRepairedVolumeMultiplier);
    }

    private void PlayerShoor_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGunSO().cooldownGunSound.Length == 0) return;
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().cooldownGunSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().cooldownSFXVolumeMultiplier);
    }
    private void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if (!PlayerShoot.Instance.GetHeldGunSO().triggersShootSFXOnEachBuller) return;

        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().shootGunSound;
        float volume = PlayerShoot.Instance.GetHeldGunSO().shootGunVolumeMultiplier;
        PlaySound2D(audioClipArray, volume);
    }

    private void PlayerShoot_OnPlayerAimedSightEnded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.aimSightEnd);
    }

    private void PlayerShoot_OnPlayerAimedSightStarted(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.aimSightStart);
    }

    private void PlayerShoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.switchGunFireMode);
    }

    private void PlayerShoot_OnPlayerOverclockedSMGStarted(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.smgOverclockStart);
    }

    private void PlayerSHoot_OnPlayerOverclockedSMGStopped(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.smgOverclockEnd);
    }

    private void Player_OnPlayerFocusBlastStopped(object sender, System.EventArgs e) {
        gunPoweringUpAudioSource.Stop();
    }


    private void PlayerSHoot_OnPlayerSetupLMGStopped(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.lmgReset);
    }

    private void PlayerSHoot_OnPlayerEmptyRevolverMagEnd(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.revolverCooldown);
    }


    private void PlayerSHoot_OnPlayerSetupLMGStarted(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.lmgSetup);
    }

    private void PlayerShoot_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        gunPoweringUpAudioSource.PlayOneShot(soundRefsSO.shotgunFocusedBlast, sfxVolume);
    }

    #endregion

    #region PLAYER 

    private void Player_OnPlayerBackToTentToRespawn(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.playerRespawnFireExtact;
        PlaySound2D(audioClip);
    }

    private void ActiveTeleportation_OnPlayerTeleported(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.playerActiveTeleport;
        PlaySound2D(audioClip);
    }

    private void PlayerSkills_OnActiveSkillReady(object sender, System.EventArgs e) {
        AudioClip skillAudioClip = soundRefsSO.activeSkillReady;
        PlaySound2D(skillAudioClip, .5f);
    }
    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        SkillItem skillItem = e.skillItemAdded;
        AudioClip skillAudioClip = skillItem.GetSkillSO().activateSkillAudioClip;
        PlaySound2D(skillAudioClip);
    }

    private void PlayerSkills_OnPassiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        AudioClip skillAudioClip = soundRefsSO.passiveSkillAdded;
        PlaySound2D(skillAudioClip, 1f);
    }

    private void PassiveShield_OnAnyPassiveShieldDied(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.passiveShieldDie;
        PlaySound2D(audioClip);
    }

    private void PassiveShield_OnAnyPassiveShieldActivated(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.passiveShieldActivate;
        PlaySound2D(audioClip);
    }

    #endregion

    #region STRUCTURES

    private void Obstacle_OnAnyObstacleBuilt(object sender, System.EventArgs e) {
        AudioClip audioClip = (sender as Obstacle).GetObstacleBuiltAudioClip();
        PlaySound2D(audioClip);
    }

    private void Structure_OnAnyStructurePrimaryFunctionUsed(object sender, System.EventArgs e) {
        StructureSO structureSO = (sender as Structure).GetStructureSO();
        AudioClip audioClip = structureSO.useFunctionAudioClip;
        PlaySound2D(audioClip, structureSO.useFunctionVolumeMultiplier);
    }

    private void Structure_OnAnyStructureUpgraded(object sender, System.EventArgs e) {
        StructureSO structureSO = (sender as Structure).GetStructureSO();
        AudioClip audioClip = structureSO.upgradeAudioClip;
        PlaySound2D(audioClip, structureSO.upgradeVolumeMultiplier);
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, System.EventArgs e) {
        StructureSO structureSO = (sender as StructureLocation).GetStructureSOToBuild();
        AudioClip audioClip = structureSO.buildAudioClip;
        PlaySound2D(audioClip, structureSO.buildVolumeMultiplier);
    }
    private void StructureLocation_OnAnyStructureSOToBuildChanged(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.structureTypeToBuildChanged;
        PlaySound2D(audioClip, 1f);
    }


    #endregion

    #region PROPS
    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hubChestOpen, .6f);
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hubChestClose,.8f) ;
    }

    private void Scavengable_OnAnyScavengableMarkedToScavenge(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.scavengableMarkedToScavenge, .6f);
    }

    private void HuntingFlag_PlayerDefined_OnAnyHuntingFlagPickedUp(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.huntingFlagPickedUp);
    }

    private void HuntingFlag_PlayerDefined_OnAnyHuntingFlagNewPositionSet(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.huntingFlagDropped);
    }

    private void HuntingFlag_OnAnyHuntingFlagReset(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.huntingFlagReset);
    }

    private void EndLevelAreaProp_OnAnyEndLevelAreaPropBurned(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = soundRefsSO.propBurned;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }

    #endregion

    #region LIGHT
    private void Tutorial_OnAnySpotLightActivated(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.gunLightSwitch);
    }

    private void GunSpotLight_OnAnyLightSwitched(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.gunLightSwitch);
    }

    #endregion

    #region OTHER

    private void HubMerchantTalkUI_OnAnyMerchantShowNewTalkLine(object sender, System.EventArgs e) {
        HubMerchantTalkUI hubMerchant = (HubMerchantTalkUI)sender;
        HubMerchant.HubMerchantType merchantType = hubMerchant.GetHubMerchantType();
        if (merchantType == HubMerchant.HubMerchantType.GemMerchant) {
            PlaySound2D(soundRefsSO.gemMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.GunMerchant) {
            PlaySound2D(soundRefsSO.gunMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.StructuresMerchant) {
            PlaySound2D(soundRefsSO.structuresMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.DogTamer) {
            PlaySound2D(soundRefsSO.dogTamerVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.WorkerMerchant) {
            PlaySound2D(soundRefsSO.workerMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.Codex) {
            PlaySound2D(soundRefsSO.codexVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.MushroomMerchant) {
            PlaySound2D(soundRefsSO.muhsroomMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.HeroMerchant) {
            PlaySound2D(soundRefsSO.heroMerchantVoiceLines);
        }
    }

    private void HubMerchant_OnPlayerInteractedWithAnyHubMerchant(object sender, System.EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;
        HubMerchant.HubMerchantType merchantType = hubMerchant.GetHubMerchantType();

        if(merchantType == HubMerchant.HubMerchantType.GemMerchant) {
            PlaySound2D(soundRefsSO.gemMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.GunMerchant) {
            PlaySound2D(soundRefsSO.gunMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.StructuresMerchant) {
            PlaySound2D(soundRefsSO.structuresMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.DogTamer) {
            PlaySound2D(soundRefsSO.dogTamerVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.WorkerMerchant) {
            PlaySound2D(soundRefsSO.workerMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.Codex) {
            PlaySound2D(soundRefsSO.codexVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.MushroomMerchant) {
            PlaySound2D(soundRefsSO.muhsroomMerchantVoiceLines);
        }
        if (merchantType == HubMerchant.HubMerchantType.HeroMerchant) {
            PlaySound2D(soundRefsSO.heroMerchantVoiceLines);
        }
    }

    private void Dog_OnPlayerCalledDog(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.playerCallDog, .6f);
    }

    private void DogReplaceButton_OnDogSwapped(object sender, System.EventArgs e) {
        AudioClip audioClip = soundRefsSO.germanShepherdSelected;

        if (Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            audioClip = soundRefsSO.retreiverSelected;
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            audioClip = soundRefsSO.darkCompanionSelected;
        }
        PlaySound2D(audioClip, .6f);
    }

    #endregion

    #region PLAY SOUNDS

    private void PlaySound3D(AudioClip[] audioClipArray, Vector3 position, float volume = 1f) {
        if (audioClipArray.Length == 0) return;
        PlaySound3D(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound3D(AudioClip audioClip, Vector3 position, float volume = 1f) {
        Vector3 newPosition = new Vector3(position.x, position.y, Camera.main.transform.position.z);

        AudioSource.PlayClipAtPoint(audioClip, newPosition, volume * sfxVolume);
    }

    private void PlaySound2D(AudioClip[] audioClipArray, float volume = 1f) {
        if (audioClipArray.Length == 0) return;


        if (audioClipArray.Length == 1) {
            float originalPitch = audioSource2D.pitch;
            float newPitch = Random.Range(0.9f, 1.1f); // Pitch légèrement aléatoire

            audioSource2D.pitch = newPitch;
            PlaySound2D(audioClipArray[0], volume);
            audioSource2D.pitch = originalPitch; // On remet le pitch à la normale

        }
        else {
            AudioClip audioClip = audioClipArray[Random.Range(0, audioClipArray.Length)];
            PlaySound2D(audioClip, volume);
        }

    }

    private void PlaySound2D(AudioClip audioClip, float volume = 1f) {
        audioSource2D.PlayOneShot(audioClip, volume * sfxVolume);
    }

    private IEnumerator PlaySound2DAfterDelay(float delay, AudioClip audioClip, float volume = 1f) {
        yield return new WaitForSeconds(delay);
        audioSource2D.PlayOneShot(audioClip, volume * sfxVolume);
    }

    #endregion

    private bool TestPlaySound(float probabilityToPlaySound) {
        float randomFloat = Random.Range(0f, 1f);

        if(probabilityToPlaySound > randomFloat) {
            return true;
        } else { return false; }
    }

    private void OnDestroy() {
        SettingsManager.Instance.OnSfxVolumeChanged -= SettingsManager_OnSfxVolumeChanged;

        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        StructureLocation.OnAnyStructureSOToBuildChanged -= StructureLocation_OnAnyStructureSOToBuildChanged;
        Structure.OnAnyStructureUpgraded -= Structure_OnAnyStructureUpgraded;
        Structure.OnAnyStructurePrimaryFunctionUsed -= Structure_OnAnyStructurePrimaryFunctionUsed;
        Obstacle.OnAnyObstacleBuilt -= Obstacle_OnAnyObstacleBuilt;

        if (Player.Instance != null) {
            PlayerShoot.Instance.OnPlayerStartedShot -= PlayerShoot_OnPlayerStartedShot;
            PlayerShoot.Instance.OnPlayerCooldownTrigger -= PlayerShoor_OnPlayerCooldownSFXTrigger;
            PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo -= PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
            PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;

            PlayerShoot.Instance.OnPlayerSwitchedFireMode -= PlayerShoot_OnPlayerSwitchedFireMode;
            PlayerShoot.Instance.OnPlayerAimedSightStarted -= PlayerShoot_OnPlayerAimedSightStarted;
            PlayerShoot.Instance.OnPlayerAimedSightEnded -= PlayerShoot_OnPlayerAimedSightEnded;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStopped -= PlayerSHoot_OnPlayerOverclockedSMGStopped;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStarted -= PlayerShoot_OnPlayerOverclockedSMGStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStarted -= PlayerShoot_OnPlayerFocusBlastStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStopped -= Player_OnPlayerFocusBlastStopped;
            PlayerShoot.Instance.OnPlayerEmptyRevolverMagEnd -= PlayerSHoot_OnPlayerEmptyRevolverMagEnd;

            PlayerShoot.Instance.OnPlayerSetupLMGStarted -= PlayerSHoot_OnPlayerSetupLMGStarted;
            PlayerShoot.Instance.OnPlayerSetupLMGStopped -= PlayerSHoot_OnPlayerSetupLMGStopped;

            PlayerSkills.Instance.OnActiveSkillReady -= PlayerSkills_OnActiveSkillReady;
            PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
            PassiveShield.OnAnyPassiveShieldActivated -= PassiveShield_OnAnyPassiveShieldActivated;
            PassiveShield.OnAnyPassiveShieldDied -= PassiveShield_OnAnyPassiveShieldDied;
            ActiveTeleportation.Instance.OnPlayerTeleported -= ActiveTeleportation_OnPlayerTeleported;
            PlayerUI_AmmoBar.Instance.OnAmmoTickAdded -= PlayerUI_AmmoBar_OnAmmoTickAdded;
            PlayerUI_HPBar.Instance.OnHPTickAdded -= PlayerUI_HPBar_OnHPTickAdded;
            PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor -= PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;

            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
            Dog.Instance.OnPlayerCalledDog -= Dog_OnPlayerCalledDog;
        }

        if (LevelUI_ObjectiveUI.Instance != null) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveUIShown -= LevelUI_ObjectiveUI_OnObjectiveUIShown;
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted -= LevelUI_OnObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUICompleted -= LevelUI_OnSubObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUIProgressed -= LevelUI_OnSubObjectiveUIProgressed;
        }
        if (LevelUI_Locations.Instance != null) {
            LevelUI_Locations.Instance.OnLocationTextShown -= LevelUI_OnLocationTextShown;
        }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            WorkerFollowPlayerHandler.Instance.OnHoveredFollowingWorkerChanged -= WorkerFollowPlayerHandler_OnHoveredFollowingWorkerChanged;
        }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected -= HubInventoryUI_OnCurrencyCollected;
        }
        if (VideoTipUI.Instance != null) {
            VideoTipUI.Instance.OnVideoTipPanelOpened -= VideoTipUI_OnVideoTipPanelOpened;
            VideoTipUI.Instance.OnVideoTipPanelClosed -= VideoTipUI_OnVideoTipPanelClosed;
        }

        StructureUI_Fire.OnMainFireTickRemoved -= StructureUI_Fire_OnFireTickRemoved;
        StructureUI_Fire.OnMainCricitalFireTickRemoved -= StructureUI_Fire_OnCricitalFireTickRemoved;
        PlayerWorldUITooltip.OnTooltipHidden -= PlayerWorldUITooltip_OnTooltipHidden;
        PlayerWorldUITooltip.OnTooltipShown -= PlayerWorldUITooltup_OnTooltipShown;
        ButtonUI.OnAnyButtonPressed -= ButtonUI_OnAnyButtonPressed;
        ItemButtonUI.OnAnyButtonSelected -= ItemButtonUI_OnAnyButtonSelected;
        ItemButtonUI.OnAnyButtonHovered -= ItemButtonUI_OnAnyButtonHovered;
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemUpgraded -= HubMerchantItem_OnAnyHubMerchantItemUpgraded;
        ItemButtonUI.OnAnyHubMerchantItemFailedBuy -= ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
        ItemButtonUI_Visual.OnAnyGemPSTriggered -= ItemButtonUI_Visual_OnAnyGemPSTriggered;
        ItemButtonUI.OnAnyLockedButtonTryPress -= ItemButtonUI_OnAnyLockedButtonTryPress;
        MenuButton.OnAnyMenuButtonHovered -= MenuButton_OnAnyMenuButtonHovered;
        MenuButton.OnAnyMenuButtonPressed -= MenuButton_OnAnyMenuButtonPressed;
        StructureBlueprint.OnAnyBlueprintWithStructureHovered -= StructureBlueprint_OnAnyBlueprintWithStructureHovered;
        GridVisualUnit.OnAnyGridWithoutStructureHovered -= GridVisualUnit_OnAnyGidWithoutStructureHovered;


        ParticleCollision.OnAnyBulletHitEnemy -= ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyPlayerBulletHitEnemyCrit -= ParticleCollision_OnAnyBulletHitEnemyCrit;
        ParticleCollision.OnAnyBulletHitGround -= ParticleCollision_OnAnyBulletHitGround;
        ParticleCollision.OnAnyPlayerBulletHitEnemy -= ParticleCollision_OnAnyPlayerBulletHitEnemy;
        ParticleCollision.OnAnyPlayerBulletHitGround -= ParticleCollision_OnAnyPlayerBulletHitGround;
        ParticleCollision.OnAnyParticleBouncedOff -= ParticleCollision_OnParticleBouncedOff;

        Collectible.OnAnyCollectibleTouchedFloor -= Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByWorker -= Collectible_OnAnyCollectiblePickedUpByWorker;
        Collectible.OnAnyCollectiblePlouffed -= Collectible_OnAnyCollectiblePlouffed;
        Chest.OnAnyChestSpawnedCollectible -= Chest_OnAnyChestSpawnedCollectible;
        LevelNPCGemReward.OnAnyCurrencyDropped -= LevelNPCGemReward_OnAnyCurrencyDropped;
        Scavengable.OnAnyScavengableMarkedToScavenge -= Scavengable_OnAnyScavengableMarkedToScavenge;
        ScavengableObstacle.OnAnyScavengableMarkedToScavenge -= Scavengable_OnAnyScavengableMarkedToScavenge;
        CurrencyStorage.OnAnyCurrencySpawned -= CurrencyStorage_OnAnyCurrencySpawned;

        Mob.OnAnyMobDamageTaken -= Mob_OnAnyMobDamageTaken;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;
        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned -= EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        HuntingFlag.OnAnyHuntingFlagReset -= HuntingFlag_OnAnyHuntingFlagReset;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagNewPositionSet -= HuntingFlag_PlayerDefined_OnAnyHuntingFlagNewPositionSet;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagPickedUp -= HuntingFlag_PlayerDefined_OnAnyHuntingFlagPickedUp;
        GunSpotLight.OnAnyLightSwitched -= GunSpotLight_OnAnyLightSwitched;
        Gun.OnAnyGunJammed -= Gun_OnAnyGunJammed;
        Gun.OnAnyGunJamRepaired -= Gun_OnAnyGunJamRepaired;

        Tutorial.OnAnySpotLightActivated -= Tutorial_OnAnySpotLightActivated;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchantTalkUI.OnAnyMerchantShowNewTalkLine -= HubMerchantTalkUI_OnAnyMerchantShowNewTalkLine;
        DogReplaceButton.OnDogSwapped -= DogReplaceButton_OnDogSwapped;
    }

}
