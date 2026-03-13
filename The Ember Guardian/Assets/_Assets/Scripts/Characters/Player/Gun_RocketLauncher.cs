using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_RocketLauncher : Gun
{
    [SerializeField] protected Transform miniRocketPrefab;
    [SerializeField] protected int miniRocketAmount;
    [SerializeField] protected float miniRocketBulletLifetime;
    [SerializeField] protected float miniRocketBulletSpeed;
    [SerializeField] protected float miniRocketBulletSpeedIncrementer;

    protected override void Shoot() {

        if (PlayerShoot.Instance.GetRocketLauncherMiniRockets()) {

            for (int i = 0; i < miniRocketAmount; i++) {

                GunProjectile gunProjectile = Instantiate(miniRocketPrefab, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
                gunProjectile.gameObject.SetActive(true);

                float miniRocketSpeed = miniRocketBulletSpeed + i * miniRocketBulletSpeedIncrementer;
                Vector2 initialForce = PlayerAim.Instance.GetEffectiveAimDir().normalized * miniRocketSpeed;

                gunProjectile.InitializeProjectile(this, miniRocketBulletLifetime, damagePerBullet / miniRocketAmount, bulletKnockback, initialForce, explosionRadiusMultiplier, 1, 1);

            }

        } else {

            GunProjectile gunProjectile = Instantiate(projectilePrefab, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
            gunProjectile.gameObject.SetActive(true);

            Vector2 initialForce = PlayerAim.Instance.GetEffectiveAimDir().normalized * bulletSpeed;

            gunProjectile.InitializeProjectile(this, bulletLifetime, damagePerBullet, bulletKnockback, initialForce, explosionRadiusMultiplier, 1, 1);

        }

    }

}
