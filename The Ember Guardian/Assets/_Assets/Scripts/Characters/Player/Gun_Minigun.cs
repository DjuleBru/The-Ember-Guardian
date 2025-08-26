using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Minigun : Gun {

    private float currentSpinCooldown;
    private float standardMaxSpinRate = 0.12f;
    private float maxSpinRate;

    private float spinningMovementDebuff = 1.5f;
    private float spinTimer;
    private float spinStartCooldown;
    private bool isSpinning;
    private bool secondaryActive;
    private bool shooting;

    private float secondaryAmmoConsumptionTimer;
    private float secondaryAmmoConsumptionTime = .35f;

    public event EventHandler OnMinigunStartedSpinning;
    public event EventHandler OnMinigunStoppedSpinning;
    public event EventHandler OnMinigunConsumeAmmoWhileSpinning;

    protected override void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityStarted += PlayerShoot_OnWeaponSecondaryAbilityStarted;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityEnded += PlayerShoot_OnWeaponSecondaryAbilityEnded;

        base.Start();
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityEnded(object sender, EventArgs e) {
        if (!gunActive) return;

        StopSecondary();
    }

    private void StopSecondary() {
        secondaryActive = false;
        if (isSpinning) {
            StopSpinning();
        }
    }

    private void StopSpinning() {
        if (!isSpinning) return;

        isSpinning = false;
        OnMinigunStoppedSpinning?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.BuffMoveSpeed(spinningMovementDebuff);

        currentSpinCooldown = spinStartCooldown;
        PlayerStats.Instance.SetShootCooldownTime(currentSpinCooldown);
    }
    private void StartSpinning() {
        isSpinning = true;
        spinTimer = 0;
        OnMinigunStartedSpinning?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.DebuffMoveSpeed(spinningMovementDebuff);
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityStarted(object sender, EventArgs e) {
        if (!gunActive) return;
        if (secondaryActive) return;

        secondaryActive = true;
        secondaryAmmoConsumptionTimer = 0;
        if (!isSpinning) {
            StartSpinning();
        }
    }

    public override void RefreshGunStats() {
       base.RefreshGunStats();

        float cooldownBuff = cooldownTime / gunSO.shootCooldownTime;

        spinStartCooldown = cooldownTime;
        currentSpinCooldown = cooldownTime;
        maxSpinRate = standardMaxSpinRate * cooldownBuff;
    }

    protected override void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (!gunActive) return;

        base.PlayerShoot_OnPlayerShot(sender, e);
        if(!isSpinning) {
            StartSpinning();
        }

        shooting = true;
    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        if (!gunActive) return;

        if(isSpinning && !secondaryActive) {
            StopSpinning();
        }
        shooting = false;
    }

    protected override void Update() {
        base.Update();
        if (isSpinning) {
            if (spinTimer < spinUpDuration) {
                spinTimer += Time.deltaTime;
                spinTimer = Mathf.Min(spinTimer, spinUpDuration);
            }

            float t = spinTimer / spinUpDuration;
            float interpolatedCooldown = Mathf.Lerp(spinStartCooldown, maxSpinRate, t);
            PlayerStats.Instance.SetShootCooldownTime(interpolatedCooldown);
        }

        if(secondaryActive && !shooting) {
            secondaryAmmoConsumptionTimer += Time.deltaTime;
            if(secondaryAmmoConsumptionTimer > secondaryAmmoConsumptionTime) {
                secondaryAmmoConsumptionTimer = 0;
                OnMinigunConsumeAmmoWhileSpinning?.Invoke(this, EventArgs.Empty);
                PlayerShoot.Instance.SetGunAmmo(gunSO, GetCurrentBullet()-1);

                if(currentBullet <= 0) {
                    StopSpinning();
                }
            }
        }
    }

    public override void SetCooldownTime_Meta(float cooldownTime) {
        base.SetCooldownTime_Meta(cooldownTime);

        float cooldownBuff =  cooldownTime / gunSO.shootCooldownTime;

        spinStartCooldown = cooldownTime;
        currentSpinCooldown = cooldownTime;
        maxSpinRate = standardMaxSpinRate * cooldownBuff;
    }
}
