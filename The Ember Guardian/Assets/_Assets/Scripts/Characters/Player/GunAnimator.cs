using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    protected Animator animator;
    protected Gun gun;

    protected void Awake() {
        animator = GetComponent<Animator>();
        gun = GetComponent<Gun>();
    }

    protected void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerCooldownAnimationTrigger += PlayerShoot_OnPlayerCooldownAnimationTrigger;
        PlayerShoot.Instance.OnPlayerReload += PlayerSHoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerSHoot_OnPlayerSwitchedFireMode;

        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    protected virtual void PlayerSHoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        animator.SetTrigger("SwitchFireMode");
    }

    protected void PlayerShoot_OnPlayerCooldownAnimationTrigger(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        if (gun.GetCurrentBullet() != 0) {
            animator.speed = 1;
            animator.SetTrigger("Cooldown");
        } else {
            animator.speed = 1;
            animator.SetTrigger("OutOfAmmo");
        }

    }

    protected void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        animator.SetTrigger("OutOfAmmo");
    }

    protected void PlayerSHoot_OnPlayerReload(object sender, System.EventArgs e) {

        float reloadAnimationSpeed = PlayerShoot.Instance.GetHeldGunSO().handsAnimationReloadTime / PlayerStats.Instance.GetHandsReloadTime();
        animator.speed = reloadAnimationSpeed;
        animator.SetTrigger("Reload");
    
    }

    protected void PlayerShoot_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        //animator.speed = 1;
        //animator.SetTrigger("Cooldown");
    }

    protected void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.Play("Idle");
    }

    protected void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.SetTrigger("Shoot");
    }

    protected void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {

    }

    protected void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShootStopped -= PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger -= PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerCooldownAnimationTrigger -= PlayerShoot_OnPlayerCooldownAnimationTrigger;
        PlayerShoot.Instance.OnPlayerReload -= PlayerSHoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo -= PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode -= PlayerSHoot_OnPlayerSwitchedFireMode;

        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
    }
}
