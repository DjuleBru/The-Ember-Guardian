using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Minigun : Gun {

    [SerializeField] private float spinUpDuration = 3f;
    private float currentSpinCooldown;
    private float maxSpinRate = 0.12f;

    private float spinTimer;
    private float spinStartCooldown;
    private bool isSpinning;
    private bool secondaryActive;

    public event EventHandler OnMinigunStartedSpinning;
    public event EventHandler OnMinigunStoppedSpinning;

    protected override void Start() {
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityStarted += PlayerShoot_OnWeaponSecondaryAbilityStarted;
        PlayerShoot.Instance.OnWeaponSecondaryAbilityEnded += PlayerShoot_OnWeaponSecondaryAbilityEnded;

        base.Start();
    }

    private void PlayerShoot_OnWeaponSecondaryAbilityEnded(object sender, EventArgs e) {
        secondaryActive = false;
        if (isSpinning) {
            isSpinning = false;
            OnMinigunStoppedSpinning?.Invoke(this, EventArgs.Empty);
        }

    }

    private void PlayerShoot_OnWeaponSecondaryAbilityStarted(object sender, EventArgs e) {
        secondaryActive = true;
        if (!isSpinning) {
            isSpinning = true;
            spinTimer = 0;
            OnMinigunStartedSpinning?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void RefreshGunStats() {
       base.RefreshGunStats();
        spinStartCooldown = cooldownTime;
        currentSpinCooldown = cooldownTime;
    }

    protected override void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        base.PlayerShoot_OnPlayerShot(sender, e);
        if(!isSpinning) {
            isSpinning = true;
            spinTimer = 0;
            OnMinigunStartedSpinning?.Invoke(this, EventArgs.Empty);
        }

    }

    private void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        if(isSpinning && !secondaryActive) {
            isSpinning = false;
            currentSpinCooldown = spinStartCooldown;
            PlayerStats.Instance.SetShootCooldownTime(currentSpinCooldown);
            OnMinigunStoppedSpinning?.Invoke(this, EventArgs.Empty);
        }
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
    }
}
