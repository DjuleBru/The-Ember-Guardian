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
    public Sprite gunSprite;
    public Sprite reticleSprite;
    public List<Sprite> shotCountSprites;
    public PlayerCurrencies.CurrencyType ammoTypeUsed;

    public RuntimeAnimatorController gunAnimator;
    public bool bulletIsParticle;
    public bool bulletIsProjectile;
    public bool bulletIsSprite;
    public bool automaticWeapon;
    public float delayBetweenClickAndShot;
    public int damagePerBullet;
    public int maxAmmo;
    public int pelletsPerBullet;
    public int shotsPerClip;
    public float critChance;
    public bool useAngleInPS;
    public float shootConeAngle;
    public float weaponPrecisionMultiplier = 1f;
    public float shootCreatureHearMultiplier = 1.75f;
    public float bulletLifetime;
    public float bulletSpeed;
    public float bulletKnockback;
    public float weightAccelerationFactor;
    public float reloadAccelerationFactor;

    public float shootCooldownTime;
    public float shootCooldownSFXTriggerTime;
    public float reloadTime;
    public float handsReloadTime;
    public float handsAnimationReloadTime;
    public float swapToWeaponTimeMultiplier;

    public float gunKnockback;
    public float gunRecoil;
    public float gunRecoilDamping;

    public bool triggersShootFeedbackOnEachBuller;
    public bool triggersShootSFXOnEachBuller;
    public AudioClip[] shootGunSound;
    public AudioClip[] reloadGunSound;
    public AudioClip[] cooldownGunSound;
    public AudioClip[] bulletHitGroundSound;
    public AudioClip[] bulletHitEnemySound;
    public AudioClip[] bulletHitEnemyCritSound;
    public AudioClip[] outOfAmmoSound;
    public AudioClip swapToWeaponSound;

    public float reloadSFXVolumeMultiplier;
    public float cooldownSFXVolumeMultiplier;
    public float shootGunVolumeMultiplier;
    public float outOfAmmoVolumeMultiplier;
    public float swapToWeaponVolumeMultiplier = 1f;
}
