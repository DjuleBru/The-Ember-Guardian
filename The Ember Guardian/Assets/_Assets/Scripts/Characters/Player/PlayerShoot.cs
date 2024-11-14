using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance;

    public event EventHandler OnPlayerShotProjectile;
    public event EventHandler OnPlayerTryShoot_OutOfAmmo;
    public event EventHandler OnPlayerShootStopped;
    public event EventHandler OnPlayerCooldownTrigger;
    public event EventHandler OnPlayerReload;
    public event EventHandler OnPlayerReloadEnded;
    public event EventHandler<OnAmmoRefilledEventArgs> OnPlayerAmmoRefilled;
    public event EventHandler OnBulletsChanged;

    public class OnAmmoRefilledEventArgs : EventArgs {
        public int ammoAmount;
    }

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilePrefab;
    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private float projectileInitialForce;

    private float shootCooldownTimer;
    private float shootCooldownTime;
    private float shootCooldownSFXTriggerTime;
    private float reloadTimer;
    private float reloadTime;
    private bool coolingDown;
    private bool reloading;
    private bool coolDownSFXTriggered;

    private int currentAmmoClip;
    private int maxAmmo;
    private int currentBullet;
    private int bulletsPerAmmoClip;

    [SerializeField] private GunSO gunSO;

    private void Awake() {
        Instance = this;

        shootCooldownTime = gunSO.shootCooldownTime;
        shootCooldownSFXTriggerTime = gunSO.shootCooldownSFXTriggerTime;
        reloadTime = gunSO.reloadTime;

        bulletsPerAmmoClip = gunSO.shotsPerClip;
        currentBullet = bulletsPerAmmoClip;

        maxAmmo = gunSO.maxAmmo;
        currentAmmoClip = maxAmmo;
    }

    private void Start() {
        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootStarted += GameInput_OnPlayerShootStarted;
        GameInput.Instance.OnPlayerReloadPerformed += GameInput_OnPlayerReloadPerformed;
    }

    private void Update() {

        if(coolingDown) {
            shootCooldownTimer -= Time.deltaTime;

            if(shootCooldownTimer <= (shootCooldownTime - shootCooldownSFXTriggerTime) && !coolDownSFXTriggered) {
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
                currentBullet = bulletsPerAmmoClip;
                reloading = false;
                OnPlayerReloadEnded?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Shoot() {
        PlayerAim.Instance.AddRecoil(gunSO.gunRecoil, gunSO.gunRecoilDamping);

        float aimDir = 1f;
        if(PlayerAim.Instance.GetAimDir().x <0) {
            aimDir = -1f;
        }

        Vector2 gunKnockbackForce = new Vector2(aimDir * gunSO.gunKnockback * -1 , 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        shootPS.Emit(5);

        currentBullet -= 1;
        OnBulletsChanged?.Invoke(this, EventArgs.Empty);

        // Handle cooldown
        if (shootCooldownTime != 0) {
            coolDownSFXTriggered = false;
            coolingDown = true;
            shootCooldownTimer = shootCooldownTime;
        };
    }

    private void CooldownFinished() {
        coolingDown = false;

        // Handle reload
        if (currentBullet <= 0) {
            reloading = true;
            reloadTimer = reloadTime;

            currentAmmoClip -= 1;
            OnPlayerReload?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddAmmoClip(int ammoCount) {

        int ammoRefilled = ammoCount;
        if(currentAmmoClip + ammoRefilled > maxAmmo) {
            ammoRefilled = maxAmmo - currentAmmoClip;
        }
        currentAmmoClip += ammoRefilled;

        OnPlayerAmmoRefilled?.Invoke(this, new OnAmmoRefilledEventArgs {
            ammoAmount = ammoRefilled
        });
    }

    public int GetCurrentAmmoClip() {
        return currentAmmoClip;
    }

    public int GetMaxAmmoClips() {
        return maxAmmo;
    }

    public int GetCurrentBullets() {
        return currentBullet;
    }

    public int GetMaxBulletsPerClip() {
        return bulletsPerAmmoClip;
    }

    private void GameInput_OnPlayerReloadPerformed(object sender, EventArgs e) {
        if (currentBullet == bulletsPerAmmoClip) return;
        if (reloading) return;
        if (coolingDown) return;

        currentAmmoClip -= 1;
        reloading = true;
        reloadTimer = reloadTime;
        OnPlayerReload?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (coolingDown) return;
        if (reloading) return;
        if (Player.Instance.GetHP() == 0) return;

        if(currentAmmoClip == 0) {
            OnPlayerTryShoot_OutOfAmmo?.Invoke(this, EventArgs.Empty);
        } else {
            Shoot();
            OnPlayerShotProjectile?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPlayerShootCanceled(object sender, System.EventArgs e) {
    }

    public GunSO GetGunSO() {
        return gunSO;
    }

    public float GetReloadTime() {
        return reloadTime;
    }

    public float GetShootCooldownTime() {
        return shootCooldownTime;
    }

}
