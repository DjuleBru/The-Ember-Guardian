using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAttack : MonoBehaviour
{
    [SerializeField] private bool isProjectileAttack;

    [SerializeField] private Transform projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float attackRate;
    [SerializeField] private float attackAnimationDelay;

    private float attackTimer;

    private IDamageable attackTargetIDamageable;
    private IDamageable previousAttackTargetIDamageable;

    public event EventHandler OnMobAttack;
    public event EventHandler OnAttackTargetSet;

    private bool attacking;

    private void Update() {

        if((attackTargetIDamageable as MonoBehaviour)!= null) {

            attacking = true;
            attackTimer -= Time.deltaTime;

            if(attackTimer <= 0 ) {
                attackTimer = attackRate;
                Attack();
            }

        } else {
            attacking = false;
        }
    }

    private void Attack() {
        OnMobAttack?.Invoke(this, EventArgs.Empty);

        if(isProjectileAttack) {
            StartCoroutine(SpawnProjectileAfterDelay(attackAnimationDelay));
        }
    }

    private IEnumerator SpawnProjectileAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        // Projectile can be instantiated AFTER attack target reset, so must keep track of previous attack target
        if ((attackTargetIDamageable as MonoBehaviour) == null) {
            previousAttackTargetIDamageable = null;
        }

        if (previousAttackTargetIDamageable != null) {

            Projectile projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity).GetComponent<Projectile>();

            Vector3 projectileTarget = new Vector3(previousAttackTargetIDamageable.GetProjectileTarget().position.x, 0, 0);
            projectile.ActivateAndInitialize(projectileTarget);
        }
    }

    public void SetAttackTarget(IDamageable iDamageable) {
        this.attackTargetIDamageable = iDamageable;
        previousAttackTargetIDamageable = attackTargetIDamageable;

        OnAttackTargetSet?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveAttackTarget() {
        attackTargetIDamageable = null;
    }

    public Vector3 GetAttackDir() {
        return (attackTargetIDamageable as MonoBehaviour).transform.position - transform.position;
    }

    public bool GetAttacking() {
        return attacking;
    }

}
