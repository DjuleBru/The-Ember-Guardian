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
    [SerializeField] private bool activateShieldRandomly;
    [SerializeField] private float timeToActivateShieldAfterDamageTaken;

    private float shieldTimer;
    private int shieldHealth;

    private bool damageTaken;
    private bool shieldActivating;
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


        if (activateShieldRandomly) {
            float randomValue = UnityEngine.Random.value;
            if (randomValue > .5f) {
                ActivateShield();
            }
        }
    }

    protected override void Update() {
        base.Update();

        if (shieldActive) return;
        if (dead) return;
        if (shieldActivating) return;
        if (activateShieldAfterDamageTaken && !damageTaken) return;

        shieldTimer -= Time.deltaTime;
        if(shieldTimer <= 0) {
            ActivateShield();
        }
    }

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if(activateShieldAfterDamageTaken && !shieldOnCooldown) {
            damageTaken = true;
        }

        if (shieldActive) {

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

        if(activateShieldAfterDamageTaken && !shieldActive && !shieldOnCooldown && !shieldActivating) {
            StartCoroutine(ActivateShieldAfterDelay(timeToActivateShieldAfterDamageTaken));
        }

    }

    private IEnumerator ActivateShieldAfterDelay(float delay) {
        shieldActivating = true;

        yield return new WaitForSeconds(delay);
        if (dead) yield break;

        shieldActivating = false;
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

    public bool GetShieldActive() {
        return shieldActive;
    }

}
