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
    protected int pelletsPerBullet_meta = 1;
    protected int damagePerBulletAtRunStart;
    protected int damagePerBullet;
    protected float explosionRadiusMultiplier = 1;
    protected float gunDamageBuffMultiplier = 1;
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
    protected float surgeReloadProbability;
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

    protected int bulletDamageStatModifierLevel = -1;
    protected int shotsPerClipStatModifierLevel = -1;
    protected int maxAmmoStatModifierLevel = -1;
    protected int cooldownTimeStatModifierLevel = -1;
    protected int reloadTimeStatModifierLevel = -1;
    protected int critChanceStatModifierLevel = -1;
    protected int shootConeAngleStatModifierLevel = -1;
    protected int pelletsPerBulletStatModifierLevel = -1;
    protected int bulletLifetimeStatModifierLevel = -1;
    protected int surgeReloadProbabilityStatModifierLevel = -1;
    protected int surgeWindowBulletAmountBuffedStatModifierLevel = -1;
    protected int explosionRadiusMultiplierStatModifierLevel = -1;
    protected int spinUpDurationStatModifierLevel = -1;
    protected int subExplosivesAmountStatModifierLevel = -1;
    protected int subExplosivesDamageStatModifierLevel = -1;

    public static event EventHandler OnAnyGunMaxAmmoChanged;
    public static event EventHandler OnAnyGunStatsUpgraded;
    public static event EventHandler OnAnyGunUnlocked;
    public static event EventHandler OnAnyGunJammed;
    public static event EventHandler OnAnySurgeReloadSuccess;
    public static event EventHandler OnAnyGunJamBuffedDamageShot;
    public event EventHandler OnGunJammed;
    public event EventHandler OnPerfectQTEDamageBuff;
    public event EventHandler OnPerfectQTEDamageBuffEnded;
    public event EventHandler OnBuffedLastBulletShot;
    public event EventHandler OnDebuffLastBulletShot;
    public event EventHandler OnAmmoAndBuleltsChanged;

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

        int buffedPelletsPerBullet = 0;
        if(pelletsPerBulletStatModifierLevel != -1) {
            buffedPelletsPerBullet = (int)gunSO.pelletsPerBulletStatModifier.statModifierList[pelletsPerBulletStatModifierLevel];
        }
        pelletsPerBullet = gunSO.pelletsPerBullet + buffedPelletsPerBullet;


        DebuffBulletDamage(focusedBlastDamageBuff, false);
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
        BuffBulletDamage(focusedBlastDamageBuff, false);
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
        pelletsPerBullet = gunSO.pelletsPerBullet;
        if (gunSO.pelletsPerBulletStatModifier != null && pelletsPerBulletStatModifierLevel != -1) {
            pelletsPerBullet = gunSO.pelletsPerBullet + (int)gunSO.pelletsPerBulletStatModifier.statModifierList[pelletsPerBulletStatModifierLevel];
        }
        pelletsPerBullet_meta = pelletsPerBullet;

        maxAmmo = gunSO.maxAmmo;
        if(gunSO.maxAmmoStatModifier != null && maxAmmoStatModifierLevel != -1) {
            maxAmmo = gunSO.maxAmmo + (int)gunSO.maxAmmoStatModifier.statModifierList[maxAmmoStatModifierLevel];
        }

        damagePerBullet = gunSO.damagePerBullet;
        if(gunSO.damageStatModifier != null && bulletDamageStatModifierLevel != -1) {
            damagePerBullet = gunSO.damagePerBullet + (int)gunSO.damageStatModifier.statModifierList[bulletDamageStatModifierLevel];
        }
        damagePerBulletAtRunStart = damagePerBullet;


        explosionRadiusMultiplier = 1;
        if(gunSO.explosionRadiusMultiplierStatModifier != null && explosionRadiusMultiplierStatModifierLevel != -1) {
            explosionRadiusMultiplier = (100 + gunSO.explosionRadiusMultiplierStatModifier.statModifierList[explosionRadiusMultiplierStatModifierLevel]) / 100;
        }

        shotsPerClip = gunSO.shotsPerClip;
        if(gunSO.shotsPerClipStatModifier != null && shotsPerClipStatModifierLevel != -1) {
            shotsPerClip = gunSO.shotsPerClip + (int)gunSO.shotsPerClipStatModifier.statModifierList[shotsPerClipStatModifierLevel];
        }


        critChance = gunSO.critChance;
        if(gunSO.critChanceStatModifier != null && critChanceStatModifierLevel != -1) {
            critChance = gunSO.critChance + gunSO.critChanceStatModifier.statModifierList[critChanceStatModifierLevel];
        }


        cooldownTime = gunSO.shootCooldownTime;
        if(gunSO.cooldownTimeStatModifier != null && cooldownTimeStatModifierLevel != -1) {
            cooldownTime = gunSO.shootCooldownTime + gunSO.shootCooldownTime * gunSO.cooldownTimeStatModifier.statModifierList[cooldownTimeStatModifierLevel] * 0.01f;
        }

        reloadTime = gunSO.reloadTime;
        if(gunSO.reloadTimeStatModifier != null && reloadTimeStatModifierLevel != -1) {
            reloadTime = gunSO.reloadTime + gunSO.reloadTime * gunSO.reloadTimeStatModifier.statModifierList[reloadTimeStatModifierLevel] * 0.01f;
        }

        float reloadTimeRecuctionFactor = reloadTime / gunSO.reloadTime;

        handsReloadTime = gunSO.handsReloadTime * reloadTimeRecuctionFactor;

        bulletLifetime = gunSO.bulletLifetime;
        if(gunSO.bulletLifetimeStatModifier  != null && bulletLifetimeStatModifierLevel != -1) {
            bulletLifetime = gunSO.bulletLifetime + gunSO.bulletLifetimeStatModifier.statModifierList[bulletLifetimeStatModifierLevel] / gunSO.bulletSpeed;
        }


        surgeReloadProbability = gunSO.surgeReloadProbability;
        if(gunSO.surgeReloadProbabilityStatModifier != null && surgeReloadProbabilityStatModifierLevel != -1) {
            surgeReloadProbability = gunSO.surgeReloadProbability + gunSO.surgeReloadProbabilityStatModifier.statModifierList[surgeReloadProbabilityStatModifierLevel];
        }


        surgeWindowBulletAmountBuffed = gunSO.perfectQTEBulletAmountDamageBuffed;
        if(gunSO.surgeWindowBulletAmountBuffedStatModifier != null && surgeWindowBulletAmountBuffedStatModifierLevel != -1) {
            surgeWindowBulletAmountBuffed = gunSO.perfectQTEBulletAmountDamageBuffed + (int)gunSO.surgeWindowBulletAmountBuffedStatModifier.statModifierList[surgeWindowBulletAmountBuffedStatModifierLevel];
        }

        spinUpDuration = gunSO.spinUpDuration;
        if(gunSO.spinUpDurationStatModifier != null && spinUpDurationStatModifierLevel != -1) {
            spinUpDuration = gunSO.spinUpDuration + gunSO.spinUpDurationStatModifier.statModifierList[spinUpDurationStatModifierLevel];
        
        }

        subExplosivesAmount = gunSO.subExplosivesAmount;
        if(gunSO.subExplosivesAmountStatModifier != null && subExplosivesAmountStatModifierLevel != -1) {
            subExplosivesAmount = gunSO.subExplosivesAmount + (int)gunSO.subExplosivesAmountStatModifier.statModifierList[subExplosivesAmountStatModifierLevel];
        
        }

        subExplosivesDamage = gunSO.subExplosivesDamage;
        if(gunSO.subExplosivesDamageStatModifier != null && subExplosivesDamageStatModifierLevel != -1) {
            subExplosivesDamage = gunSO.subExplosivesDamage + (int)gunSO.subExplosivesDamageStatModifier.statModifierList[subExplosivesDamageStatModifierLevel];
        }

        defaultAngle = gunSO.shootConeAngle;
        if(gunSO.shootConeAngleStatModifier != null && shootConeAngleStatModifierLevel != -1) {
            defaultAngle = gunSO.shootConeAngle + gunSO.shootConeAngleStatModifier.statModifierList[shootConeAngleStatModifierLevel];
        }

        weightAccelerationFactor = gunSO.weightAccelerationFactor;
        reloadAccelerationFactor = gunSO.reloadAccelerationFactor;
        bulletSpeed = gunSO.bulletSpeed;
        swapToWeaponTimeMultiplier = gunSO.swapToWeaponTimeMultiplier;
        bulletKnockback = gunSO.bulletKnockback;
        shootCreatureHearMultiplier = gunSO.shootCreatureHearMultiplier;

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

    public void SetGunAmmo(int ammoClip, int currentBullet) {
        this.currentAmmoClip = ammoClip;
        this.currentBullet = currentBullet;

        // force le refresh du visuel même si l'arme est inactive
         GetComponent<GunVisual>().ForceInitializeAmmoVisual();
    }

    protected virtual void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (!gunActive) return;

        //Check Passive SKills
        CheckPassiveSkillEffectsOnBullet();
        //HandleGunJams();

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
        if (!PlayerShoot.Instance.GetCanSurgeWindow()) return;
        if (PlayerShoot.Instance.GetNotHeldGun() != null && PlayerShoot.Instance.GetNotHeldGun().GetGunJammed()) return;
        if (gunJamInCooldown) return;

        if (UnityEngine.Random.value < surgeReloadProbability/100f) {
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
        if (gunSO.gunType == GunSO.GunType.Rifle) return true;

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
        return pelletsPerBullet_meta;
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
    public float GetSurgeReloadProbability() {
        return surgeReloadProbability;
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
        gunJammed = false;

        if(gunJamSuccess) {
            OnAnySurgeReloadSuccess?.Invoke(this, EventArgs.Empty);
        }

        this.damageSurgeBuffed = gunJamSuccess;

        if(gunJamSuccess) {
            OnPerfectQTEDamageBuff?.Invoke(this, EventArgs.Empty);
            BuffBulletDamage(perfectJamDamageBuff, false);
        }
    }

    public void ApplySurgeWindowBuffAfterDelay(float delay) {
        StartCoroutine(ApplySurgeWindowBuffAfterDelayCoroutine(delay));
    }

    private IEnumerator ApplySurgeWindowBuffAfterDelayCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        OnAnySurgeReloadSuccess?.Invoke(this, EventArgs.Empty);
        OnPerfectQTEDamageBuff?.Invoke(this, EventArgs.Empty);
        BuffBulletDamage(perfectJamDamageBuff, false);
        damageSurgeBuffed = true;
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

    public void SetBulletDamage_StatModifierListLevel(int bulletDamageStatModifierLevel) {
        this.bulletDamageStatModifierLevel = bulletDamageStatModifierLevel;

        this.damagePerBullet = gunSO.damagePerBullet + (int)gunSO.damageStatModifier.statModifierList[bulletDamageStatModifierLevel];
        damagePerBulletAtRunStart = damagePerBullet;
    }

    public void SetShotsPerClip_StatModifierListLevel(int shotsPerClipStatModifierLevel) {
        this.shotsPerClipStatModifierLevel = shotsPerClipStatModifierLevel;

        int modifiedShotsPerClip = gunSO.shotsPerClip + (int)gunSO.shotsPerClipStatModifier.statModifierList[shotsPerClipStatModifierLevel];
        this.shotsPerClip = modifiedShotsPerClip;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetMaxAmmo_StatModifierListLevel(int maxAmmoModifierLevel) {
        this.maxAmmoStatModifierLevel = maxAmmoModifierLevel;
        int modifiedMaxAmmo = gunSO.maxAmmo + (int)gunSO.maxAmmoStatModifier.statModifierList[maxAmmoModifierLevel];

        this.maxAmmo = modifiedMaxAmmo;
        OnAnyGunMaxAmmoChanged?.Invoke(this, EventArgs.Empty);
    }

    public virtual void SetCooldownTime_StatModifierListLevel(int cooldownTimeLevel) {
        this.cooldownTimeStatModifierLevel = cooldownTimeLevel;
        float modifiedCooldown = gunSO.shootCooldownTime + gunSO.shootCooldownTime * gunSO.cooldownTimeStatModifier.statModifierList[cooldownTimeLevel] * 0.01f;

        this.cooldownTime = modifiedCooldown;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetReloadTime_StatModifierListLevel(int reloadTimeStatModifierLevel) {
        this.reloadTimeStatModifierLevel = reloadTimeStatModifierLevel;

        float modifiedReloadTime = gunSO.reloadTime + gunSO.reloadTime * gunSO.reloadTimeStatModifier.statModifierList[reloadTimeStatModifierLevel] * 0.01f;
        float reloadTimeRecuctionFactor = modifiedReloadTime / reloadTime;

        reloadTime = modifiedReloadTime;
        handsReloadTime *= reloadTimeRecuctionFactor;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetCritChange_StatModifierListLevel(int critChanceStatModifierLevel) {
        this.critChanceStatModifierLevel = critChanceStatModifierLevel;

        float modifiedCritChance = gunSO.critChance + gunSO.critChanceStatModifier.statModifierList[critChanceStatModifierLevel];

        this.critChance = modifiedCritChance;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetShootConeAngle_StatModifierListLevel(int shootConeAngleStatModifierLevel) {
        this.shootConeAngleStatModifierLevel = shootConeAngleStatModifierLevel;

        float modifiedShootAngle = gunSO.shootConeAngle + gunSO.shootConeAngleStatModifier.statModifierList[shootConeAngleStatModifierLevel];

        this.defaultAngle = modifiedShootAngle;
        ParticleSystem.ShapeModule shootPSShape = shootPS.shape;
        shootPSShape.angle = defaultAngle;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetPelletsPerBullet_StatModifierListLevel(int pelletsPerBulletStatModifierLevel) {
        this.pelletsPerBulletStatModifierLevel = pelletsPerBulletStatModifierLevel;

        int modifiedPelletsPerBullet = gunSO.pelletsPerBullet + (int)gunSO.pelletsPerBulletStatModifier.statModifierList[pelletsPerBulletStatModifierLevel];

        this.pelletsPerBullet_meta = modifiedPelletsPerBullet;
        this.pelletsPerBullet = modifiedPelletsPerBullet;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetGunBulletLifetime_StatModifierListLevel(int bulletLifetimeStatModifierLevel) {
        this.bulletLifetimeStatModifierLevel = bulletLifetimeStatModifierLevel;

        float modifiedBulletLifetime = gunSO.bulletLifetime + gunSO.bulletLifetimeStatModifier.statModifierList[bulletLifetimeStatModifierLevel] / gunSO.bulletSpeed;

        this.bulletLifetime = modifiedBulletLifetime;

        ParticleSystem.MainModule shootPSMain = shootPS.main;
        shootPSMain.startLifetime = modifiedBulletLifetime;
        shootPSMain.startSpeed = bulletSpeed;

        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSubExplosivesAmount_StatModifierListLevel(int subExplosivesAmountStatModifierLevel) {
        this.subExplosivesAmountStatModifierLevel = subExplosivesAmountStatModifierLevel;

        int modifiedSubExplosivesAmount = gunSO.subExplosivesAmount + (int)gunSO.subExplosivesAmountStatModifier.statModifierList[subExplosivesAmountStatModifierLevel];

        this.subExplosivesAmount = modifiedSubExplosivesAmount;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSubExplosivesDamage_StatModifierListLevel(int subExplosivesDamageStatModifierLevel) {
        this.subExplosivesDamageStatModifierLevel = subExplosivesDamageStatModifierLevel;

        int modifiedSubExplosivesDamage = gunSO.subExplosivesDamage + (int)gunSO.subExplosivesDamageStatModifier.statModifierList[subExplosivesDamageStatModifierLevel];

        this.subExplosivesDamage = modifiedSubExplosivesDamage;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void SetSpinUpDuration_StatModifierListLevel(int spinUpDurationStatModifierLevel) {
        this.spinUpDurationStatModifierLevel = spinUpDurationStatModifierLevel;

        float modifiedSpinUpTime = gunSO.spinUpDuration + gunSO.spinUpDurationStatModifier.statModifierList[spinUpDurationStatModifierLevel];

        this.spinUpDuration = modifiedSpinUpTime;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetSurgeReloadProbability_StatModifierListLevel(int surgeReloadProbabilityStatModifierLevel) {
        this.surgeReloadProbabilityStatModifierLevel = surgeReloadProbabilityStatModifierLevel;

        float modifiedSurgeReloadProbability = gunSO.surgeReloadProbability + gunSO.surgeReloadProbabilityStatModifier.statModifierList[this.surgeReloadProbabilityStatModifierLevel];

        this.surgeReloadProbability = modifiedSurgeReloadProbability;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetSurgeWindowBulletBoost_StatModifierListLevel(int surgeWindowBulletAmountStatModifierLevel) {
        this.surgeWindowBulletAmountBuffedStatModifierLevel = surgeWindowBulletAmountStatModifierLevel;

        int modifiedBulletsAmount = gunSO.perfectQTEBulletAmountDamageBuffed + (int)gunSO.surgeWindowBulletAmountBuffedStatModifier.statModifierList[surgeWindowBulletAmountStatModifierLevel];

        surgeWindowBulletAmountBuffed = modifiedBulletsAmount;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SetExplosionRadiusModified_StatModifierListLevel(int radiusModifiedStatModifierLevel) {
        this.explosionRadiusMultiplierStatModifierLevel = radiusModifiedStatModifierLevel;
        float modifiedExplosionRadius = (100 + gunSO.explosionRadiusMultiplierStatModifier.statModifierList[radiusModifiedStatModifierLevel]) / 100;

        this.explosionRadiusMultiplier = modifiedExplosionRadius;
        OnAnyGunStatsUpgraded?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public void SaveGunStatModifierLevels() {
        // Dictionnaire pour stocker toutes les valeurs de cette arme
        Dictionary<string, object> gunData = new Dictionary<string, object>();

        // --- Niveaux d'amélioration ---
        gunData["bulletDamageLevel"] = bulletDamageStatModifierLevel;
        gunData["explosionRadiusMultiplierLevel"] = explosionRadiusMultiplierStatModifierLevel;
        gunData["shotsPerClipLevel"] = shotsPerClipStatModifierLevel;
        gunData["maxAmmoLevel"] = maxAmmoStatModifierLevel;
        gunData["cooldownTimeLevel"] = cooldownTimeStatModifierLevel;
        gunData["reloadTimeLevel"] = reloadTimeStatModifierLevel;
        gunData["critChanceLevel"] = critChanceStatModifierLevel;
        gunData["shootConeAngleLevel"] = shootConeAngleStatModifierLevel;
        gunData["pelletsPerBulletLevel"] = pelletsPerBulletStatModifierLevel;
        gunData["bulletLifetimeLevel"] = bulletLifetimeStatModifierLevel;
        gunData["subExplosivesDamageLevel"] = subExplosivesDamageStatModifierLevel;
        gunData["subExplosivesAmountLevel"] = subExplosivesAmountStatModifierLevel;
        gunData["spinUpDurationLevel"] = spinUpDurationStatModifierLevel;
        gunData["surgeWindowBulletAmountBuffedLevel"] = surgeWindowBulletAmountBuffedStatModifierLevel;
        gunData["surgeReloadProbabilityStatModifierLevel"] = surgeReloadProbabilityStatModifierLevel;

        gunData["secondaryAbilityUnlocked"] = secondaryAbilityUnlocked;

        // Sauvegarde en batch
        string key = gunSO.gunType + "_metaData";
    }

    public void LoadGunStatModifierLevels() {
        string key = gunSO.gunType + "_metaData";
        gunUnlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);

        if (!ES3.KeyExists(key)) {
            RefreshGunStats();
            return;
        };

        var gunData = ES3.Load<Dictionary<string, object>>(key);

        int GetLevelSafe(string dataKey, HubMerchantItemStatModifierSO statModifier) {
            int value = gunData.ContainsKey(dataKey) ? Convert.ToInt32(gunData[dataKey]) : 0;

            if (statModifier == null || statModifier.statModifierList == null || statModifier.statModifierList.Count == 0)
                return 0;

            return Mathf.Clamp(value, -1, statModifier.statModifierList.Count - 1);
        }

        bulletDamageStatModifierLevel = GetLevelSafe("bulletDamageLevel", gunSO.damageStatModifier);
        explosionRadiusMultiplierStatModifierLevel = GetLevelSafe("explosionRadiusMultiplierLevel", gunSO.explosionRadiusMultiplierStatModifier);
        shotsPerClipStatModifierLevel = GetLevelSafe("shotsPerClipLevel", gunSO.shotsPerClipStatModifier);
        maxAmmoStatModifierLevel = GetLevelSafe("maxAmmoLevel", gunSO.maxAmmoStatModifier);
        cooldownTimeStatModifierLevel = GetLevelSafe("cooldownTimeLevel", gunSO.cooldownTimeStatModifier);
        reloadTimeStatModifierLevel = GetLevelSafe("reloadTimeLevel", gunSO.reloadTimeStatModifier);
        critChanceStatModifierLevel = GetLevelSafe("critChanceLevel", gunSO.critChanceStatModifier);
        shootConeAngleStatModifierLevel = GetLevelSafe("shootConeAngleLevel", gunSO.shootConeAngleStatModifier);
        pelletsPerBulletStatModifierLevel = GetLevelSafe("pelletsPerBulletLevel", gunSO.pelletsPerBulletStatModifier);
        bulletLifetimeStatModifierLevel = GetLevelSafe("bulletLifetimeLevel", gunSO.bulletLifetimeStatModifier);
        subExplosivesDamageStatModifierLevel = GetLevelSafe("subExplosivesDamageLevel", gunSO.subExplosivesDamageStatModifier);
        subExplosivesAmountStatModifierLevel = GetLevelSafe("subExplosivesAmountLevel", gunSO.subExplosivesAmountStatModifier);
        spinUpDurationStatModifierLevel = GetLevelSafe("spinUpDurationLevel", gunSO.spinUpDurationStatModifier);
        surgeWindowBulletAmountBuffedStatModifierLevel = GetLevelSafe("surgeWindowBulletAmountBuffedLevel", gunSO.surgeWindowBulletAmountBuffedStatModifier);
        surgeReloadProbabilityStatModifierLevel = GetLevelSafe("surgeReloadProbabilityStatModifierLevel", gunSO.surgeReloadProbabilityStatModifier);

        secondaryAbilityUnlocked = gunData.ContainsKey("secondaryAbilityUnlocked") && Convert.ToBoolean(gunData["secondaryAbilityUnlocked"]);

        RefreshGunStats();
    }

}
