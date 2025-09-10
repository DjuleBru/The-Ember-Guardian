using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature_Shielded : Creature
{
    [SerializeField] private int shieldMaxHealth;
    [SerializeField] private float shieldCooldown;
    [SerializeField] private bool activateShieldOnStart = true;
    [SerializeField] private bool activateShieldAfterDamageTaken;
    [SerializeField] private float timeToActivateShieldAfterDamageTaken;

    private float shieldTimer;
    private int shieldHealth;

    private bool shieldActive;
    private bool shieldOnCooldown;

    public event EventHandler OnShieldRegenerated;
    public event EventHandler OnShieldDestroyed;
    public event EventHandler OnShieldTakesDamage;

    protected override void Start() {
        base.Start();
        shieldHealth = shieldMaxHealth;

        if(activateShieldOnStart) {
            ActivateShield();
        }
    }

    protected override void Update() {
        base.Update();

        if (shieldActive) return;
        if (activateShieldAfterDamageTaken) return;
        if (dead) return;

        shieldTimer -= Time.deltaTime;
        if(shieldTimer <= 0) {
            ActivateShield();
        }
    }

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if(shieldActive) {

            bool playerIsDamageSource = (damageSource.GetComponent<Player>() != null);
            ShowDamageNumber(damage, critHit, weakSpotHit, playerIsDamageSource);
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

        if(activateShieldAfterDamageTaken && !shieldActive && !shieldOnCooldown) {
            StartCoroutine(ActivateShieldAfterDelay(timeToActivateShieldAfterDamageTaken));
        }

    }

    private IEnumerator ActivateShieldAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        if (dead) yield break;

        ActivateShield();
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
        shieldOnCooldown = true;
        instantiatePSOnHit = true;

        OnShieldDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
