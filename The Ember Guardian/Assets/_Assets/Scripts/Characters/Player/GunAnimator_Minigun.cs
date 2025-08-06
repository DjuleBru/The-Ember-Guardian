using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator_Minigun : GunAnimator {

    [SerializeField] private List<Animator> muzzleFlashAnimatorList;
    private int muzzleFlashAnimatorIndex;

    protected override void Start() {
        base.Start();
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        animator.SetBool("Shooting", false);
    }

    protected override void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        animator.SetBool("Shooting", true);

        // Only for passive skill shoot on reload
        if (reloading) return;
        animator.speed = 1;

        muzzleFlashAnimatorList[muzzleFlashAnimatorIndex].SetTrigger("Shoot");

        muzzleFlashAnimatorIndex++;
        if(muzzleFlashAnimatorIndex == muzzleFlashAnimatorList.Count) {
            muzzleFlashAnimatorIndex = 0;
        }
    }

}
