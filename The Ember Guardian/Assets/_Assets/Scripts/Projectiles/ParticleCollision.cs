using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{

    private ParticleSystem ps;
    private ParticleSystem.CollisionModule collisionModule;
    public List<ParticleCollisionEvent> collisionEvents;
    public CinemachineVirtualCamera cam;
    public GameObject explosionPrefab;

    private Vector3 previousPosition;
    private Vector3 particleMoveDir;

    private float collisionDistanceThreshold = .75f;
    [SerializeField] private bool groundDestroysBullet = true;
    [SerializeField] private bool isPlayerWeaponPS;

    public static event EventHandler<OnBulletHitEventArgs> OnAnyBulletHitGround;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyBulletHitEnemy;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyPlayerBulletHitGround;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyPlayerBulletHitEnemy;
    public static event EventHandler<OnBulletHitEventArgs> OnAnyPlayerBulletHitEnemyCrit;
    public static event EventHandler OnAnyParticleBouncedOff;
    public static event EventHandler OnAnyParticleHitBarricade;

    public class OnBulletHitEventArgs {
        public Vector3 bulletHitPosition;
        public Mob mobHit;
    }

    private bool initialized;
    private int damage;
    private float knockback;
    private Transform source;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionModule = ps.collision;
        collisionEvents = new List<ParticleCollisionEvent>();
        previousPosition = transform.position;
    }

    void LateUpdate() {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int particleCount = ps.GetParticles(particles);

        for (int i = 0; i < particleCount; i++) {
            Vector3 velocity = particles[i].velocity;
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            particles[i].rotation = angle;
        }

        ps.SetParticles(particles, particleCount);
    }

    private void OnParticleCollision(GameObject other) {
        int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

        // Bounce config global
        bool bounceOff = other.CompareTag("BounceOff");
        collisionModule.bounce = bounceOff ? 1 : 0;
        collisionModule.lifetimeLoss = bounceOff ? 0 : 1;

        for (int i = 0; i < numCollisionEvents; i++) {
            Vector3 collisionPosition = collisionEvents[i].intersection;
            Vector3 moveDir = (collisionPosition - PlayerShoot.Instance.GetHeldGun().transform.position).normalized;
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

            if (bounceOff) {
                OnAnyParticleBouncedOff?.Invoke(this, EventArgs.Empty);
            }

            // --- Crit zone check ---
            Mob mobHit = other.GetComponent<Mob>();
            CreatureSpawnerContinuous spawnerHit = other.GetComponent<CreatureSpawnerContinuous>();
            bool hitWeakSpot = false;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(collisionPosition, .15f, Vector2.zero);
            foreach (var hit in hits) {
                if (hit.collider != null && hit.collider.CompareTag("CritHitZone")) {
                    mobHit = hit.collider.GetComponentInParent<Mob>();
                    hitWeakSpot = true;
                }
            }

            // --- Damage init ---
            int bulletDamage = damage;
            float knockBack = knockback;
            bool critHit = false;
            Transform bulletSource = source;

            if (!initialized) {
                damage = PlayerShoot.Instance.GetDamagePerBullet();
                bulletSource = Player.Instance.transform;
                knockBack = PlayerShoot.Instance.GetBulletKnockback();
                critHit = UnityEngine.Random.Range(0f, 1f) < PlayerShoot.Instance.GetHeldGun().GetCritChance() / 100f;
            }

            // --- Hit handling ---
            if (mobHit == null) {
                Projectile projectileHit = other.GetComponent<Projectile>();
                if (projectileHit != null) {
                    projectileHit.TryDestroyProjectile();
                }

                Barricade barricade = other.GetComponentInParent<Barricade>();
                if (barricade != null) {
                    OnAnyParticleHitBarricade?.Invoke(this, EventArgs.Empty);
                }

                if (groundDestroysBullet) {
                    Instantiate(explosionPrefab, collisionPosition, Quaternion.Euler(0, 0, angle));

                    if (isPlayerWeaponPS) {
                        OnAnyPlayerBulletHitGround?.Invoke(this, new OnBulletHitEventArgs {
                            bulletHitPosition = collisionPosition
                        });
                    }
                    else {
                        OnAnyBulletHitGround?.Invoke(this, new OnBulletHitEventArgs {
                            bulletHitPosition = collisionPosition
                        });
                    }
                }
            }
            else {
                if (!initialized) {
                    mobHit.HandlePlayerSkillEffects(angle, collisionPosition.y);
                }

                mobHit.TakeDamage(damage, bulletSource, critHit, false, hitWeakSpot);
                mobHit.InstantiateHitPS(angle, collisionPosition.y, critHit, damage, collisionPosition.x);

                PlayerShoot.Instance.CheckWeaponStatusFX(mobHit);

                if (critHit) {
                    OnAnyPlayerBulletHitEnemyCrit?.Invoke(this, new OnBulletHitEventArgs {
                        bulletHitPosition = collisionPosition,
                        mobHit = mobHit
                    });
                }
                else {
                    if (isPlayerWeaponPS) {
                        OnAnyPlayerBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                            bulletHitPosition = collisionPosition,
                            mobHit = mobHit
                        });
                    }
                    else {
                        OnAnyBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                            bulletHitPosition = collisionPosition,
                            mobHit = mobHit
                        });
                    }
                }

                Vector2 bulletDirNormalized = new Vector2(moveDir.x, moveDir.y).normalized;
                mobHit.TakeKnockback(knockBack, bulletDirNormalized);
            }

            if (spawnerHit != null) {
                spawnerHit.TakeDamage(bulletDamage, bulletSource, false);
                spawnerHit.InstantiateHitPS(angle, collisionPosition.y, false);

                if (isPlayerWeaponPS) {
                    OnAnyPlayerBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                        bulletHitPosition = collisionPosition
                    });
                }
                else {
                    OnAnyBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                        bulletHitPosition = collisionPosition
                    });
                }
            }
        }
    }

    public void InitializeBulletPS(Transform source, int damage, float knockBack, float collisionDistanceTreshold) {
        this.source = source;
        this.damage = damage;
        this.knockback = knockBack;
        initialized = true;
        this.collisionDistanceThreshold = collisionDistanceTreshold;
    } 

    public bool GetIsPlayerBullet() {
        //Debug.Log("source " + source);
        return source == null;
    }

}
