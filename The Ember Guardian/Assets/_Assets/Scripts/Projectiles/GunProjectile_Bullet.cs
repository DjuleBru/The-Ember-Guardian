using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_Bullet : GunProjectile {
    [SerializeField] protected LayerMask raycastMask;
    [SerializeField] protected float circleCastRadius = 0.15f;
    [SerializeField] protected GameObject explosionPrefab;

    public static event EventHandler<OnBulletHitEventArgs> OnAnyBulletHitGround;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyBulletHitEnemy;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyPlayerBulletHitEnemyCrit;
    public static event EventHandler OnAnyParticleBouncedOff;
    public static event EventHandler<OnBulletPierceCreatureEventArgs> OnAnyBulletPierceCreature;

    public class OnBulletHitEventArgs {
        public Vector3 bulletHitPosition;
        public Mob mobHit;
    }
    public class OnBulletPierceCreatureEventArgs {
        public Creature creatureHit;
    }

    protected Vector2 previousPosition;

    protected float penetrationSlowDownFactor = 1.5f;
    protected int penetrationIndex;
    protected bool projectileFadedOut;

    protected bool critHit;
    protected bool boucedOffCreatureShell;
    protected Transform bulletSource;

    protected HashSet<Creature> creaturesHit = new HashSet<Creature>();
    protected HashSet<CreatureSpawnerContinuous> spawnersHit = new HashSet<CreatureSpawnerContinuous>();

    public event EventHandler OnProjectileFadedOut;

    protected override void Awake() {
        previousPosition = transform.position;

        bulletSource = Player.Instance.transform;

        critHit = UnityEngine.Random.Range(0f, 1f) < PlayerShoot.Instance.GetHeldGun().GetCritChance() / 100f;
    }

    protected override void Update() {
        if (projectileFadedOut) return;

        HandleCircleCast();

        previousPosition = transform.position;

        lifetimeTimer -= Time.deltaTime;

        if (lifetimeTimer < 0) {
            if(explodeOnContact && !projectileExploded) {
                Explode();
            } else {
                FadeOutProjectile();
            }

        }
    }

    private void HandleCircleCast() {

        Vector2 currentPosition = transform.position;

        Vector2 direction = currentPosition - previousPosition;

        float distance = direction.magnitude;

        if (distance <= 0f) {
            return;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            previousPosition,
            circleCastRadius,
            direction.normalized,
            distance,
            raycastMask
        );

        if (hits.Length == 0) {
            return;
        }

        for (int i = 0; i < hits.Length; i++) {
            if (boucedOffCreatureShell) return;

            HandleHit(hits[i]);

            if (projectileFadedOut) {
                return;
            }
        }
    }

    private void HandleHit(RaycastHit2D hit) {
        Collider2D collision = hit.collider;
        Vector3 collisionPosition = hit.point;
        Vector2 moveDir = (Vector2)transform.position - previousPosition;
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

        bool bounceOff = collision.CompareTag("BounceOff");

        if (bounceOff) {

            Vector2 incomingVelocity = rb.velocity;

            // Reculer le projectile au point exact de collision
            Vector2 collisionPoint = hit.point;
            float distance = moveDir.magnitude;

            if (distance > 0f) {
                float backDistance = Vector2.Distance(previousPosition, collisionPoint);
                transform.position = previousPosition + moveDir.normalized * backDistance;
            }

            // Calcul du rebond
            Vector2 reflectedVelocity = Vector2.Reflect(
                incomingVelocity.normalized,
                hit.normal
            );

            rb.velocity = reflectedVelocity * incomingVelocity.magnitude;

            OnAnyParticleBouncedOff?.Invoke(this, EventArgs.Empty);

            previousPosition = transform.position;
            boucedOffCreatureShell = true;
            return;
        }

        Creature creatureHit = collision.GetComponent<Creature>();
        CreatureSpawnerContinuous spawnerHit = collision.GetComponent<CreatureSpawnerContinuous>();

        bool hitWeakSpot = false;

        RaycastHit2D[] weakHits = Physics2D.CircleCastAll(
            collisionPosition,
            .15f,
            Vector2.zero
        );

        for (int i = 0; i < weakHits.Length; i++) {

            if (weakHits[i].collider.CompareTag("CritHitZone")) {

                Creature weakCreature = weakHits[i].collider.GetComponentInParent<Creature>();

                if (weakCreature != null) {

                    creatureHit = weakCreature;
                    hitWeakSpot = true;
                    break;

                }
            }
        }

        if (creatureHit != null) {

            if (creaturesHit.Contains(creatureHit)) {
                return;
            }

            creaturesHit.Add(creatureHit);
            creatureHit.HandlePlayerSkillEffects(angle, collisionPosition.y);
            creatureHit.TakeDamage(projectileExplosionDamage, bulletSource, critHit, false, hitWeakSpot);
            creatureHit.InstantiateHitPS(angle,collisionPosition.y,critHit,projectileExplosionDamage,collisionPosition.x);

            if (critHit) {

                OnAnyPlayerBulletHitEnemyCrit?.Invoke(this,new OnBulletHitEventArgs {
                        bulletHitPosition = collisionPosition,
                        mobHit = creatureHit as Mob
                    }
                );

            }
            else {

                OnAnyBulletHitEnemy?.Invoke(this,new OnBulletHitEventArgs {
                        bulletHitPosition = collisionPosition,
                        mobHit = creatureHit as Mob
                    }
                );

            }

            Vector2 bulletDirNormalized = moveDir.normalized;

            creatureHit.TakeKnockback(knockBackForce, bulletDirNormalized);

            PierceEnemy(creatureHit);
        }

        if (spawnerHit != null) {

            if (spawnersHit.Contains(spawnerHit)) {
                return;
            }

            spawnersHit.Add(spawnerHit);
            spawnerHit.TakeDamage(projectileExplosionDamage, bulletSource, false);
            spawnerHit.InstantiateHitPS(angle, collisionPosition.y, false);
            PierceEnemy(null);

        }

        if (penetrationIndex >= penetrationMaxAmount) {

            transform.position = collisionPosition;
            previousPosition = collisionPosition;

            rb.velocity = Vector2.zero;

            FadeOutProjectile();

            return;

        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {

            Instantiate(explosionPrefab, collisionPosition, Quaternion.Euler(0, 0, angle));

            OnAnyBulletHitGround?.Invoke(
                this,
                new OnBulletHitEventArgs {
                    bulletHitPosition = collisionPosition
                }
            );

            FadeOutProjectile();

        }
    }

    protected void FadeOutProjectile() {
        transform.rotation = Quaternion.identity;

        rb.bodyType = RigidbodyType2D.Static;

        OnProjectileFadedOut?.Invoke(this, EventArgs.Empty);

        transform.localScale = Vector3.one * explosionRadiusMultiplier;

        projectileFadedOut = true;

        StartCoroutine(DestroyGameObjectAfterDelay(1f));

    }

    protected virtual void PierceEnemy(Creature creatureHit) {
        penetrationIndex++;
        rb.velocity /= penetrationSlowDownFactor;

        OnAnyBulletPierceCreature?.Invoke(this, new OnBulletPierceCreatureEventArgs {
            creatureHit = creatureHit
        });
    }
}