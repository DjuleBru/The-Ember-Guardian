using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask animalLayer;

    [SerializeField] private AnimationCurve projectileTrajectoryAnimationCurve;
    [SerializeField] private AnimationCurve projectileYDifferentialWithTargetAnimationCurve;
    [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;

    [SerializeField] private float projectileMaxMoveSpeed;
    [SerializeField] protected float projectileTrajectoryYCurve = .2f;
    [SerializeField] private float trajectoryEndPointRandomOffsetValue;

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

    private void Awake() {
        //rb = GetComponent<Rigidbody2D>();
    }

    public void ActivateAndInitialize(Vector3 targetPosition) {
       
        trajectoryStartPoint = transform.position;

        Vector3 trajectoryEndPointRandomOffset = new Vector3(UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), 0, 0);
        trajectoryEndPointRandomized = targetPosition + trajectoryEndPointRandomOffset;

        trajectoryEndPoint = trajectoryEndPointRandomized;
        trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        float distanceToTarget = Mathf.Abs(trajectoryEndPointRandomized.x - transform.position.x);
        trajectoryMaxRelativeHeight = distanceToTarget * projectileTrajectoryYCurve;
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

        if(transform.position.y < 0 ) {
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

        if (mobHit) return;
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

        Animal animalHit = collision.GetComponentInParent<Animal>();

        if(animalHit != null) {
            animalHit.OnMobDied += AnimalHit_OnMobDied;

            ProjectileHasHit(true);
            animalHit.TakeDamage(1, trajectoryStartPoint);
            transform.parent = animalHit.GetProjectileParent();
            return;
        }
    }

    private void AnimalHit_OnMobDied(object sender, EventArgs e) {
        Debug.Log("AnimalHit_OnMobDied");
        Destroy(gameObject);
    }

}
