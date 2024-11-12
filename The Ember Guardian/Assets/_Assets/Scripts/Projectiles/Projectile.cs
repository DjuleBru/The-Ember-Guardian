using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask animalLayer;

    private ProjectileSO projectileSO;

    private AnimationCurve projectileTrajectoryAnimationCurve;
    private AnimationCurve projectileYDifferentialWithTargetAnimationCurve;
    private AnimationCurve projectileSpeedAnimationCurve;

    private float projectileMaxMoveSpeed;
    protected float projectileTrajectoryYCurve = .2f;
    private float trajectoryEndPointRandomOffsetValue;

    private Rigidbody2D rb;

    private Vector3 trajectoryRange;
    private Vector3 trajectoryStartPoint;
    private Vector3 trajectoryEndPoint;
    private Vector3 projectileStartPoint;
    private Vector3 projectileMoveDir;

    private Vector3 trajectoryEndPointRandomized;

    private float trajectoryMaxRelativeHeight;
    private float projectileMoveSpeed;
    private float nextYTrajectoryPosition;
    private float nextPositionXNormalized;
    private float nextPositionYCorrectionAbsolute;

    private bool projectileHasHit;
    public event EventHandler OnProjectileHit;
    public static event EventHandler OnAnyProjectileInstantiated;
    public static event EventHandler OnAnyProjectileHit;

    private Mob parentMob;
    private Mob mobHit;
    private bool enemyProjectile;

    public void ActivateAndInitialize(Vector3 targetPosition, ProjectileSO projectileSO, Mob parentMob) {
        this.parentMob = parentMob;
        this.projectileSO = projectileSO;

        if(parentMob is Creature) {
            enemyProjectile = true;
        }

        projectileTrajectoryAnimationCurve = projectileSO.projectileTrajectoryAnimationCurve;
        projectileYDifferentialWithTargetAnimationCurve = projectileSO.projectileYDifferentialWithTargetAnimationCurve;
        projectileSpeedAnimationCurve = projectileSO.projectileSpeedAnimationCurve;
        projectileMaxMoveSpeed = projectileSO.projectileMaxMoveSpeed;
        projectileTrajectoryYCurve = projectileSO.projectileTrajectoryYCurve;
        trajectoryEndPointRandomOffsetValue = projectileSO.trajectoryEndPointRandomOffsetValue;

        trajectoryStartPoint = transform.position;

        Vector3 trajectoryEndPointRandomOffset = new Vector3(UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), 0, 0);
        trajectoryEndPointRandomized = targetPosition + trajectoryEndPointRandomOffset;

        trajectoryEndPoint = trajectoryEndPointRandomized;
        trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        if(trajectoryRange.x == 0) {
            Debug.LogError("Projectile trajectory range is 0");
        }

        float distanceToTarget = Mathf.Abs(trajectoryEndPointRandomized.x - transform.position.x);
        trajectoryMaxRelativeHeight = distanceToTarget * projectileTrajectoryYCurve;

        OnAnyProjectileInstantiated?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        if (projectileHasHit) return;
        UpdateProjectilePosition();
    }

    private void UpdateProjectilePosition() {

        if (trajectoryRange.x < 0) {
            // Target is located behind shooted
            projectileMoveSpeed = -projectileMoveSpeed;
        }

        if(transform.position.y < 0 && !projectileHasHit) {

            // Fire hit ?
            if (Mathf.Abs(transform.position.x) < .5f) {
                Fire.Instance.TakeDamage(1, trajectoryStartPoint);
            }

            ProjectileHasHit(false);
            return;
        }

        float nextPositionX = transform.position.x + projectileMoveSpeed * Time.deltaTime;

        nextPositionXNormalized = Mathf.Abs((nextPositionX - trajectoryStartPoint.x) / (trajectoryRange.x));
        float nextPositionYNormalized = projectileTrajectoryAnimationCurve.Evaluate(nextPositionXNormalized);

        nextYTrajectoryPosition = nextPositionYNormalized * trajectoryMaxRelativeHeight;

        float nextPositionYCorrectionNormalized = projectileYDifferentialWithTargetAnimationCurve.Evaluate(nextPositionXNormalized);
        nextPositionYCorrectionAbsolute = nextPositionYCorrectionNormalized * trajectoryRange.y;

        float nextPositionY = trajectoryStartPoint.y + nextYTrajectoryPosition + nextPositionYCorrectionAbsolute;

        Vector3 nextPosition = new Vector3(nextPositionX, nextPositionY, 0);

        CalculateNewProjectileMoveSpeed(nextPositionXNormalized);
        projectileMoveDir = nextPosition - transform.position;

        transform.position = nextPosition;
    }

    protected void CalculateNewProjectileMoveSpeed(float newPositionXNormalized) {
        float projectileMoveSpeedNormalized = projectileSpeedAnimationCurve.Evaluate(newPositionXNormalized);
        projectileMoveSpeed = projectileMaxMoveSpeed * projectileMoveSpeedNormalized;
    }

    protected virtual void ProjectileHasHit(bool mobHit) {
        projectileHasHit = true;
        OnProjectileHit?.Invoke(this, EventArgs.Empty);
        OnAnyProjectileHit?.Invoke(this, EventArgs.Empty);

        StartCoroutine(DestroySelf(2f));
    }

    private IEnumerator DestroySelf(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public Vector2 GetTrajectoryEndPoint() {
        return trajectoryEndPointRandomized;
    }

    public Vector3 GetProjectileMoveDir() {
        return projectileMoveDir;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (projectileHasHit) return;
        if (collision.gameObject.GetComponent<CreatureDetectionCollider>() != null) return;

        mobHit = collision.GetComponentInParent<Mob>();

        if (mobHit != null && !enemyProjectile && mobHit != parentMob) {
            HandleMobCollision(mobHit);
            return;
        }

        if(collision.GetComponentInParent<Player>() != null && enemyProjectile) {
            ProjectileHasHit(false);
            Player.Instance.TakeDamage(1, trajectoryStartPoint);
        }

    }

    private void HandleMobCollision(Mob mob) {

        if (mobHit != null) {
            mobHit.OnMobDied += MobHit_OnMobDied;

            ProjectileHasHit(true);
            mobHit.TakeDamage(1, trajectoryStartPoint);
            transform.parent = mobHit.GetProjectileParent();
            return;
        }
    }

    public ProjectileSO GetProjectileSO() {
        return projectileSO;
    }

    private void MobHit_OnMobDied(object sender, EventArgs e) {

        mobHit.OnMobDied -= MobHit_OnMobDied;
        Destroy(gameObject);

    }
}
