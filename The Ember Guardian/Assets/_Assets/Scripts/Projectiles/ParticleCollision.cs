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
    public GameObject critHitPrefab;

    private Vector3 previousPosition;
    private Vector3 particleMoveDir;

    [SerializeField] private float collisionDistanceThreshold = .25f;
    [SerializeField] private float hitKnockbackForce = 15f;

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

        if(Mathf.Abs(Vector3.Distance(previousPosition, transform.position)) > .5f) {
            particleMoveDir = transform.position - previousPosition;
            previousPosition = transform.position;
        };

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
                    float angle = Mathf.Atan2(particleMoveDir.y, particleMoveDir.x) * Mathf.Rad2Deg;

                    particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact
                    ps.SetParticles(particles, particleCount); // Réinjecte les particules mises à jour dans le système

                    if (other.GetComponent<Mob>() != null) {
                        other.GetComponent<Mob>().TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform, false);
                        OnAnyBulletHitEnemy?.Invoke(this, EventArgs.Empty);
                    } else {

                        Instantiate(explosionPrefab, collisionEvents[0].intersection, Quaternion.Euler(0, 0, angle));
                        OnAnyBulletHitGround?.Invoke(this, EventArgs.Empty);

                    }

                    // Effectue un CircleCast autour du point d'impact
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(collisionPosition, .15f, Vector2.zero);

                    foreach (var hit in hits) {
                        if (hit.collider != null) {

                            // Vérifie si le collider appartient à une zone critique
                            if (hit.collider.CompareTag("CritHitZone")) {
                                hit.collider.GetComponentInParent<Mob>().TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform, true);
                                Instantiate(critHitPrefab, collisionEvents[0].intersection, Quaternion.Euler(0, 0, angle));
                                OnAnyBulletHitEnemyCrit?.Invoke(this, EventArgs.Empty);
                            } 
                        }
                    }

                    damagedParticles.Add(j); // Marque cette particule comme ayant déjà infligé des dégâts
                    break; // Sort de la boucle pour passer à la collision suivante
                }

                if (Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {

                    particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact

                    // Réinjecte les particules mises à jour dans le système
                    ps.SetParticles(particles, particleCount);
                }

            }


        }

    }

}
