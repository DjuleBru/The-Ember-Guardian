using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected ParticleSystem shootPS;
    [SerializeField] protected GunSO gunSO;
    [SerializeField] protected Animator gunBodyAnimator;
    [SerializeField] protected Animator armBodyAnimator;

    protected bool gunActive;
    protected bool gunUnlocked;
    protected bool secondaryAbilityUnlocked;
    protected bool lerpingGunAngle;

    protected int pelletsPerBullet = 1;
    protected int damagePerBullet;
    protected int currentAmmoClip;
    protected int maxAmmo;
    protected int currentBullet;
    protected int shotsPerClip;
    protected float cooldownTime;
    protected float reloadTime;
    protected float handsReloadTime;
    protected float swapToWeaponTimeMultiplier;

    protected float bulletLifetime;
    protected float bulletSpeed;
    protected float defaultAngle; // Angle initial du cône (en degrés)
    protected float sightAngle; // Angle resserré du cône lorsqu'on vise
    protected float overclockedAngle; // Angle resserré du cône lorsqu'on vise
    protected float emptyRevolverAngle = 6f; // Angle resserré du cône lorsqu'on vise
    protected float lmgSetupAngle; // Angle resserré du cône lorsqu'on vise
    protected float adjustmentSpeed = 5f; // Vitesse de transition (plus grand = plus rapide)
    protected float critChance = .15f;
    protected float reloadAccelerationFactor;
    protected float weightAccelerationFactor;

    protected float currentAngle; // L'angle actuel du cône
    protected float targetAngle; // L'angle cible vers lequel le cône doit se diriger
    protected float focusedBlastAngle = 1f; // L'angle cible vers lequel le cône doit se diriger

    public static event EventHandler OnAnyGunMaxAmmoChanged;
    public static event EventHandler OnAnyGunUnlocked;


    protected void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerOverclockedSMGStarted += Playershoot_OnPlayerOverclockedSMGStarted;
        PlayerShoot.Instance.OnPlayerOverclockedSMGStopped += PlayerShoot_OnPlayerOverclockedSMGStopped;
        PlayerShoot.Instance.OnPlayerSetupLMGBipod += PlayerShoot_OnPlayerSetupLMGStarted;
        PlayerShoot.Instance.OnPlayerSetupLMGStopped += PlayerShoot_OnPlayerSetupLMGStopped;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAim_OnPlayerAimSightStarted;
        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAim_OnPlayerAimSightEnded;
        PlayerShoot.Instance.OnPlayerEmptyRevolverMagEnd += PlayerShoot_OnPlayerEmptyRevolverMagEnd;
        PlayerShoot.Instance.OnPlayerEmptyRevolverMagStart += PlayerShoot_OnPlayerEmptyRevolverMagStart;
        PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerShoot_OnPlayerFocusBlastStarted;
        PlayerShoot.Instance.OnPlayerFocusBlastStopped += PlayerShoot_OnPlayerFocusBlastStopped;
    }

    protected void Update() {
        
        if (lerpingGunAngle) {
            // Interpolation linéaire vers l'angle cible
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * adjustmentSpeed);

            // Appliquer l'angle au Particle System (conversion en radians)
            ParticleSystem.ShapeModule shape = shootPS.shape;
            shape.angle = currentAngle;

            if (Mathf.Abs(currentAngle - targetAngle) < 0.1f) {
                lerpingGunAngle = false;
            }
        };
    }

    private void PlayerShoot_OnPlayerOverclockedSMGStopped(object sender, System.EventArgs e) {
        // Rétablit l'angle par défaut pour desserrer le cône
        lerpingGunAngle = true;
        targetAngle = defaultAngle;
    }

    private void Playershoot_OnPlayerOverclockedSMGStarted(object sender, System.EventArgs e) {
        // Augmente l'angle
        targetAngle = overclockedAngle;
        lerpingGunAngle = true;
    }
    private void PlayerShoot_OnPlayerSetupLMGStopped(object sender, System.EventArgs e)
    {
        // Rétablit l'angle par défaut pour desserrer le cône
        lerpingGunAngle = true;
        targetAngle = defaultAngle;
    }

    private void PlayerShoot_OnPlayerSetupLMGStarted(object sender, System.EventArgs e)
    {
        // Augmente l'angle
        targetAngle = lmgSetupAngle;
        lerpingGunAngle = true;
    }

    private void PlayerShoot_OnPlayerEmptyRevolverMagStart(object sender, EventArgs e) {
        ParticleSystem.ShapeModule shape = shootPS.shape;
        shape.angle = emptyRevolverAngle;
    }

    private void PlayerShoot_OnPlayerEmptyRevolverMagEnd(object sender, EventArgs e) {
        ParticleSystem.ShapeModule shape = shootPS.shape;
        shape.angle = defaultAngle;
    }

    protected void PlayerAim_OnPlayerAimSightEnded(object sender, System.EventArgs e) {
        // Réduit l'angle pour resserrer le cône
        lerpingGunAngle = true;
        targetAngle = defaultAngle;
    }

    protected void PlayerAim_OnPlayerAimSightStarted(object sender, System.EventArgs e) {
        // Rétablit l'angle par défaut pour desserrer le cône
        targetAngle = sightAngle;
        lerpingGunAngle = true;
    }

    private void PlayerShoot_OnPlayerFocusBlastStopped(object sender, System.EventArgs e) {
        // Réduit l'angle pour resserrer le cône
        lerpingGunAngle = true;
        targetAngle = defaultAngle;

        pelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(gunSO);
        damagePerBullet = MetaProgressionManager.Instance.GetGunDamagePerBullet(gunSO);
        ParticleSystem.MainModule shootPSMainModule = shootPS.main;
        shootPSMainModule.startSize = .2f;
    }

    private void PlayerShoot_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        // Rétablit l'angle par défaut pour desserrer le cône

        targetAngle = focusedBlastAngle;
        lerpingGunAngle = true;

        float sizePerBullet = .04f;
        float totalBullerSize = sizePerBullet * (pelletsPerBullet * (PlayerShoot.Instance.GetCurrentBullets()));

        if(totalBullerSize < .4f) {
            totalBullerSize = .4f;
        }

        damagePerBullet *= (pelletsPerBullet * (PlayerShoot.Instance.GetCurrentBullets()));
        pelletsPerBullet = 1;
        ParticleSystem.MainModule shootPSMainModule = shootPS.main;
        shootPSMainModule.startSize = totalBullerSize;
    }

    public void RefreshGunStats() {
        gunUnlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);

        pelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(gunSO);

        maxAmmo = MetaProgressionManager.Instance.GetGunMaxAmmo(gunSO);
        damagePerBullet = MetaProgressionManager.Instance.GetGunDamagePerBullet(gunSO);
        shotsPerClip = MetaProgressionManager.Instance.GetGunShotsPerClip(gunSO);
        critChance = MetaProgressionManager.Instance.GetGunCritChance(gunSO);
        cooldownTime = MetaProgressionManager.Instance.GetGunCooldown(gunSO);
        reloadTime = MetaProgressionManager.Instance.GetGunReloadTime(gunSO);
        handsReloadTime = MetaProgressionManager.Instance.GetHandsGunReloadTime(gunSO);
        swapToWeaponTimeMultiplier = MetaProgressionManager.Instance.GetSwapToWeaponTimeMultiplier(gunSO);
        secondaryAbilityUnlocked = MetaProgressionManager.Instance.GetGunSecondaryAbilityUnlocked(gunSO);
        pelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(gunSO);
        bulletLifetime = MetaProgressionManager.Instance.GetGunBulletLifetime(gunSO);
        bulletSpeed = MetaProgressionManager.Instance.GetGunBulletSpeed(gunSO);
        reloadAccelerationFactor = MetaProgressionManager.Instance.GetGunReloadAccelerationFactor(gunSO);
        weightAccelerationFactor = MetaProgressionManager.Instance.GetGunWeightAccelerationFactor(gunSO);

        defaultAngle = MetaProgressionManager.Instance.GetGunShootConeAnle(gunSO);
        defaultAngle = gunSO.shootConeAngle;
        currentAngle = defaultAngle;
        targetAngle = defaultAngle;
        sightAngle = defaultAngle / 3;
        overclockedAngle = defaultAngle * 2f;
        lmgSetupAngle = defaultAngle / 5f;

        ParticleSystem.ShapeModule shootPSShape = shootPS.shape;
        shootPSShape.angle = defaultAngle;

        ParticleSystem.MainModule shootPSMain = shootPS.main;
        shootPSMain.startLifetime = bulletLifetime;
        shootPSMain.startSpeed = bulletSpeed;

        currentBullet = shotsPerClip;

        int ammoClip = 0;
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            ammoClip = maxAmmo;
        }
        currentAmmoClip = ammoClip;
    }

    public void InitializeTutorialGun() {
        pelletsPerBullet = gunSO.pelletsPerBullet;

        maxAmmo = gunSO.maxAmmo;
        damagePerBullet = gunSO.damagePerBullet;
        shotsPerClip = gunSO.shotsPerClip;
        currentBullet = 0;
        currentAmmoClip = 0;
    }

    public void SetGunAmmo(int ammoCount, int currentBuller) {
        currentAmmoClip = ammoCount;
        currentBullet = currentBuller;
    }

    protected void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        shootPS.Emit(pelletsPerBullet);
    }

    public GunSO GetGunSO() {
        return gunSO;
    }

    #region GET PARAMETERS

    public bool GetGunUnlocked() {
        return gunUnlocked;
    }
    public Animator GetGunBodyAnimator() {
        return gunBodyAnimator;
    }
    public Animator GetArmBodyAnimator() {
        return armBodyAnimator;
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
        return shotsPerClip;
    }

    public int GetMaxAmmo() {
        return maxAmmo;
    }
    public int GetShotsPerClip() {
        return shotsPerClip;
    }
    public int GetDamagePerBullet() {
        return damagePerBullet;
    }
    public int GetPelletsPerBullet() {
        return pelletsPerBullet;
    }
    public float GetDefaultShootAngle() {
        return defaultAngle;
    }
    public float GetBulletLifetime() {
        return bulletLifetime;
    }
    public float GetReloadTime() {
        return reloadTime;
    }
    public float GetHandsReloadTime() {
        return handsReloadTime;
    }
    public float GetSwapToWeaponTimeMultiplier() {
        return swapToWeaponTimeMultiplier;
    }
    public float GetCooldownTime() {
        return cooldownTime;
    }
    public float GetCritChance() {
        return critChance;
    }

    public float GetReloadAccelerationFactor() {
        return reloadAccelerationFactor;
    }
    public float GetWeightAccelerationFactor() {
        return weightAccelerationFactor;
    }

    public bool GetSecondaryAbilityUnlocked() {
        return secondaryAbilityUnlocked;
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

    #region SET META PARAMETERS

    public void SetGunUnlocked() {
        gunUnlocked = true;
        OnAnyGunUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public void SetSecondaryAbilityUnlocked() {
        secondaryAbilityUnlocked = true;
    }

    public void SetBulletDamage_Meta(int bulletDamage) {
        this.damagePerBullet = bulletDamage;
    }
    public void SetShotsPerClip_Meta(int shotsPerClip) {
        this.shotsPerClip = shotsPerClip;
    }
    public void SetMaxAmmo_Meta(int maxAmmo) {
        this.maxAmmo = maxAmmo;
        OnAnyGunMaxAmmoChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SetCooldownTime_Meta(float cooldownTime) {
        this.cooldownTime = cooldownTime;
    }
    public void SetReloadTime_Meta(float reloadTime) {
        this.reloadTime = reloadTime;
    }
    public void SetCritChange_Meta(float critChance) {
        this.critChance = critChance;
    }
    public void SetShootConeAngle_Meta(float shootConeAngle) {
        this.defaultAngle = shootConeAngle;
        ParticleSystem.ShapeModule shootPSShape = shootPS.shape;
        shootPSShape.angle = defaultAngle;
    }
    public void SetPelletsPerBullet_Meta(int pelletsPerBullet) {
        this.pelletsPerBullet = pelletsPerBullet;
    }

    public void SetGunBulletLifetime_Meta(float bulletLifetime) {
        this.bulletLifetime = bulletLifetime;

        ParticleSystem.MainModule shootPSMain = shootPS.main;
        shootPSMain.startLifetime = bulletLifetime;
        shootPSMain.startSpeed = bulletSpeed;
    }

    #endregion

    public void SaveMetaParameters() {
        if (!gunUnlocked) return;

        MetaProgressionManager.Instance.SetGunDamagePerBullet(gunSO, damagePerBullet);
        MetaProgressionManager.Instance.SetGunShotsPerClip(gunSO, shotsPerClip);
        MetaProgressionManager.Instance.SetGunMaxAmmo(gunSO, maxAmmo);
        MetaProgressionManager.Instance.SetGunCooldown(gunSO, cooldownTime);
        MetaProgressionManager.Instance.SetGunReloadTime(gunSO, reloadTime);
        MetaProgressionManager.Instance.SetGunCritChance(gunSO, critChance);
        MetaProgressionManager.Instance.SetGunShootConeAnle(gunSO, defaultAngle);
        MetaProgressionManager.Instance.SetGunPelletsPerBullet(gunSO, pelletsPerBullet);
        MetaProgressionManager.Instance.SetGunBulletLifetime(gunSO, bulletLifetime);
        MetaProgressionManager.Instance.SetGunSwapToWeaponTimeMultiplier(gunSO, swapToWeaponTimeMultiplier);

        MetaProgressionManager.Instance.SetGunSecondaryAbilityUnlocked(gunSO, secondaryAbilityUnlocked);
        MetaProgressionManager.Instance.SetGunUnlocked(gunSO, gunUnlocked);
    }
}
