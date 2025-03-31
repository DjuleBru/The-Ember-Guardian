using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAttack : MonoBehaviour
{
    [SerializeField] protected bool isProjectileAttack;
    [SerializeField] protected bool isStaticProjectileAttack;
    [ShowIf("isStaticProjectileAttack")]
    [SerializeField] protected bool staticProjectileAutoTargetsPlayer;
    [SerializeField] protected bool isAnimatedAttack;
    [SerializeField] protected ProjectileSO projectileSO;
    [SerializeField] protected Transform staticProjectilePrefab;

    [SerializeField] protected Transform projectileSpawnPoint;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected float attackAnimationDelay;
    [SerializeField] protected float totalAttackAnimationTime;

    [SerializeField] protected int projectileAmountInPool;
    protected Queue<Projectile> availableProjectiles = new Queue<Projectile>();
    protected Queue<StaticProjectile> availableStaticProjectiles = new Queue<StaticProjectile>();

    protected Mob mob;

    protected float attackTimer;
    protected int attackDamage;
    protected int initialAttackDamage;

    protected GameObject attackTargetGameObject;
    protected IDamageable attackTargetIDamageable;
    protected IDamageable previousAttackTargetIDamageable;

    public event EventHandler OnMobAttack;
    public event EventHandler OnMobAttackHit;
    public event EventHandler OnAttackTargetSet;

    protected bool attacking;
    protected bool attackStarted;
    protected bool homingProjectile;
    protected bool attackIgnoresTemporaryInvincibility;

    protected virtual void Awake() {
        mob = GetComponent<Mob>();
        if(isProjectileAttack) {
            InitializeProjectilePool();
        }
        if (isStaticProjectileAttack) {
            InitializeStaticProjectilePool();
        }
    }

    protected void Update() {
        attackTimer -= Time.deltaTime;

        if ((attackTargetIDamageable as MonoBehaviour)!= null) {
            attacking = true;

            if(attackTimer <= 0 ) {
                attackTimer = attackCooldown;
                Attack();
            }

        } else {
            attacking = false;
        }

    }

    protected virtual void Attack() {
        OnMobAttack?.Invoke(this, EventArgs.Empty);
        if(isAnimatedAttack) {
            StartCoroutine(AnimatedAttackCoroutine(totalAttackAnimationTime));
            return;
        }

        if(isProjectileAttack) {
            StartCoroutine(SpawnProjectileAfterDelay(attackAnimationDelay, totalAttackAnimationTime));
        } else if (isStaticProjectileAttack) {
            StartCoroutine(SpawnStaticProjectileAfterDelay(attackAnimationDelay, totalAttackAnimationTime));
        } else {
            StartCoroutine(DealDamageAfterDelay(attackAnimationDelay, totalAttackAnimationTime));
        }
    }

    protected IEnumerator SpawnProjectileAfterDelay(float delay, float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(delay);

        if (mob.GetDead()) yield break;

        // Projectile can be instantiated AFTER attack target reset, so must keep track of previous attack target

        if (previousAttackTargetIDamageable != null) {

            Projectile projectile = null;
            if (availableProjectiles.Count > 0) {
                projectile = availableProjectiles.Dequeue(); // Prendre un projectile disponible
            } else {
                projectile = AddNewProjectileInProjectilePool();
            }


            Vector3 endPointRandomized = GetEndPointRandomized();
            projectile.gameObject.SetActive(true);
            projectile.transform.SetParent(null);
            projectile.ActivateAndInitialize(previousAttackTargetIDamageable.GetProjectileTarget(), projectileSO, mob, attackDamage, endPointRandomized, homingProjectile);
        }

        yield return new WaitForSeconds(totalAttackAnimationTime - delay);

        attackStarted = false;
    }

    protected IEnumerator SpawnStaticProjectileAfterDelay(float delayToSpawnStaticProjectile, float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(delayToSpawnStaticProjectile);

        if (mob.GetDead()) yield break;

        // Projectile can be instantiated AFTER attack target reset, so must keep track of previous attack target

        if (previousAttackTargetIDamageable != null) {
            Vector3 spawnPosition = projectileSpawnPoint.position;

            if(staticProjectileAutoTargetsPlayer) {
                float xRandomizer = UnityEngine.Random.Range(-1.5f, 1.5f);
                spawnPosition = attackTargetGameObject.transform.position;
                spawnPosition.x += xRandomizer;
            }

            StaticProjectile projectile = null;
            if(availableStaticProjectiles.Count > 1) {
                projectile = availableStaticProjectiles.Dequeue(); // Prendre un projectile disponible
            } else {
                projectile = AddNewStaticProjectileInProjectilePool();
            }

            projectile.gameObject.SetActive(true);
            projectile.transform.SetParent(null);
            projectile.transform.position = spawnPosition;
            projectile.Initialize(GetAttackDir().x, mob, attackDamage);
        }

        OnMobAttackHit?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(totalAttackAnimationTime - delayToSpawnStaticProjectile);

        attackStarted = false;
    }

    protected IEnumerator DealDamageAfterDelay(float delayToDealDamage, float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(delayToDealDamage);
        if (mob.GetDead()) yield break;

        DealDamage();

        yield return new WaitForSeconds(totalAttackAnimationTime - delayToDealDamage);

        attackStarted = false;
    }
    protected IEnumerator AnimatedAttackCoroutine(float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(totalAttackAnimationTime);
        attackStarted = false;
    }

    protected void InitializeProjectilePool() {
        Transform projectilePrefab = projectileSO.projectilePrefab;

        for (int i = 0; i < projectileAmountInPool; ++i) {
            Projectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity, projectileSpawnPoint).GetComponent<Projectile>();
            projectile.gameObject.SetActive(false);
            availableProjectiles.Enqueue(projectile);
        }
    }
    protected void InitializeStaticProjectilePool() {
        Transform projectilePrefab = staticProjectilePrefab;

        for (int i = 0; i < projectileAmountInPool; ++i) {
            StaticProjectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity, projectileSpawnPoint).GetComponent<StaticProjectile>();
            projectile.gameObject.SetActive(false);
            availableStaticProjectiles.Enqueue(projectile);
        }

    }

    protected Projectile AddNewProjectileInProjectilePool() {
        Transform projectilePrefab = projectileSO.projectilePrefab;

        Projectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity, projectileSpawnPoint).GetComponent<Projectile>();
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    protected StaticProjectile AddNewStaticProjectileInProjectilePool() {
        Transform projectilePrefab = staticProjectilePrefab;

        StaticProjectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity, projectileSpawnPoint).GetComponent<StaticProjectile>();
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    public virtual void DealDamage() {

        if (attackTargetIDamageable != null) {
            attackTargetIDamageable.TakeDamage(attackDamage, transform, false, attackIgnoresTemporaryInvincibility);
        }
        if ((attackTargetIDamageable as MonoBehaviour) == Fire.Instance) {
            mob.Die();
        }

        OnMobAttackHit?.Invoke(this, EventArgs.Empty);
    }

    protected virtual Vector3 GetEndPointRandomized() {
        return Vector3.zero;
    }

    public void SetAttackTarget(IDamageable iDamageable) {

        if(GetIsRangedAttack() && attackTimer == 0) {
            attackTimer = UnityEngine.Random.Range(0, attackCooldown / 3);
        }

        this.attackTargetIDamageable = iDamageable;
        previousAttackTargetIDamageable = attackTargetIDamageable;
        attackTargetGameObject = (attackTargetIDamageable as MonoBehaviour).gameObject;

        OnAttackTargetSet?.Invoke(this, EventArgs.Empty);
    }

    public virtual void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
        attackTargetGameObject = null;
    }

    public void SetHomingProjectile(bool homing) {
        homingProjectile = homing;
    }

    public void SetProjectileAttack(bool projectileAttack) {
        this.isProjectileAttack = projectileAttack;
    }

    public Vector3 GetAttackDir() {
        if((attackTargetIDamageable as MonoBehaviour) == null) {
            return Vector3.zero;
        }
        return (attackTargetIDamageable as MonoBehaviour).transform.position - transform.position;
    }

    public bool GetAttacking() {
        return attacking;
    }

    public bool GetAttackStarted() {
        return attackStarted;
    }

    public int GetAttackDamage() {
        return attackDamage;
    }

    public bool GetIsRangedAttack() {
        return isProjectileAttack || isStaticProjectileAttack;
    }

    public void BuffDamage(float buff) {
        attackDamage = (int)(attackDamage*buff);
    }

    public void ResetDamageBuff() {
        attackDamage = initialAttackDamage;
    }

    public void ResetProjectileInObjectPool(Projectile projectile) {
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.SetParent(projectileSpawnPoint);
        availableProjectiles.Enqueue(projectile); // Remettre le projectile dans la queue
    }

    public void ResetStaticProjectileInObjectPool(StaticProjectile projectile) {
        Debug.Log("ResetStaticProjectileInObjectPool");
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.SetParent(projectileSpawnPoint);
        availableStaticProjectiles.Enqueue(projectile); // Remettre le projectile dans la queue
    }

    public void InvokeAttackHit() {
        OnMobAttackHit?.Invoke(this, EventArgs.Empty);
    }

    public void SetAttackIgnoresTemporaryInvincibility() {
        attackIgnoresTemporaryInvincibility = true;
    }

}
