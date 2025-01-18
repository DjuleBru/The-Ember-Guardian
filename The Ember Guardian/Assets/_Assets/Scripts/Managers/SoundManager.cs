using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundRefsSO soundRefsSO;
    [SerializeField] private AudioSource gunPoweringUpAudioSource;
    
    private AudioSource audioSource2D;
    private float sfxVolume;

    private bool initialEmberGiven;
    private bool initialGunEquipped;

    private void Awake() {
        Instance = this;
        audioSource2D = GetComponent<AudioSource>();
        audioSource2D.spatialBlend = 0;
        audioSource2D.ignoreListenerPause = true;
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;

        if (Player.Instance != null) {
            PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
            PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
            PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoor_OnPlayerCooldownSFXTrigger;
            PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
            PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
            PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;
            PlayerShoot.Instance.OnPlayerAimedSightStarted += PlayerShoot_OnPlayerAimedSightStarted;
            PlayerShoot.Instance.OnPlayerAimedSightEnded += PlayerShoot_OnPlayerAimedSightEnded;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStopped += PlayerSHoot_OnPlayerOverclockedSMGStopped;
            PlayerShoot.Instance.OnPlayerOverclockedSMGStarted += PlayerShoot_OnPlayerOverclockedSMGStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerShoot_OnPlayerFocusBlastStarted;
            PlayerShoot.Instance.OnPlayerFocusBlastStopped += Player_OnPlayerFocusBlastStopped;

            PlayerSkills.Instance.OnActiveSkillReady += PlayerSkills_OnActiveSkillReady;
            PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;


            PassiveShield.OnAnyPassiveShieldActivated += PassiveShield_OnAnyPassiveShieldActivated;
            PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;
            ActiveTeleportation.Instance.OnPlayerTeleported += ActiveTeleportation_OnPlayerTeleported;
            PlayerUI_AmmoBar.Instance.OnAmmoTickAdded += PlayerUI_AmmoBar_OnAmmoTickAdded;
            PlayerUI_HPBar.Instance.OnHPTickAdded += PlayerUI_HPBar_OnHPTickAdded;
            PlayerWorldUITooltip.OnTooltipHidden += PlayerWorldUITooltip_OnTooltipHidden;
            PlayerWorldUITooltip.OnTooltipShown += PlayerWorldUITooltup_OnTooltipShown;

            UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
            PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
            Dog.Instance.OnPlayerCalledDog += Dog_OnPlayerCalledDog;
        }

        if (LevelUI_ObjectiveUI.Instance != null) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveUIShown += LevelUI_ObjectiveUI_OnObjectiveUIShown;
            LevelUI_ObjectiveUI.Instance.OnObjectiveUICompleted += LevelUI_OnObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUICompleted += LevelUI_OnSubObjectiveUICompleted;
        }
        if (LevelUI_Locations.Instance != null) {
            LevelUI_Locations.Instance.OnLocationTextShown += LevelUI_OnLocationTextShown;
        }

        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructureUpgraded += Structure_OnAnyStructureUpgraded;
        Structure.OnAnyStructurePrimaryFunctionUsed += Structure_OnAnyStructurePrimaryFunctionUsed;
        Obstacle.OnAnyObstacleBuilt += Obstacle_OnAnyObstacleBuilt;

        StructureUI_Fire.OnFireTickRemoved += StructureUI_Fire_OnFireTickRemoved;
        StructureUI_Fire.OnCricitalFireTickRemoved += StructureUI_Fire_OnCricitalFireTickRemoved;
        MenuButton.OnAnyMenuButtonHovered += MenuButton_OnAnyMenuButtonHovered;
        MenuButton.OnAnyMenuButtonPressed += MenuButton_OnAnyMenuButtonPressed;

        ItemButtonUI.OnAnyButtonSelected += ItemButtonUI_OnAnyButtonSelected;
        ItemButtonUI.OnAnyButtonHovered += ItemButtonUI_OnAnyButtonHovered;
        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemUpgraded += HubMerchantItem_OnAnyHubMerchantItemUpgraded;
        ItemButtonUI.OnAnyHubMerchantItemFailedBuy += ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
        ItemButtonUI_Visual.OnAnyGemPSTriggered += ItemButtonUI_Visual_OnAnyGemPSTriggered;
        ItemButtonUI.OnAnyLockedButtonTryPress += ItemButtonUI_OnAnyLockedButtonTryPress;
        ItemButtonUI.OnAnyHubMerchantItemTryBuyMaxedItem += ItemButtonUI_OnAnyHubMerchantItemTryBuyMaxedItem;

        ParticleCollision.OnAnyBulletHitEnemy += ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitEnemyCrit += ParticleCollision_OnAnyBulletHitEnemyCrit;
        ParticleCollision.OnAnyBulletHitGround += ParticleCollision_OnAnyBulletHitGround;

        Collectible.OnAnyCollectibleTouchedFloor += Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByWorker += Collectible_OnAnyCollectiblePickedUpByWorker;
        Collectible.OnAnyCollectiblePlouffed += Collectible_OnAnyCollectiblePlouffed; ;
        Chest.OnAnyChestSpawnedCollectible += Chest_OnAnyChestSpawnedCollectible;

        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;

        Projectile.OnAnyProjectileHit += Projectile_OnAnyProjectileHit;
        Projectile.OnAnyProjectileInstantiated += Projectile_OnAnyProjectileInstantiated;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned += EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        HuntingFlag.OnAnyHuntingFlagReset += HuntingFlag_OnAnyHuntingFlagReset;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagNewPositionSet += HuntingFlag_PlayerDefined_OnAnyHuntingFlagNewPositionSet;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagPickedUp += HuntingFlag_PlayerDefined_OnAnyHuntingFlagPickedUp;
        GunSpotLight.OnAnyLightSwitched += GunSpotLight_OnAnyLightSwitched;

        Tutorial.OnAnySpotLightActivated += Tutorial_OnAnySpotLightActivated;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchantTalkUI.OnAnyMerchantShowNewTalkLine += HubMerchantTalkUI_OnAnyMerchantShowNewTalkLine;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }


    #region UI

    private void MenuButton_OnAnyMenuButtonPressed(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.pressMenuButton, 1);
    }

    private void MenuButton_OnAnyMenuButtonHovered(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hoverOrSelectMenuButton, 1);
    }

    private void LevelUI_OnLocationTextShown(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.locationRevealed, .5f);
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

    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.buyHubMerchantItem);
    }

    private void HubMerchantItem_OnAnyHubMerchantItemUpgraded(object sender, System.EventArgs e) {
        Debug.Log(soundRefsSO.upgradeHubMerchantItem);
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

    private void LevelUI_OnObjectiveUICompleted(object sender, System.EventArgs e) {
        //PlaySound2D(soundRefsSO.objectiveCompleted, .5f);
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
        PlaySound2D(soundRefsSO.ammoTickAdded,.7f);
    }

    private void StructureUI_Fire_OnFireTickRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.fireTickRemoved, 1);
    }

    private void StructureUI_Fire_OnCricitalFireTickRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.criticalFireTickRemoved);
    }

    #endregion

    #region Attacks

    private void Projectile_OnAnyProjectileInstantiated(object sender, System.EventArgs e) {
        Projectile projectile = (Projectile)sender;
        PlaySound3D(projectile.GetProjectileSO().projectileInstantiatedAudioClips, (sender as MonoBehaviour).transform.position);
    }

    private void Projectile_OnAnyProjectileHit(object sender, System.EventArgs e) {
        Projectile projectile = (Projectile)sender;
        PlaySound3D(projectile.GetProjectileSO().projectileHitAudioClips, (sender as MonoBehaviour).transform.position);
    }

    #endregion

    #region WORKERS

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
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.greenGem) {
            PlaySound3D(soundRefsSO.gemTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.redGem) {
            PlaySound3D(soundRefsSO.gemTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound3D(soundRefsSO.ammoTouchedFloor, (sender as MonoBehaviour).transform.position, .7f);
        }
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyTypeCollected = e.currencyUIDropped.GetCurrencyType();

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
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.greenGem) {
            PlaySound2D(soundRefsSO.greenGemPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.redGem) {
            PlaySound2D(soundRefsSO.redGemPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound2D(soundRefsSO.ammoPickedUpByPlayer, .7f);
        }
        if (currencyTypeCollected == PlayerCurrencies.CurrencyType.ember) {
            if(!initialEmberGiven) {
                initialEmberGiven = true;
                return;
            }
            PlaySound2D(soundRefsSO.emberPickedUpByPlayer, .7f);
        }
    }

    private void Chest_OnAnyChestSpawnedCollectible(object sender, Chest.OnAnyChestSpawnedCollectibleEventArgs e) {
        if (e.currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            PlaySound3D(soundRefsSO.bigBlueOrbDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            PlaySound3D(soundRefsSO.smallBlueOrbDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.bigRedOrb) {
            PlaySound3D(soundRefsSO.bigRedOrbDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.smallRedOrb) {
            PlaySound3D(soundRefsSO.smallRedOrbDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.greenGem) {
            PlaySound3D(soundRefsSO.gemDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.redGem) {
            PlaySound3D(soundRefsSO.gemDropped, (sender as MonoBehaviour).transform.position);
        }
        if (e.currencyType == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound3D(soundRefsSO.ammoDropped, (sender as MonoBehaviour).transform.position);
        }
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

    private void ParticleCollision_OnAnyBulletHitEnemyCrit(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitEnemyCritSound;
        PlaySound2D(audioClipArray,  1f);
    }
    private void ParticleCollision_OnAnyBulletHitGround(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitGroundSound;
        PlaySound2D(audioClipArray, .5f);
    }

    private void ParticleCollision_OnAnyBulletHitEnemy(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().bulletHitEnemySound;
        PlaySound2D(audioClipArray,  .5f);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if(!initialGunEquipped) {
            initialGunEquipped = true;
            return;
        }
        AudioClip audioClipArray = PlayerShoot.Instance.GetHeldGunSO().swapToWeaponSound;
        PlaySound2D(audioClipArray);
    }

    private void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().outOfAmmoSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().outOfAmmoVolumeMultiplier);
    }

    private void PlayerShoor_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGunSO().cooldownGunSound.Length == 0) return;
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().cooldownGunSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().cooldownSFXVolumeMultiplier);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetHeldGunSO().reloadGunSound;
        PlaySound2D(audioClipArray, PlayerShoot.Instance.GetHeldGunSO().reloadSFXVolumeMultiplier);
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
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

    private void PlayerShoot_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        gunPoweringUpAudioSource.PlayOneShot(soundRefsSO.shotgunFocusedBlast, sfxVolume);
    }

    #endregion

    #region PLAYER SKILLS

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
        AudioClip audioClip = (sender as Structure).GetStructureSO().useFunctionAudioClip;
        PlaySound2D(audioClip);
    }

    private void Structure_OnAnyStructureUpgraded(object sender, System.EventArgs e) {
        AudioClip audioClip = (sender as Structure).GetStructureSO().upgradeAudioClip;
        PlaySound2D(audioClip);
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, System.EventArgs e) {
        AudioClip audioClip = (sender as StructureLocation).GetStructureSOToBuild().buildAudioClip;
        PlaySound2D(audioClip);
    }

    #endregion

    #region PROPS
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
    #endregion

    #region PLAY SOUNDS

    private void PlaySound3D(AudioClip[] audioClipArray, Vector3 position, float volume = 1f) {
        PlaySound3D(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound3D(AudioClip audioClip, Vector3 position, float volume = 1f) {
        Vector3 newPosition = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        AudioSource.PlayClipAtPoint(audioClip, newPosition, volume * sfxVolume);
    }

    private void PlaySound2D(AudioClip[] audioClipArray, float volume = 1f) {
        AudioClip audioClip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        PlaySound2D(audioClip, volume);
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
        Structure.OnAnyStructureUpgraded -= Structure_OnAnyStructureUpgraded;
        Structure.OnAnyStructurePrimaryFunctionUsed -= Structure_OnAnyStructurePrimaryFunctionUsed;
        Obstacle.OnAnyObstacleBuilt -= Obstacle_OnAnyObstacleBuilt;

        if (Player.Instance != null) {
            PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShot;
            PlayerShoot.Instance.OnPlayerReload -= PlayerShoot_OnPlayerReload;
            PlayerShoot.Instance.OnPlayerCooldownTrigger -= PlayerShoor_OnPlayerCooldownSFXTrigger;
            PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo -= PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
            PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;

            PlayerSkills.Instance.OnActiveSkillReady -= PlayerSkills_OnActiveSkillReady;
            PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
            PassiveShield.OnAnyPassiveShieldActivated -= PassiveShield_OnAnyPassiveShieldActivated;
            PassiveShield.OnAnyPassiveShieldDied -= PassiveShield_OnAnyPassiveShieldDied;
            ActiveTeleportation.Instance.OnPlayerTeleported -= ActiveTeleportation_OnPlayerTeleported;
            PlayerUI_AmmoBar.Instance.OnAmmoTickAdded -= PlayerUI_AmmoBar_OnAmmoTickAdded;
            PlayerUI_HPBar.Instance.OnHPTickAdded -= PlayerUI_HPBar_OnHPTickAdded;
            PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor -= PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;

            UICurrencyManager.Instance.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
            Dog.Instance.OnPlayerCalledDog -= Dog_OnPlayerCalledDog;
        }

        if (LevelUI_ObjectiveUI.Instance != null) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveUIShown -= LevelUI_ObjectiveUI_OnObjectiveUIShown;
            LevelUI_ObjectiveUI.Instance.OnObjectiveUICompleted -= LevelUI_OnObjectiveUICompleted;
            LevelUI_ObjectiveUI.Instance.OnSubObjectiveUICompleted -= LevelUI_OnSubObjectiveUICompleted;
        }
        if (LevelUI_Locations.Instance != null) {
            LevelUI_Locations.Instance.OnLocationTextShown -= LevelUI_OnLocationTextShown;
        }

        StructureUI_Fire.OnFireTickRemoved -= StructureUI_Fire_OnFireTickRemoved;
        StructureUI_Fire.OnCricitalFireTickRemoved -= StructureUI_Fire_OnCricitalFireTickRemoved;
        PlayerWorldUITooltip.OnTooltipHidden -= PlayerWorldUITooltip_OnTooltipHidden;
        PlayerWorldUITooltip.OnTooltipShown -= PlayerWorldUITooltup_OnTooltipShown;
        ItemButtonUI.OnAnyButtonSelected -= ItemButtonUI_OnAnyButtonSelected;
        ItemButtonUI.OnAnyButtonHovered -= ItemButtonUI_OnAnyButtonHovered;
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemUpgraded -= HubMerchantItem_OnAnyHubMerchantItemUpgraded;
        ItemButtonUI.OnAnyHubMerchantItemFailedBuy -= ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
        ItemButtonUI_Visual.OnAnyGemPSTriggered -= ItemButtonUI_Visual_OnAnyGemPSTriggered;
        ItemButtonUI.OnAnyLockedButtonTryPress -= ItemButtonUI_OnAnyLockedButtonTryPress;
        MenuButton.OnAnyMenuButtonHovered -= MenuButton_OnAnyMenuButtonHovered;
        MenuButton.OnAnyMenuButtonPressed -= MenuButton_OnAnyMenuButtonPressed;


        ParticleCollision.OnAnyBulletHitEnemy -= ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitEnemyCrit -= ParticleCollision_OnAnyBulletHitEnemyCrit;
        ParticleCollision.OnAnyBulletHitGround -= ParticleCollision_OnAnyBulletHitGround;

        Collectible.OnAnyCollectibleTouchedFloor -= Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByWorker -= Collectible_OnAnyCollectiblePickedUpByWorker;
        Collectible.OnAnyCollectiblePlouffed -= Collectible_OnAnyCollectiblePlouffed; ;
        Chest.OnAnyChestSpawnedCollectible -= Chest_OnAnyChestSpawnedCollectible;

        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;

        Projectile.OnAnyProjectileHit -= Projectile_OnAnyProjectileHit;
        Projectile.OnAnyProjectileInstantiated -= Projectile_OnAnyProjectileInstantiated;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned -= EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        HuntingFlag.OnAnyHuntingFlagReset -= HuntingFlag_OnAnyHuntingFlagReset;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagNewPositionSet -= HuntingFlag_PlayerDefined_OnAnyHuntingFlagNewPositionSet;
        HuntingFlag_PlayerDefined.OnAnyHuntingFlagPickedUp -= HuntingFlag_PlayerDefined_OnAnyHuntingFlagPickedUp;
        GunSpotLight.OnAnyLightSwitched -= GunSpotLight_OnAnyLightSwitched;

        Tutorial.OnAnySpotLightActivated -= Tutorial_OnAnySpotLightActivated;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchantTalkUI.OnAnyMerchantShowNewTalkLine -= HubMerchantTalkUI_OnAnyMerchantShowNewTalkLine;
    }

}
