using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks_Rifle : GunFeedbacks
{

    [SerializeField] protected MMF_Player loadShotFeedbacks;
    [SerializeField] protected MMF_Player loadShot_ShootFeedbacks;


    protected override void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;

        if(PlayerShoot.Instance.GetRifleLoadShotModeActive() || PlayerShoot.Instance.GetSniperPiercingRoundsActive()) {
            loadShot_ShootFeedbacks.PlayFeedbacks();
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

        if (loadShotFeedbacks != null) {
            loadShotFeedbacks.PlayFeedbacks();
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

        if (loadShotFeedbacks != null) {
            loadShotFeedbacks.StopFeedbacks();
        }
    }
}
