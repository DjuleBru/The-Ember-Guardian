using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerReload += PlayerSHoot_OnPlayerReload;

        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void PlayerSHoot_OnPlayerReload(object sender, System.EventArgs e) {

        float reloadAnimationSpeed = PlayerShoot.Instance.GetGunSO().animationReloadTime/PlayerStats.Instance.GetReloadTime();
        animator.speed = reloadAnimationSpeed;
        animator.SetTrigger("Reload");
    
    }

    private void PlayerShoot_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.SetTrigger("Cooldown");
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.Play("Idle");
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.SetTrigger("Shoot");
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {

    }
}
