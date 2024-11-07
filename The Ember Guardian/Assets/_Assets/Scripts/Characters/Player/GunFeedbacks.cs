using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player mmfPlayer;
    [SerializeField] private ParticleSystem shellOutPS;

    private void Start() {
        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownTrigger;
    }

    private void PlayerShoot_OnPlayerCooldownTrigger(object sender, System.EventArgs e) {
        shellOutPS.Emit(1);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        mmfPlayer.PlayFeedbacks();
    }
}
