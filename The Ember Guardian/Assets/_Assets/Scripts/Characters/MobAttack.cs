using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAttack : MonoBehaviour
{
    [SerializeField] protected bool isProjectileAttack;
    [SerializeField] protected ProjectileSO projectileSO;

    [SerializeField] protected Transform projectileSpawnPoint;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected float attackAnimationDelay;
    [SerializeField] protected float totalAttackAnimationTime;

    protected Mob mob;

    protected float attackTimer;
    protected int attackDamage;

    protected IDamageable attackTargetIDamageable;
    protected IDamageable previousAttackTargetIDamageable;

    public event EventHandler OnMobAttack;
    public event EventHandler OnAttackTargetSet;

    protected bool attacking;
    protected bool attackStarted;

    protected virtual void Awake() {
        mob = GetComponent<Mob>();
    }

    protected void Update() {

        if((attackTargetIDamageable as MonoBehaviour)!= null) {

            attacking = true;
            attackTimer -= Time.deltaTime;

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

        if(isProjectileAttack) {
            StartCoroutine(SpawnProjectileAfterDelay(attackAnimationDelay));
        } else {
            StartCoroutine(DealDamageAfterDelay(attackAnimationDelay, totalAttackAnimationTime));
        }
    }

    protected IEnumerator SpawnProjectileAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        // Projectile can be instantiated AFTER attack target reset, so must keep track of previous attack target
        if ((attackTargetIDamageable as MonoBehaviour) == null) {
            previousAttackTargetIDamageable = null;
        }

        if (previousAttackTargetIDamageable != null) {

            Projectile projectile = Instantiate(projectileSO.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity).GetComponent<Projectile>();

            Vector3 projectileTarget = new Vector3(previousAttackTargetIDamageable.GetProjectileTarget().position.x, 0, 0);
            projectile.ActivateAndInitialize(projectileTarget, projectileSO, mob);
        }
    }

    protected IEnumerator DealDamageAfterDelay(float delayToDealDamage, float totalAttackAnimationTime) {
        attackStarted = true;
        yield return new WaitForSeconds(delayToDealDamage);

        if (previousAttackTargetIDamageable != null) {
            previousAttackTargetIDamageable.TakeDamage(attackDamage, transform.position);
        }

        if ((attackTargetIDamageable as MonoBehaviour) == Fire.Instance) {
            mob.Die();
        }

        yield return new WaitForSeconds(totalAttackAnimationTime - delayToDealDamage);

        attackStarted = false;
    }

    public void SetAttackTarget(IDamageable iDamageable) {
        this.attackTargetIDamageable = iDamageable;
        previousAttackTargetIDamageable = attackTargetIDamageable;

        OnAttackTargetSet?.Invoke(this, EventArgs.Empty);
    }

    public virtual void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
    }

    public Vector3 GetAttackDir() {
        return (attackTargetIDamageable as MonoBehaviour).transform.position - transform.position;
    }

    public bool GetAttacking() {
        return attacking;
    }

    public bool GetAttackStarted() {
        return attackStarted;
    }

}
