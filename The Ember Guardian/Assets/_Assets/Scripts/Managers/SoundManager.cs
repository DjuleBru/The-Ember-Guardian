using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundRefsSO soundRefsSO;
    
    private AudioSource audioSource2D;
    private float sfxVolume;

    private void Awake() {
        Instance = this;
        audioSource2D = GetComponent<AudioSource>();
        audioSource2D.spatialBlend = 0;
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;

        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructureUpgraded += Structure_OnAnyStructureUpgraded;
        Structure.OnAnyStructurePrimaryFunctionUsed += Structure_OnAnyStructurePrimaryFunctionUsed;

        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoor_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;

        PlayerSkills.Instance.OnActiveSkillReady += PlayerSkills_OnActiveSkillReady;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PassiveShield.OnAnyPassiveShieldActivated += PassiveShield_OnAnyPassiveShieldActivated;
        PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;
        ActiveTeleportation.Instance.OnPlayerTeleported += ActiveTeleportation_OnPlayerTeleported;

        PlayerUI_AmmoBar.Instance.OnAmmoTickAdded += PlayerUI_AmmoBar_OnAmmoTickAdded;
        PlayerUI_HPBar.Instance.OnHPTickAdded += PlayerUI_HPBar_OnHPTickAdded;
        StructureUI_Fire.OnFireTickRemoved += StructureUI_Fire_OnFireTickRemoved;

        ParticleCollision.OnAnyBulletHitEnemy += ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitGround += ParticleCollision_OnAnyBulletHitGround;

        Collectible.OnAnyCollectibleTouchedFloor += Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByPlayer += Collectible_OnAnyCollectiblePickedUpByPlayer;
        Collectible.OnAnyCollectiblePickedUpByWorker += Collectible_OnAnyCollectiblePickedUpByWorker;
        Collectible.OnAnyCollectiblePlouffed += Collectible_OnAnyCollectiblePlouffed; ;
        Chest.OnAnyChestSpawnedCollectible += Chest_OnAnyChestSpawnedCollectible;
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;

        Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;

        Projectile.OnAnyProjectileHit += Projectile_OnAnyProjectileHit;
        Projectile.OnAnyProjectileInstantiated += Projectile_OnAnyProjectileInstantiated;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned += EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        GunSpotLight.OnAnyLightSwitched += GunSpotLight_OnAnyLightSwitched;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }


    #region UI

    private void PlayerUI_HPBar_OnHPTickAdded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.hpTickAdded,.7f);
    }

    private void PlayerUI_AmmoBar_OnAmmoTickAdded(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.ammoTickAdded,.7f);
    }

    private void StructureUI_Fire_OnFireTickRemoved(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.fireTickRemoved, .7f);
    }

    #endregion

    #region Attacks

    private void Projectile_OnAnyProjectileInstantiated(object sender, System.EventArgs e) {
        Projectile projectile = (Projectile)sender;
        PlaySound3D(projectile.GetProjectileSO().projectileInstantiatedAudioClips, (sender as MonoBehaviour).transform.position, .5f);
    }

    private void Projectile_OnAnyProjectileHit(object sender, System.EventArgs e) {
        Projectile projectile = (Projectile)sender;
        PlaySound3D(projectile.GetProjectileSO().projectileHitAudioClips, (sender as MonoBehaviour).transform.position, .5f);
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
        PlaySound2D(soundRefsSO.workerHunterJobAssigned);
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
    private void Collectible_OnAnyCollectiblePickedUpByPlayer(object sender, System.EventArgs e) {
        Collectible collectible = (Collectible)sender;

        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            PlaySound3D(soundRefsSO.bigBlueOrbPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            PlaySound3D(soundRefsSO.smallBlueOrbPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigRedOrb) {
            PlaySound3D(soundRefsSO.bigRedOrbPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallRedOrb) {
            PlaySound3D(soundRefsSO.smallRedOrbPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.greenGem) {
            PlaySound3D(soundRefsSO.gemPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.redGem) {
            PlaySound3D(soundRefsSO.gemPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
            PlaySound3D(soundRefsSO.ammoPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
        }
        if (collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            PlaySound3D(soundRefsSO.emberPickedUpByPlayer, (sender as MonoBehaviour).transform.position);
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
    private void ParticleCollision_OnAnyBulletHitGround(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().bulletHitGroundSound;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }

    private void ParticleCollision_OnAnyBulletHitEnemy(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().bulletHitEnemySound;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }


    private void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().outOfAmmoSound;
        PlaySound2D(audioClipArray, .75f);
    }

    private void PlayerShoor_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().cooldownGunSound;
        PlaySound2D(audioClipArray, .5f);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().reloadGunSound;
        PlaySound2D(audioClipArray, .5f);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().shootGunSound;
        PlaySound2D(audioClipArray, .5f);
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

    private void EndLevelAreaProp_OnAnyEndLevelAreaPropBurned(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = soundRefsSO.propBurned;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }

    #endregion

    #region OTHER
    private void GunSpotLight_OnAnyLightSwitched(object sender, System.EventArgs e) {
        PlaySound2D(soundRefsSO.gunLightSwitch);
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
        PlaySound2D(audioClipArray[Random.Range(0, audioClipArray.Length)], volume);
    }

    private void PlaySound2D(AudioClip audioClip, float volume = 1f) {
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
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructureUpgraded -= Structure_OnAnyStructureUpgraded;

        PlayerShoot.Instance.OnPlayerShotProjectile -= PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerReload -= PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerCooldownTrigger -= PlayerShoor_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo -= PlayerShoot_OnPlayerTryShoot_OutOfAmmo;

        PlayerUI_AmmoBar.Instance.OnAmmoTickAdded -= PlayerUI_AmmoBar_OnAmmoTickAdded;
        PlayerUI_HPBar.Instance.OnHPTickAdded -= PlayerUI_HPBar_OnHPTickAdded;
        StructureUI_Fire.OnFireTickRemoved -= StructureUI_Fire_OnFireTickRemoved;

        ParticleCollision.OnAnyBulletHitEnemy -= ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitGround -= ParticleCollision_OnAnyBulletHitGround;

        Collectible.OnAnyCollectibleTouchedFloor -= Collectible_OnAnyCollectibleTouchedFloor;
        Collectible.OnAnyCollectiblePickedUpByPlayer -= Collectible_OnAnyCollectiblePickedUpByPlayer;
        Collectible.OnAnyCollectiblePickedUpByWorker -= Collectible_OnAnyCollectiblePickedUpByWorker;
        Chest.OnAnyChestSpawnedCollectible -= Chest_OnAnyChestSpawnedCollectible;
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor -= PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;

        Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;

        Projectile.OnAnyProjectileHit -= Projectile_OnAnyProjectileHit;
        Projectile.OnAnyProjectileInstantiated -= Projectile_OnAnyProjectileInstantiated;

        EndLevelAreaProp.OnAnyEndLevelAreaPropBurned -= EndLevelAreaProp_OnAnyEndLevelAreaPropBurned;
        GunSpotLight.OnAnyLightSwitched -= GunSpotLight_OnAnyLightSwitched;
    }

}
