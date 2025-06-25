using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_Manner_MG : SpecialTower_Manner {

    [SerializeField] private ParticleSystem level2PS_Top;
    [SerializeField] private ParticleSystem level2PS_Bot;
    [SerializeField] private ParticleCollision shootPS_Collision_Bot;
    [SerializeField] private ParticleCollision shootPS_Collision_Top;
    [SerializeField] protected float mannerCooldownTime_Level2;
    [SerializeField] protected int shotsPerAmmoClip_level2;

    private bool shootingTop;
    private bool level2MG;

    public event EventHandler OnTopPSShot;
    public event EventHandler OnBotPSShot;
    protected override void Awake() {
        base.Awake();
        shootPS_Collision_Bot.InitializeBulletPS(transform, bulletDamage, bulletKnockback, collisionDistanceTreshold);
        shootPS_Collision_Top.InitializeBulletPS(transform, bulletDamage, bulletKnockback, collisionDistanceTreshold);
       
    }
    protected override void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        level2MG = true;
        mannerCooldownTime = mannerCooldownTime_Level2;
        shotsPerAmmoClip = shotsPerAmmoClip_level2;
    }

    protected override void Shoot() {
        if (specialTower.GetCurrentAmmoClip() == 0) {
            towerOutOfAmmo = true;
        }

        currentShotIndex--;
        if (currentShotIndex == 0 && !towerOutOfAmmo) {

            StartCoroutine(HandleReloading());
            reloading = true;
        }


        InvokeOnMannerShot();
        if (shootingTop) {
            level2PS_Top.Emit(pelletsPerBullet);
            OnTopPSShot?.Invoke(this, EventArgs.Empty);
        }
        else {
            level2PS_Bot.Emit(pelletsPerBullet);
            OnBotPSShot?.Invoke(this, EventArgs.Empty);
        }
        shootingTop = !shootingTop;

        readyToShoot = false;
        cooldownTriggered = false;
    }

    public bool GetLevel2MG() {
        return level2MG;
    }
    public bool GetShootingTop() {
        return shootingTop;
    }

}
