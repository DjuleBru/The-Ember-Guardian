using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileForces : Projectile {


    [Header("Physique")]
    private float gravityScale = 1f;
    private float homingStrength = 2f;

    private Transform target;
    private bool hasHit = false;
    private bool homing;
    private bool hasPassedApex;

    public void ActivateAndInitializeWithForces(Transform targetTransform, ProjectileSO projectileSO, Mob parentMob, int damage, float targetRandomizer, bool homing) {
        if (targetTransform == null) {
            ResetInObjectPool();
            return;
        }

        this.parentMob = parentMob;
        this.damage = damage;
        this.homing = homing;
        this.projectileSO = projectileSO;
        target = targetTransform;
        hasPassedApex = false;

        if (parentMob is Creature) {
            enemyProjectile = true;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        this.gravityScale = projectileSO.gravityScale;
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;
        transform.right = (target.position - transform.position).normalized;

        float randomizedX = UnityEngine.Random.Range(-targetRandomizer, targetRandomizer);
        
        Vector2 targetPosition = new Vector2(target.position.x - randomizedX, target.position.y);
        if(homing) {
            targetPosition = target.position;
        }

        Vector2 launchVelocity = CalculateLaunchVelocityWithApex(transform.position, targetPosition);
        rb.AddForce(launchVelocity, ForceMode2D.Impulse);

        hasHit = false;

        InvokeOnAnyProjectileInstantiated();
    }

    protected override void Update() {
        if (transform.position.y < 0 && !projectileHasHit) {

            // Fire hit ?
            if (enemyProjectile && Mathf.Abs(transform.position.x) < .5f) {
                Fire.Instance.TakeDamage(1, parentMob.transform);
            }

            // Ground hit
            ProjectileHasHit(false);
            return;
        }
    }

    private void FixedUpdate() {
        if (hasHit || !homing || target == null) return;

        // Vérifie si on a passé l’apex (le point le plus haut)
        if (!hasPassedApex && rb.velocity.y < .5f) {
            Debug.Log(rb.velocity.y + " hasPassedApex " + hasPassedApex);
            hasPassedApex = true;
        }

        if (!hasPassedApex) return; // Laisse la parabole se faire tranquillement

        Vector2 desiredDir = ((Vector2)(target.position - transform.position)).normalized;
        Vector2 currentVelocity = rb.velocity;
        Vector2 newVelocity = Vector2.Lerp(currentVelocity, desiredDir * currentVelocity.magnitude, Time.fixedDeltaTime * homingStrength);
        rb.velocity = newVelocity;
    }

    private Vector2 CalculateLaunchVelocityWithApex(Vector2 start, Vector2 end) {

        float distanceToTargetForMinApex = projectileSO.distanceToTargetForMinApex;
        float distanceToTargetForMaxApex = projectileSO.distanceToTargetForMaxApex;
        float minApexY = projectileSO.minApexY;
        float maxApexY = projectileSO.maxApexY;
        float apexRandomizer = projectileSO.apexRandomizer;

        float g = Mathf.Abs(Physics2D.gravity.y * gravityScale);

        float distance = Vector2.Distance(start, end);

        // Interpolation entre min et max apex
        float t = Mathf.InverseLerp(distanceToTargetForMinApex, distanceToTargetForMaxApex, distance);
        float apexY = Mathf.Lerp(minApexY, maxApexY, t);

        // Ajout d'une variation aléatoire contrôlée
        apexY += UnityEngine.Random.Range(-apexRandomizer, apexRandomizer);

        // Assure que l'apex est au-dessus du départ et de la cible
        float highestY = Mathf.Max(start.y, end.y);
        apexY = Mathf.Max(apexY, highestY + 0.1f); // évite les apex en dessous de start/end

        // Étape 1 : monter à apex
        float dyUp = apexY - start.y;
        float vyUp = Mathf.Sqrt(2 * g * dyUp);

        float tUp = vyUp / g;

        // Étape 2 : descendre de apex à end
        float dyDown = apexY - end.y;
        float tDown = Mathf.Sqrt(2 * dyDown / g);

        float totalTime = tUp + tDown;

        float vx = (end.x - start.x) / totalTime;

        Vector2 velocity = new Vector2(vx, vyUp);

        return velocity;
    }

    protected override void ProjectileHasHit(bool mobHit) {
        base.ProjectileHasHit(mobHit);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
    }

    public override Vector3 GetProjectileMoveDir() {
        return rb.velocity;
    }
    public override Vector2 GetTrajectoryEndPoint() {
        return target.transform.position;
    }
}
