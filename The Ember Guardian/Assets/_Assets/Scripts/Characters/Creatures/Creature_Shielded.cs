using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature_Shielded : Creature
{
    [SerializeField] private int shieldMaxHealth;
    [SerializeField] private float shieldCooldown;

    private float shieldTimer;
    private int shieldHealth;

    private bool shieldActive;

    public event EventHandler OnShieldRegenerated;
    public event EventHandler OnShieldDestroyed;
    public event EventHandler OnShieldTakesDamage;

    protected override void Start() {
        base.Start();
        shieldHealth = shieldMaxHealth;
    }

    protected override void Update() {
        base.Update();

        if (shieldActive) return;

        shieldTimer -= Time.deltaTime;
        if(shieldTimer <= 0) {
            ActivateShield();
        }
    }

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if(shieldActive) {

            ShowDamageNumber(damage, critHit, weakSpotHit);
            if (critHit) {
                damage *= 2;
            }

            shieldHealth -= damage;
            if(shieldHealth <= 0) {
                DestroyShield();
            }
            else {
                //InvokeOnMobDamageTaken(damageSource);
                OnShieldTakesDamage?.Invoke(this, EventArgs.Empty);
            }

        } else {

            if (weakSpotHit) {
                float scaledDamage = damage * 1.2f;
                int baseDamage = Mathf.FloorToInt(scaledDamage);
                float fractional = scaledDamage - baseDamage;

                if (UnityEngine.Random.value < fractional)
                    baseDamage += 1;

                damage = baseDamage;
            }

            base.TakeDamage(damage, damageSource, critHit, ignoreTemporaryInvincibility, weakSpotHit);
        }

    }

    private void ActivateShield() {
        shieldHealth = shieldMaxHealth;
        shieldActive = true;
        instantiatePSOnHit = false;

        OnShieldRegenerated?.Invoke(this, EventArgs.Empty); 
    }

    private void DestroyShield() {
        shieldHealth = 0;
        shieldActive = false;
        shieldTimer = shieldCooldown;
        instantiatePSOnHit = true;

        OnShieldDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
