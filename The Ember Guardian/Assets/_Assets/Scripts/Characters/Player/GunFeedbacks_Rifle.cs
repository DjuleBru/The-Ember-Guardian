using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks_Rifle : GunFeedbacks
{

    [SerializeField] protected MMF_Player rifleLoadShotFeedbacks;
    [SerializeField] protected MMF_Player rifleLoadShot_ShootFeedbacks;


    protected override void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;

        if(PlayerShoot.Instance.GetRifleLoadShotModeActive()) {
            rifleLoadShot_ShootFeedbacks.PlayFeedbacks();
        } else {
            mmfPlayer.PlayFeedbacks();
        }

    }


    protected override void PlayerShoot_OnShotStartedLoading(object sender, System.EventArgs e) {
        if (loadGunPS1 != null) {
            loadGunPS1.Play();
        }
        if (loadGunPS2 != null) {
            loadGunPS2.Play();
        }

        if (rifleLoadShotFeedbacks != null) {
            rifleLoadShotFeedbacks.PlayFeedbacks();
        }
    }

    protected override void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        if (loadGunPS1 != null) {
            if (loadGunPS1.isPlaying) {
                loadGunPS1.Stop();
            }
        }
        if (loadGunPS2 != null) {
            if (loadGunPS2.isPlaying) {
                loadGunPS2.Stop();
            }
        }

        if (rifleLoadShotFeedbacks != null) {
            rifleLoadShotFeedbacks.StopFeedbacks();
        }
    }
}
