using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature_ActivateShieldOnBounce : Creature
{
    [SerializeField] private float probabilityToActivateShieldOnHit = .3f;
    [SerializeField] private float activateShieldDelay = 5f;
    private bool justActivatedShield;
    private float activatedShieldTimer;

    public event EventHandler OnShieldActivated;

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        base.TakeDamage(damage, damageSource, critHit, ignoreTemporaryInvincibility, weakSpotHit);

        if (dead) return;

        float randomFloat = UnityEngine.Random.value;
        if(randomFloat < probabilityToActivateShieldOnHit) {
            justActivatedShield = true;
            activatedShieldTimer = 0;
            OnShieldActivated?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void Update() {
        base.Update();

        if(justActivatedShield) {
            activatedShieldTimer += Time.deltaTime;
            if(activatedShieldTimer > activateShieldDelay) {
                justActivatedShield = false;
            }
        }
    }

}
