using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileFeedbacks : MonoBehaviour
{
    private MMF_Player mmfPlayer;
    private GunProjectile gunProjectile;

    private void Awake() {
        mmfPlayer = GetComponent<MMF_Player>();
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;
    }

    private void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        mmfPlayer.PlayFeedbacks();
    }
}
