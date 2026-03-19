using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_BulletAR : GunProjectile_Bullet
{
    [Header("Homing")]
    [SerializeField] private float homingRange = 20f;
    [SerializeField] private float homingRotationSpeed = 360f;
    [SerializeField] private LayerMask homingTargetMask;
    private bool useHoming;

    private Creature currentTarget;

    protected override void Awake() {
        base.Awake();

        useHoming = AssaultRifleSecondaryAbility.Instance.GetHomingBulletsActive();

        if (useHoming) {
            //AcquireTarget();
            AcquireTargetFromPlayerAim();
        }
    }

    protected override void Update() {
        if (projectileFadedOut) return;

        if (useHoming) {
            HandleHoming();
        }

        base.Update();
    }

    private void AcquireTargetFromPlayerAim() {

        Transform target = PlayerAim.Instance.GetCurrentAutoAimTarget();

        if (target != null) {
            currentTarget = target.GetComponent<Creature>();
            return;
        }

        // fallback si pas d’auto-aim
        AcquireTargetInCone();
    }

    private void AcquireTargetInCone() {

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            homingRange,
            homingTargetMask
        );

        Vector2 aimDir = PlayerAim.Instance.GetEffectiveAimDir().normalized;

        float bestScore = float.MaxValue;
        Creature bestTarget = null;

        for (int i = 0; i < hits.Length; i++) {


            Creature creature = hits[i].GetComponent<Creature>();


            if (creature == null) {
                continue;
            }

            Vector2 dirToEnemy = (creature.transform.position - transform.position).normalized;

            float angle = Vector2.Angle(aimDir, dirToEnemy);

            if (angle > 40f) { // cone
                continue;
            }

            float distance = Vector2.Distance(transform.position, creature.transform.position);

            float score = angle + distance * 0.1f;

            if (score < bestScore) {
                bestScore = score;
                bestTarget = creature;
            }
        }

        currentTarget = bestTarget;
    }

    private void AcquireTarget() {

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            homingRange,
            homingTargetMask
        );

        float closestDistance = Mathf.Infinity;
        Creature bestTarget = null;

        for (int i = 0; i < hits.Length; i++) {

            Creature creature = hits[i].GetComponent<Creature>();

            if (creature == null) {
                continue;
            }

            float distance = Vector2.Distance(transform.position, creature.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                bestTarget = creature;
            }
        }

        currentTarget = bestTarget;
    }

    private void HandleHoming() {

        if (currentTarget == null) {
            return;
        }

        Vector2 directionToTarget = (currentTarget.transform.position - transform.position).normalized;

        Vector2 currentVelocity = rb.velocity;

        if (currentVelocity.magnitude <= 0.01f) {
            return;
        }

        Vector2 currentDirection = currentVelocity.normalized;

        float maxRotation = homingRotationSpeed * Mathf.Deg2Rad * Time.deltaTime;

        Vector2 newDirection = Vector2.Lerp(
            currentDirection,
            directionToTarget,
            maxRotation
        ).normalized;

        float speed = currentVelocity.magnitude;

        rb.velocity = newDirection * speed;
    }

}
