using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private GunSO gunSO;

    private bool gunActive;

    private int pelletsPerBullet = 1;
    private int damagePerBullet;
    private int currentAmmoClip;
    private int maxAmmo;
    private int currentBullet;
    private int bulletsPerAmmoClip;

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;

    }

    public void InitializeGun() {
        pelletsPerBullet = gunSO.pelletsPerBullet;

        maxAmmo = gunSO.maxAmmo;
        damagePerBullet = gunSO.damagePerBullet;
        bulletsPerAmmoClip = gunSO.shotsPerClip;
        currentBullet = bulletsPerAmmoClip;
        currentAmmoClip = maxAmmo;
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        shootPS.Emit(pelletsPerBullet);
    }

    public GunSO GetGunSO() {
        return gunSO;
    }

    #region GET PARAMETERS
    public bool GetGunActive() {
        return gunActive;
    }

    public int GetCurrentAmmoClip() {
        return currentAmmoClip;
    }

    public int GetCurrentBullet() {
        return currentBullet;
    }

    public int GetBulletsPerAmmoClip() {
        return bulletsPerAmmoClip;
    }
    public int GetMaxAmmo() {
        return maxAmmo;
    }
    public int GetDamagePerBullet() {
        return damagePerBullet;
    }

    #endregion

    #region SET PARAMETERS

    public void SetGunActive(bool gunActive) {
        this.gunActive = gunActive;
    }

    public void SetCurrentAmmoClip(int currentAmmoClip) {
        this.currentAmmoClip = currentAmmoClip;
    }

    public void SetCurrentBullet(int currentBullet) {
        this.currentBullet = currentBullet;
    } 

    #endregion
}
