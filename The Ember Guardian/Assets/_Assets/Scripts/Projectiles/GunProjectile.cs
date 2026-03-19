using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile : MonoBehaviour
{

    [SerializeField] protected GunProjectile_BounceHandler gunProjectile_BounceHandler;
    [SerializeField] protected bool explodeOnContact;
    [SerializeField] protected bool damagesPlayer;
    [SerializeField] protected float maxDistanceToDamagePlayer = 1f;
    protected int penetrationMaxAmount = 1;

    protected Rigidbody2D rb;
    protected float projectileLifetime;
    protected float knockBackForce;
    protected float bulletSizeMultiplier;
    protected int projectileExplosionDamage;

    protected float lifetimeTimer;
    protected float explosionRadiusMultiplier;
    protected bool projectileExploded;
    protected bool projectileHitCreature;
    protected bool projectileBouncedOnGround;

    protected bool projectileExplodesOnContact;

    protected Vector2 initialForce;
    protected Gun parentGun;
    public event EventHandler OnProjectileExploded;

    protected virtual void Awake() {
        gunProjectile_BounceHandler.OnProjectileBouncedOnGround += GunProjectile_BounceHandler_OnProjectileBouncedOnGround;
    }

    protected void GunProjectile_BounceHandler_OnProjectileBouncedOnGround(object sender, EventArgs e) {
        projectileBouncedOnGround = true;
    }

    protected virtual void Update() {
        if (projectileExploded) return;

        lifetimeTimer -= Time.deltaTime;

        if (lifetimeTimer < 0) {
            Explode();
        }
    }

    protected virtual void Explode() {
        transform.rotation = Quaternion.identity;

        rb.bodyType = RigidbodyType2D.Static;
        OnProjectileExploded?.Invoke(this, EventArgs.Empty);
        transform.localScale = Vector3.one * explosionRadiusMultiplier * bulletSizeMultiplier;

        projectileExploded = true;

        if(damagesPlayer) {
            if(Vector3.Distance(transform.position, Player.Instance.transform.position) < maxDistanceToDamagePlayer) {
                Player.Instance.TakeDamage(1, this.transform);
            }
        }

        StartCoroutine(DestroyGameObjectAfterDelay(1f));
    }

    protected IEnumerator DestroyGameObjectAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        Creature creatureHit = collision.GetComponent<Creature>();
        CreatureSpawnerContinuous spawnerHit = collision.GetComponent<CreatureSpawnerContinuous>();

        if (creatureHit != null || spawnerHit != null) {
            if (explodeOnContact) {
                if (!projectileExploded) {
                    Explode();
                    return;
                };

                if(creatureHit != null) {
                    DamageCreatureHit(creatureHit, collision);
                    return;
                }
                if(spawnerHit != null) {
                    DamageSpawnerHit(spawnerHit, collision);
                }

            }
            else {

                if (!projectileExploded) {
                    if (projectileHitCreature) return;
                    if (projectileBouncedOnGround) return;
                    projectileHitCreature = true;
                    int projectileHitDamage = Mathf.RoundToInt(projectileExplosionDamage / 10f);

                    if(creatureHit != null) {
                        creatureHit.TakeDamage(projectileHitDamage, transform, false);
                        return;
                    }
                    if(spawnerHit != null) {
                        spawnerHit.TakeDamage(projectileHitDamage, transform, false);
                    }

                }
                else {
                    if (creatureHit != null) {
                        DamageCreatureHit(creatureHit, collision);
                        return;
                    }
                    if (spawnerHit != null) {
                        DamageSpawnerHit(spawnerHit, collision);
                    }
                }
            }

        }

        // Détection du sol (Layer "Ground")
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            if (projectileExploded) return;

            if (explodeOnContact) {
                Explode();
            }
        }
    }

    protected virtual void DamageCreatureHit(Creature creatureHit, Collider2D collision) {
        creatureHit.TakeDamage(projectileExplosionDamage, transform, false);

        Vector2 knockbackDirNormalized = (collision.transform.position - transform.position).normalized;
        knockbackDirNormalized.y = 0;
        creatureHit.TakeKnockback(knockBackForce, knockbackDirNormalized);
    }

    protected virtual void DamageSpawnerHit(CreatureSpawnerContinuous spawner, Collider2D collision) {
        spawner.TakeDamage(projectileExplosionDamage, transform, false);
    }

    public virtual void InitializeProjectile(Gun parentGun, float projectileLifetime, int projectileDamage, float knockbackForce, Vector2 initialForce, float explosionRadiusMultiplier, int pierceAmount, float bulletSizeMultiplier) {
        this.parentGun = parentGun;
        this.projectileLifetime = projectileLifetime;
        this.projectileExplosionDamage = projectileDamage;
        this.explosionRadiusMultiplier = explosionRadiusMultiplier;
        penetrationMaxAmount = pierceAmount;
        this.knockBackForce = knockbackForce;
        this.bulletSizeMultiplier = bulletSizeMultiplier;
        lifetimeTimer = projectileLifetime;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(initialForce, ForceMode2D.Impulse);
        float torque = UnityEngine.Random.Range(-10f, 10f);
        rb.AddTorque(torque);

        Vector3 localScale = Vector3.one * bulletSizeMultiplier;
        transform.localScale = localScale;

        this.initialForce = initialForce;
    }

    public void InvokeOnProjectileExploded() {
        OnProjectileExploded?.Invoke(this, EventArgs.Empty);    
    }
}
