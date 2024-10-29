using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player mmfPlayer;

    private void Start() {
        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
    }
    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        mmfPlayer.PlayFeedbacks();
    }
}
