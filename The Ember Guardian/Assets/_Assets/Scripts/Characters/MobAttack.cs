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
    [SerializeField] protected bool shootsMultipleProjectilesInAttack;
    [SerializeField] protected int projectileAmountShotInAttack = 1;
    [SerializeField] protected float delayBetweenProjectileSpawns;

    protected float attackCooldownBuff = 1f;

    [SerializeField] protected int projectileAmountInPool; 
    protected Dictionary<ProjectileSO, Queue<Projectile>> projectilePools = new Dictionary<ProjectileSO, Queue<Projectile>>();
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
    public event EventHandler OnAttackSpeedModified;

    protected bool stunned = true;
    protected bool attacking;
    protected bool attackStarted;
    protected bool homingProjectile;
    protected bool attackIgnoresTemporaryInvincibility;

    protected virtual void Awake() {
        mob = GetComponent<Mob>();
        if(isProjectileAttack) {
            InitializeProjectilePool(projectileSO);
        }
        if (isStaticProjectileAttack) {
            InitializeStaticProjectilePool();
        }
    }

    protected void Update() {
        if (!stunned) return;

        attackTimer -= Time.deltaTime;

        if ((attackTargetIDamageable as MonoBehaviour)!= null) {
            attacking = true;

            if(attackTimer <= 0 ) {
                attackTimer = attackCooldown / attackCooldownBuff;
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

        for(int i = 0; i < projectileAmountShotInAttack; i++) {
            if (mob.GetDead()) yield break;

            // Projectile can be instantiated AFTER attack target reset, so must keep track of previous attack target

            if (previousAttackTargetIDamageable != null) {
                SpawnProjectile();
            }
            yield return new WaitForSeconds(delayBetweenProjectileSpawns);
        }
       

        yield return new WaitForSeconds(totalAttackAnimationTime - delay);
        attackStarted = false;
    }

    protected void SpawnProjectile() {

        Projectile projectile = GetNextProjectileInPool(projectileSO);

        Vector3 endPointRandomOffsetValue = GetEndPointRandomOffstetValue();
        projectile.gameObject.SetActive(true);
        projectile.transform.SetParent(null);

        if (projectileSO.usesAnimationCurve) {
            projectile.ActivateAndInitialize(previousAttackTargetIDamageable.GetProjectileTarget(), projectileSO, transform, attackDamage, endPointRandomOffsetValue, homingProjectile);
        }
        if (projectileSO.usesForce) {
            ProjectileForces projectileForce = projectile.GetComponent<ProjectileForces>();
            projectileForce.ActivateAndInitializeWithForces(previousAttackTargetIDamageable.GetProjectileTarget(), projectileSO, transform, attackDamage, endPointRandomOffsetValue.x, homingProjectile);
        }
    }

    public Projectile GetNextProjectileInPool(ProjectileSO projectileSO) {
        if (!projectilePools.ContainsKey(projectileSO)) {
            projectilePools[projectileSO] = new Queue<Projectile>();
        }

        Queue<Projectile> pool = projectilePools[projectileSO];

        if (pool.Count > 0) {
            return pool.Dequeue();
        }
        else {
            return AddNewProjectileInProjectilePool(projectileSO);
        }
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
            projectile.Initialize(GetAttackDir().x, mob, attackDamage, false, true);
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
    protected virtual IEnumerator AnimatedAttackCoroutine(float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(totalAttackAnimationTime);
        attackStarted = false;
    }

    public void InitializeProjectilePool(ProjectileSO projectileSO) {
        if (!projectilePools.ContainsKey(projectileSO)) {
            projectilePools[projectileSO] = new Queue<Projectile>();
        }

        Transform projectilePrefab = projectileSO.projectilePrefab;

        for (int i = 0; i < projectileAmountInPool; ++i) {
            Projectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity, projectileSpawnPoint).GetComponent<Projectile>();
            projectile.gameObject.SetActive(false);
            projectilePools[projectileSO].Enqueue(projectile);
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

    public Projectile AddNewProjectileInProjectilePool(ProjectileSO projectileSO) {
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

    protected virtual Vector3 GetEndPointRandomOffstetValue() {
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

    public void BuffAttackSpeed(float buff) {
        attackCooldownBuff += buff;
        Debug.Log("BuffAttackSpeed " + attackCooldownBuff);
        OnAttackSpeedModified?.Invoke(this, EventArgs.Empty);
    }
    public void DebuffAttackSpeed(float deBuff) {
        attackCooldownBuff -= deBuff;
        Debug.Log("DebuffAttackSpeed " + attackCooldownBuff);
        OnAttackSpeedModified?.Invoke(this, EventArgs.Empty);
    }
    public float GetAttackSpeedBuff() {
        return attackCooldownBuff;
    }

    public void ResetProjectileInObjectPool(Projectile projectile) {
        ProjectileSO projectileSO = projectile.GetProjectileSO();

        if (!projectilePools.ContainsKey(projectileSO)) {
            projectilePools[projectileSO] = new Queue<Projectile>();
        }

        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.SetParent(projectileSpawnPoint);
        projectile.gameObject.SetActive(false);
        projectilePools[projectileSO].Enqueue(projectile);
    }

    public void ResetStaticProjectileInObjectPool(StaticProjectile projectile) {
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
