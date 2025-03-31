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
    public event EventHandler OnPlayerStartedShot;
    public event EventHandler OnPlayerTryShoot_OutOfAmmo;
    public event EventHandler OnPlayerShootStopped;
    public event EventHandler OnPlayerCooldownTrigger;
    public event EventHandler OnPlayerCooldownAnimationTrigger;
    public event EventHandler OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag;
    public event EventHandler OnPlayerReload;
    public event EventHandler OnPlayerReloadHandEnded;
    public event EventHandler OnPlayerReloadInterrupted;
    public event EventHandler OnPlayerReloadEnded;
    public event EventHandler<OnAmmoRefilledEventArgs> OnPlayerAmmoRefilled;
    public event EventHandler OnBulletsChanged;
    public event EventHandler OnPlayerSwappedGunStarted;
    public event EventHandler OnPlayerSwappedGun;
    public event EventHandler OnGunsLoaded;

    public event EventHandler OnPrimaryWeaponChanged;
    public event EventHandler OnSecondaryWeaponChanged;

    public event EventHandler OnPlayerAimedSightStarted;
    public event EventHandler OnPlayerAimedSightEnded;
    public event EventHandler OnPlayerSwitchedFireMode;
    public event EventHandler OnPlayerOverclockedSMGStarted;
    public event EventHandler OnPlayerOverclockedSMGStopped;
    public event EventHandler OnPlayerFocusBlastStarted;
    public event EventHandler OnPlayerFocusBlastStopped;
    public event EventHandler OnPlayerSetupLMGStarted;
    public event EventHandler OnPlayerSetupLMGBipod;
    public event EventHandler OnPlayerResetLMGBipod;
    public event EventHandler OnPlayerSetupLMGStopped;
    public event EventHandler OnPlayerEmptyRevolverMagStart;
    public event EventHandler OnPlayerEmptyRevolverMagEnd;

    private float setupLMGTime = 2.5f;
    private float setupLMGTimer;
    private bool settingUpLMG;
    private bool holdingStationaryGun;
    private bool emptyingRevolverMag;

    public class OnAmmoRefilledEventArgs : EventArgs {
        public int ammoAmount;
    }

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform ammoDestinationPoint;
    [SerializeField] private Transform ammoSpawnPoint;
    [SerializeField] private Transform projectilePrefab;
    [SerializeField] private float projectileInitialForce;


    private float shootCooldownTimer;
    private float shootCooldownSFXTriggerTime;
    private float playerJustPressedReloadTimer;
    private float transferringAmmoFromBagTimer;
    private float transferringAmmoFromBagCooldown = 1f;
    private float reloadTimer;
    private float reloadTime;
    private float handsReloadTime;

    private bool swappingGun;
    private bool autoReload;
    private bool canShoot = true;
    private bool coolingDown;
    private bool reloading;
    private bool reloadingInterruptedByRoll;
    private bool coolDownSFXTriggered;
    private bool coolDownAnimationTriggered;
    private bool playerJustPressedReload;
    private bool transferringAmmoFromBag;
    private bool rifleSemiAutoModeActive;
    private bool secondaryAbilityActive;
    private bool canHold2Guns;

    private bool automaticWeapon;
    private bool playerIsHoldingDownShoot;
    private bool hasAmmoRegen;
    private float ammoRegenTime;
    private float ammoRegenTimer;

    protected bool reloadingHands;
    protected bool loadingShot;
    protected bool shotLoaded;
    protected float loadingShotTimer;
    protected float loadingShotTime;
    protected float gunRecoil;
    protected float gunKnockback;

    private Gun heldGun;
    private GunSO heldGunSO;

    private GunSO primaryGunSO;
    private GunSO secondayGunSO;

    [SerializeField] private List<Gun> allGunsList;
    [SerializeField] private List<GunSO> allGunSOList;

    private bool useDebugGun;
    [SerializeField] private GunSO debugGun;
    [SerializeField] private GunSO debugSecondaryGun;
    [SerializeField] private bool debugSecondaryAbilityUnlocked;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitializeGuns();

        useDebugGun = DebugManager.Instance.GetDebugMode_PlayerWeapons();
        if (useDebugGun) {
            SetActiveGun(debugGun);
            if (debugSecondaryGun != null) {
                canHold2Guns = true;
                secondayGunSO = debugSecondaryGun;
            }
        } else {
            SetActiveGun(PlayerSave.Instance.GetPrimaryActiveGun());
            canHold2Guns = PlayerStats.Instance.GetCanHold2WeaponsUnlocked();
            if (canHold2Guns) {
                this.secondayGunSO = PlayerSave.Instance.GetSecondaryActiveGun();
            }
        }

        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed += GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed += GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled += GameInput_OnPlayerReloadCanceled;
        GameInput.Instance.OnPlayerPrimaryGunSelected += GameInput_OnPlayerPrimaryGunSelected;
        GameInput.Instance.OnPlayerSecondaryGunSelected += GameInput_OnPlayerSecondaryGunSelected;
        GameInput.Instance.OnPlayerSwapGunPerformed += GameInput_OnPlayerSwapGunPerformed;

        GameInput.Instance.OnWeaponSecondaryAbilityCanceled += GameInput_OnWeaponSecondaryAbilityCanceled;
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilitytPerformed;

        PlayerStats.Instance.OnPlayerAmmoRegenTimeChanged += PlayerStats_OnPlayerAmmoRegenTimeChanged;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;

        if(UICurrencyManager.PlayerInventoryUI != null) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
        }
    }
    private void Update() {

        if (loadingShot && !shotLoaded) {
            loadingShotTimer += Time.deltaTime;
            if (loadingShotTimer > loadingShotTime) {
                shotLoaded = true;
                Shoot();
                heldGun.SetCurrentBullet(0);
                OnBulletsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        if (playerJustPressedReload) {
            playerJustPressedReloadTimer += Time.deltaTime;
            if (playerJustPressedReloadTimer > .35f) {
                playerJustPressedReload = false;
                StartTransferringAmmoFromBagInGun();
            }
        }

        if (transferringAmmoFromBag) {
            transferringAmmoFromBagTimer += Time.deltaTime;
            if (transferringAmmoFromBagTimer > transferringAmmoFromBagCooldown) {
                transferringAmmoFromBagTimer = 0;
                TransferNextAmmoFromBag();
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

        if (reloading) {
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

            if (setupLMGTimer > setupLMGTime) {
                settingUpLMG = false;
                canShoot = true;

                if (secondaryAbilityActive) {
                    OnPlayerSetupLMGBipod?.Invoke(this, EventArgs.Empty);
                }
                else {
                    holdingStationaryGun = false;
                    PlayerAim.Instance.SetLimitAimAngle(false);
                    OnPlayerResetLMGBipod?.Invoke(this, EventArgs.Empty);
                }
            }

        }
    }


    public void SetPrimaryWeaponSO(GunSO gunSO) {
        primaryGunSO = gunSO;
        SetActiveGun(gunSO, true);
        OnPrimaryWeaponChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSecondaryWeaponSO(GunSO gunSO) {
        secondayGunSO = gunSO;
        SetActiveGun(gunSO, false);
        OnSecondaryWeaponChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetActiveGun(GunSO gunSO, bool primaryGun = true) {
        Gun activeGun = null;

        foreach(Gun gun in allGunsList) {
            gun.gameObject.SetActive(false);
            gun.SetGunActive(false);
            if(gun.GetGunSO() == gunSO) {
                activeGun = gun;
            }
        }

        activeGun.gameObject.SetActive(true);
        heldGunSO = gunSO;
        heldGun = activeGun;
        heldGun.SetGunActive(true);

        shootCooldownSFXTriggerTime = heldGunSO.shootCooldownSFXTriggerTime;
        gunRecoil = heldGunSO.gunRecoil;
        gunKnockback = heldGunSO.gunKnockback; 
        automaticWeapon = gunSO.automaticWeapon;
        if (rifleSemiAutoModeActive) {
            automaticWeapon = true;
        }

        PlayerStats.Instance.SetShootCooldownTime(heldGun.GetCooldownTime());
        PlayerStats.Instance.SetReloadTime(heldGun.GetReloadTime());
        PlayerStats.Instance.SetHandsReloadTime(heldGun.GetHandsReloadTime());

        if(primaryGun) {
            primaryGunSO = gunSO;
        } else {
            secondayGunSO = gunSO;
        }

        OnPlayerSwappedGun?.Invoke(this, EventArgs.Empty);
    }

    public void SetGunAmmo(GunSO gunSO, int ammoCount) {
        foreach (Gun gun in allGunsList) {
            if (gun.GetGunSO() == gunSO) {
                gun.SetGunAmmo(ammoCount, 0);
            }
        }
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InitializeGuns() {
        foreach (Gun gun in allGunsList) {
            gun.RefreshGunStats();
            gun.gameObject.SetActive(false);

            allGunSOList.Add(gun.GetGunSO());
        }
    }

    private void Shoot() {
        StartCoroutine(ShootAfterDelay(heldGunSO.delayBetweenClickAndShot));
    }

    private IEnumerator ShootAfterDelay(float delay) {

        heldGun.SetCurrentBullet(heldGun.GetCurrentBullet() - 1);
        OnPlayerStartedShot?.Invoke(this, EventArgs.Empty);

        // Handle cooldown
        if (PlayerStats.Instance.GetShootCooldownTime() != 0) {
            coolDownSFXTriggered = false;
            coolDownAnimationTriggered = false;
            coolingDown = true;
            shootCooldownTimer = PlayerStats.Instance.GetShootCooldownTime() + delay;
        };

        yield return new WaitForSeconds(delay);

        PlayerAim.Instance.AddRecoil(gunRecoil, heldGunSO.gunRecoilDamping);

        float aimDir = 1f;
        if (PlayerAim.Instance.GetAimDir().x < 0) {
            aimDir = -1f;
        }

        Vector2 gunKnockbackForce = new Vector2(aimDir * gunKnockback * -1, 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);

        OnPlayerShot?.Invoke(this, EventArgs.Empty);
    }

    private void CooldownFinished() {
        coolingDown = false;

        // Handle reload
        if (heldGun.GetCurrentBullet() <= 0) {
            if(autoReload) {
                reloading = true;
                reloadTimer = 0;
                reloadTime = PlayerStats.Instance.GetReloadTime();
                handsReloadTime = PlayerStats.Instance.GetHandsReloadTime();
                reloadingHands = true;

                heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
                OnPlayerReload?.Invoke(this, EventArgs.Empty);
            } else {
                return;
            }

        } else {
            if(automaticWeapon && playerIsHoldingDownShoot) {
                Shoot();
            }
        }
    }

    private void StartTransferringAmmoFromBagInGun() {
        transferringAmmoFromBagTimer = 0;
        transferringAmmoFromBag = true;
        TransferNextAmmoFromBag();
    }

    private void TransferNextAmmoFromBag() {
        int ammoAmountInBag = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.ammo).Count;

        if (ammoAmountInBag > 0 && heldGun.GetCurrentAmmoClip() < heldGun.GetMaxAmmo()) {
            UICurrencyManager.PlayerInventoryUI.DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
        } else {
            transferringAmmoFromBag = false;
        }
        
    }

    public void AddAmmoClip(int ammoCount) {

        int ammoRefilled = ammoCount;
        if(heldGun.GetCurrentAmmoClip() + ammoRefilled > heldGun.GetMaxAmmo()) {
            ammoRefilled = heldGun.GetMaxAmmo() - heldGun.GetCurrentAmmoClip();
        }

        if (ammoRefilled == 0) return;
        heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() + ammoRefilled);

        OnPlayerAmmoRefilled?.Invoke(this, new OnAmmoRefilledEventArgs {
            ammoAmount = ammoRefilled
        });
    }

    private void UIOrbManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.ammo), ammoSpawnPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetMovingForPayment(true, 5f, ammoDestinationPoint);
            collectible.SetScale(.5f);
        }
    }

    private void GameInput_OnPlayerReloadPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!canShoot) return;
        if (swappingGun) return;

        playerJustPressedReload = true;
        playerJustPressedReloadTimer = 0;
    }

    private void GameInput_OnWeaponSecondaryAbilitytPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        bool secondaryAbilityUnlocked = heldGun.GetSecondaryAbilityUnlocked();
        if (useDebugGun) {
            secondaryAbilityUnlocked = true;
        }

        if (!secondaryAbilityUnlocked) return;
        if (!canShoot) return;
        if (reloading) return;

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper) {
            OnPlayerAimedSightStarted?.Invoke(this, EventArgs.Empty);
            secondaryAbilityActive = true;
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Shotgun) {
            if (coolingDown) return;
            if (!canShoot) return;
            if (heldGun.GetCurrentAmmoClip() < 0 || heldGun.GetCurrentBullet() == 0) {
                OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
                return;
            }

            loadingShot = true;
            loadingShotTime = 3f;
            loadingShotTimer = 0f;
            shotLoaded = false;

            OnPlayerFocusBlastStarted?.Invoke(this, EventArgs.Empty);
            secondaryAbilityActive = true;
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.UZI) {
            float shootCooldownBuffValue = 1.4f;
            PlayerStats.Instance.BuffShootCooldown(shootCooldownBuffValue);

            OnPlayerOverclockedSMGStarted?.Invoke(this, EventArgs.Empty);
            secondaryAbilityActive = true;
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Rifle) {
            rifleSemiAutoModeActive = !rifleSemiAutoModeActive;

            if (rifleSemiAutoModeActive) {
                automaticWeapon = true;
            } else {
                automaticWeapon = false;
            }

            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.LMG)
        {
            if (settingUpLMG) return;
            if(!secondaryAbilityActive)
            {
                gunRecoil = 0f;
                gunKnockback = 0f;

                PlayerAim.Instance.SetGunStraight();
                PlayerAim.Instance.SetLimitAimAngle(true, 10);
                OnPlayerSetupLMGStarted?.Invoke(this, EventArgs.Empty);
                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                settingUpLMG = true;
                setupLMGTimer = 0f;
                canShoot = false;

                secondaryAbilityActive = true;
                holdingStationaryGun = true;

            } else
            {
                gunRecoil = heldGunSO.gunRecoil;
                gunKnockback = heldGunSO.gunKnockback;

                OnPlayerSetupLMGStopped?.Invoke(this, EventArgs.Empty);
                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                settingUpLMG = true;
                setupLMGTimer = 0f;
                canShoot = false;

                secondaryAbilityActive = false;

            }

        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Revolver) {
            if (GetCurrentBullets() == 0) return;
            if (!secondaryAbilityActive) {
                gunRecoil = .25f;
                gunKnockback = 1f;

                OnPlayerEmptyRevolverMagStart?.Invoke(this, EventArgs.Empty);

                secondaryAbilityActive = true;
                emptyingRevolverMag = true;

                StartCoroutine(EmptyRevolverMag());
            }

        }
    }

    private void GameInput_OnWeaponSecondaryAbilityCanceled(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (secondaryAbilityActive) {

            if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper) {
                OnPlayerAimedSightEnded?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }

            if (heldGun.GetGunSO().gunType == GunSO.GunType.Shotgun) {
                loadingShot = false;

                OnPlayerFocusBlastStopped?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }

            if (heldGun.GetGunSO().gunType == GunSO.GunType.UZI) {
                float shootCooldownBuffValue = 1.4f;
                PlayerStats.Instance.DebuffShootCooldown(shootCooldownBuffValue);
                OnPlayerOverclockedSMGStopped?.Invoke(this, EventArgs.Empty);
                secondaryAbilityActive = false;
            }

        }
    }

    private void GameInput_OnPlayerReloadCanceled(object sender, EventArgs e) {
        if (!playerJustPressedReload) {
            // Player is transferring ammo from bag in gun
            transferringAmmoFromBag = false;
            return;
        };

        playerJustPressedReload = false;
        playerJustPressedReloadTimer = 0;

        if (heldGun.GetCurrentBullet() == heldGun.GetBulletsPerAmmoClip()) return;
        if (reloading) return;
        if (coolingDown) return;

        if (heldGun.GetCurrentAmmoClip() == 0) {
            OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);

            if(UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.ammo).Count > 0) {
                OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag?.Invoke(this, EventArgs.Empty);
            }

            return;
        };

        ReloadGun();
    }

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

        if (secondayGunSO != null) {
            if (heldGunSO == secondayGunSO) return;
            StartCoroutine(SetActiveGunAfterDelay(secondayGunSO, false));
        }
    }

    private void GameInput_OnPlayerPrimaryGunSelected(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!CanSwapGun()) return;

        if (useDebugGun) {
            primaryGunSO = debugGun;
        }

        if (secondayGunSO != null) {
            if (heldGunSO == primaryGunSO) return;
            StartCoroutine(SetActiveGunAfterDelay(primaryGunSO));
        }
    }

    private void SwapGun() {
        if (!CanSwapGun()) return;

        if (heldGunSO == secondayGunSO) {
            StartCoroutine(SetActiveGunAfterDelay(primaryGunSO));
        }
        else {
            StartCoroutine(SetActiveGunAfterDelay(secondayGunSO, false));
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
        SetActiveGun(gunSO, primaryGunSO);
        float swapGunEndAnimationTime = .35f * heldGun.GetSwapToWeaponTimeMultiplier();

        yield return new WaitForSeconds(swapGunEndAnimationTime);
        swappingGun = false;
    }

    private void ReloadGun() {
        reloading = true;
        reloadingHands = true;
        playerJustPressedReload = false;
        reloadTimer = 0;
        reloadTime = PlayerStats.Instance.GetReloadTime();
        handsReloadTime = PlayerStats.Instance.GetHandsReloadTime();
        OnPlayerReload?.Invoke(this, EventArgs.Empty);
    }

    private void EndHandReload() {
        reloadingHands = false;
        heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
        OnPlayerReloadHandEnded?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator EmptyRevolverMag() {
        int remainingBullets = GetCurrentBullets();
        float delayBetweenBullets = .175f;

        for (int i = 0; i < remainingBullets; i++) {
            Shoot();
            if(i < remainingBullets-1) {
                yield return new WaitForSeconds(delayBetweenBullets);
            }
        }

        OnPlayerEmptyRevolverMagEnd?.Invoke(this, EventArgs.Empty);
        secondaryAbilityActive = false;

        gunRecoil = heldGunSO.gunRecoil;
        gunKnockback = heldGunSO.gunKnockback;

        float cooldownDelay = 2f;
        yield return new WaitForSeconds(cooldownDelay);

        emptyingRevolverMag = false;
    }

    private void PlayerStats_OnPlayerAmmoRegenTimeChanged(object sender, EventArgs e) {
        hasAmmoRegen = true;
        ammoRegenTime = PlayerStats.Instance.GetAmmoRegenTime();
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        canShoot = true;
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (reloadingHands) {
            reloading = false;
            reloadingHands = false; 
            OnPlayerReloadInterrupted?.Invoke(this, EventArgs.Empty);
        }
        canShoot = false;
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!canShoot) return;
        if (coolingDown) return;
        if (reloading) return;
        if (emptyingRevolverMag) return;
        if (swappingGun) return;
        if (loadingShot && !shotLoaded) return;

        if (Player.Instance.GetHP() == 0) return;

        if(heldGun.GetCurrentAmmoClip() < 0 || heldGun.GetCurrentBullet() ==0) {
            OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
        } else {
            Shoot();
            playerIsHoldingDownShoot = true;
        }
    }

    private void GameInput_OnPlayerShootCanceled(object sender, System.EventArgs e) {
        playerIsHoldingDownShoot = false;
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

    public int GetDamagePerBullet() {
        return heldGun.GetDamagePerBullet();
    }

    public float GetBulletKnockback() {
        return heldGun.GetBulletKnockback();
    }
    
    public int GetCurrentAmmoClip() {
        return heldGun.GetCurrentAmmoClip();
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

    public List<GunSO> GetAllGunSOList() {
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

    public bool GetReloadingHands() {
        return reloadingHands;
    }


    public bool GetHoldingStationaryGun() {
        return holdingStationaryGun;
    }

    #endregion

    public void SaveAllGunStats() {
        foreach(Gun gun in allGunsList) {
            gun.SaveMetaParameters();
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerShootCanceled -= GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed -= GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed -= GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled -= GameInput_OnPlayerReloadCanceled;
        GameInput.Instance.OnPlayerPrimaryGunSelected -= GameInput_OnPlayerPrimaryGunSelected;
        GameInput.Instance.OnPlayerSecondaryGunSelected -= GameInput_OnPlayerSecondaryGunSelected;
        GameInput.Instance.OnPlayerSwapGunPerformed -= GameInput_OnPlayerSwapGunPerformed;

        GameInput.Instance.OnWeaponSecondaryAbilityCanceled -= GameInput_OnWeaponSecondaryAbilityCanceled;
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed -= GameInput_OnWeaponSecondaryAbilitytPerformed;

        PlayerStats.Instance.OnPlayerAmmoRegenTimeChanged -= PlayerStats_OnPlayerAmmoRegenTimeChanged;
        PlayerMovement.Instance.OnPlayerRoll -= PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded -= PlayerMovement_OnPlayerRollEnded;

        if (UICurrencyManager.PlayerInventoryUI != null) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped -= UIOrbManager_OnCurrencyDropped;
        }
    }

}
