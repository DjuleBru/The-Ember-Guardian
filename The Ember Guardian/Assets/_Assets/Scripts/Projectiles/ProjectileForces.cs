using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileForces : Projectile {

    public enum TrajectoryMode {
        CurvedApex,
        StraightLine
    }

    [Header("Physique")]
    private float gravityScale = 1f;
    private float homingStrength = 4f;

    private bool hasHit = false;
    private bool homing;
    private bool hasPassedApex;

    public void ActivateAndInitializeWithForces(IDamageable targetIDamageable, ProjectileSO projectileSO, Transform damageSource, int damage, float targetRandomizer, bool homing) {
        Transform targetTransform = targetIDamageable.GetProjectileTarget();

        if (targetTransform == null) {
            ResetInObjectPool();
            return;
        }

        this.damageSource = damageSource;
        parentMob = damageSource.GetComponent<Mob>();
        this.damage = damage;
        this.homing = homing;
        this.projectileSO = projectileSO;
        transform.rotation = Quaternion.identity;
        projectileTarget = targetTransform;
        hasPassedApex = false;

        if (!projectileSO.canBeDestoyedByBullets) {
            solidCollider.enabled = false;
        }
        else {
            solidCollider.enabled = true;
        }

        if (parentMob is Creature) {
            enemyProjectile = true;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        this.gravityScale = projectileSO.gravityScale;
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;
        //transform.right = (projectileTarget.position - transform.position).normalized;

        float randomizedX = UnityEngine.Random.Range(-targetRandomizer, targetRandomizer);
        
        Vector2 targetPosition = new Vector2(projectileTarget.position.x - randomizedX, projectileTarget.position.y);
        if(homing) {
            float estFlightTime = Vector2.Distance(transform.position, projectileTarget.position)
                      / projectileSO.straightLineSpeed;


            Vector2 targetVel = (targetIDamageable as MonoBehaviour).GetComponent<Rigidbody2D>().velocity;

            targetPosition = (Vector2)projectileTarget.position + targetVel * estFlightTime;
        }

        Vector2 launchVelocity = Vector2.zero;

        switch (projectileSO.trajectoryMode) {
            case TrajectoryMode.StraightLine:
                launchVelocity = CalculateStraightLineVelocity(transform.position, targetPosition, projectileSO.straightLineSpeed);
                break;
            case TrajectoryMode.CurvedApex:
            default:
                launchVelocity = CalculateLaunchVelocityWithApex(transform.position, targetPosition);
                break;
        }

        rb.AddForce(launchVelocity, ForceMode2D.Impulse);
        hasHit = false;

        InvokeOnProjectileInitialized();
        InvokeOnAnyProjectileInstantiated();
    }

    protected override void Update() {
        if (transform.position.y < 0 && !projectileHasHit) {
            // Ground hit
            ProjectileHasHit(false);
            return;
        }
    }

    private void FixedUpdate() {
        if (hasHit || !homing || projectileTarget == null) return;
        Vector2 toTarget = ((Vector2)(projectileTarget.position - transform.position)).normalized;
        Vector2 velocity = rb.velocity;
        float speed = velocity.magnitude;

        // Clamp la rotation de la velocity
        float maxTurnRate = 90f * Mathf.Deg2Rad; // max 180°/s
        float angleBetween = Vector2.SignedAngle(velocity, toTarget);
        float maxAngleDelta = maxTurnRate * Time.fixedDeltaTime;

        float clampedAngle = Mathf.Clamp(angleBetween, -maxAngleDelta, maxAngleDelta);
        Vector2 newDir = Quaternion.Euler(0, 0, clampedAngle) * velocity.normalized;

        rb.velocity = newDir * speed;
    }

    private Vector2 CalculateLaunchVelocityWithApex(Vector2 start, Vector2 end) {

        float distanceToTargetForMinApex = projectileSO.distanceToTargetForMinApex;
        float distanceToTargetForMaxApex = projectileSO.distanceToTargetForMaxApex;
        float minApexY = projectileSO.minApexY;
        float maxApexY = projectileSO.maxApexY;
        float apexRandomizer = projectileSO.apexRandomizer;

        Creature targetCreature = projectileTarget.GetComponentInParent<Creature>();
        if (targetCreature != null) {
            if(targetCreature.GetCreatureSO().flying) {
                minApexY = targetCreature.transform.position.y + 2;
                maxApexY = targetCreature.transform.position.y + 3;
            }
        }

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

    private Vector2 CalculateStraightLineVelocity(Vector2 start, Vector2 end, float speed) {
        Vector2 dir = (end - start).normalized;
        return dir * speed;
    }

    protected override void ProjectileHasHit(bool mobHit) {
        base.ProjectileHasHit(mobHit);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

        // Fire hit ?
        if (enemyProjectile && Mathf.Abs(transform.position.x) < .5f) {
            Fire.Instance.TakeDamage(1, parentMob.transform);
        }

        if (alignToGroundOnImpact) {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
    }

    public override Vector3 GetProjectileMoveDir() {
        return rb.velocity;
    }
    public override Vector2 GetTrajectoryEndPoint() {
        return projectileTarget.transform.position;
    }

}
