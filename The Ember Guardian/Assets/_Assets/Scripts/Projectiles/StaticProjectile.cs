using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile : MonoBehaviour
{
    [SerializeField] protected float projectileLifetime;

    protected Mob parentMob;
    protected Animator staticProjectileAnimator;
    protected float projectileLifetimer;
    protected bool hasHit;
    protected bool isPlayerStaticProjectile;
    protected bool projectileIsActive;
    protected int damage;

    protected bool burningEffect;
    protected int burnDuration;
    protected bool immobilizeEffect;
    protected bool stunEffect;
    protected float immobilizeDuration;
    protected float stunDuration;
    protected bool poisonEffect;
    protected int poisonAmount;
    protected bool knockbackEffect;
    protected float knockbackAmount;

    public event EventHandler OnStaticProjectileHitCreature;
    public event EventHandler OnTrapTriggered;

    protected virtual void Update() {
        projectileLifetimer += Time.deltaTime;

        if (projectileLifetimer > projectileLifetime && projectileIsActive) {

            projectileIsActive = false;

            if (isPlayerStaticProjectile) {
                Destroy(gameObject);
            } else {
                ResetInProjectilePool();
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if ((hasHit)) return;

        if(isPlayerStaticProjectile) {
            Creature creatureHit = collision.GetComponent<Creature>();
            if (creatureHit != null) {
                creatureHit.TakeDamage(damage, transform);

                if(burningEffect) {
                    creatureHit.ApplyBurning(burnDuration);
                }
                if (immobilizeEffect) {
                    creatureHit.ApplyImmobilizeEffect(immobilizeDuration, creatureHit.transform.position);
                }
                if (stunEffect) {
                    creatureHit.ApplyStunEffect(stunDuration, creatureHit.transform.position);
                }
                if (poisonEffect) {
                    creatureHit.ApplyPoisonStackedEffect(poisonAmount, 10f);
                }
                if (knockbackEffect) {
                    Vector2 knockBackDirNormalized = (creatureHit.transform.position - transform.position).normalized;
                    knockBackDirNormalized.y = 0;
                    creatureHit.TakeKnockback(knockbackAmount, knockBackDirNormalized);
                }
                OnStaticProjectileHitCreature?.Invoke(this, EventArgs.Empty);
            }

        } else {

            if (collision.GetComponent<Player>() != null) {
                if (collision.GetComponent<HuntingFlag_PlayerDefined>() != null) return;
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

          
            // Hit Fire
            Fire fire = collision.gameObject.GetComponent<Fire>();
            if (fire != null) {
                if (parentMob is Creature) {
                    collision.GetComponent<Fire>().TakeDamage(1, transform, false);
                    if(!(parentMob as Creature).GetCreatureSO().isBoss) {
                        parentMob.Die();
                    }
                };
                hasHit = true;
            }
        }

    }

    public virtual void Initialize(float watchDir, Mob parentMob, int damage, bool isPlayerStaticProjectile = false, bool takeWatchDirInAccount = false) {
        this.parentMob = parentMob;
        this.damage = damage;
        this.isPlayerStaticProjectile = isPlayerStaticProjectile;

        if(takeWatchDirInAccount) {
            if (watchDir < 0) {
                Vector3 localScale = Vector3.one;
                localScale.x = -1f;
                transform.localScale = localScale;
            } else {
                transform.localScale = Vector3.one;
            }
        }

        staticProjectileAnimator = GetComponent<Animator>();
        staticProjectileAnimator.Play(staticProjectileAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
        staticProjectileAnimator.Update(0); // Force une mise à jour immédiate

        projectileIsActive = true;
    }

    public void InitializeBurning(int burnDuration = 5) {
        burningEffect = true;
        this.burnDuration = burnDuration;
    }
    public void InitializeImmobilize(float immobilizeDuration = 5) {
        immobilizeEffect = true;
        this.immobilizeDuration = immobilizeDuration;
    }
    public void InitializeStun(float stunDuration = 5) {
        stunEffect = true;
        this.stunDuration = stunDuration;
    }

    public void InitializePoison(int poisonAmount = 5) {
        Debug.Log("poisonAmount " + poisonAmount);
        poisonEffect = true;
        this.poisonAmount = poisonAmount;
    }
    public void InitializeKnockback(float knockBackAmount) {
        Debug.Log("knockBackAmount " + knockBackAmount);
        knockbackEffect = true;
        this.knockbackAmount = knockBackAmount;
    }

    protected void ResetInProjectilePool() {
        if(parentMob != null) {
            parentMob.GetComponent<MobAttack>().ResetStaticProjectileInObjectPool(this);
            gameObject.SetActive(false);
            hasHit = false;
            projectileIsActive = true;
            projectileLifetimer = 0;
        } else {
            Destroy(gameObject);
        }

    }

    protected void InvokeOnStaticProjectileHitCreature() {
        OnStaticProjectileHitCreature?.Invoke(this, EventArgs.Empty);
    }
    protected void InvokeOnTrapTriggered() {
        OnTrapTriggered?.Invoke(this, EventArgs.Empty);
    }
}
