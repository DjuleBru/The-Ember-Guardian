using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance;

    public event EventHandler OnPlayerShot;
    public event EventHandler OnPlayerTryShoot_OutOfAmmo;
    public event EventHandler OnPlayerShootStopped;
    public event EventHandler OnPlayerCooldownTrigger;
    public event EventHandler OnPlayerCooldownAnimationTrigger;
    public event EventHandler OnPlayerReload;
    public event EventHandler OnPlayerReloadHandEnded;
    public event EventHandler OnPlayerReloadEnded;
    public event EventHandler<OnAmmoRefilledEventArgs> OnPlayerAmmoRefilled;
    public event EventHandler OnBulletsChanged;
    public event EventHandler OnPlayerSwappedGun;

    public event EventHandler OnPlayerAimedSightStarted;
    public event EventHandler OnPlayerAimedSightEnded;
    public event EventHandler OnPlayerSwitchedFireMode;
    public event EventHandler OnPlayerOverclockedSMGStarted;
    public event EventHandler OnPlayerOverclockedSMGStopped;
    public event EventHandler OnPlayerFocusBlastStarted;
    public event EventHandler OnPlayerFocusBlastStopped;
    public event EventHandler OnPlayerSetupLMGStarted;
    public event EventHandler OnPlayerSetupLMGStopped;

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

    private bool autoReload;
    private bool canShoot = true;
    private bool coolingDown;
    private bool reloading;
    private bool coolDownSFXTriggered;
    private bool coolDownAnimationTriggered;
    private bool playerJustPressedReload;
    private bool transferringAmmoFromBag;
    private bool secondaryAbilityActive;

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
    [SerializeField] private GunSO debugGun;
    [SerializeField] private bool useDebugGun;
    [SerializeField] private bool debugSecondaryAbilityUnlocked;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitializeGuns();

        if(useDebugGun) {
            SetGun(debugGun);
        } else {
            SetGun(PlayerSave.Instance.GetPrimaryActiveGun());
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

        UICurrencyManager.Instance.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
    }

    public void SetGun(GunSO gunSO) {
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

        automaticWeapon = gunSO.automaticWeapon;
        shootCooldownSFXTriggerTime = heldGunSO.shootCooldownSFXTriggerTime;
        gunRecoil = heldGunSO.gunRecoil;
        gunKnockback = heldGunSO.gunKnockback;

        PlayerStats.Instance.SetShootCooldownTime(heldGun.GetCooldownTime());
        PlayerStats.Instance.SetReloadTime(heldGun.GetReloadTime());
        PlayerStats.Instance.SetHandsReloadTime(heldGun.GetHandsReloadTime());

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
            if(playerJustPressedReloadTimer > .35f) {
                playerJustPressedReload = false;
                StartTransferringAmmoFromBagInGun();
            }
        }

        if(transferringAmmoFromBag) {
            transferringAmmoFromBagTimer += Time.deltaTime;
            if(transferringAmmoFromBagTimer > transferringAmmoFromBagCooldown) {
                transferringAmmoFromBagTimer = 0;
                TransferNextAmmoFromBag();
            }
        }

        if(coolingDown) {
            shootCooldownTimer -= Time.deltaTime;

            if(!coolDownAnimationTriggered) {
                OnPlayerCooldownAnimationTrigger?.Invoke(this, EventArgs.Empty);
                coolDownAnimationTriggered = true;
            }

            if(shootCooldownTimer <= (PlayerStats.Instance.GetShootCooldownTime() - shootCooldownSFXTriggerTime) && !coolDownSFXTriggered) {
                OnPlayerCooldownTrigger?.Invoke(this, EventArgs.Empty);
                coolDownSFXTriggered = true;
            }

            if(shootCooldownTimer <= 0 ) {
                CooldownFinished();
            }
            return;
        }

        if(reloading) {
            reloadTimer += Time.deltaTime;

            if(reloadTimer >= handsReloadTime && reloadingHands) {
                reloadingHands = false;
                OnPlayerReloadHandEnded?.Invoke(this, EventArgs.Empty);
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
    }

    private void Shoot() {
        PlayerAim.Instance.AddRecoil(gunRecoil, heldGunSO.gunRecoilDamping);

        float aimDir = 1f;
        if(PlayerAim.Instance.GetAimDir().x <0) {
            aimDir = -1f;
        }

        Vector2 gunKnockbackForce = new Vector2(aimDir * gunKnockback * -1 , 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        heldGun.SetCurrentBullet(heldGun.GetCurrentBullet()-1);
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);

        // Handle cooldown
        if (PlayerStats.Instance.GetShootCooldownTime() != 0) {
            coolDownSFXTriggered = false;
            coolDownAnimationTriggered = false;
            coolingDown = true;
            shootCooldownTimer = PlayerStats.Instance.GetShootCooldownTime();
        };

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
        int ammoAmountInBag = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.ammo).Count;

        if (ammoAmountInBag > 0 && heldGun.GetCurrentAmmoClip() < heldGun.GetMaxAmmo()) {
            UICurrencyManager.Instance.DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
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

    public int GetDamagePerBullet() {
        return heldGun.GetDamagePerBullet();
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


    private void UIOrbManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.ammo), ammoSpawnPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
            collectible.SetMovingForPayment(true, 5f, ammoDestinationPoint);
            collectible.SetScale(.5f);
        }
    }

    private void GameInput_OnPlayerReloadPerformed(object sender, EventArgs e) {
        if (!canShoot) return;

        playerJustPressedReload = true;
        playerJustPressedReloadTimer = 0;
    }

    private void GameInput_OnWeaponSecondaryAbilitytPerformed(object sender, EventArgs e) {
        bool secondaryAbilityUnlocked = MetaProgressionManager.Instance.GetGunSecondaryAbilityUnlocked(heldGun.GetGunSO()) || debugSecondaryAbilityUnlocked;

        if (!secondaryAbilityUnlocked) return;

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Sniper) {
            OnPlayerAimedSightStarted?.Invoke(this, EventArgs.Empty);
            secondaryAbilityActive = true;
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Shotgun) {
            if (coolingDown) return;
            if (reloading) return;
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
            float shootCooldownBuffValue = 1.5f;
            PlayerStats.Instance.BuffShootCooldown(shootCooldownBuffValue);

            OnPlayerOverclockedSMGStarted?.Invoke(this, EventArgs.Empty);
            secondaryAbilityActive = true;
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.Rifle) {
            secondaryAbilityActive = !secondaryAbilityActive;

            if (secondaryAbilityActive) {
                automaticWeapon = true;
            } else {
                automaticWeapon = false;
            }

            OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);
        }

        if (heldGun.GetGunSO().gunType == GunSO.GunType.LMG)
        {
            if(!secondaryAbilityActive)
            {
                gunRecoil = 0f;
                gunKnockback = 0f;

                Player.Instance.SetCanMove(false);
                PlayerAim.Instance.SetGunStraight();
                PlayerAim.Instance.SetLimitAimAngle(true, 10);
                OnPlayerSetupLMGStarted?.Invoke(this, EventArgs.Empty);
                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                secondaryAbilityActive = true;

            } else
            {
                gunRecoil = heldGunSO.gunRecoil;
                gunKnockback = heldGunSO.gunKnockback;

                Player.Instance.SetCanMove(true);
                PlayerAim.Instance.SetLimitAimAngle(false);
                OnPlayerSetupLMGStopped?.Invoke(this, EventArgs.Empty);
                OnPlayerSwitchedFireMode?.Invoke(this, EventArgs.Empty);

                secondaryAbilityActive = false;

            }

        }

    }

    private void GameInput_OnWeaponSecondaryAbilityCanceled(object sender, EventArgs e) {
        if(secondaryAbilityActive) {

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
                float shootCooldownBuffValue = 1.5f;
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
            return;
        };

        ReloadGun();
    }

    private void GameInput_OnPlayerSwapGunPerformed(object sender, EventArgs e) {
        if(secondayGunSO != null) {
            SwapGun();
        }
    }

    private void GameInput_OnPlayerSecondaryGunSelected(object sender, EventArgs e) {
        if(secondayGunSO != null) {
            SetGun(secondayGunSO);
        }
    }

    private void GameInput_OnPlayerPrimaryGunSelected(object sender, EventArgs e) {
        SetGun(primaryGunSO);
    }

    private void ReloadGun() {
        heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
        reloading = true;
        reloadingHands = true;
        playerJustPressedReload = false;
        reloadTimer = 0;
        reloadTime = PlayerStats.Instance.GetReloadTime();
        handsReloadTime = PlayerStats.Instance.GetHandsReloadTime();
        OnPlayerReload?.Invoke(this, EventArgs.Empty);
    }

    private void SwapGun() {
        if(heldGunSO == secondayGunSO) {
            SetGun(primaryGunSO);
        } else {
            SetGun(secondayGunSO);
        }
    }

    private void PlayerStats_OnPlayerAmmoRegenTimeChanged(object sender, EventArgs e) {
        hasAmmoRegen = true;
        ammoRegenTime = PlayerStats.Instance.GetAmmoRegenTime();
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        canShoot = true;
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        canShoot = false;
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (coolingDown) return;
        if (reloading) return;
        if (!canShoot) return;
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

    public bool GetReloadingHands() {
        return reloadingHands;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerShootCanceled -= GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed -= GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed -= GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled -= GameInput_OnPlayerReloadCanceled;
    }

}
