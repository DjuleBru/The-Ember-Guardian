using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance;

    public event EventHandler OnPlayerShot;
    public event EventHandler OnCooldownEnded;
    public event EventHandler OnPlayerStartedShot;
    public event EventHandler OnPlayerTryShoot_OutOfAmmo;
    public event EventHandler OnPlayerTryShoot_GunJammed;
    public event EventHandler OnPlayerShootStopped;
    public event EventHandler OnPlayerCooldownTrigger;
    public event EventHandler OnPlayerCooldownAnimationTrigger;
    public event EventHandler OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag;
    public event EventHandler OnPlayerTryReload_FullAmmoBelt;
    public event EventHandler OnPlayerTryReloadAmmoBelt_NoAmmoInBag;
    public event EventHandler<OnPlayerReloadEventArgs> OnPlayerReload;
    public event EventHandler OnPlayerReloadHandEnded;
    public event EventHandler OnPlayerReloadInterrupted;
    public event EventHandler OnPlayerReloadInterruptedEnded;
    public event EventHandler OnPlayerReloadEnded;
    public event EventHandler<OnAmmoRefilledEventArgs> OnPlayerAmmoRefilled;
    public event EventHandler OnBulletsChanged;
    public event EventHandler OnPlayerSwappedGunStarted;
    public event EventHandler OnPlayerSwappedGunEnded;
    public event EventHandler<OnPlayerSwappedGunEventArgs> OnPlayerSwappedGun;
    public event EventHandler OnGunsLoaded;
    public event EventHandler OnShotStartedLoading;

    public event EventHandler OnPrimaryWeaponChanged;
    public event EventHandler OnSecondaryWeaponChanged;
    public event EventHandler OnPlayerWeaponReplaced;

    public event EventHandler OnWeaponSecondaryAbilityStarted;
    public event EventHandler OnWeaponSecondaryAbilityEnded;
    public event EventHandler OnPlayerAimedSightStarted;
    public event EventHandler OnPlayerAimedSightEnded;
    public event EventHandler OnPlayerSwitchedFireMode;
    public event EventHandler OnPlayerOverclockedSMGStarted;
    public event EventHandler OnPlayerOverclockedSMGStopped;
    public event EventHandler OnPlayerFocusBlastStarted;
    public event EventHandler OnPlayerFocusBlastStopped;
    public event EventHandler OnPlayerSetupLMGStarted;
    public event EventHandler OnPlayerSetupLMGBipod;
    public event EventHandler<OnPlayerResetLMGBipodEventArgs> OnPlayerResetLMGBipod;
    public event EventHandler<OnPlayerResetLMGBipodEventArgs> OnPlayerSetupLMGStopped;
    public event EventHandler OnPlayerEmptyRevolverMagStart;
    public event EventHandler OnPlayerEmptyRevolverMagEnd;
    public event EventHandler OnPlayerTriggersProjectileExplosion;
    public event EventHandler OnSurgeReloadStart;
    public event EventHandler OnSpinningBulletFail;
    public event EventHandler OnSpinningBulletSuccess;

    public class OnPlayerResetLMGBipodEventArgs : EventArgs {
        public bool removeBecauseDied;
    }
    public class OnPlayerSwappedGunEventArgs : EventArgs {
        public bool triggerSFX;
    }

    private float handlingLMGTime = .9f;
    private float setupLMGTime = .75f;
    private float removeLMGTime = .4f;
    private float setupLMGTimer;
    private float lmgBipodAimAngleLimit = 40f;
    private bool settingUpLMG;
    private bool holdingStationaryGun;
    private bool emptyingRevolverMag;
    private bool silencerActive;
    private bool blastingLMGModeActive;
    private bool rightClickHeldDown;

    public class OnAmmoRefilledEventArgs : EventArgs {
        public int ammoAmount;
    }

    public class OnPlayerReloadEventArgs:EventArgs {
        public bool surgeReload;
    }

    [SerializeField] private Transform ammoDestinationPoint;
    [SerializeField] private Transform ammoSpawnPoint;

    private float shootCooldownTimer;
    private float shootCooldownSFXTriggerTime;
    private float playerJustPressedReloadTimer;
    private float transferringAmmoFromBagTimer;
    private float transferringAmmoFromBagCooldown = .3f;
    private float reloadTimer;
    private bool playerJustPressedReload;
    private bool transferringAmmoFromBag;
    private float reloadTime;
    private float handsReloadTime;

    private bool swappingGun;
    private bool autoReload;
    private bool canShoot = true;
    private bool canStartSurgeWindow = true;
    private bool nextBulletSurgeWindow = true;
    private bool coolingDown;
    private bool reloading;
    private bool reloadingInterrupted;
    private bool coolDownSFXTriggered;
    private bool coolDownAnimationTriggered;
    private bool surgeBulletSpinning;
    private bool surgeBulletSpinningInWindow;
    private float surgeBulletSpinningTimer;
    private float surgeBulletSpinningDelay = .6f;
    private float surgeBulletSpinningWindow = .3f;
    private float surgeBulletSpinningStartWindow = .15f;

    private float reloadingFromBeltReloadBuff = .95f;
    private float reloadingFromBagReloadDebuff = 1.2f;

    private bool secondaryAbilityActive;
    private bool rifleSemiAutoModeActive;
    private bool shotgunSemiAutoModeActive;
    private bool rifleLoadShotModeActive;
    private bool smgPoisonRoundsActive;
    private bool sniperPiercingRoundsActive;
    private bool grenadeLauncherMultipleGrenadesActive;
    private bool revolverBouncingBulletsActive;
    private bool pistolExplosiveBulletsActive;
    private bool minigunExplosiveBulletsActive;
    private bool rocketLauncherNukeModeActive;
    private bool projectileExplodesOnPlayerClickModeActive;
    private bool projectileExplodesOnPlayerClick;
    private bool aaGunSpawnsChildProjectiles;
    private bool aaGunSpawnsMines;
    private bool rocketLauncherSpawnsMiniRockets;

    private bool canHold2Guns;

    private bool automaticWeapon;
    private bool playerIsHoldingDownShoot;
    private bool hasAmmoRegen;
    private float ammoRegenTime;
    private float ammoRegenTimer;

    protected bool reloadingHands;
    protected bool shotNeedsLoading;
    protected bool loadedShotFiredIfNotFullyLoaded;
    protected bool loadingShot;
    protected bool shotLoaded;
    protected float loadingShotTimer;
    protected float loadingShotTime;
    protected float loadingShotHeldGunTime;
    protected float gunRecoil;
    protected float gunKnockback;

    private Gun heldGun;
    private GunSO heldGunSO;

    private GunSO primaryGunSO;
    private GunSO secondayGunSO;
    private List<GunSO> replacedGunSOList = new List<GunSO>();

    [SerializeField] private List<Gun> allGunsList;
    [SerializeField] private List<GunSO> allGunSOList;

    private bool useDebugGun;
    [SerializeField] private GunSO debugGun;
    [SerializeField] private GunSO debugSecondaryGun;
    [SerializeField] private bool debugUseFirstSecondaryAbility;
    [SerializeField] private bool debugUseSecondSecondaryAbility;

    private Coroutine swappingGunCoroutine;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitializeGuns();

        autoReload = SettingsManager.Instance.GetAutoReload();
        useDebugGun = DebugManager.Instance.GetDebugMode_PlayerWeapons();

        if (useDebugGun) {

            SetActiveGun(debugGun.gunType, true, false);
            if (debugSecondaryGun != null) {
                canHold2Guns = true;
                secondayGunSO = debugSecondaryGun;
            }

        } else {

            if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level && LevelManager.Instance.IsHordeMode()) {
                // HORDE MODE
                canHold2Guns = true;
                SetActiveGun(HordeModeCustomizationManager.Instance.GetSelectedWeaponType(), true, false);

            } else {

                canHold2Guns = PlayerStats.Instance.GetCanHold2WeaponsUnlocked();

                SetActiveGun(PlayerSave.Instance.GetPrimaryActiveGunType(), true, false);
                if (canHold2Guns) {
                    if (PlayerSave.Instance.GetSecondaryGunIsEquipped()) {
                        secondayGunSO = GetGunSO(PlayerSave.Instance.GetSecondaryActiveGunType());
                    }
                }
            }

        }

        SettingsManager.Instance.OnAutoReloadChanged += SettingsManager_OnAutoReloadChanged;
        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed += GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed += GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled += GameInput_OnPlayerReloadCanceled;
        GameInput.Instance.OnPlayerPrimaryGunSelected += GameInput_OnPlayerPrimaryGunSelected;
        GameInput.Instance.OnPlayerSecondaryGunSelected += GameInput_OnPlayerSecondaryGunSelected;
        GameInput.Instance.OnPlayerSwapGunCanceled += GameInput_OnPlayerSwapGunPerformed;

        GameInput.Instance.OnWeaponSecondaryAbilityCanceled += GameInput_OnWeaponSecondaryAbilityCanceled;
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilitytPerformed;

        PlayerStats.Instance.OnPlayerAmmoRegenTimeChanged += PlayerStats_OnPlayerAmmoRegenTimeChanged;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Gun.OnAnyGunStatsUpgraded += Gun_OnAnyGunStatsUpgraded;
    }

    private void SettingsManager_OnAutoReloadChanged(object sender, EventArgs e) {
        autoReload = SettingsManager.Instance.GetAutoReload();
    }

    private void Update() {
        if (loadingShot && !shotLoaded) {
            loadingShotTimer += Time.deltaTime;
            if (loadingShotTimer > loadingShotTime) {
                shotLoaded = true;
                loadingShot = false;
                Shoot();

                if(heldGunSO.gunType == GunSO.GunType.Shotgun) {
                    heldGun.SetCurrentBullet(0);
                    OnBulletsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        if (playerJustPressedReload && !swappingGun) {
            playerJustPressedReloadTimer += Time.deltaTime;

            if (playerJustPressedReloadTimer > .2f) {
                playerJustPressedReload = false;
                StartTransferringAmmoFromBagInGun();
            }

        }

        if (transferringAmmoFromBag && !swappingGun) {
            transferringAmmoFromBagTimer += Time.deltaTime;
            if (transferringAmmoFromBagTimer > transferringAmmoFromBagCooldown) {
                transferringAmmoFromBagTimer = 0;
                TransferNextAmmoFromBag();
            }
        }

        if (surgeBulletSpinning) {
            surgeBulletSpinningTimer += Time.deltaTime;

            if(surgeBulletSpinningTimer > surgeBulletSpinningDelay && !surgeBulletSpinningInWindow) {
                surgeBulletSpinningInWindow = true;
            }

            if(surgeBulletSpinningTimer > (surgeBulletSpinningDelay+surgeBulletSpinningWindow)) {
                surgeBulletSpinningInWindow = false;
                surgeBulletSpinning = false;
            }
        }

        if (coolingDown) {
            shootCooldownTimer -= Time.deltaTime;

            if (!coolDownAnimationTriggered) {
                OnPlayerCooldownAnimationTrigger?.Invoke(this, EventArgs.Empty);
                coolDownAnimationTriggered = true;
            }

            if (shootCooldownTimer <= (PlayerStats.Instance.GetShootCooldownTime() - shootCooldownSFXTriggerTime) && !coolDownSFXTriggered) {
                OnPlayerCooldownTrigger?.Invoke(this, EventArgs.Empty);
                coolDownSFXTriggered = true;
            }

            if (shootCooldownTimer <= 0) {
                CooldownFinished();
            }
            return;
        }

        if (reloading && !reloadingInterrupted) {
            reloadTimer += Time.deltaTime;

            if (reloadTimer >= handsReloadTime && reloadingHands) {
                EndHandReload();
            }

            if (reloadTimer >= reloadTime) {
                heldGun.SetCurrentBullet(heldGun.GetBulletsPerAmmoClip());
                reloading = false;
                OnPlayerReloadEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        if (hasAmmoRegen) {
            ammoRegenTimer -= Time.deltaTime;
            if (ammoRegenTimer <= 0) {
                ammoRegenTimer = ammoRegenTime;
                AddAmmoClip(1);
            }
        }

        if (settingUpLMG) {
            setupLMGTimer += Time.deltaTime;

            if (setupLMGTimer > handlingLMGTime) {
                settingUpLMG = false;
                canShoot = true;

                if (secondaryAbilityActive) {
                    OnPlayerSetupLMGBipod?.Invoke(this, EventArgs.Empty);
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    RemoveLMGBipod(false);
                }
            }

        }

    }

    #region INITIALIZATION

    public void SetGunSOInStock(GunSO gunSO) {
        if (!replacedGunSOList.Contains(gunSO)) {
            replacedGunSOList.Add(gunSO);
        }

        OnPlayerWeaponReplaced?.Invoke(this, EventArgs.Empty);
    }

    public void SetActiveGun(GunSO.GunType gunType, bool primaryGun = true, bool triggerSFX = true) {
        Gun activeGun = null;
        GunSO activeGunSO = null;

        foreach (Gun gun in allGunsList) {
            gun.gameObject.SetActive(false);
            gun.SetGunActive(false);

            if (gun.GetGunSO().gunType == gunType) {
                activeGun = gun;
                activeGunSO = gun.GetGunSO();
            }

        }



        activeGun.gameObject.SetActive(true);
        heldGunSO = activeGunSO;
        heldGun = activeGun;
        heldGun.SetGunActive(true);

        if (heldGunSO.shotNeedsLoading) {
            loadingShotHeldGunTime = heldGunSO.loadShotTime;
            loadingShotTime = loadingShotHeldGunTime;
            loadingShotTimer = 0;
        }

        shootCooldownSFXTriggerTime = heldGunSO.shootCooldownSFXTriggerTime;
        gunRecoil = heldGunSO.gunRecoil;
        gunKnockback = heldGunSO.gunKnockback;
        automaticWeapon = activeGunSO.automaticWeapon;
        shotNeedsLoading = activeGunSO.shotNeedsLoading;
        loadedShotFiredIfNotFullyLoaded = activeGunSO.loadedShotFiredIfNotFullyLoaded;

        PlayerStats.Instance.SetShootCooldownTime(heldGun.GetCooldownTime());
        PlayerStats.Instance.SetReloadTime(heldGun.GetReloadTime());
        PlayerStats.Instance.SetHandsReloadTime(heldGun.GetHandsReloadTime());

        if (primaryGun) {
            primaryGunSO = activeGunSO;
        }
        else {
            secondayGunSO = activeGunSO;
        }

        OnPlayerSwappedGun?.Invoke(this, new OnPlayerSwappedGunEventArgs {
            triggerSFX = triggerSFX
        });
    }

    public GunSO GetGunSO(GunSO.GunType gunType) {
        GunSO gunSO = null;

        foreach (Gun gun in allGunsList) {
            if (gun.GetGunSO().gunType == gunType) {
                gunSO = gun.GetGunSO();
            }

        }
        return gunSO;
    }

    private void InitializeGuns() {
        foreach (Gun gun in allGunsList) {
            gun.LoadGunStatModifierLevels();
            gun.gameObject.SetActive(false);

            allGunSOList.Add(gun.GetGunSO());
        }
    }

    #endregion

    #region SHOOT

    private void Shoot(bool shootOnReload = false) {
        //Debug.Log("Shoot " + loadingShot);

        StartCoroutine(ShootAfterDelay(heldGunSO.delayBetweenClickAndShot, shootOnReload));
    }

    private IEnumerator ShootAfterDelay(float delay, bool shootOnReload = false) {

        heldGun.SetCurrentBullet(heldGun.GetCurrentBullet() - 1);
        OnPlayerStartedShot?.Invoke(this, EventArgs.Empty);

        // Handle cooldown
        if (PlayerStats.Instance.GetShootCooldownTime() != 0 && !shootOnReload) {
            coolDownSFXTriggered = false;
            coolDownAnimationTriggered = false;
            coolingDown = true;
            shootCooldownTimer = PlayerStats.Instance.GetShootCooldownTime() + delay;
        };

        yield return new WaitForSeconds(delay);

        for(int i = 0; i < heldGun.GetProjectilesShotAmount(); i++) {
            ShootSingleProjectile();

            yield return new WaitForSeconds(heldGun.GetDelayBetweenSubShots());
        }

         OnBulletsChanged?.Invoke(this, EventArgs.Empty);
        if (shotNeedsLoading) {
            loadingShotTimer = 0;
        }

    }

    private void ShootSingleProjectile() {
        PlayerAim.Instance.AddRecoil(gunRecoil, heldGunSO.gunRecoilDamping);
        float aimDir = 1f;
        if (PlayerAim.Instance.GetAimDir().x < 0) {
            aimDir = -1f;
        }

        Vector2 gunKnockbackForce = new Vector2(aimDir * gunKnockback * -1, 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        OnPlayerShot?.Invoke(this, EventArgs.Empty);
    }

    private void CooldownFinished() {
        coolingDown = false;
        OnCooldownEnded?.Invoke(this, EventArgs.Empty);
        // Handle reload
        if (heldGun.GetCurrentBullet() > 0) {
            if (automaticWeapon && playerIsHoldingDownShoot && !Player.Instance.GetDead()) {
                Shoot();
            }
            else {
                OnPlayerShootStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        else {
            OnPlayerShootStopped?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        HandleSurgeBulletSpinningInput();

        // Grenade Launcher Secondary
        if (projectileExplodesOnPlayerClickModeActive && projectileExplodesOnPlayerClick && heldGun.GetGunSO().gunType == GunSO.GunType.GrenadeLauncher) {
            OnPlayerTriggersProjectileExplosion?.Invoke(this, EventArgs.Empty);
            projectileExplodesOnPlayerClick = false;
            return;
        }

        if (!canShoot) return;
        if (reloading) return;
        if (emptyingRevolverMag) return;
        if (swappingGun) return;
        if (loadingShot && !shotLoaded) return;
        if (Player.Instance.GetHP() == 0) return;

        playerIsHoldingDownShoot = true;
        if (coolingDown) return;

        if (heldGun.GetCurrentBullet() <= 0) {
            TryAutoReload();
            return;
        }

        if (shotNeedsLoading) {
            loadingShot = true;
            shotLoaded = false;
            loadingShotTime = loadingShotHeldGunTime;
            loadingShotTimer = 0;
            OnShotStartedLoading?.Invoke(this, EventArgs.Empty);
        }
        else {
            Shoot();
        }


        if (projectileExplodesOnPlayerClickModeActive && !projectileExplodesOnPlayerClick) {
            projectileExplodesOnPlayerClick = true;
            return;
        }
    }

    private void GameInput_OnPlayerShootCanceled(object sender, System.EventArgs e) {
        if (loadingShot && shotNeedsLoading && loadedShotFiredIfNotFullyLoaded) {
            if (loadingShotTimer > heldGunSO.minLoadShotTime) {
                if (heldGun.GetCurrentBullet() > 0) {
                    Shoot();
                }
            };
        }

        if (!rightClickHeldDown) {
            loadingShot = false;
            shotLoaded = false;

            OnPlayerShootStopped?.Invoke(this, EventArgs.Empty);
        }

        playerIsHoldingDownShoot = false;
    }

    #endregion

    #region AMMO AND RELOAD

    private void GameInput_OnPlayerReloadPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        HandleSurgeBulletSpinningInput();

        if (!canShoot) return;
        if (heldGun.GetGunJammedAndNextInputSequence(GameInput.Binding.reload)) return;

        playerJustPressedReload = true;
        playerJustPressedReloadTimer = 0;

    }

    private void GameInput_OnPlayerReloadCanceled(object sender, EventArgs e) {
        if (playerJustPressedReload) {
            playerJustPressedReload = false;
        }

        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!canShoot) return;
        if (swappingGun) return;
        if (heldGun.GetGunJammedAndNextInputSequence(GameInput.Binding.reload)) return;

        if (transferringAmmoFromBag) {
            transferringAmmoFromBag = false;
            return;
        };

        if (heldGun.GetCurrentBullet() != heldGun.GetBulletsPerAmmoClip()) {
            // Weapon is NOT full : reload
            if (CanReloadGun()) {
                bool canReloadGunFromBelt = CanReloadGunFromBelt();
                StartCoroutine(ReloadGunCoroutine(!canReloadGunFromBelt));
            }

        }
    }

    public void SetGunAmmo(GunSO gunSO, int ammoCount, int currentBullet) {
        foreach (Gun gun in allGunsList) {
            if (gun.GetGunSO() == gunSO) {
                gun.SetGunAmmo(ammoCount, currentBullet);
            }
        }

        OnBulletsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetGunToMaxAmmo(GunSO gunSO) {
        int ammoRefill = 0;
        foreach (Gun gun in allGunsList) {
            if (gun.GetGunSO() == gunSO) {
                ammoRefill = gun.GetMaxAmmo();
                gun.SetGunAmmo(ammoRefill, gun.GetBulletsPerAmmoClip());
            }
        }
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);
        OnPlayerAmmoRefilled?.Invoke(this, new OnAmmoRefilledEventArgs {
            ammoAmount = ammoRefill
        });
    }

    private void EndHandReload() {
        reloadingHands = false;
        OnPlayerReloadHandEnded?.Invoke(this, EventArgs.Empty);
    }

    private void StartTransferringAmmoFromBagInGun() {
        transferringAmmoFromBagTimer = 0;
        transferringAmmoFromBag = true;
        TransferNextAmmoFromBag(false);
    }

    private bool CanReloadGun() {

        if (heldGun.GetCurrentBullet() == heldGun.GetBulletsPerAmmoClip()) return false;
        if (reloading) return false;
        if (coolingDown) return false;

        if (heldGun.GetCurrentAmmoClip() > 0) {
            // Gun has ammo in belt
            return true;

        }
        else {
            // Gun has no ammo in belt
            PlayerCurrencies.CurrencyType ammoType = heldGunSO.ammoTypeUsed;

            if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(ammoType).Count > 0) {
                // There is ammo in bag
                return true;
            }
            else {
                // There is no ammo in bag
                OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
                return false;
            }
        };
    }

    private bool CanReloadGunFromBelt() {
        if (heldGun.GetCurrentAmmoClip() == 0) {
            return false;
        }
        else {
            return true;
        }
    }
    private IEnumerator ReloadGunCoroutine(bool reloadDirectlyFromBag) {
        PlayerCurrencies.CurrencyType ammoType = heldGunSO.ammoTypeUsed;

        float handsReloadTimeModified = heldGun.GetHandsReloadTime();
        float reloadTimeModified = heldGun.GetReloadTime();
        if (reloadDirectlyFromBag) {
            handsReloadTimeModified *= reloadingFromBagReloadDebuff;
            reloadTimeModified *= reloadingFromBagReloadDebuff;
        }
        else {
            handsReloadTimeModified *= reloadingFromBeltReloadBuff;
            reloadTimeModified *= reloadingFromBeltReloadBuff;
        }
        PlayerStats.Instance.SetReloadTime(reloadTimeModified);
        PlayerStats.Instance.SetHandsReloadTime(handsReloadTimeModified);

        reloading = true;
        reloadingHands = true;
        reloadTimer = 0;
        reloadTime = PlayerStats.Instance.GetReloadTime();
        handsReloadTime = PlayerStats.Instance.GetHandsReloadTime();

        if (reloadDirectlyFromBag) {
            TransferNextAmmoFromBag(true);
        }

        if (PlayerSkills.Instance.GetShootOnReload()) {

            int shotAmount = PlayerSkills.Instance.GetShootOnReloadShotAmount();
            if (heldGunSO.gunType == GunSO.GunType.RocketLauncher || heldGunSO.gunType == GunSO.GunType.GrenadeLauncher || heldGunSO.gunType == GunSO.GunType.AAGun) {
                shotAmount = 1;
            }

            for (int i = 0; i < shotAmount; i++) {
                Shoot(true);
                yield return new WaitForSeconds(.13f);
            }

        }

        yield return new WaitForEndOfFrame();
        if (!reloadDirectlyFromBag) {
            heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
        }

        surgeBulletSpinningInWindow = false;
        OnPlayerReload?.Invoke(this, new OnPlayerReloadEventArgs {
            surgeReload = TrySurgeReload(!reloadDirectlyFromBag)
        });

    }

    private void TransferNextAmmoFromBag(bool reloadGunDirectlyFromBag = false) {
        PlayerCurrencies.CurrencyType ammoType = heldGunSO.ammoTypeUsed;
        int ammoAmountInBag = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(ammoType).Count;

        if (heldGun.GetCurrentAmmoClip() == heldGun.GetMaxAmmo()) {
            OnPlayerTryReload_FullAmmoBelt?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (ammoAmountInBag > 0 && heldGun.GetCurrentAmmoClip() < heldGun.GetMaxAmmo()) {
            UICurrencyManager.PlayerInventoryUI.DropNextCurrencyInBag(ammoType);

            if (!reloadGunDirectlyFromBag) {
                AddAmmoClip(1);
            }

        }
        else {
            OnPlayerTryReloadAmmoBelt_NoAmmoInBag?.Invoke(this, EventArgs.Empty);
        }

    }

    public void AddAmmoClip(int ammoCount) {
        int ammoRefilled = ammoCount;
        if (heldGun.GetCurrentAmmoClip() + ammoRefilled > heldGun.GetMaxAmmo()) {
            ammoRefilled = heldGun.GetMaxAmmo() - heldGun.GetCurrentAmmoClip();
        }

        if (ammoRefilled == 0) return;
        heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() + ammoRefilled);

        OnPlayerAmmoRefilled?.Invoke(this, new OnAmmoRefilledEventArgs {
            ammoAmount = ammoRefilled
        });
    }

    private void TryAutoReload() {
        if (!autoReload) return;
        if (CanReloadGun()) {
            bool canReloadGunFromBelt = CanReloadGunFromBelt();
            StartCoroutine(ReloadGunCoroutine(!canReloadGunFromBelt));
        }

    }

    #endregion

    #region SURGE RELOAD

    private bool TrySurgeReload(bool reloadFromBelt) {
        if (!reloadFromBelt) return false;

        float surgeReloadProbability = heldGun.GetSurgeReloadProbability();

        bool surgeReload = UnityEngine.Random.Range(0f, 1f) < surgeReloadProbability;

        if (!canStartSurgeWindow) {
            return false;
        }
        surgeReload = surgeReload || nextBulletSurgeWindow || DebugManager.Instance.GetDebugSurgeReload();
        if (surgeReload) {
            surgeBulletSpinning = true;
            nextBulletSurgeWindow = false;
            surgeBulletSpinningTimer = 0f;
            OnSurgeReloadStart?.Invoke(this, EventArgs.Empty);
        }

        return surgeReload;
    }

    private void HandleSurgeBulletSpinningInput() {
        if (surgeBulletSpinning) {
            if (surgeBulletSpinningInWindow) {

                // Success
                OnSpinningBulletSuccess?.Invoke(this, EventArgs.Empty);
                surgeBulletSpinningInWindow = false;

            }
            else {

                if (surgeBulletSpinningTimer > surgeBulletSpinningStartWindow) {
                    // Fail
                    EndBulletSpinning();
                }

            }
        }
    }

    private void EndBulletSpinning() {
        surgeBulletSpinning = false;
        surgeBulletSpinningInWindow = false;
        OnSpinningBulletFail?.Invoke(this, EventArgs.Empty);
    }

    public void SetCanStartSurgeWindow(bool canStartSurgeWindow) {
        this.canStartSurgeWindow = canStartSurgeWindow;
    }

    public void SurgeWindowNextBullet(bool nextBulletSurgeWindow) {
        this.nextBulletSurgeWindow = nextBulletSurgeWindow;
    }
    #endregion

    #region SECONDARY ABILITIES
    private void GameInput_OnWeaponSecondaryAbilitytPerformed(object sender, EventArgs e) {
        rightClickHeldDown = true;
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        if (heldGun.GetGunJammedAndNextInputSequence(GameInput.Binding.secondary)) {
            OnPlayerTryShoot_GunJammed?.Invoke(this, EventArgs.Empty);
            return;
        }

        bool secondaryAbilityUnlocked = heldGun.GetSecondaryAbilityUnlocked();
        bool primarySecondaryAbilityEquipped = heldGun.GetPrimarySecondaryAbilityEquipped();
        bool secondarySecondaryAbilityEquipped = heldGun.GetSecondSecondaryAbilityEquipped();
        if (useDebugGun) {
            secondaryAbilityUnlocked = true;
        }

        if (!secondaryAbilityUnlocked) return;
        if (!canShoot) return;
        if (reloading) return;
        if (swappingGun) return;

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Rifle) {

            if (!secondarySecondaryAbilityEquipped) {
                rifleSemiAutoModeActive = !rifleSemiAutoModeActive;

                if (rifleSemiAutoModeActive) {
                    automaticWeapon = true;
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    automaticWeapon = false;
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }

            if (secondarySecondaryAbilityEquipped) {

                rifleLoadShotModeActive = !rifleLoadShotModeActive;
                if (rifleLoadShotModeActive) {
                    shotNeedsLoading = true;
                    loadingShotHeldGunTime = .5f;
                    loadingShotTime = loadingShotHeldGunTime;
                    loadingShotTimer = 0;
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    shotNeedsLoading = false;
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Shotgun) {
            if(!secondarySecondaryAbilityEquipped) {
                if (coolingDown) return;
                if (!canShoot) return;

                if (heldGun.GetCurrentBullet() == 0) {
                    TryAutoReload();
                    return;
                }
                if (heldGun.GetCurrentAmmoClip() < 0) {
                    OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
                    return;
                }

                loadingShot = true;
                loadingShotTime = 1.75f;
                loadingShotTimer = 0f;
                shotLoaded = false;

                OnPlayerFocusBlastStarted?.Invoke(this, EventArgs.Empty);
                OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = true;
            }

            if(secondarySecondaryAbilityEquipped) {

                shotgunSemiAutoModeActive = !shotgunSemiAutoModeActive;

                if (shotgunSemiAutoModeActive) {
                    automaticWeapon = true;
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    automaticWeapon = false;
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

            }
          
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.SMG) {

            if(!secondarySecondaryAbilityEquipped) {
                float shootCooldownBuffValue = 1.4f;
                PlayerStats.Instance.BuffShootCooldown(shootCooldownBuffValue);

                OnPlayerOverclockedSMGStarted?.Invoke(this, EventArgs.Empty);
                OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = true;
            }

            if(secondarySecondaryAbilityEquipped) {
                smgPoisonRoundsActive = !smgPoisonRoundsActive;

                if (smgPoisonRoundsActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }
            
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper) {
            if(!secondarySecondaryAbilityEquipped) {
                OnPlayerAimedSightStarted?.Invoke(this, EventArgs.Empty);
                OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = true;
            }

            if(secondarySecondaryAbilityEquipped) {
                sniperPiercingRoundsActive = !sniperPiercingRoundsActive;

                if (sniperPiercingRoundsActive) {
                    shotNeedsLoading = true;
                    loadingShotHeldGunTime = 1.025f;
                    loadingShotTime = 1.025f;
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    shotNeedsLoading = false;
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Revolver) {
            if(!secondarySecondaryAbilityEquipped) {
                if (heldGun.GetCurrentBullet() == 0) {
                    TryAutoReload();
                    return;
                }
                if (heldGun.GetCurrentAmmoClip() < 0) {
                    OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
                    return;
                }
                if (!secondaryAbilityActive) {
                    gunRecoil = .25f;
                    gunKnockback = 1f;

                    OnPlayerEmptyRevolverMagStart?.Invoke(this, EventArgs.Empty);
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);

                    secondaryAbilityActive = true;
                    emptyingRevolverMag = true;

                    StartCoroutine(EmptyRevolverMag());
                }
            }

            if(secondarySecondaryAbilityEquipped) {
                revolverBouncingBulletsActive = !revolverBouncingBulletsActive;

                if (revolverBouncingBulletsActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }
          

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.GrenadeLauncher) {

            if(!secondarySecondaryAbilityEquipped) {
                projectileExplodesOnPlayerClickModeActive = !projectileExplodesOnPlayerClickModeActive;
                projectileExplodesOnPlayerClick = true;

                if (projectileExplodesOnPlayerClickModeActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }
            
            if(secondarySecondaryAbilityEquipped) {
                grenadeLauncherMultipleGrenadesActive = !grenadeLauncherMultipleGrenadesActive;

                if (grenadeLauncherMultipleGrenadesActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.LMG) {
            if (!secondarySecondaryAbilityEquipped) {
                if (settingUpLMG) return;
                if (!secondaryAbilityActive) {
                    gunKnockback = 0f;

                    PlayerAim.Instance.SetGunStraight();
                    PlayerAim.Instance.SetLimitAimAngle(true, lmgBipodAimAngleLimit);
                    OnPlayerSetupLMGStarted?.Invoke(this, EventArgs.Empty);
                    OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                    settingUpLMG = true;
                    setupLMGTimer = 0f;
                    handlingLMGTime = setupLMGTime;
                    canShoot = false;

                    secondaryAbilityActive = true;
                    holdingStationaryGun = true;

                }
                else {
                    gunKnockback = heldGunSO.gunKnockback;

                    OnPlayerSetupLMGStopped?.Invoke(this, new OnPlayerResetLMGBipodEventArgs {
                        removeBecauseDied = false
                    });
                    OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                    settingUpLMG = true;
                    setupLMGTimer = 0f;
                    handlingLMGTime = removeLMGTime;
                    canShoot = false;

                    secondaryAbilityActive = false;

                }
            }

            if (secondarySecondaryAbilityEquipped) {
                blastingLMGModeActive = !blastingLMGModeActive;

                if (blastingLMGModeActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.AAGun) {
            if(!secondarySecondaryAbilityEquipped) {
                aaGunSpawnsChildProjectiles = !aaGunSpawnsChildProjectiles;

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                if (aaGunSpawnsChildProjectiles) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }
            }
           
            if(secondarySecondaryAbilityEquipped) {
                aaGunSpawnsMines = !aaGunSpawnsMines;

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                if (aaGunSpawnsMines) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }
            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.RocketLauncher) {
            if(!secondarySecondaryAbilityEquipped) {
                rocketLauncherSpawnsMiniRockets = !rocketLauncherSpawnsMiniRockets;

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                if (rocketLauncherSpawnsMiniRockets) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }
            }
           
            if(secondarySecondaryAbilityEquipped) {
                rocketLauncherNukeModeActive = !rocketLauncherNukeModeActive;

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                if (rocketLauncherNukeModeActive) {
                    shotNeedsLoading = true;
                    loadingShotHeldGunTime = 1.5f;
                    loadingShotTime = 1.5f;
                    loadedShotFiredIfNotFullyLoaded = true;
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    shotNeedsLoading = false;
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Pistol) {
            if(!secondarySecondaryAbilityEquipped) {
                silencerActive = !silencerActive;

                if (silencerActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            } 

            if(secondarySecondaryAbilityEquipped) {
                pistolExplosiveBulletsActive = !pistolExplosiveBulletsActive;

                if (pistolExplosiveBulletsActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }
           
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.MiniGun) {
            if(!secondarySecondaryAbilityEquipped) {
                OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = true;
            }

            if(secondarySecondaryAbilityEquipped) {
                minigunExplosiveBulletsActive = !minigunExplosiveBulletsActive;

                if (minigunExplosiveBulletsActive) {
                    OnWeaponSecondaryAbilityStarted?.Invoke(this, EventArgs.Empty);
                }
                else {
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                }

                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
            }

        }

    }

    private void GameInput_OnWeaponSecondaryAbilityCanceled(object sender, EventArgs e) {
        rightClickHeldDown = false;
        bool secondarySecondaryAbilityEquipped = debugUseSecondSecondaryAbility;

        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (secondaryAbilityActive) {

            if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper) {
                OnPlayerAimedSightEnded?.Invoke(this, EventArgs.Empty);
                OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }

            if (heldGun.GetGunSO().gunType == GunSO.GunType.Shotgun) {

                if(!secondarySecondaryAbilityEquipped) {
                    loadingShot = false;
                    OnPlayerFocusBlastStopped?.Invoke(this, EventArgs.Empty);
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                    secondaryAbilityActive = false;
                }
              
            }

            if (heldGun.GetGunSO().gunType == GunSO.GunType.SMG) {
                if (!secondarySecondaryAbilityEquipped) {
                    float shootCooldownBuffValue = 1.4f;
                    PlayerStats.Instance.DebuffShootCooldown(shootCooldownBuffValue);
                    OnPlayerOverclockedSMGStopped?.Invoke(this, EventArgs.Empty);
                    OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                    secondaryAbilityActive = false;
                }

            }

            if (heldGun.GetGunSO().gunType == GunSO.GunType.MiniGun) {
                OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }
        }
    }
   
    public void RemoveLMGBipod(bool removeBecauseDied) {
        if (!holdingStationaryGun) return;

        secondaryAbilityActive = false;

        holdingStationaryGun = false;
        PlayerAim.Instance.SetLimitAimAngle(false);
        OnPlayerResetLMGBipod?.Invoke(this, new OnPlayerResetLMGBipodEventArgs {
            removeBecauseDied = removeBecauseDied
        });
        OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator EmptyRevolverMag() {
        int remainingBullets = GetCurrentBullets();
        float delayBetweenBullets = .155f;

        for (int i = 0; i < remainingBullets; i++) {
            Shoot();
            if (i < remainingBullets - 1) {
                yield return new WaitForSeconds(delayBetweenBullets);
            }
        }

        OnPlayerEmptyRevolverMagEnd?.Invoke(this, EventArgs.Empty);
        OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
        secondaryAbilityActive = false;

        gunRecoil = heldGunSO.gunRecoil;
        gunKnockback = heldGunSO.gunKnockback;

        float cooldownDelay = 2f;
        yield return new WaitForSeconds(cooldownDelay);

        emptyingRevolverMag = false;
    }

    public bool GetRifleLoadShotModeActive() {
        return rifleLoadShotModeActive;
    }
    public bool GetShotgunSemiAutoModeActive() {
        return shotgunSemiAutoModeActive;
    }

    public bool GetSMGPoisonRoundsActive() {
        return smgPoisonRoundsActive;
    }
    public bool GetSniperPiercingRoundsActive() {
        return sniperPiercingRoundsActive;
    }

    public bool GetGrenadeLauncherMultipleGrenadesActive() {
        return grenadeLauncherMultipleGrenadesActive;
    }
    public bool GetRevolverBouncingBulletsActive() {
        return revolverBouncingBulletsActive;
    }
    public bool GetPistolExplosiveBulletsActive() {
        return pistolExplosiveBulletsActive;
    }
    public bool GetMinigunExplosiveBulletsActive() {
        return minigunExplosiveBulletsActive;
    }

    public bool GetBlastingLMGModeActive() {
        return blastingLMGModeActive;
    }

    public void CheckWeaponStatusFX(Mob mobHit) {
        Creature creatureHit = mobHit as Creature;
        if(creatureHit != null) {
            if (smgPoisonRoundsActive) {
                creatureHit.ApplyPoisonEffect(5);
            }
        }    
    }

    public bool GetFirstSecondaryAbilityEquipped() {
        return heldGun.GetPrimarySecondaryAbilityEquipped();
    }

    public bool GetSecondSecondaryAbilityEquipped() {
        return heldGun.GetSecondSecondaryAbilityEquipped();
    }


    #endregion

    #region SWAP GUNS
    private void GameInput_OnPlayerSwapGunPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (secondayGunSO != null) {
            SwapGun();
        }
    }

    private void GameInput_OnPlayerSecondaryGunSelected(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!CanSwapGun()) return;

        if(useDebugGun) {
            secondayGunSO = debugSecondaryGun;
        }

        CancelSecondaryFireMode();

        if (secondayGunSO != null) {
            if (heldGunSO == secondayGunSO) return;
            swappingGunCoroutine = StartCoroutine(SetActiveGunAfterDelay(secondayGunSO, false));
        }
    }

    private void GameInput_OnPlayerPrimaryGunSelected(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!CanSwapGun()) return;

        CancelSecondaryFireMode();

        if (useDebugGun) {
            primaryGunSO = debugGun;
        }

        if (secondayGunSO != null) {
            if (heldGunSO == primaryGunSO) return;
            swappingGunCoroutine = StartCoroutine(SetActiveGunAfterDelay(primaryGunSO));
        }
    }

    public void SetPrimaryWeaponSO(GunSO gunSO, bool setGunActive = true) {
        CancelSecondaryFireMode();

        primaryGunSO = gunSO;
        if (setGunActive) {
            SetActiveGun(gunSO.gunType, true);
        }


        OnPrimaryWeaponChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSecondaryWeaponSO(GunSO gunSO, bool setGunActive = true) {
        CancelSecondaryFireMode();

        secondayGunSO = gunSO;

        if (setGunActive) {
            SetActiveGun(gunSO.gunType, false);
        }

        OnSecondaryWeaponChanged?.Invoke(this, EventArgs.Empty);
    }

    public void PickUpWeapon(GunSO gunSO) {

        if (canHold2Guns && secondayGunSO == null) {
            SetSecondaryWeaponSO(gunSO);
        }
        else {
            ReplaceHeldWeaponSO(gunSO);
        }
    }

    private void ReplaceHeldWeaponSO(GunSO gunSO) {
        CancelSecondaryFireMode();

        if (replacedGunSOList.Contains(gunSO)) {
            replacedGunSOList.Remove(gunSO);
        }

        if (heldGunSO == primaryGunSO) {
            replacedGunSOList.Add(primaryGunSO);
            SetPrimaryWeaponSO(gunSO);
        }
        else {
            replacedGunSOList.Add(secondayGunSO);
            SetSecondaryWeaponSO(gunSO);
        }

        OnPlayerWeaponReplaced?.Invoke(this, EventArgs.Empty);
    }

    public void ReplaceWeaponSO(GunSO gunSO, bool replacePrimaryWeapon) {
        CancelSecondaryFireMode();

        if (replacedGunSOList.Contains(gunSO)) {
            replacedGunSOList.Remove(gunSO);
        }

        if (replacePrimaryWeapon) {
            replacedGunSOList.Add(primaryGunSO);
            SetPrimaryWeaponSO(gunSO);
        }
        else {
            replacedGunSOList.Add(secondayGunSO);
            SetSecondaryWeaponSO(gunSO);
        }

        OnPlayerWeaponReplaced?.Invoke(this, EventArgs.Empty);
    }

    private void SwapGun() {
        if (!CanSwapGun()) return;

        CancelSecondaryFireMode();

        if (heldGunSO == secondayGunSO) {
            swappingGunCoroutine = StartCoroutine(SetActiveGunAfterDelay(primaryGunSO));
        }
        else {
            swappingGunCoroutine = StartCoroutine(SetActiveGunAfterDelay(secondayGunSO, false));
        }
    }

    public void CancelSecondaryFireMode() {
        if (rifleSemiAutoModeActive) {
            automaticWeapon = false;
            rifleSemiAutoModeActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (rifleLoadShotModeActive) {
            shotNeedsLoading = false;
            rifleLoadShotModeActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (shotgunSemiAutoModeActive) {
            automaticWeapon = false;
            shotgunSemiAutoModeActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (smgPoisonRoundsActive) {
            smgPoisonRoundsActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (sniperPiercingRoundsActive) {
            sniperPiercingRoundsActive = false;

            shotNeedsLoading = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (revolverBouncingBulletsActive) {
            revolverBouncingBulletsActive = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (pistolExplosiveBulletsActive) {
            pistolExplosiveBulletsActive = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (minigunExplosiveBulletsActive) {
            minigunExplosiveBulletsActive = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (grenadeLauncherMultipleGrenadesActive) {
            grenadeLauncherMultipleGrenadesActive = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (blastingLMGModeActive) {
            blastingLMGModeActive = false;

            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (aaGunSpawnsChildProjectiles) {
            aaGunSpawnsChildProjectiles = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (aaGunSpawnsMines) {
            aaGunSpawnsMines = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (rocketLauncherSpawnsMiniRockets) {
            rocketLauncherSpawnsMiniRockets = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (rocketLauncherNukeModeActive) {
            rocketLauncherNukeModeActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (projectileExplodesOnPlayerClickModeActive) {
            projectileExplodesOnPlayerClickModeActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (silencerActive) {
            silencerActive = false;
            OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool CanSwapGun() {
        bool canSwapGun = !swappingGun && !reloading && !secondaryAbilityActive;
        return canSwapGun;
    }

    private IEnumerator SetActiveGunAfterDelay(GunSO gunSO, bool primaryGunSO = true) {
        OnPlayerSwappedGunStarted?.Invoke(this, EventArgs.Empty);
        swappingGun = true;

        float swapGunStartAnimationTime = .25f * heldGun.GetSwapToWeaponTimeMultiplier();

        yield return new WaitForSeconds(swapGunStartAnimationTime);
        SetActiveGun(gunSO.gunType, primaryGunSO);
        float swapGunEndAnimationTime = .35f * heldGun.GetSwapToWeaponTimeMultiplier();

        yield return new WaitForSeconds(swapGunEndAnimationTime);
        swappingGun = false;
        OnPlayerSwappedGunEnded?.Invoke(this, EventArgs.Empty);
    }

    private void InterruptGunSwap() {
        if(swappingGunCoroutine != null) {
            StopCoroutine(swappingGunCoroutine);
            swappingGun = false;
            OnPlayerSwappedGunEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    private void PlayerStats_OnPlayerAmmoRegenTimeChanged(object sender, EventArgs e) {
        hasAmmoRegen = true;
        ammoRegenTime = PlayerStats.Instance.GetAmmoRegenTime();
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        canShoot = true;
        if(reloadingInterrupted) {
            reloadingInterrupted = false;
            OnPlayerReloadInterruptedEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {

        if(settingUpLMG || holdingStationaryGun) {
            RemoveLMGBipod(true);
        }

        if(secondaryAbilityActive) {
            bool primarySecondaryAbilityEquipped = debugUseFirstSecondaryAbility;
            bool secondarySecondaryAbilityEquipped = debugUseSecondSecondaryAbility;

            if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper && primarySecondaryAbilityEquipped) {
                OnPlayerAimedSightEnded?.Invoke(this, EventArgs.Empty);
                OnWeaponSecondaryAbilityEnded?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }
        }

        if(swappingGun) {
            InterruptGunSwap();
        }

    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (reloadingHands) {
            reloadingInterrupted = true;
            OnPlayerReloadInterrupted?.Invoke(this, EventArgs.Empty);
        }
        canShoot = false;
    }

    private void Gun_OnAnyGunStatsUpgraded(object sender, EventArgs e) {
        PlayerStats.Instance.SetShootCooldownTime(heldGun.GetCooldownTime());
        PlayerStats.Instance.SetReloadTime(heldGun.GetReloadTime());
        PlayerStats.Instance.SetHandsReloadTime(heldGun.GetHandsReloadTime());
    }

    public void SetCanShoot(bool canShoot) {
        this.canShoot = canShoot;
    }


    #region GET PARAMETERS


    public Gun GetGun(GunSO gunSO) {
        Gun returnGun = null;
        foreach (Gun gun in allGunsList) {
            if (gun.GetGunSO() == gunSO) {
                returnGun = gun;
            }
        }

        return returnGun;
    }

    public float GetLoadingShotTimerNormalized() {
        return loadingShotTimer / loadingShotTime;
    }

    public int GetDamagePerBullet() {
        return heldGun.GetDamagePerBullet();
    }

    public float GetBulletKnockback() {
        return heldGun.GetBulletKnockback();
    }
    
    public int GetCurrentAmmoClip() {
        return heldGun.GetCurrentAmmoClip();
    }
    public PlayerCurrencies.CurrencyType GetCurrentAmmoType() {
        return heldGunSO.ammoTypeUsed;
    }
    public int GetMaxAmmoClips() {
        return heldGun.GetMaxAmmo();
    }

    public int GetCurrentBullets() {
        return heldGun.GetCurrentBullet();
    }

    public int GetMaxBulletsPerClip() {
        return heldGun.GetBulletsPerAmmoClip();
    }

    public float GetGunReloadAccelerationFactor() {
        return heldGun.GetReloadAccelerationFactor();
    }

    public float GetGunWeightAccelerationFactor() {
        return heldGun.GetWeightAccelerationFactor();
    }

    public GunSO GetHeldGunSO() {
        return heldGunSO;
    }

    public GunSO GetPrimaryGunSO() {
        return primaryGunSO;
    }

    public GunSO GetSecondaryGunSO() {
        return secondayGunSO;
    }

    public Gun GetHeldGun() {
        return heldGun;
    }
    
    public Gun GetNotHeldGun() {
        if(heldGunSO == primaryGunSO) {
            return GetGun(secondayGunSO);
        } else {
            return GetGun(primaryGunSO);
        }
    }

    public List<GunSO> GetAllGunSOList() {
        List<GunSO> allGunSOList = new List<GunSO>();

        foreach (Gun gun in allGunsList) {
            allGunSOList.Add(gun.GetGunSO());
        }
        return allGunSOList;
    }

    public List<GunSO> GetUnlockedGunSOList() {
        List<GunSO> unlockedGunSOList = new List<GunSO>();

        foreach(Gun gun in allGunsList) {
            if(gun.GetGunUnlocked()) {
                unlockedGunSOList.Add(gun.GetGunSO());
            }
        }

        return unlockedGunSOList;
    }

    public List<GunSO> GetUnlockedAndUnequippedGunSOList() {
        List<GunSO> unlockedGunSOList = GetUnlockedGunSOList();
        List<GunSO> unlockedAndUnequippedGunSOList = new List<GunSO>();

        foreach (GunSO gunSO in unlockedGunSOList) {
            if(gunSO != primaryGunSO && gunSO != secondayGunSO) {
                unlockedAndUnequippedGunSOList.Add(gunSO);
            }
        }
        return unlockedAndUnequippedGunSOList;
    }

    public List<GunSO> GetGunSOInStock() {
        return replacedGunSOList;
    }

    public bool GetReloadingHands() {
        return reloadingHands;
    }


    public bool GetHoldingStationaryGun() {
        return holdingStationaryGun;
    }
    public bool GetProjectileExplodesOnPlayerClickModeActive() {
        return projectileExplodesOnPlayerClickModeActive;
    }

    public bool GetAAGunSpawnsChildBullets() {
        return aaGunSpawnsChildProjectiles;
    }
    public bool GetAAGunSpawnsMines() {
        return aaGunSpawnsMines;
    }
    public bool GetRocketLauncherMiniRockets() {
        return rocketLauncherSpawnsMiniRockets;
    }

    public bool GetRocketLauncherNukeMode() {
        return rocketLauncherNukeModeActive;
    }
    public bool GetSilencerActive() {
        return silencerActive;
    }

    public bool GetCanShoot() {
        return canShoot;
    }
    public bool GetReloading() {
        return reloading;
    }

    public bool GetCanSurgeWindow() {
        return canStartSurgeWindow;
    }

    public bool GetIsCarryingGun(GunSO gunSO) {
        bool carryingGun = false;
        if(primaryGunSO == gunSO || secondayGunSO == gunSO || replacedGunSOList.Contains(gunSO)) {
            carryingGun = true;
        }

        return carryingGun;
    }

    #endregion

    public void SaveAllGunStats(bool mainGame) {
        foreach (Gun gun in allGunsList) {
            gun.SaveGunStatModifierLevels(mainGame);
        }
    }

    public bool GetHasOnlySpecialAmmo() {
        bool hasOnlySpecialAmmo = false;
        bool holding2Weapons = secondayGunSO != null;

        if(holding2Weapons) {
            hasOnlySpecialAmmo = primaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special && (secondayGunSO != null && secondayGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special);
        } else {
            hasOnlySpecialAmmo = primaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special;
        }

        return hasOnlySpecialAmmo;
    }

    public bool GetHasBothAmmoTypes() {
        bool hasBothAmmo = false;
        bool hasOnlySpecialAmmo = false;
        bool hasOnlyStandardAmmo = false;
        bool holding2Weapons = secondayGunSO != null;

        if (holding2Weapons) {
            hasOnlySpecialAmmo = primaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special && (secondayGunSO != null && secondayGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special);
            hasOnlyStandardAmmo = primaryGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo && (secondayGunSO != null && secondayGunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo);
        }
        else {
            return false;
        }

        hasBothAmmo = !hasOnlySpecialAmmo && !hasOnlyStandardAmmo;

        return hasBothAmmo;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerShootCanceled -= GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed -= GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed -= GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled -= GameInput_OnPlayerReloadCanceled;
        GameInput.Instance.OnPlayerPrimaryGunSelected -= GameInput_OnPlayerPrimaryGunSelected;
        GameInput.Instance.OnPlayerSecondaryGunSelected -= GameInput_OnPlayerSecondaryGunSelected;
        GameInput.Instance.OnPlayerSwapGunCanceled -= GameInput_OnPlayerSwapGunPerformed;

        GameInput.Instance.OnWeaponSecondaryAbilityCanceled -= GameInput_OnWeaponSecondaryAbilityCanceled;
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed -= GameInput_OnWeaponSecondaryAbilitytPerformed;

        PlayerStats.Instance.OnPlayerAmmoRegenTimeChanged -= PlayerStats_OnPlayerAmmoRegenTimeChanged;
        PlayerMovement.Instance.OnPlayerRoll -= PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded -= PlayerMovement_OnPlayerRollEnded;

        Gun.OnAnyGunStatsUpgraded -= Gun_OnAnyGunStatsUpgraded;
    }

}
