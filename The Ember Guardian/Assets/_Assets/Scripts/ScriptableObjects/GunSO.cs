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
    }

    public GunType gunType;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite gunSprite;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite reticleSprite;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public List<Sprite> shotCountSprites;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public RuntimeAnimatorController gunAnimator;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite weaponCursorSprite;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite weaponHitCursorSprite;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite mouseCursorSprite_NE;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite mouseCursorSprite_NW;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite mouseCursorSprite_SE;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public Sprite mouseCursorSprite_SW;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public float initialMouseCursorWidth;
    [BoxGroup("Visual")]
    [LabelWidth(200)]
    public float initialMouseCursorHeight;

    [BoxGroup("General")]
    [LabelWidth(200)]
    public PlayerCurrencies.CurrencyType ammoTypeUsed;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool bulletIsParticle;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool bulletIsProjectile;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool bulletIsSprite;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool automaticWeapon;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float delayBetweenClickAndShot;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool useAngleInPS;

    [BoxGroup("Stats")]
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public int damagePerBullet;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public int maxAmmo;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public int pelletsPerBullet;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public int shotsPerClip;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float critChance;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float bulletLifetime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float bulletSpeed;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float shootCooldownTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float reloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float handsReloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float handsAnimationReloadTime;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float bulletKnockback;
    [BoxGroup("Stats/Shoot")]
    [LabelWidth(200)]
    public float shootCreatureHearMultiplier = 1.75f;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float weightAccelerationFactor;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float reloadAccelerationFactor;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float followMouseSpeed = 40f;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float swapToWeaponTimeMultiplier;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float gunKnockback;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float gunRecoil;
    [BoxGroup("Stats/Weight")]
    [LabelWidth(200)]
    public float gunRecoilDamping;

    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float shootConeAngle;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float weaponPrecisionMultiplier = 1f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float weaponSecondaryAbilityPrecisionFactor = 1f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float weaponMaxRecoilImpactOnMouseReticle = .25f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float crouchPrecisionBuff = 2f;
    [BoxGroup("Stats/Precision")]
    [LabelWidth(200)]
    public float crouchRecoilReductionFactor = 2f;

    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float shootCooldownSFXTriggerTime;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public bool triggersShootFeedbackOnEachBuller;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public bool triggersShootSFXOnEachBuller;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] shootGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] reloadGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] cooldownGunSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] bulletHitGroundSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] bulletHitEnemySound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] bulletHitEnemyCritSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip[] outOfAmmoSound;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public AudioClip swapToWeaponSound;

    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float reloadSFXVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float cooldownSFXVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float shootGunVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float outOfAmmoVolumeMultiplier;
    [BoxGroup("Sound")]
    [LabelWidth(200)]
    public float swapToWeaponVolumeMultiplier = 1f;
}
