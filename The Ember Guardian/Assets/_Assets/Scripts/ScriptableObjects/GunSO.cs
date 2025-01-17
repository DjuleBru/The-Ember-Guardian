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
    }
    public GunType gunType;
    public Sprite gunSprite;
    public Sprite reticleSprite;
    public List<Sprite> shotCountSprites;

    public RuntimeAnimatorController gunAnimator;
    public bool automaticWeapon;
    public int damagePerBullet;
    public int maxAmmo;
    public int pelletsPerBullet;
    public int shotsPerClip;
    public float critChance;
    public float shootConeAngle;
    public float bulletLifetime;
    public float bulletSpeed;
    public float weightAccelerationFactor;
    public float reloadAccelerationFactor;

    public float shootCooldownTime;
    public float shootCooldownSFXTriggerTime;
    public float reloadTime;
    public float handsReloadTime;
    public float handsAnimationReloadTime;

    public float gunKnockback;
    public float gunRecoil;
    public float gunRecoilDamping;


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
}
