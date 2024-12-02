using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player mmfPlayer;
    [SerializeField] private ParticleSystem shellOutPS;

    private Gun gun;

    private void Awake() {
        gun = GetComponentInParent<Gun>();
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownTrigger;
    }

    private void PlayerShoot_OnPlayerCooldownTrigger(object sender, System.EventArgs e) {
        if(!gun.GetGunActive()) return;
        shellOutPS.Emit(1);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        mmfPlayer.PlayFeedbacks();
    }
}
