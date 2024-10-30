using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform projectileTarget;
    [SerializeField] private Transform projectileParent;
    protected MobSpawner mobSpawner;
    
    protected int health;

    public event EventHandler OnMobDied;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobDamageTaken;

    public class OnMobDamageTakenEventArgs {
        public Vector3 damageOriginPosition;
    }

    public void SetMobSpawner(MobSpawner mobSpawner) {
        this.mobSpawner = mobSpawner;
    }

    public MobSpawner GetMobSpawner() {
        return mobSpawner;
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        health -= damage;
        OnMobDamageTaken?.Invoke(this, new OnMobDamageTakenEventArgs {
            damageOriginPosition = damageSourcePosition,
        });

        if (health <= 0) {
            Die();
        }
    }

    public virtual void Die() {
        OnMobDied?.Invoke(this, EventArgs.Empty);
        GetComponent<MobMovement>().enabled = false;
    }

    protected IEnumerator DestroyGameObjectAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetProjectileParent() {
        return projectileParent;
    }
}
