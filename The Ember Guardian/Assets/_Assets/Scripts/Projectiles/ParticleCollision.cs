using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{

    private ParticleSystem ps;
    public List<ParticleCollisionEvent> collisionEvents;
    public CinemachineVirtualCamera cam;
    public GameObject explosionPrefab;

    private Vector3 previousPosition;
    private Vector3 particleMoveDir;

    [SerializeField] private float collisionDistanceThreshold = .25f;
    [SerializeField] private bool groundDestroysBullet = true;

    public static event EventHandler OnAnyBulletHitGround;
    public static event EventHandler OnAnyBulletHitEnemy;
    public static event EventHandler OnAnyBulletHitEnemyCrit;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
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

                // Vérifie si la particule n'a pas déjà infligé des dégâts pour cette collision
                if (!damagedParticles.Contains(j) && Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {

                    // Calcule l'angle pour orienter l'explosion prefab
                    Vector3 moveDir = (collisionPosition - PlayerShoot.Instance.GetHeldGun().transform.position).normalized;
                    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

                    if(groundDestroysBullet) {
                        particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact
                        ps.SetParticles(particles, particleCount); // Réinjecte les particules mises à jour dans le système
                    }

                    // Try fetch mobHit or spawner Hit
                    Mob mobHit = other.GetComponent<Mob>();
                    CreatureSpawnerContinuous spawnerHit = other.GetComponent<CreatureSpawnerContinuous>();
                    bool critHit = false;

                    // Try fetch crit zone hit
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(collisionPosition, .15f, Vector2.zero);

                    foreach (var hit in hits) {
                        if (hit.collider != null) {
                            // Vérifie si le collider appartient à une zone critique
                            if (hit.collider.CompareTag("CritHitZone")) {
                                mobHit = hit.collider.GetComponentInParent<Mob>();
                                critHit = true;
                            }
                        }
                    }

                    if (mobHit == null) {

                        if (groundDestroysBullet) {
                            Instantiate(explosionPrefab, collisionEvents[0].intersection, Quaternion.Euler(0, 0, angle));
                            OnAnyBulletHitGround?.Invoke(this, EventArgs.Empty);
                        };


                    } else {
                        float randomNumber = UnityEngine.Random.Range(0f, 1f);
                        if (critHit && randomNumber < PlayerShoot.Instance.GetHeldGun().GetCritChance()/100) {
                           
                            mobHit.TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform, true);
                            mobHit.InstantiateHitPS(angle, collisionPosition.y, true, PlayerShoot.Instance.GetDamagePerBullet(), collisionPosition.x);

                            OnAnyBulletHitEnemyCrit?.Invoke(this, EventArgs.Empty);
                            
                        }
                        else {
                            mobHit.TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform, false);
                            mobHit.InstantiateHitPS(angle, collisionPosition.y, false, PlayerShoot.Instance.GetDamagePerBullet(), collisionPosition.x);

                            OnAnyBulletHitEnemy?.Invoke(this, EventArgs.Empty);
                        }

                        Vector2 bulletDirNormalized = new Vector2(moveDir.x, moveDir.y).normalized;
                        mobHit.TakeKnockback(PlayerShoot.Instance.GetBulletKnockback(), bulletDirNormalized);
                    }

                    if(spawnerHit != null) {
                        other.GetComponent<CreatureSpawnerContinuous>().TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform, false);
                        other.GetComponent<CreatureSpawnerContinuous>().InstantiateHitPS(angle, collisionPosition.y, false);


                        OnAnyBulletHitEnemy?.Invoke(this, EventArgs.Empty);
                    }

                    damagedParticles.Add(j); // Marque cette particule comme ayant déjà infligé des dégâts
                    break; // Sort de la boucle pour passer à la collision suivante
                }

                if (Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {

                    if(groundDestroysBullet) {
                        particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact

                        // Réinjecte les particules mises à jour dans le système
                        ps.SetParticles(particles, particleCount);
                    }

                }

            }


        }

    }

}
