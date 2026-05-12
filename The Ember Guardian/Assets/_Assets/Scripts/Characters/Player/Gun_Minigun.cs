using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Minigun : Gun {

    private float currentSpinCooldown;
    private float standardMaxSpinRate = 0.1f;
    private float maxSpinRate;

    private float spinningMovementDebuff = 1.5f;
    private float spinTimer;
    private float spinStartCooldown;
    private bool isSpinning;
    private bool secondaryActive;
    private bool shooting;

    private float secondaryAmmoConsumptionTimer;
    private float secondaryAmmoConsumptionTime = .35f;

    private float minigunPrewarmSpinUpDurationBuff = 1.4f;
    private float minigunExplosiveBulletsShootCooldownDebuff = 1.7f;
    private float minigunExplosiveBulletsRangeBuff = 1.35f;

    public event EventHandler OnMinigunStartedSpinning;
    public event EventHandler OnMinigunStoppedSpinning;
    public event EventHandler OnMinigunConsumeAmmoWhileSpinning;

    protected override void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityStarted += PlayerShoot_OnWeaponSecondaryAbilityStarted;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityEnded += PlayerShoot_OnWeaponSecondaryAbilityEnded;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;

        base.Start();
    }

    private void PlayerShoot_OnPlayerSwitchedFireMode(object sender, EventArgs e) {
        if (!gunActive) return;

        if (PlayerShoot.Instance.GetSecondSecondaryAbilityEquipped()) {
            if(PlayerShoot.Instance.GetMinigunExplosiveBulletsActive()) {

                maxSpinRate *= minigunExplosiveBulletsShootCooldownDebuff;
                bulletLifetime *= minigunExplosiveBulletsRangeBuff;

            } else {

                maxSpinRate /= minigunExplosiveBulletsShootCooldownDebuff;
                bulletLifetime /= minigunExplosiveBulletsRangeBuff;

            }
        }
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityEnded(object sender, EventArgs e) {
        if (!gunActive) return;

        if(PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
            StopSecondary();
            spinUpDuration *= minigunPrewarmSpinUpDurationBuff;
        }

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
        PlayerMovement.Instance.BuffMoveSpeed("MinigunSpin", spinningMovementDebuff);

        currentSpinCooldown = spinStartCooldown;
        PlayerStats.Instance.SetShootCooldownTime(currentSpinCooldown);
    }

    private void StartSpinning() {
        isSpinning = true;
        spinTimer = 0;
        OnMinigunStartedSpinning?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.DebuffMoveSpeed("MinigunSpin", spinningMovementDebuff);
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityStarted(object sender, EventArgs e) {
        if (!gunActive) return;
        if (secondaryActive) return;

        if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
            secondaryActive = true;
            secondaryAmmoConsumptionTimer = 0;

            spinUpDuration /= minigunPrewarmSpinUpDurationBuff;

            if (!isSpinning) {
                StartSpinning();
            }
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
                PlayerShoot.Instance.SetGunAmmo(gunSO, currentAmmoClip , GetCurrentBullet()-1);

                if(currentBullet <= 0) {
                    StopSpinning();
                }
            }
        }
    }

    public override void SetCooldownTime_StatModifierListLevel(int cooldownTimeStatModifierLevel) {
        this.cooldownTimeStatModifierLevel = cooldownTimeStatModifierLevel;
        float modifiedCooldown = 0;
        if (cooldownTimeStatModifierLevel == -1) {
            modifiedCooldown = gunSO.shootCooldownTime;
        }
        else {
            modifiedCooldown = gunSO.shootCooldownTime + gunSO.shootCooldownTime * gunSO.cooldownTimeStatModifier.statModifierList[cooldownTimeStatModifierLevel] * 0.01f;
        }

        float cooldownBuff = modifiedCooldown / gunSO.shootCooldownTime;

        spinStartCooldown = modifiedCooldown;
        currentSpinCooldown = modifiedCooldown;
        maxSpinRate = standardMaxSpinRate * cooldownBuff;
    }
}
