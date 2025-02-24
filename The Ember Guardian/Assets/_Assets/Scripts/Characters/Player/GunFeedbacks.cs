using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player mmfPlayer;
    [SerializeField] private ParticleSystem shellOutPS;

    [SerializeField] private ParticleSystem loadGunPS1;
    [SerializeField] private ParticleSystem loadGunPS2;

    private Gun gun;

    private void Awake() {
        gun = GetComponentInParent<Gun>();
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerStartedShot += PlayerShoot_OnPlayerStartedShot;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownTrigger;
    }

    private void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if(loadGunPS1 != null) {
            loadGunPS1.Play();
        }
        if (loadGunPS2 != null) {
            loadGunPS2.Play();
        }
    }

    private void PlayerShoot_OnPlayerCooldownTrigger(object sender, System.EventArgs e) {
        if(!gun.GetGunActive()) return;
        shellOutPS.Emit(1);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;
        mmfPlayer.PlayFeedbacks();
    }
}
