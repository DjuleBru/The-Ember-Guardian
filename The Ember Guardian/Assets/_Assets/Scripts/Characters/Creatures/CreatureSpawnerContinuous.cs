using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnerContinuous : MobSpawner, IDamageable {

    [SerializeField] protected int spawnerHealth;
    [SerializeField] protected float spawnRate;
    [SerializeField] protected float spawnAnimationDelay;
    [SerializeField] protected Transform mobHitPS_Splatter;
    [SerializeField] protected Transform mobHitPS_Splatter_Continuous;

    protected float spawnTimer;

    private bool dead;

    public event EventHandler OnSpawnerDamaged;
    public event EventHandler OnSpawnerDied;
    public event EventHandler OnSpawnerSpawnStart;
    public event EventHandler OnSpawnerSpawned;

    protected void Update() {
        if (dead) return;
        if (mobSpawnedList.Count >= mobAmountToSpawn) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0) {
            spawnTimer = spawnRate;
            StartCoroutine(SpawnCreature());
        }
    }

    private IEnumerator SpawnCreature() {
        OnSpawnerSpawnStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(spawnAnimationDelay);

        OnSpawnerSpawned?.Invoke(this, EventArgs.Empty);
        SpawnMobs(1);
    }

    public void Die() {
        dead = true;
        GetComponent<Collider2D>().enabled = false;
        OnSpawnerDied?.Invoke(this, EventArgs.Empty);
    }


    public Transform GetMeleeAttackPosition() {
        return transform;
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public void TakeDamage(int damage, Transform damageSource, bool crit = false) {
        spawnerHealth -= damage;

        if(spawnerHealth <= 0) {
            Die();
        } else {
            OnSpawnerDamaged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void InstantiateHitPS(float angle, float height, bool critHit) {
        Vector3 localPosition = new Vector3(transform.position.x, height, 0);

        Instantiate(mobHitPS_Splatter, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
        Instantiate(mobHitPS_Splatter_Continuous, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
    }

}
