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
    public event EventHandler OnClipsChanged;

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

    private int currentAmmo;
    private int maxAmmo;
    private int currentClip;
    private int clipsPerAmmo;

    [SerializeField] private GunSO gunSO;

    private void Awake() {
        Instance = this;

        shootCooldownTime = gunSO.shootCooldownTime;
        shootCooldownSFXTriggerTime = gunSO.shootCooldownSFXTriggerTime;
        reloadTime = gunSO.reloadTime;

        clipsPerAmmo = gunSO.shotsPerClip;
        currentClip = clipsPerAmmo;

        maxAmmo = gunSO.maxAmmo;
        currentAmmo = maxAmmo;
    }

    private void Start() {
        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootStarted += GameInput_OnPlayerShootStarted;
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
                currentClip = clipsPerAmmo;
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

        currentClip -= 1;
        OnClipsChanged?.Invoke(this, EventArgs.Empty);

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
        if (currentClip <= 0) {
            reloading = true;
            reloadTimer = reloadTime;

            currentAmmo -= 1;
            OnPlayerReload?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddAmmo(int ammoCount) {

        int ammoRefilled = ammoCount;
        if(currentAmmo + ammoRefilled > maxAmmo) {
            ammoRefilled = maxAmmo - currentAmmo;
        }
        currentAmmo += ammoRefilled;

        OnPlayerAmmoRefilled?.Invoke(this, new OnAmmoRefilledEventArgs {
            ammoAmount = ammoRefilled
        });
    }

    public int GetCurrentAmmo() {
        return currentAmmo;
    }

    public int GetMaxAmmo() {
        return maxAmmo;
    }

    public int GetCurrentClips() {
        return currentClip;
    }

    public int GetMaxClips() {
        return clipsPerAmmo;
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (coolingDown) return;
        if (reloading) return;
        if (Player.Instance.GetHP() == 0) return;

        if(currentAmmo == 0) {
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

}
