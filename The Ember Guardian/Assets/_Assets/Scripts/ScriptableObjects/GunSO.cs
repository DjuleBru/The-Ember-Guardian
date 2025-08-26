using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GunSO : ScriptableObject
{
    public enum GunType {
        Rifle,
        UZI,
        Shotgun,
        Sniper,
        Revolver,
        LMG,
        GrenadeLauncher,
        AssaultRifle,
        FlameThrower,
        MiniGun,
        RocketLauncher,
        DoubleBarreledShotgun,
        AAGun,
        Pistol,
    }

    public GunType gunType;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite gunSprite;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite reticleSprite;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public List<Sprite> shotCountSprites;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public RuntimeAnimatorController gunAnimator;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite weaponCursorSprite;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite weaponHitCursorSprite;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite mouseCursorSprite_NE;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite mouseCursorSprite_NW;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite mouseCursorSprite_SE;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public Sprite mouseCursorSprite_SW;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public float initialMouseCursorWidth;
    [BoxGroup("Visual")]
    [LabelWidth(300)]
    public float initialMouseCursorHeight;

    [BoxGroup("General")]
    [LabelWidth(300)]
    public PlayerCurrencies.CurrencyType ammoTypeUsed;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool bulletIsParticle;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool bulletIsProjectile;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool bulletIsSprite;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool canUseMeleeAttack = true;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool automaticWeapon;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool shotNeedsLoading;
    [BoxGroup("General")]
    [LabelWidth(300)]
    [ShowIf("shotNeedsLoading")]
    public bool loadedShotFiredIfNotFullyLoaded;
    [BoxGroup("General")]
    [LabelWidth(300)]
    [ShowIf("shotNeedsLoading")]
    public float loadShotTime;
    [BoxGroup("General")]
    [LabelWidth(300)]
    [ShowIf("shotNeedsLoading")]
    public float minLoadShotTime;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public float delayBetweenClickAndShot;
    [BoxGroup("General")]
    [LabelWidth(300)]
    public bool useAngleInPS;

    [BoxGroup("Stats")]
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int damagePerBullet;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int maxAmmo;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int pelletsPerBullet;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int shotsPerClip;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float critChance;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float bulletLifetime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float bulletSpeed;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float shootCooldownTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float reloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float handsReloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float handsAnimationReloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float bulletKnockback;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float spinUpDuration = 3.5f;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int subExplosivesAmount;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int subExplosivesDamage;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float shootCreatureHearMultiplier = 1.75f;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public float jamProbability = 0.05f;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int jamRepairHitAmount = 3;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(300)]
    public int perfectQTEBulletAmountDamageBuffed;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float weightAccelerationFactor;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float reloadAccelerationFactor;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float followMouseSpeed = 40f;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float swapToWeaponTimeMultiplier;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float gunKnockback;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float gunRecoil;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(300)]
    public float gunRecoilDamping;

    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float shootConeAngle;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float weaponPrecisionMultiplier = 1f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float movePrecisionDebuff = 1.25f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float runPrecisionDebuff = 1.5f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float weaponSecondaryAbilityPrecisionFactor = 1f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float weaponSecondaryRecoilReductionFactor = 1f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float weaponMaxRecoilImpactOnMouseReticle = .25f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float crouchPrecisionBuff = 2f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(300)]
    public float crouchRecoilReductionFactor = 2f;

    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float shotCooldownAnimationTriggerTime;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float shootCooldownSFXTriggerTime;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public bool triggersShootFeedbackOnEachBuller;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public bool triggersShootSFXOnEachBuller;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] shootGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    [ShowIf("shotNeedsLoading")]
    public AudioClip startLoadingShotGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] reloadGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] cooldownGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] bulletHitGroundSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float bulletHitSoundMultiplier = 1f;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] bulletHitEnemyCritSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] outOfAmmoSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] tryShootGunJammedSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] gunJammedSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] gunJammHitProgressSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip[] gunJamRepairedSound;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public AudioClip swapToWeaponSound;

    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float reloadSFXVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float cooldownSFXVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float shootGunVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float outOfAmmoVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float tryShootGunJammedVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float gunJammedVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float gunJamHitProgressVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float gunJamRepairedVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(300)]
    public float swapToWeaponVolumeMultiplier = 1f;
}
