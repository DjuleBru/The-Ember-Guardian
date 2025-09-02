using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected ParticleSystem shootPS;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform projectileSpawnPosition;
    [SerializeField] protected GunSO gunSO;
    [SerializeField] protected Animator gunBodyAnimator;
    [SerializeField] protected Animator armBodyAnimator;
    protected GunJamHandler gunJamHandler;

    protected bool gunActive;
    protected bool gunUnlocked;
    protected bool secondaryAbilityUnlocked;
    protected bool lerpingGunAngle;
    protected bool lastBulletShot;
    protected bool gunJammed;
    protected bool gunJustJammed;
    protected bool gunJamInCooldown;
    protected float gunJustJammedTimer;
    protected float gunJustJammedTime = 1f;
    protected float delayBetweenJams = 60f;

    protected int pelletsPerBullet = 1;
    protected int damagePerBulletAtRunStart;
    protected int damagePerBullet;
    protected float explosionRadiusMultiplier = 1;
    protected static float gunDamageBuffMultiplier = 1;
    protected static float totalBuffMultiplier = 1;
    protected float bulletKnockback;
    protected int currentAmmoClip;
    protected int maxAmmo;
    protected int currentBullet;
    protected int shotsPerClip;
    protected float cooldownTime;
    protected float reloadTime;
    protected float handsReloadTime;
    protected float swapToWeaponTimeMultiplier;
    protected float shootCreatureHearMultiplier;

    protected int jamRepairHitAmount;
    protected float jamProbability;
    protected bool damageSurgeBuffed;
    protected bool damageSurgeBuffedLastBullet;
    protected int bulletAfterPerfectJamSucceededIndex;
    protected int surgeWindowBulletAmountBuffed;
    protected float perfectJamDamageBuff = 2f;

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
    protected float spinUpDuration;
    protected int subExplosivesAmount;
    protected int subExplosivesDamage;

    protected float currentAngle; // L'angle actuel du cône
    protected float targetAngle; // L'angle cible vers lequel le cône doit se diriger
    protected float focusedBlastAngle = .1f; // L'angle cible vers lequel le cône doit se diriger
    protected float focusedBlastDamageBuff;

    public static event EventHandler OnAnyGunMaxAmmoChanged;
    public static event EventHandler OnAnyGunStatsUpgraded;
    public static event EventHandler OnAnyGunUnlocked;
    public static event EventHandler OnAnyGunJammed;
    public static event EventHandler OnAnyGunJamRepaired;
    public static event EventHandler OnAnyGunJamBuffedDamageShot;
    public event EventHandler OnGunJammed;
    public event EventHandler OnPerfectQTEDamageBuff;
    public event EventHandler OnPerfectQTEDamageBuffEnded;
    public event EventHandler OnBuffedLastBulletShot;
    public event EventHandler OnDebuffLastBulletShot;

    protected virtual void Start() {
        gunJamHandler = GetComponent<GunJamHandler>();

        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerShoot_OnPlayerFocusBlastStarted;
        PlayerShoot.Instance.OnPlayerFocusBlastStopped += PlayerShoot_OnPlayerFocusBlastStopped;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;

        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg += PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg += PlayerSkills_OnPlayerInFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg += PlayerSkills_OnPlayerOutFireLightBuffed;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg += PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
    }


    protected virtual void Update() {

        if (DebugManager.Instance.GetGunJamDebugInputsAllowed() && gunActive) {
            if (Input.GetKeyDown(KeyCode.J)) {
                JamGun();
            }
        }

        if (!gunJamInCooldown) return;

        gunJustJammedTimer += Time.deltaTime;
        if(gunJustJammedTimer > gunJustJammedTime && gunJustJammed) {
            gunJustJammed = false;
        }

        if (gunJustJammedTimer > delayBetweenJams) {
            gunJamInCooldown = false;
        }

    }


    protected void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (!gunActive) return;
        RecalculateDamage();
    }

    protected void PlayerShoot_OnPlayerFocusBlastStopped(object sender, System.EventArgs e) {
        if (gunSO.gunType != GunSO.GunType.Shotgun) return;
        SetPSShootAngle(defaultAngle);

        pelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(gunSO);
        DebuffBulletDamage(focusedBlastDamageBuff);
        ParticleSystem.MainModule shootPSMainModule = shootPS.main;
        shootPSMainModule.startSize = .2f;

    }

    protected void PlayerShoot_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        if (gunSO.gunType != GunSO.GunType.Shotgun) return;
        SetPSShootAngle(focusedBlastAngle);

        float sizePerBullet = .04f;
        float totalBullerSize = sizePerBullet * (pelletsPerBullet * (PlayerShoot.Instance.GetCurrentBullets()));

        if(totalBullerSize < .4f) {
            totalBullerSize = .4f;
        }

        focusedBlastDamageBuff = pelletsPerBullet * PlayerShoot.Instance.GetCurrentBullets();
        BuffBulletDamage(focusedBlastDamageBuff);
        pelletsPerBullet = 1;
        ParticleSystem.MainModule shootPSMainModule = shootPS.main;
        shootPSMainModule.startSize = totalBullerSize;
    }

    protected void SetPSShootAngle(float angle) {
        ParticleSystem.ShapeModule shootPSShapeModule = shootPS.shape;
        shootPSShapeModule.angle = angle;
    }

    protected void PlayerSkills_OnPlayerOutFireLightDebuffedDmg(object sender, System.EventArgs e) {
        if (!gunActive) return;
        Debug.Log("PlayerSkills_OnPlayerOutFireLightDebuffedDmg " + PlayerSkills.Instance.GetDamageOutFireLightCurrentlyBuffed());
        if (!PlayerSkills.Instance.GetDamageOutFireLightCurrentlyBuffed()) return;

        DebuffBulletDamage(PlayerSkills.Instance.GetDamageBuffOutFireLight());
    }

    protected void PlayerSkills_OnPlayerInFireLightDebuffedDmg(object sender, System.EventArgs e) {
        if (!gunActive) return;
        if (!PlayerSkills.Instance.GetDamageInFireLightCurrentlyBuffed()) return;

        DebuffBulletDamage(PlayerSkills.Instance.GetDamageBuffInFireLight());
    }

    protected void PlayerSkills_OnPlayerOutFireLightBuffed(object sender, System.EventArgs e) {
        if (!gunActive) return;
        if (PlayerSkills.Instance.GetDamageOutFireLightCurrentlyBuffed()) return;

        BuffBulletDamage(PlayerSkills.Instance.GetDamageBuffOutFireLight());
    }

    protected void PlayerSkills_OnPlayerInFireLightBuffedDmg(object sender, System.EventArgs e) {
        if (!gunActive) return;
        if (PlayerSkills.Instance.GetDamageInFireLightCurrentlyBuffed()) return;

        BuffBulletDamage(PlayerSkills.Instance.GetDamageBuffInFireLight());
    }

    public virtual void RefreshGunStats() {
        gunUnlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);

        pelletsPerBullet = MetaProgressionManager.Instance.GetGunPelletsPerBullet(gunSO);

        maxAmmo = MetaProgressionManager.Instance.GetGunMaxAmmo(gunSO);
        damagePerBullet = MetaProgressionManager.Instance.GetGunDamagePerBullet(gunSO);
        damagePerBulletAtRunStart = damagePerBullet;
        explosionRadiusMultiplier = MetaProgressionManager.Instance.GetGunExplosionRadiusMultiplier(gunSO);
        bulletKnockback = MetaProgressionManager.Instance.GetGunBulletKnockback(gunSO);
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
        jamRepairHitAmount = MetaProgressionManager.Instance.GetGunJamRepairHitAmount(gunSO);
        surgeWindowBulletAmountBuffed = MetaProgressionManager.Instance.GetGunSurgeWindowBulletsAmountBuffed(gunSO);
        spinUpDuration = MetaProgressionManager.Instance.GetGunSpinUpDuration(gunSO);
        subExplosivesAmount = MetaProgressionManager.Instance.GetGunSubExplosivesAmount(gunSO);
        subExplosivesDamage = MetaProgressionManager.Instance.GetGunSubExplosivesDamage(gunSO);

        jamProbability = gunSO.jamProbability;
        shootCreatureHearMultiplier = gunSO.shootCreatureHearMultiplier;

        defaultAngle = MetaProgressionManager.Instance.GetGunShootConeAnle(gunSO);
        currentAngle = defaultAngle;
        targetAngle = defaultAngle;
        sightAngle = defaultAngle / 3;
        overclockedAngle = defaultAngle * 3f;
        lmgSetupAngle = defaultAngle / 5f;

        if(gunSO.bulletIsParticle) {
            ParticleSystem.ShapeModule shootPSShape = shootPS.shape;

            if(gunSO.useAngleInPS) {
               shootPSShape.angle = defaultAngle;
            } else {
                shootPSShape.angle = 0.01f;
            }
            
            ParticleSystem.MainModule shootPSMain = shootPS.main;
            shootPSMain.startLifetime = bulletLifetime;
            shootPSMain.startSpeed = bulletSpeed;
        }

        currentBullet = shotsPerClip;

        int ammoClip = 0;
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            ammoClip = maxAmmo;
        }
        currentAmmoClip = ammoClip;
    }

    public void SetGunAmmo(int ammoCount, int currentBuller) {
        currentAmmoClip = ammoCount;
        currentBullet = currentBuller;
    }

    protected virtual void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (!gunActive) return;

        //Check Passive SKills
        CheckPassiveSkillEffectsOnBullet();
        HandleGunJams();

        Shoot();
    }

    protected void Player_OnPlayerDied(object sender, EventArgs e) {
        if (!gunActive) return;

        if (damageSurgeBuffed) {
            damageSurgeBuffed = false;
            OnPerfectQTEDamageBuffEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual void Shoot() {
        if(gunSO.bulletIsSprite) {
            return;
        }

        if (gunSO.bulletIsParticle) {
            shootPS.Emit(pelletsPerBullet);
        }

        if (gunSO.bulletIsProjectile) {
            GunProjectile gunProjectile = Instantiate(projectilePrefab, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
            gunProjectile.gameObject.SetActive(true);

            float loadingShotMultiplier = 1;
            if(PlayerShoot.Instance.GetHeldGunSO().shotNeedsLoading && PlayerShoot.Instance.GetHeldGunSO().loadedShotFiredIfNotFullyLoaded) {
                loadingShotMultiplier = PlayerShoot.Instance.GetLoadingShotTimerNormalized();
            }

            Vector2 initialForce = loadingShotMultiplier * PlayerAim.Instance.GetEffectiveAimDir().normalized * bulletSpeed;

            gunProjectile.InitializeProjectile(this, bulletLifetime, damagePerBullet, bulletKnockback, initialForce, explosionRadiusMultiplier);
        }

        if (damageSurgeBuffedLastBullet) {
            damageSurgeBuffedLastBullet = false;
            DebuffBulletDamage(perfectJamDamageBuff, false);
        }

        if (damageSurgeBuffed) {

            bulletAfterPerfectJamSucceededIndex++;

            if(bulletAfterPerfectJamSucceededIndex >= surgeWindowBulletAmountBuffed) {
                damageSurgeBuffed = false;
                damageSurgeBuffedLastBullet = true;
                bulletAfterPerfectJamSucceededIndex = 0;
                OnPerfectQTEDamageBuffEnded?.Invoke(this, EventArgs.Empty);
            } else {
                OnAnyGunJamBuffedDamageShot?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected void HandleGunJams() {
        if (!PlayerShoot.Instance.GetGunCanJam()) return;
        if (PlayerShoot.Instance.GetNotHeldGun() != null && PlayerShoot.Instance.GetNotHeldGun().GetGunJammed()) return;
        if (gunJamInCooldown) return;

        if (UnityEngine.Random.value < jamProbability/100f) {
            JamGun();
        }
    }

    public GunSO GetGunSO() {
        return gunSO;
    }

    public void BuffBulletDamage(float buffAmount, bool globalBuff = true) {
        if(globalBuff) {
            totalBuffMultiplier *= buffAmount;
        } else {
            gunDamageBuffMultiplier *= buffAmount;
        }

        RecalculateDamage();
    }

    public void DebuffBulletDamage(float debuffAmount, bool globalDebuff = true) {
        if (globalDebuff) {
            totalBuffMultiplier /= debuffAmount;
        }
        else {
            gunDamageBuffMultiplier /= debuffAmount;
        }

        RecalculateDamage();
    }

    protected void RecalculateDamage() {
        damagePerBullet = (int)(damagePerBulletAtRunStart * totalBuffMultiplier * gunDamageBuffMultiplier);
    }

    protected void CheckPassiveSkillEffectsOnBullet() {
        if(PlayerSkills.Instance.GetLastBulletDealsMoreDamage()) {

            if (lastBulletShot) {
                lastBulletShot = false;
                OnDebuffLastBulletShot?.Invoke(this, EventArgs.Empty);
            }

            if (PlayerShoot.Instance.GetCurrentBullets() == 0 && !lastBulletShot) {
                //Last bullet
                float damageBuff = PlayerSkills.Instance.GetLastBulletDealsMoreDamageBuff();
                BuffBulletDamage(damageBuff);
                OnBuffedLastBulletShot?.Invoke(this, EventArgs.Empty);
                lastBulletShot = true;
            }

        }
    }

    #region GET PARAMETERS

    public bool GetGunUnlocked() {
        return gunUnlocked;
    }

    public bool GetGunJammedAndNextInputSequence(GameInput.Binding binding) {
        return gunJammed && gunJamHandler.GetIsExpectedBinding(binding);
    }
    public bool GetGunJustJammed() {
        return gunJustJammed;
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

    public float GetBulletKnockback() {
        return bulletKnockback;
    }

    public float GetShootCreatureHearMultiplier() {
        return shootCreatureHearMultiplier;
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
    public float GetSpinUpDuration() {
        return spinUpDuration;
    }
    public int GetSubExplosivesDamage() {
        return subExplosivesDamage;
    }
    public int GetSubExplosivesAmount() {
        return subExplosivesAmount;
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

    public float GetRange() {
        return bulletSpeed * bulletLifetime;
    }

    public float GetWeaponPrecisionModifier() {
        float precisionMultiplier = gunSO.shootConeAngle/ defaultAngle;
        // precisionMultiplier = meta upgrades on precision
        return gunSO.weaponPrecisionMultiplier * precisionMultiplier;
    }

    public bool GetGunJammed() {
        return gunJammed;
    }

    public int GetJamRepairHitAmount() {
        return jamRepairHitAmount;
    }
    public float GetJamProbability() {
        return jamProbability;
    }
    public float GetExplosionRadiusMultiplier() {
        return explosionRadiusMultiplier;
    }
    public int GetSurgeWindowBulletAmountBuffed() {
        return surgeWindowBulletAmountBuffed;
    }

    public bool GetDamageSurgeBuffed() {
        return damageSurgeBuffed;
    }
    public bool GetDamageSurgeBuffedLastBullet() {
        return damageSurgeBuffed || damageSurgeBuffedLastBullet;
    }

    #endregion

    #region SET PARAMETERS
    public void SetGunUnJammed(bool gunJamSuccess) {
        Debug.Log(this + " SetGunUnJammed gunJamSuccess " + gunJamSuccess);
        gunJammed = false;

        if(gunJamSuccess) {
            OnAnyGunJamRepaired?.Invoke(this, EventArgs.Empty);
        }

        this.damageSurgeBuffed = gunJamSuccess;

        if(gunJamSuccess) {
            OnPerfectQTEDamageBuff?.Invoke(this, EventArgs.Empty);
            BuffBulletDamage(perfectJamDamageBuff, false);
        }
    }

    [Button]
    public void JamGun() {
        gunJammed = true;
        gunJustJammed = true;
        gunJamInCooldown = true;
        gunJustJammedTimer = 0;

        OnGunJammed?.Invoke(this, EventArgs.Empty);
        OnAnyGunJammed?.Invoke(this, EventArgs.Empty);
    }

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
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetMaxAmmo_Meta(int maxAmmo) {
        this.maxAmmo = maxAmmo;
        OnAnyGunMaxAmmoChanged?.Invoke(this, EventArgs.Empty);
    }
    public virtual void SetCooldownTime_Meta(float cooldownTime) {
        this.cooldownTime = cooldownTime;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetReloadTime_Meta(float newReloadTime) {
        float reloadTimeRecuctionFactor = newReloadTime / reloadTime;
        reloadTime = newReloadTime;
        handsReloadTime *= reloadTimeRecuctionFactor;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetCritChange_Meta(float critChance) {
        this.critChance = critChance;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetShootConeAngle_Meta(float shootConeAngle) {
        this.defaultAngle = shootConeAngle;
        ParticleSystem.ShapeModule shootPSShape = shootPS.shape;
        shootPSShape.angle = defaultAngle;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetPelletsPerBullet_Meta(int pelletsPerBullet) {
        this.pelletsPerBullet = pelletsPerBullet;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetGunBulletLifetime_Meta(float bulletLifetime) {
        this.bulletLifetime = bulletLifetime;

        ParticleSystem.MainModule shootPSMain = shootPS.main;
        shootPSMain.startLifetime = bulletLifetime;
        shootPSMain.startSpeed = bulletSpeed;

        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetSubExplosivesAmount(int subExplosivesAmount) {
        this.subExplosivesAmount = subExplosivesAmount;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSubExplosivesDamage(int subExplosivesDamage) {
        this.subExplosivesDamage = subExplosivesDamage;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSpinUpDuration(float spinUpDuration) {
        this.spinUpDuration = spinUpDuration;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetJamRepairHitAmount(int jamRepairHitAmount) {
        this.jamRepairHitAmount = jamRepairHitAmount;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetJamProbability(float jamProbability) {
        this.jamProbability = jamProbability;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSurgeWindowBulletBoost(int bulletAmount) {
        surgeWindowBulletAmountBuffed = bulletAmount;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetExplosionRadiusModified(float radiusModified) {
        this.explosionRadiusMultiplier = radiusModified;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public void SaveMetaParameters() {
        if (!gunUnlocked) return;

        MetaProgressionManager.Instance.SetGunDamagePerBullet(gunSO, damagePerBullet);
        MetaProgressionManager.Instance.SetGunExplosionRadiusMultiplier(gunSO, explosionRadiusMultiplier);
        MetaProgressionManager.Instance.SetGunShotsPerClip(gunSO, shotsPerClip);
        MetaProgressionManager.Instance.SetGunMaxAmmo(gunSO, maxAmmo);
        MetaProgressionManager.Instance.SetGunCooldown(gunSO, cooldownTime);
        MetaProgressionManager.Instance.SetGunReloadTime(gunSO, reloadTime);
        MetaProgressionManager.Instance.SetGunHandsReloadTime(gunSO, handsReloadTime);
        MetaProgressionManager.Instance.SetGunCritChance(gunSO, critChance);
        MetaProgressionManager.Instance.SetGunShootConeAnle(gunSO, defaultAngle);
        MetaProgressionManager.Instance.SetGunPelletsPerBullet(gunSO, pelletsPerBullet);
        MetaProgressionManager.Instance.SetGunBulletLifetime(gunSO, bulletLifetime);
        MetaProgressionManager.Instance.SetGunSubExplosivesDamage(gunSO, subExplosivesDamage);
        MetaProgressionManager.Instance.SetGunSubExplosivesAmount(gunSO, subExplosivesAmount);
        MetaProgressionManager.Instance.SetGunSpinUpDuration(gunSO, spinUpDuration);
        MetaProgressionManager.Instance.SetGunSwapToWeaponTimeMultiplier(gunSO, swapToWeaponTimeMultiplier);

        MetaProgressionManager.Instance.SetGunJamProbability(gunSO, jamProbability);
        MetaProgressionManager.Instance.SetGunSurgeWindowBulletsAmountBuffed(gunSO, surgeWindowBulletAmountBuffed);
        MetaProgressionManager.Instance.SetGunJamRepairHitAmount(gunSO, jamRepairHitAmount);

        MetaProgressionManager.Instance.SetGunSecondaryAbilityUnlocked(gunSO, secondaryAbilityUnlocked);
        MetaProgressionManager.Instance.SetGunUnlocked(gunSO, gunUnlocked);
    }
}
