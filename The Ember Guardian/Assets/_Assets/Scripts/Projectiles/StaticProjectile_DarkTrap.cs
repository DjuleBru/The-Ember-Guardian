using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile_DarkTrap : StaticProjectile
{

    protected bool trapTriggered;
    protected bool trapPoisonEffectApplied;

    protected override void Update() {
        if (!trapTriggered) return;

        projectileLifetimer += Time.deltaTime;

        if (projectileLifetimer > projectileLifetime && projectileIsActive) {

            projectileIsActive = false;

            if (isPlayerStaticProjectile) {
                Destroy(gameObject);
            }
            else {
                ResetInProjectilePool();
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {

        if (isPlayerStaticProjectile) {
            Creature creatureHit = collision.GetComponent<Creature>();
          
            if (creatureHit != null) {

                if(!trapTriggered) {
                    // Trap has not been triggered yet
                    trapTriggered = true;
                    InvokeOnTrapTriggered();

                }
                else {
                    // Trap has been triggered 

                    if (trapPoisonEffectApplied) {
                        // Second time it hits the creature
                        creatureHit.TakeDamage(damage, transform);

                        if (burningEffect) {
                            creatureHit.ApplyBurning(burnDuration);
                        }
                        if (immobilizeEffect) {
                            creatureHit.ApplyImmobilizeEffect(immobilizeDuration, creatureHit.transform.position);
                        }
                        if (knockbackEffect) {
                            Vector2 knockBackDirNormalized = (creatureHit.transform.position - transform.position).normalized;
                            knockBackDirNormalized.y = 0;
                            creatureHit.TakeKnockback(knockbackAmount, knockBackDirNormalized);
                        }

                        InvokeOnStaticProjectileHitCreature();

                    }
                    else {
                        // First time it hits the creature

                        if (poisonEffect) {
                            creatureHit.ApplyPoisonEffect(poisonAmount);
                        }

                    }
                }

            }

        }
        else {

            if (collision.GetComponent<Player>() != null) {
                Player.Instance.TakeDamage(damage, transform);
                hasHit = true;
            }

            if (collision.GetComponent<Barricade>() != null) {
                collision.GetComponent<Barricade>().TakeDamage(1, transform);
                hasHit = true;
            }

            if (collision.GetComponent<Worker>() != null) {
                Worker worker = collision.GetComponent<Worker>();
                if (worker.GetRecruited()) {
                    collision.GetComponent<Worker>().TakeDamage(1, transform, false);
                    hasHit = true;
                }
            }

            // Hit Barricade
            Fire fire = collision.gameObject.GetComponent<Fire>();
            if (fire != null) {
                collision.GetComponent<Fire>().TakeDamage(1, transform, false);
                parentMob.Die();
                hasHit = true;
            }
        }

    }

    public void SetPoisonEffectAppliedFromAnimator() {
        trapPoisonEffectApplied = true;
    }
}
