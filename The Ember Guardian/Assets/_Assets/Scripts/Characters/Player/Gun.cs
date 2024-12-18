using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private GunSO gunSO;
    [SerializeField] private Animator gunBodyAnimator;

    private bool gunActive;
    private bool lerpingGunAngle;

    private int pelletsPerBullet = 1;
    private int damagePerBullet;
    private int currentAmmoClip;
    private int maxAmmo;
    private int currentBullet;
    private int bulletsPerAmmoClip;

    private float defaultAngle; // Angle initial du cône (en degrés)
    private float sightAngle; // Angle resserré du cône lorsqu'on vise
    private float adjustmentSpeed = 5f; // Vitesse de transition (plus grand = plus rapide)
    private float critChance = .15f;

    private float currentAngle; // L'angle actuel du cône
    private float targetAngle; // L'angle cible vers lequel le cône doit se diriger

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAim_OnPlayerAimSightStarted;
        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAim_OnPlayerAimSightEnded;

        defaultAngle = shootPS.shape.angle;
        currentAngle = defaultAngle;
        targetAngle = defaultAngle;
        sightAngle = defaultAngle / 2;

    }

    private void Update() {
        if (!lerpingGunAngle) return;
        // Interpolation linéaire vers l'angle cible
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * adjustmentSpeed);

        // Appliquer l'angle au Particle System (conversion en radians)
        ParticleSystem.ShapeModule shape = shootPS.shape;
        shape.angle = currentAngle;
    }

    private void PlayerAim_OnPlayerAimSightEnded(object sender, System.EventArgs e) {
        // Réduit l'angle pour resserrer le cône
        targetAngle = defaultAngle;
    }

    private void PlayerAim_OnPlayerAimSightStarted(object sender, System.EventArgs e) {
        // Rétablit l'angle par défaut pour desserrer le cône
        targetAngle = sightAngle;
    }

    public void InitializeGun() {
        pelletsPerBullet = gunSO.pelletsPerBullet;

        maxAmmo = gunSO.maxAmmo;
        damagePerBullet = gunSO.damagePerBullet;
        bulletsPerAmmoClip = gunSO.shotsPerClip;
        critChance = gunSO.critChance;
        currentBullet = bulletsPerAmmoClip;
        currentAmmoClip = maxAmmo;
    }

    public void InitializeTutorialGun() {
        pelletsPerBullet = gunSO.pelletsPerBullet;

        maxAmmo = gunSO.maxAmmo;
        damagePerBullet = gunSO.damagePerBullet;
        bulletsPerAmmoClip = gunSO.shotsPerClip;
        currentBullet = 0;
        currentAmmoClip = 0;
    }

    public void SetGunAmmo(int ammoCount, int currentBuller) {
        currentAmmoClip = ammoCount;
        currentBullet = currentBuller;
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        shootPS.Emit(pelletsPerBullet);
    }

    public GunSO GetGunSO() {
        return gunSO;
    }

    #region GET PARAMETERS

    public Animator GetGunBodyAnimator() {
        return gunBodyAnimator;
    }

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

    public float GetCritChance() {
        return critChance;
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
