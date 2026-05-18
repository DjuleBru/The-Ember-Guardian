using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_RocketLauncher : Gun {

    [SerializeField] private GrenadeTrajectoryPreview trajectoryPreview;
    [SerializeField] private LineRenderer trajectoryPreviewLR;

    [SerializeField] private GameObject standardRocket;
    [SerializeField] private GameObject nukeRocket;

    protected override void Start() {
        base.Start();
        trajectoryPreview.enabled = false;
    }
    protected override void ShootProjectile(float bulletLifetimeMultiplier = 1f, float forceMultiplier = 1f) {

        GameObject rocket = standardRocket;
        if(PlayerShoot.Instance.GetRocketLauncherNukeMode()) {
            rocket = nukeRocket;
        }

        GunProjectile gunProjectile = Instantiate(rocket, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
        gunProjectile.gameObject.SetActive(true);


        float loadingShotMultiplier = 1f;

        if (gunSO.gunType == GunSO.GunType.RocketLauncher && PlayerShoot.Instance.GetRocketLauncherNukeMode()) {
            float loadingShotTimer = PlayerShoot.Instance.GetLoadingShotTimerNormalized(); // 0 -> 1
            float minLoadShotForceNormalized = .2f;
            // Remapper pour que 0 -> minForce, 1 -> 1
            loadingShotMultiplier = Mathf.Lerp(minLoadShotForceNormalized, 1f, loadingShotTimer);
        }

        Vector2 initialForce = loadingShotMultiplier * PlayerAim.Instance.GetEffectiveAimDir().normalized * bulletSpeed * forceMultiplier;

        // --- Force minimale ---
        gunProjectile.InitializeProjectile(this, bulletLifetime * bulletLifetimeMultiplier, damagePerBullet, bulletKnockback, initialForce, explosionRadiusMultiplier, pierceAmount, bulletSizeMultiplier);
    }


    protected override void PlayerShoot_OnPlayerSwitchedFireMode(object sender, EventArgs e) {
        if (!gunActive) return;

        if (gunSO.gunType == GunSO.GunType.RocketLauncher) {
            if (secondSecondaryAbilityEquipped) {

                if (PlayerShoot.Instance.GetRocketLauncherNukeMode()) {

                    trajectoryPreviewLR.enabled = true;
                    trajectoryPreview.enabled = true;
                    bulletSpeed = 30;
                    bulletLifetime = 3f;
                    BuffBulletDamage(rocketLauncherNukeDmgBuff);

                } else {

                    trajectoryPreviewLR.enabled = false;
                    trajectoryPreview.enabled = false;
                    bulletSpeed = 12.5f;
                    bulletLifetime = 1;
                    DebuffBulletDamage(rocketLauncherNukeDmgBuff);

                }

            }
        }

    }
}
