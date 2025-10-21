using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    protected Animator animator;
    protected Gun gun;
    protected GunJamHandler gunJamHandler;

    protected float meleeAttackSpeed = 1.5f;
    protected bool reloading; 
    protected float reloadAnimTime = 0f;

    protected void Awake() {
        animator = GetComponent<Animator>();
        gun = GetComponent<Gun>();
        gunJamHandler = GetComponent<GunJamHandler>();
    }

    protected virtual void Start() {
        gun.OnGunJammed += Gun_OnGunJammed;
        gunJamHandler.OnCorrectJamSequenceInput += GunJamHandler_OnCorrectJamSequenceInput;
        gunJamHandler.OnJamSequenceFailStarted += GunJamHandler_OnJamSequenceFailStarted;

        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerCooldownAnimationTrigger += PlayerShoot_OnPlayerCooldownAnimationTrigger;
        PlayerShoot.Instance.OnPlayerReload += PlayerSHoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded += PlayerShoot_OnPlayerReloadInterruptedEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerSHoot_OnPlayerSwitchedFireMode;
        PlayerShoot.Instance.OnPlayerSwappedGunStarted += PlayerShoot_OnPlayerSwappedGunStarted;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerUI_TickTemplate.OnAnyBulletPingShineReachedGun += PlayerUI_TickTemplate_OnAnyBulletPingShineReachedGun;

        PlayerMeleeAttack.Instance.OnMeleeAttackStarted += PlayerMeleeAttack_OnMeleeAttackStarted;

        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    protected void GunJamHandler_OnJamSequenceFailStarted(object sender, System.EventArgs e) {
        //animator.SetTrigger("GunJamHit");
    }

    protected void Gun_OnGunJammed(object sender, System.EventArgs e) {
        animator.SetTrigger("OutOfAmmo");
    }

    protected void GunJamHandler_OnCorrectJamSequenceInput(object sender, System.EventArgs e) {

    }

    private void PlayerUI_TickTemplate_OnAnyBulletPingShineReachedGun(object sender, System.EventArgs e) {
        animator.SetTrigger("GunJamHit");
    }

    protected void PlayerMeleeAttack_OnMeleeAttackStarted(object sender, System.EventArgs e) {
        animator.SetTrigger("MeleeAttack");
        animator.speed = meleeAttackSpeed;
    }

    protected void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        animator.SetTrigger("SwapGunEnd");
        float animationMultiplier =  1/gun.GetSwapToWeaponTimeMultiplier();
        animator.SetFloat("SwapGunMultiplier", animationMultiplier);
    }

    protected void PlayerShoot_OnPlayerSwappedGunStarted(object sender, System.EventArgs e) {
        animator.SetTrigger("SwapGunStart");
        float animationMultiplier = 1/gun.GetSwapToWeaponTimeMultiplier();
        animator.SetFloat("SwapGunMultiplier", animationMultiplier);
    }

    protected virtual void PlayerSHoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        animator.SetTrigger("SwitchFireMode");
    }

    protected void PlayerShoot_OnPlayerCooldownAnimationTrigger(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        StartCoroutine(TriggerCDAnimationAfterDelay(gun.GetGunSO().shotCooldownAnimationTriggerTime));
    }

    protected IEnumerator TriggerCDAnimationAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        if (gun.GetCurrentBullet() != 0) {
            animator.speed = 1;
            animator.SetTrigger("Cooldown");
        }
        else {
            animator.speed = 1;
            animator.SetTrigger("OutOfAmmo");
        }

    }

    protected void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        animator.SetTrigger("OutOfAmmo");
    }

    protected void PlayerSHoot_OnPlayerReload(object sender, System.EventArgs e) {
        reloading = true;

        float reloadAnimationSpeed = PlayerShoot.Instance.GetHeldGunSO().handsAnimationReloadTime / PlayerStats.Instance.GetHandsReloadTime();
        animator.speed = reloadAnimationSpeed;

        animator.SetTrigger("Reload");
    
    }

    protected void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        reloading = false;
    }
    protected void PlayerShoot_OnPlayerReloadInterruptedEnded(object sender, System.EventArgs e) {
        animator.Play("Reload", 0, reloadAnimTime); // Reprend à la même position
        animator.speed = 1f;
    }

    protected void PlayerShoot_OnPlayerReloadInterrupted(object sender, System.EventArgs e) {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        reloadAnimTime = state.normalizedTime;

        animator.speed = 0f;

        //reloading = false;

        //animator.SetTrigger("InterruptReload");
        //animator.speed = 1;
    }

    protected void PlayerShoot_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        //animator.speed = 1;
        //animator.SetTrigger("Cooldown");
    }

    protected void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        animator.speed = 1;
        animator.Play("Idle");
    }

    protected virtual void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        animator.SetTrigger("Shoot");

        // Only for passive skill shoot on reload
        if (reloading) return;

        animator.speed = 1;
    }


    protected void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerCooldownTrigger -= PlayerShoot_OnPlayerCooldownSFXTrigger;
        PlayerShoot.Instance.OnPlayerCooldownAnimationTrigger -= PlayerShoot_OnPlayerCooldownAnimationTrigger;
        PlayerShoot.Instance.OnPlayerReload -= PlayerSHoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadInterrupted -= PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo -= PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode -= PlayerSHoot_OnPlayerSwitchedFireMode;

        PlayerMeleeAttack.Instance.OnMeleeAttackStarted -= PlayerMeleeAttack_OnMeleeAttackStarted;
        PlayerUI_TickTemplate.OnAnyBulletPingShineReachedGun -= PlayerUI_TickTemplate_OnAnyBulletPingShineReachedGun;

        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
    }
}
