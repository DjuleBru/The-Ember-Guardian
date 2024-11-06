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
    private float collisionDistanceThreshold = .25f;

    [SerializeField] private float hitKnockbackForce = 15f;
    [SerializeField] private int bulletDamage = 1;

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

                if (Vector3.Distance(particles[j].position, collisionPosition) < collisionDistanceThreshold) {

                    particles[j].remainingLifetime = 0; // Détruit seulement la particule proche de l'impact

                    // Réinjecte les particules mises à jour dans le système
                    ps.SetParticles(particles, particleCount);
                }

            }
        }


        // Knockback : Calculer la direction du tir
        if (other.GetComponent<Rigidbody2D>() != null) {
            Vector3 direction = (collisionEvents[0].intersection - transform.position).normalized;
            direction.y = 0;
            direction.z = 0;

            other.GetComponent<Rigidbody2D>().AddForceAtPosition(direction * hitKnockbackForce, collisionEvents[0].intersection, ForceMode2D.Impulse);
        }
        
        // Damage : 
        if(other.GetComponent<Mob>() != null) {
            other.GetComponent<Mob>().TakeDamage(bulletDamage, collisionEvents[0].intersection);
            OnAnyBulletHitEnemy?.Invoke(this, EventArgs.Empty);
        } else {
            OnAnyBulletHitGround?.Invoke(this, EventArgs.Empty);
        }
    }

}
