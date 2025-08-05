using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator_Flamethrower : GunAnimator
{
    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnShotStartedLoading += PlayerShoot_OnShotStartedLoading;
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        Debug.Log("PlayerShoot_OnPlayerShootStopped");
        animator.ResetTrigger("Shoot_Start");
        animator.SetTrigger("Shoot_End");
    }

    private void PlayerShoot_OnShotStartedLoading(object sender, System.EventArgs e) {
        Debug.Log("PlayerShoot_OnShotStartedLoading");
        animator.ResetTrigger("Shoot_End");
        animator.SetTrigger("Shoot_Start");
    }
}
