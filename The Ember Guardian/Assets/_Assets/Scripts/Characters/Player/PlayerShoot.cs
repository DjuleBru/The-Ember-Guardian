using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance;

    public event EventHandler OnPlayerShotProjectile;
    public event EventHandler OnPlayerShootStopped;
    public event EventHandler OnPlayerReload;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilePrefab;
    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private float projectileInitialForce;

    private float shootCooldownTimer;
    private float shootCooldownTime;
    private float reloadTimer;
    private float reloadTime;
    private bool coolingDown;
    private bool reloading;

    private int currentAmmo;
    private int maxAmmo;
    private int currentClip;
    private int shotsPerClip;

    [SerializeField] private GunSO gunSO;

    private void Awake() {
        Instance = this;

        shootCooldownTime = gunSO.shootCooldownTime;
        reloadTime = gunSO.reloadTime;

        shotsPerClip = gunSO.shotsPerClip;
        currentClip = shotsPerClip;

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

            if(shootCooldownTimer <= 0 ) {
                coolingDown = false;
            }
        }
        if(reloading) {
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0) {
                currentClip = shotsPerClip;
                reloading = false;
            }
        }
    }

    private void Shoot() {
        PlayerAim.Instance.AddRecoil(gunSO.gunRecoil, gunSO.gunRecoilDamping);

        Vector2 gunKnockbackForce = new Vector2(PlayerMovement.Instance.GetLastMoveDir() * gunSO.gunKnockback * -1 , 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        shootPS.Emit(5);

        currentClip -= 1;

        if(currentClip <= 0 ) {

            reloading = true;
            reloadTimer = reloadTime;
            currentAmmo -= 1;
            OnPlayerReload?.Invoke(this, EventArgs.Empty);

        } else {

            coolingDown = true;
            shootCooldownTimer = shootCooldownTime;

        }

    }

    public int GetCurrentAmmo() {
        return currentAmmo;
    }

    public int GetMaxAmmo() {
        return maxAmmo;
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        if (coolingDown) return;
        if (reloading) return;

        Shoot();
        OnPlayerShotProjectile?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPlayerShootCanceled(object sender, System.EventArgs e) {
    }

}
