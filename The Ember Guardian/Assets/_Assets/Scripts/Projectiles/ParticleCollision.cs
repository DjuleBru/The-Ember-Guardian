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

    [SerializeField] private float collisionDistanceThreshold = .25f;
    [SerializeField] private float hitKnockbackForce = 15f;

    public static event EventHandler OnAnyBulletHitGround;
    public static event EventHandler OnAnyBulletHitEnemy;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();

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

        GameObject explosion = Instantiate(explosionPrefab, collisionEvents[0].intersection, Quaternion.identity);
        ParticleSystem p = explosion.GetComponent<ParticleSystem>();
        //var pmain = p.main;

        // Récupère la liste des particules actives
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int particleCount = ps.GetParticles(particles);

        // Parcours chaque collision détectée
        for (int i = 0; i < numCollisionEvents; i++) {
            Vector3 collisionPosition = collisionEvents[i].intersection;

            // Parcours chaque particule pour voir laquelle est proche de la collision
            for (int j = 0; j < particleCount; j++) {

                // Vérifie si la particule n'a pas déjà infligé des dégâts pour cette collision
                if (!damagedParticles.Contains(j) && Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {
                    particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact
                    ps.SetParticles(particles, particleCount); // Réinjecte les particules mises à jour dans le système

                    // Applique des dégâts et ajoute l'index de la particule à la liste
                    if (other.GetComponent<Mob>() != null) {
                        other.GetComponent<Mob>().TakeDamage(PlayerShoot.Instance.GetDamagePerBullet(), Player.Instance.transform);
                        OnAnyBulletHitEnemy?.Invoke(this, EventArgs.Empty);
                    }
                    else {
                        OnAnyBulletHitGround?.Invoke(this, EventArgs.Empty);
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
