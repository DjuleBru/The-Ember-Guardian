using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    private Animator animator;
    private Gun gun;

    private void Awake() {
        animator = GetComponent<Animator>();
        gun = GetComponent<Gun>();
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerCooldownAnimationTrigger += PlayerShoot_OnPlayerCooldownAnimationTrigger;
        PlayerShoot.Instance.OnPlayerReload += PlayerSHoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerSHoot_OnPlayerSwitchedFireMode;

        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void PlayerSHoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        animator.SetTrigger("SwitchFireMode");
    }

    private void PlayerShoot_OnPlayerCooldownAnimationTrigger(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        if (gun.GetCurrentBullet() != 0) {
            animator.speed = 1;
            animator.SetTrigger("Cooldown");
        } else {
            animator.speed = 1;
            animator.SetTrigger("OutOfAmmo");
        }

    }

    private void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        animator.SetTrigger("OutOfAmmo");
    }

    private void PlayerSHoot_OnPlayerReload(object sender, System.EventArgs e) {

        float reloadAnimationSpeed = PlayerShoot.Instance.GetHeldGunSO().animationReloadTime/PlayerStats.Instance.GetReloadTime();
        animator.speed = reloadAnimationSpeed;
        animator.SetTrigger("Reload");
    
    }

    private void PlayerShoot_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        //animator.speed = 1;
        //animator.SetTrigger("Cooldown");
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
