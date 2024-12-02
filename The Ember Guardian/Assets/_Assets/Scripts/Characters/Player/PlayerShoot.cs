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
    public event EventHandler OnPlayerReload;
    public event EventHandler OnPlayerReloadEnded;
    public event EventHandler<OnAmmoRefilledEventArgs> OnPlayerAmmoRefilled;
    public event EventHandler OnBulletsChanged;
    public event EventHandler OnPlayerSwappedGun;

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

    private bool canShoot = true;
    private bool coolingDown;
    private bool reloading;
    private bool coolDownSFXTriggered;
    private bool playerJustPressedReload;
    private bool transferringAmmoFromBag;

    private bool automaticWeapon;
    private bool playerIsHoldingDownShoot;
    private bool hasAmmoRegen;
    private float ammoRegenTime;
    private float ammoRegenTimer;

    private Gun heldGun;
    private GunSO heldGunSO;
    [SerializeField] private GunSO primaryGunSO;
    [SerializeField] private GunSO secondayGunSO;
    [SerializeField] private List<Gun> allGunsList;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitializeGuns();
        SetGun(primaryGunSO);

        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed += GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed += GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled += GameInput_OnPlayerReloadCanceled;
        GameInput.Instance.OnPlayerPrimaryGunSelected += GameInput_OnPlayerPrimaryGunSelected;
        GameInput.Instance.OnPlayerSecondaryGunSelected += GameInput_OnPlayerSecondaryGunSelected;
        GameInput.Instance.OnPlayerSwapGunPerformed += GameInput_OnPlayerSwapGunPerformed;

        PlayerStats.Instance.OnPlayerAmmoRegenTimeChanged += PlayerStats_OnPlayerAmmoRegenTimeChanged;

        UICurrencyManager.Instance.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
    }


    private void SetGun(GunSO gunSO) {
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
        PlayerStats.Instance.SetShootCooldownTime(heldGunSO.shootCooldownTime);
        PlayerStats.Instance.SetReloadTime(heldGunSO.reloadTime);

        OnPlayerSwappedGun?.Invoke(this, EventArgs.Empty);
    }

    private void InitializeGuns() {
        foreach (Gun gun in allGunsList) {
            gun.InitializeGun();
            gun.gameObject.SetActive(false);
        }
    }

    private void Update() {

        if (playerJustPressedReload) {
            playerJustPressedReloadTimer += Time.deltaTime;
            if(playerJustPressedReloadTimer > .15f) {
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
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0) {
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
        PlayerAim.Instance.AddRecoil(heldGunSO.gunRecoil, heldGunSO.gunRecoilDamping);

        float aimDir = 1f;
        if(PlayerAim.Instance.GetAimDir().x <0) {
            aimDir = -1f;
        }

        Vector2 gunKnockbackForce = new Vector2(aimDir * heldGunSO.gunKnockback * -1 , 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        heldGun.SetCurrentBullet(heldGun.GetCurrentBullet()-1);
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);

        // Handle cooldown
        if (PlayerStats.Instance.GetShootCooldownTime() != 0) {
            coolDownSFXTriggered = false;
            coolingDown = true;
            shootCooldownTimer = PlayerStats.Instance.GetShootCooldownTime();
        };

        OnPlayerShot?.Invoke(this, EventArgs.Empty);
    }

    private void CooldownFinished() {
        coolingDown = false;

        // Handle reload
        if (heldGun.GetCurrentBullet() <= 0) {
            reloading = true;
            reloadTimer = PlayerStats.Instance.GetReloadTime();

            heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
            OnPlayerReload?.Invoke(this, EventArgs.Empty);
        } else {
            if(automaticWeapon && playerIsHoldingDownShoot) {
                Shoot();
            }
        }
    }

    private void StartTransferringAmmoFromBagInGun() {
        Debug.Log("StartTransferringAmmoFromBagInGun");
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

        ReloadGun();
    }

    private void GameInput_OnPlayerSwapGunPerformed(object sender, EventArgs e) {
        SwapGun();
    }

    private void GameInput_OnPlayerSecondaryGunSelected(object sender, EventArgs e) {
        SetGun(secondayGunSO);
    }

    private void GameInput_OnPlayerPrimaryGunSelected(object sender, EventArgs e) {
        SetGun(primaryGunSO);
    }

    private void ReloadGun() {
        heldGun.SetCurrentAmmoClip(heldGun.GetCurrentAmmoClip() - 1);
        reloading = true;
        playerJustPressedReload = false;
        reloadTimer = PlayerStats.Instance.GetReloadTime();
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

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (coolingDown) return;
        if (reloading) return;
        if (!canShoot) return;
        if (Player.Instance.GetHP() == 0) return;

        if(heldGun.GetCurrentAmmoClip() == 0) {
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

    private void OnDestroy() {
        GameInput.Instance.OnPlayerShootCanceled -= GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootPerformed -= GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed -= GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerReloadCanceled -= GameInput_OnPlayerReloadCanceled;
    }

}
