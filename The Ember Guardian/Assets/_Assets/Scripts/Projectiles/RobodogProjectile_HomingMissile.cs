using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobodogProjectile_HomingMissile : GunProjectile_Bullet {

    [Header("Launch")]
    [SerializeField] private float launchUpDuration = 0.35f; // phase verticale avant que le homing prenne le relai

    [Header("Homing")]
    [SerializeField] private float homingSpeed = 18f;
    [SerializeField] private float maxTurnRateDegPerSec = 400f;
    [SerializeField] private float retargetInterval = 0.15f;
    [SerializeField] private LayerMask homingTargetMask;
    [SerializeField] private float acquireRange = 20f;

    [Header("Noise")]
    [SerializeField] private float noiseAmplitudeDeg = 40f;
    [SerializeField] private float noiseFrequency = 3f;
    [SerializeField] private float noiseFalloffDistance = 2.5f; // sous cette distance, le bruit est coupé pour garantir le hit

    private Vector2 previousMoveDir;

    private Creature currentTarget;
    private float launchTimer;
    private float retargetTimer;
    private float noiseSeed;
    private float currentAngle;

    protected override void Awake() {
        base.Awake();

        noiseSeed = Random.Range(0f, 1000f);
        currentAngle = 90f; // tir vertical vers le haut

        previousMoveDir = new Vector2(0, 1);

        AcquireClosestTarget();
    }

    protected override void Update() {
        if (projectileExploded) return;

        launchTimer += Time.deltaTime;
        retargetTimer += Time.deltaTime;

        if (retargetTimer >= retargetInterval) {
            retargetTimer = 0f;
            ValidateOrReacquireTarget();
        }

        if (launchTimer >= launchUpDuration) {
            HandleHoming();
        }

        base.Update();
    }

    private void ValidateOrReacquireTarget() {
        if (currentTarget == null || !IsTargetValid(currentTarget)) {
            AcquireClosestTarget();
        }
    }

    private bool IsTargetValid(Creature creature) {
        if (creature == null) return false;
        return !creature.GetDead();
    }

    private void AcquireClosestTarget() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, acquireRange, homingTargetMask);

        float closestDistance = Mathf.Infinity;
        Creature bestTarget = null;

        for (int i = 0; i < hits.Length; i++) {
            Creature creature = hits[i].GetComponent<Creature>();
            if (creature == null || !IsTargetValid(creature)) continue;

            float distance = Vector2.Distance(transform.position, creature.GetAutoAimPosition().position);
            if (distance < closestDistance) {
                closestDistance = distance;
                bestTarget = creature;
            }
        }

        currentTarget = bestTarget;
    }

    private void HandleHoming() {
        if (currentTarget == null) {
            rb.velocity = previousMoveDir * homingSpeed;
            return;
        }

        Vector2 toTarget = (Vector2)currentTarget.GetAutoAimPosition().position - (Vector2)transform.position;
        float distanceToTarget = toTarget.magnitude;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        // bruit angulaire qui s'estompe en approchant de la cible -> garantit le hit
        float noiseFalloff = Mathf.Clamp01(distanceToTarget / noiseFalloffDistance);
        float noiseValue = (Mathf.PerlinNoise(noiseSeed, Time.time * noiseFrequency) - 0.5f) * 2f;
        float angularNoise = noiseValue * noiseAmplitudeDeg * noiseFalloff;

        float desiredAngle = targetAngle + angularNoise;

        currentAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, maxTurnRateDegPerSec * Time.deltaTime);

        Vector2 newDirection = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
        rb.velocity = newDirection * homingSpeed;

        previousMoveDir = newDirection; ;
    }

}
