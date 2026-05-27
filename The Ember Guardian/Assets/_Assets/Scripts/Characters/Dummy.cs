using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Creature
{

    [SerializeField] private Animator dummyAnimator;
    [SerializeField] private int heavyHitDamageTreshold = 20;

    protected override void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start() {
    }


    protected override void Update() {
        if (isPaused) return;

        if (dead) return;

        HandleStatusEffects();

    }


    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        Debug.Log("TakeDamage " + damage);
        if (weakSpotHit) {
            float scaledDamage = damage * 1.2f;
            int baseDamage = Mathf.FloorToInt(scaledDamage);
            float fractional = scaledDamage - baseDamage;

            if (UnityEngine.Random.value < fractional)
                baseDamage += 1;

            damage = baseDamage;
        }

        bool playerIsDamageSource = (damageSource.GetComponent<Player>() != null);

        ShowDamageNumber(damage, critHit, weakSpotHit, playerIsDamageSource);
        HandleBodyAnimator(damage, damageSource);
    }

    private void HandleBodyAnimator(int damage, Transform damageSource) {
        float dir = transform.position.x - damageSource.position.x;

        if(dir > 0) {
            if(damage < heavyHitDamageTreshold) {
                dummyAnimator.SetTrigger("Light_E");
            } else {
                dummyAnimator.SetTrigger("Heavy_E");
            }
        }

        if (dir < 0) {
            if (damage < heavyHitDamageTreshold) {
                dummyAnimator.SetTrigger("Light_W");
            }
            else {
                dummyAnimator.SetTrigger("Heavy_W");
            }
        }
    }
    protected override void OnEnable() {
    }

    protected override void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;

    }
}
