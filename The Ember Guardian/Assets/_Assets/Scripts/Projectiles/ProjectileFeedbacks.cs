using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFeedbacks : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private MMF_Player projectileFeedbacks;

    private void Start() {
        projectile.OnProjectileHit += Projectile_OnProjectileHit;
    }

    private void Projectile_OnProjectileHit(object sender, System.EventArgs e) {
        if(projectileFeedbacks != null) {
            projectileFeedbacks.PlayFeedbacks();
        }
    }
}
