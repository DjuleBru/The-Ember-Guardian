using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GunSO : ScriptableObject
{
    public Sprite gunSprite;
    public Sprite reticleSprite;
    public List<Sprite> shotCountSprites;

    public RuntimeAnimatorController gunAnimator;
    public bool automaticWeapon;
    public int damagePerBullet;
    public int maxAmmo;
    public int pelletsPerBullet;
    public int shotsPerClip;

    public float shootCooldownTime;
    public float shootCooldownSFXTriggerTime;
    public float reloadTime;
    public float animationReloadTime;

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
}
