using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Minigun : Gun {

    [SerializeField] private float spinUpRate = .01f;
    private float currentSpinCooldown;
    private float maxSpinRate = 0.12f;

    private bool spinCooldownInitialized;

    protected override void Start() {
        base.Start();

        PlayerShoot.Instance.OnPlayerShootStopped += PlayerSHoot_OnPlayerShootStopped;
    }

    private void PlayerSHoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        currentSpinCooldown = cooldownTime;
        PlayerStats.Instance.SetShootCooldownTime(currentSpinCooldown);
    }

    protected override void Shoot() {
        if(!spinCooldownInitialized) {
            currentSpinCooldown = cooldownTime;
            spinCooldownInitialized = true;
        }
        base.Shoot();

        if(currentSpinCooldown > maxSpinRate) {
            currentSpinCooldown -= spinUpRate;
            PlayerStats.Instance.SetShootCooldownTime(currentSpinCooldown);
        }

    }

}
