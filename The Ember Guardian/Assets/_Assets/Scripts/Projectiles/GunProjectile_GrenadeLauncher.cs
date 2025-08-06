using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_GrenadeLauncher : GunProjectile
{

    protected void Start() {
        PlayerShoot.Instance.OnPlayerTriggersProjectileExplosion += PlayerShoot_OnPlayerTriggersProjectileExplosion;
    }

    protected void PlayerShoot_OnPlayerTriggersProjectileExplosion(object sender, EventArgs e) {
        Explode();
    }

    protected override void Update() {
        if (projectileExploded) return;

        lifetimeTimer -= Time.deltaTime;

        if (PlayerShoot.Instance.GetProjectileExplodesOnPlayerClickModeActive()) return;

        if (lifetimeTimer < 0) {
            Explode();
        }
    }

    protected void OnDestroy() {
        PlayerShoot.Instance.OnPlayerTriggersProjectileExplosion -= PlayerShoot_OnPlayerTriggersProjectileExplosion;
    }
}
