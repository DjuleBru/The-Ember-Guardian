using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask animalLayer;
    [SerializeField] protected Collider2D solidCollider;

    [SerializeField] protected bool alignToGroundOnImpact;
    [SerializeField] protected Vector3 rotationOnImpact;
    protected ProjectileSO projectileSO;

    private AnimationCurve projectileTrajectoryAnimationCurve;
    private AnimationCurve projectileYDifferentialWithTargetAnimationCurve;
    private AnimationCurve projectileSpeedAnimationCurve;

    private float projectileMaxMoveSpeed;
    private float projectileTrajectoryYCurve = .2f;

    protected Rigidbody2D rb;

    protected Vector3 trajectoryRange;
    protected Vector3 trajectoryStartPoint;
    protected Vector3 trajectoryEndPoint;
    protected Transform damageSource;
    protected Transform projectileTarget;
    protected Vector3 projectileStartPoint;
    protected Vector3 projectileMoveDir;

    protected Vector3 nextPosition;
    protected Vector3 trajectoryEndPointRandomized;
    protected Vector3 trajectoryEndPointRandomizer;

    protected float trajectoryMaxRelativeHeight;
    protected float projectileMoveSpeed;
    protected float nextYTrajectoryPosition;
    protected float nextPositionXNormalized;
    protected float nextPositionYCorrectionAbsolute;

    protected bool projectileHasHit;
    public event EventHandler OnProjectileHit;
    public event EventHandler OnProjectileReset;
    public event EventHandler OnProjectileInitialized;
    public static event EventHandler OnAnyProjectileInstantiated;
    public static event EventHandler OnAnyProjectileHit;

    protected Mob parentMob;
    protected Mob mobHit;
    protected bool enemyProjectile;
    protected bool homingProjectile;
    protected int damage;

    public void ActivateAndInitialize(Transform targetTransform, ProjectileSO projectileSO, Transform damageSource, int damage, Vector3 endPointRandomOffsetValue,  bool homingProjectile) {
        if(targetTransform == null) {
            ResetInObjectPool();
            return;
        }

        this.damageSource = damageSource;
        parentMob = damageSource.GetComponent<Mob>();
        this.projectileSO = projectileSO;
        this.damage = damage;
        this.homingProjectile = homingProjectile;
        projectileTarget = targetTransform;

        if(!projectileSO.canBeDestoyedByBullets) {
            solidCollider.enabled = false;
        } else {
            solidCollider.enabled = true;
        }

        if (parentMob is Creature) {
            enemyProjectile = true;
        }

        projectileTrajectoryAnimationCurve = projectileSO.projectileTrajectoryAnimationCurve;
        projectileYDifferentialWithTargetAnimationCurve = projectileSO.projectileYDifferentialWithTargetAnimationCurve;
        projectileSpeedAnimationCurve = projectileSO.projectileSpeedAnimationCurve;
        projectileMaxMoveSpeed = projectileSO.projectileMaxMoveSpeed;
        projectileTrajectoryYCurve = projectileSO.projectileTrajectoryYCurve;

        trajectoryStartPoint = transform.position;

        trajectoryEndPointRandomizer = endPointRandomOffsetValue;
        trajectoryEndPointRandomized = targetTransform.position + endPointRandomOffsetValue;

        trajectoryEndPoint = trajectoryEndPointRandomized;
        trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        if(trajectoryRange.x == 0) {
            Debug.LogError("Projectile trajectory range is 0");
        }

        float distanceToTarget = Mathf.Abs(trajectoryEndPointRandomized.x - transform.position.x);
        trajectoryMaxRelativeHeight = distanceToTarget * projectileTrajectoryYCurve;

        OnProjectileInitialized?.Invoke(this, EventArgs.Empty);
        OnAnyProjectileInstantiated?.Invoke(this, EventArgs.Empty);
    }

    protected void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update() {
        if (projectileHasHit) return;
        UpdateProjectilePosition();
    }

    protected void UpdateProjectilePosition() {

        if (homingProjectile) {

            if(projectileTarget != null) {
                trajectoryEndPoint = projectileTarget.transform.position + trajectoryEndPointRandomizer;
            }

            trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;
        }

        if (trajectoryRange.x < 0) {
            // Target is located behind shooted
            projectileMoveSpeed = -projectileMoveSpeed;
        }

        if(transform.position.y < 0 && !projectileHasHit) {

            //// Fire hit ?
            //if (enemyProjectile && Mathf.Abs(transform.position.x) < .5f) {
            //    Debug.Log("Fire Hit on Update");
            //    Fire.Instance.TakeDamage(1, parentMob.transform);
            //    parentMob.Die();
            //}

            // Ground hit
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

        nextPosition.x = nextPositionX;
        nextPosition.y = nextPositionY;

        CalculateNewProjectileMoveSpeed(nextPositionXNormalized);
        projectileMoveDir = nextPosition - transform.position;

        // Guide projectile along animation curve
        transform.position = nextPosition;

        if (nextPositionXNormalized > 1.2) {
            // Projectile has reached the end of its animation curve
            ProjectileHasHit(false);
        }
    }

    protected void CalculateNewProjectileMoveSpeed(float newPositionXNormalized) {
        float projectileMoveSpeedNormalized = projectileSpeedAnimationCurve.Evaluate(newPositionXNormalized);
        projectileMoveSpeed = projectileMaxMoveSpeed * projectileMoveSpeedNormalized;
    }

    protected virtual void ProjectileHasHit(bool mobHit) {
        if (projectileHasHit) return;

        projectileHasHit = true;
        OnProjectileHit?.Invoke(this, EventArgs.Empty);
        OnAnyProjectileHit?.Invoke(this, EventArgs.Empty);
        solidCollider.enabled = false;
        if(!gameObject.activeInHierarchy) {
            Debug.Log(this + " projectile is not active in hierarchy !");
        }

        StartCoroutine(ResetInObjectPoolAfterDelay(2f));
    }

    protected IEnumerator ResetInObjectPoolAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        ResetInObjectPool();
    }

    public virtual Vector2 GetTrajectoryEndPoint() {
        return trajectoryEndPointRandomized;
    }

    public virtual Vector3 GetProjectileMoveDir() {
        return projectileMoveDir;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (projectileHasHit && !projectileSO.isExplosiveProjectile) return;

        mobHit = collision.GetComponentInParent<Mob>();
        bool playerHit = collision.GetComponentInParent<Player>() != null;
        Barricade barricade = collision.gameObject.GetComponentInParent<Barricade>();
        bool groundHitOrOther = groundLayer == (groundLayer | (1 << collision.gameObject.layer));

        FireOrbCollider fireOrbCollider = collision.gameObject.GetComponent<FireOrbCollider>();

        if (mobHit == null && !playerHit && barricade == null && !groundHitOrOther && fireOrbCollider == null) return;
        if (collision.gameObject.GetComponent<CreatureDetectionCollider>() != null) return;
        if (collision.gameObject.GetComponent<WorkerDetectionCollider>() != null) return;
        //if (collision.gameObject.GetComponent<WorkerInteractionCollider>() != null) return;

        // Hit mob
        if (mobHit != null  && mobHit != parentMob) {

            Worker worker = mobHit as Worker;
            if (worker != null) {
                // Check if worker is shooting another worker
                if (!enemyProjectile) return;
                // Check if worker is in a tower
                if (worker.GetDefensiveStructureAssigned() != null) return;
            }

            if (mobHit is Creature) {
                if(enemyProjectile) return;
            }

            HandleMobCollision(mobHit);
            return;
        }

        // Hit Player
        if (collision.GetComponentInParent<Player>() != null && enemyProjectile) {
            if (collision.GetComponent<HuntingFlag_PlayerDefined>() != null) return;
            ProjectileHasHit(false);
            Transform damageSource = null;
            if(parentMob != null) {
                damageSource = parentMob.transform;
            }
            Player.Instance.TakeDamage(damage, damageSource);
            return;
        }

        // Hit Barricade
        if (barricade != null && enemyProjectile && barricade.GetBarricadeHealthNormalized() > 0) {
            ProjectileHasHit(false);
            barricade.TakeDamage(damage, parentMob.transform);
        }

        // Hit Secondary Fire ?
        if (fireOrbCollider != null) {
            Fire fire = fireOrbCollider.GetComponentInParent<Fire>();
            if (fire.GetIsSecondaryFire()) {
                ProjectileHasHit(false);
                if (enemyProjectile) {
                    fire.TakeDamage(1, parentMob.transform);
                }
            };

            if (fire.GetIsMainFire()) {
                if (parentMob is Creature) {
                    parentMob.Die();
                };
            }
        }

        if (barricade != null && enemyProjectile && barricade.GetBarricadeHealthNormalized() > 0) {
            ProjectileHasHit(false);
            barricade.TakeDamage(damage, parentMob.transform);
        }


        // Sol ou autres
        if (groundLayer == (groundLayer | (1 << collision.gameObject.layer))) {
            ProjectileHasHit(false);
        }

    }

    protected void HandleMobCollision(Mob mob) {

        if (mobHit != null) {
            ProjectileHasHit(true);
            mobHit.TakeDamage(damage, damageSource, false);
            return;
        }
    }

    public ProjectileSO GetProjectileSO() {
        return projectileSO;
    }

    protected void ResetInObjectPool() {
        OnProjectileReset?.Invoke(this, EventArgs.Empty);
        projectileHasHit = false;
        gameObject.SetActive(false);

        if(parentMob != null) {
            parentMob.GetComponent<MobAttack>().ResetProjectileInObjectPool(this);
        } else {
            Destroy(gameObject);
        }
    }

    public void TryDestroyProjectile() {
        if(projectileSO.canBeDestoyedByBullets) {
            ProjectileHasHit(false);
        }
    }

    public void InvokeOnAnyProjectileInstantiated() {
        OnAnyProjectileInstantiated?.Invoke(this, EventArgs.Empty);
    }
    public void InvokeOnProjectileInitialized() {
        OnProjectileInitialized?.Invoke(this, EventArgs.Empty);
    }
    public bool GetAlignToGroundOnImpact() {
        return alignToGroundOnImpact;
    }

    public Vector3 GetRotationOnImpact() {
        return rotationOnImpact;
    }
}
