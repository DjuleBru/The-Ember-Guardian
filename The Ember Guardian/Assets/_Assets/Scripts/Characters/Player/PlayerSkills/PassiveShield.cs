using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveShield : MonoBehaviour, IDamageable
{
    public event EventHandler OnShieldActivated;
    public event EventHandler OnShieldDied;
    public static event EventHandler OnAnyPassiveShieldActivated;
    public static event EventHandler OnAnyPassiveShieldDied;

    private bool shieldUnlocked;
    private bool shieldActive;

    private float activateShieldTimer;
    private float activateShieldTime = 150f;

    private void Update() {
        if (!shieldUnlocked) return;
        if (shieldActive) return;

        activateShieldTimer += Time.deltaTime;
        if(activateShieldTimer > activateShieldTime) {
            activateShieldTimer = 0;
            ActivateShield();
        }

    }

    public void UnlockShield(float shieldRegenTime) {
        shieldUnlocked = true;
        activateShieldTime = shieldRegenTime;
        ActivateShield();
    }

    public void ActivateShield() {
        shieldActive = true;
        OnShieldActivated?.Invoke(this, EventArgs.Empty);
        OnAnyPassiveShieldActivated?.Invoke(this, EventArgs.Empty);
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit) {
        shieldActive = false;
        OnShieldDied?.Invoke(this, EventArgs.Empty);
        OnAnyPassiveShieldDied?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {

    }


    public bool GetShieldActive() {
        return shieldActive;
    }
}
