using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile : MonoBehaviour
{

    [SerializeField] protected GunProjectile_BounceHandler gunProjectile_BounceHandler;
    protected Rigidbody2D rb;
    protected float projectileLifetime;
    protected float knockBackForce;
    protected int projectileExplosionDamage;

    protected float lifetimeTimer;
    protected bool projectileExploded;
    protected bool projectileHitCreature;
    protected bool projectileBouncedOnGround;

    public event EventHandler OnProjectileExploded;

    private void Awake() {
        gunProjectile_BounceHandler.OnProjectileBouncedOnGround += GunProjectile_BounceHandler_OnProjectileBouncedOnGround;
    }

    private void GunProjectile_BounceHandler_OnProjectileBouncedOnGround(object sender, EventArgs e) {
        projectileBouncedOnGround = true;
    }

    private void Update() {
        if (projectileExploded) return;

        lifetimeTimer -= Time.deltaTime;

        if(lifetimeTimer < 0) {
            Explode();
            StartCoroutine(DestroyGameObjectAfterDelay());
        }
    }

    private void Explode() {
        rb.bodyType = RigidbodyType2D.Static;
        OnProjectileExploded?.Invoke(this, EventArgs.Empty);

        projectileExploded = true;
    }

    private IEnumerator DestroyGameObjectAfterDelay() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
       
        Creature creatureHit = collision.GetComponent<Creature>();

        if (creatureHit != null) {
            if (!projectileExploded) {
                if (projectileHitCreature) return;
                if (projectileBouncedOnGround) return;
                projectileHitCreature = true;
                int projectileHitDamage = Mathf.RoundToInt(projectileExplosionDamage / 10f);
                creatureHit.TakeDamage(projectileHitDamage, transform, false);
            }
            else {
                creatureHit.TakeDamage(projectileExplosionDamage, transform, false);

                Vector2 knockbackDirNormalized = (collision.transform.position - transform.position).normalized;
                knockbackDirNormalized.y = 0;
                creatureHit.TakeKnockback(knockBackForce, knockbackDirNormalized);
            }
        }
    }
     
    public void InitializeProjectile(float projectileLifetime, int projectileDamage, float knockbackForce, Vector2 initialForce) {
        this.projectileLifetime = projectileLifetime;
        this.projectileExplosionDamage = projectileDamage;
        this.knockBackForce = knockbackForce;
        lifetimeTimer = projectileLifetime;

        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(initialForce, ForceMode2D.Impulse);
        float torque = UnityEngine.Random.Range(-10f, 10f);
        rb.AddTorque(torque);
    }

}
