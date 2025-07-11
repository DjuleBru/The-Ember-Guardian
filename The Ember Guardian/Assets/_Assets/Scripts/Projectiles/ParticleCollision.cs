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
        // Liste pour garder la trace des particules ayant déjà infligé des dégâts lors de cette collision
        List<int> damagedParticles = new List<int>();

        int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

        // Récupère la liste des particules actives
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int particleCount = ps.GetParticles(particles);

        // Parcours chaque collision détectée
        for (int i = 0; i < numCollisionEvents; i++) {
            Vector3 collisionPosition = collisionEvents[i].intersection;

            // Trouve la particule responsable de cette collision

            // Parcours chaque particule pour voir laquelle est proche de la collision
            for (int j = 0; j < particleCount; j++) {

                bool bounceOff = false;
                // Bounce
                if (other.CompareTag("BounceOff")) {
                    bounceOff = true;
                    collisionModule.bounce = 1;
                    collisionModule.lifetimeLoss = 0;
                }
                else {
                    collisionModule.bounce = 0;
                    collisionModule.lifetimeLoss = 1;
                }

                // Vérifie si la particule n'a pas déjà infligé des dégâts pour cette collision
                if (!damagedParticles.Contains(j) && Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {

                    // Calcule l'angle pour orienter l'explosion prefab
                    Vector3 moveDir = (collisionPosition - PlayerShoot.Instance.GetHeldGun().transform.position).normalized;
                    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

                    if(!bounceOff && groundDestroysBullet) {
                        particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact
                        ps.SetParticles(particles, particleCount); // Réinjecte les particules mises à jour dans le système
                    }

                    if (bounceOff) {
                        ps.SetParticles(particles, particleCount);
                        OnAnyParticleBouncedOff?.Invoke(this, EventArgs.Empty);
                    }

                    // Try fetch mobHit or spawner Hit
                    Mob mobHit = other.GetComponent<Mob>();
                    CreatureSpawnerContinuous spawnerHit = other.GetComponent<CreatureSpawnerContinuous>();
                    bool hitWeakSpot = false;

                    // Try fetch crit zone hit
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(collisionPosition, .15f, Vector2.zero);

                    foreach (var hit in hits) {
                        if (hit.collider != null) {
                            // Vérifie si le collider appartient à une zone critique
                            if (hit.collider.CompareTag("CritHitZone")) {
                                mobHit = hit.collider.GetComponentInParent<Mob>();
                                hitWeakSpot = true;
                            }
                        }
                    }

                    int bulletDamage = damage;
                    float knockBack = knockback;
                    bool critHit = false;
                    Transform bulletSource = source;

                    if (!initialized) {
                        // Bullet shot by player
                        damage = PlayerShoot.Instance.GetDamagePerBullet();
                        bulletSource = Player.Instance.transform;
                        knockBack = PlayerShoot.Instance.GetBulletKnockback();
                        critHit = UnityEngine.Random.Range(0f, 1f) < PlayerShoot.Instance.GetHeldGun().GetCritChance() / 100;
                    }

                    if (mobHit == null) {

                        Projectile projectileHit = other.GetComponent<Projectile>();
                        if (projectileHit != null) {
                            projectileHit.TryDestroyProjectile();
                        }

                        if (groundDestroysBullet) {
                            Instantiate(explosionPrefab, collisionEvents[0].intersection, Quaternion.Euler(0, 0, angle));

                            if(isPlayerWeaponPS) {
                                OnAnyPlayerBulletHitGround?.Invoke(this, new OnBulletHitEventArgs {
                                    bulletHitPosition = collisionPosition
                                });
                            } else {
                                OnAnyBulletHitGround?.Invoke(this, new OnBulletHitEventArgs {
                                    bulletHitPosition = collisionPosition
                                });
                            }
                        };


                    } else {
                        if(!initialized) {
                            // Bullet shot by player
                            mobHit.HandlePlayerSkillEffects(angle, collisionPosition.y);
                        }

                        mobHit.TakeDamage(damage, bulletSource, critHit, false, hitWeakSpot);
                        mobHit.InstantiateHitPS(angle, collisionPosition.y, critHit, damage, collisionPosition.x);

                        if (critHit) {
                            OnAnyPlayerBulletHitEnemyCrit?.Invoke(this, new OnBulletHitEventArgs {
                                bulletHitPosition = collisionPosition,
                                mobHit = mobHit
                            });
                        }
                        else {
                            if(isPlayerWeaponPS) {
                                OnAnyPlayerBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                                    bulletHitPosition = collisionPosition,
                                    mobHit = mobHit
                                });
                            } else {
                                OnAnyBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                                    bulletHitPosition = collisionPosition,
                                    mobHit = mobHit
                                });
                            }
                        }

                        Vector2 bulletDirNormalized = new Vector2(moveDir.x, moveDir.y).normalized;
                        mobHit.TakeKnockback(knockBack, bulletDirNormalized);
                    }

                    if(spawnerHit != null) {
                        other.GetComponent<CreatureSpawnerContinuous>().TakeDamage(bulletDamage, bulletSource, false);
                        other.GetComponent<CreatureSpawnerContinuous>().InstantiateHitPS(angle, collisionPosition.y, false);

                        if(isPlayerWeaponPS) {
                            OnAnyPlayerBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                                bulletHitPosition = collisionPosition
                            });
                        } else {
                            OnAnyBulletHitEnemy?.Invoke(this, new OnBulletHitEventArgs {
                                bulletHitPosition = collisionPosition
                            });
                        }
                    }

                    damagedParticles.Add(j); // Marque cette particule comme ayant déjà infligé des dégâts
                    break; // Sort de la boucle pour passer à la collision suivante
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

}
