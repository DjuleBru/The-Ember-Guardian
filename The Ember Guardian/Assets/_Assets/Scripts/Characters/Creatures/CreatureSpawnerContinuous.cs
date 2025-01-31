using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnerContinuous : MobSpawner, IDamageable {

    [SerializeField] protected int spawnerHealth;
    [SerializeField] protected float spawnRate;
    [SerializeField] protected float spawnAnimationDelay;
    [SerializeField] protected bool canSpawnMobsAtNight;

    protected float spawnTimer;

    private bool dead;

    public event EventHandler OnSpawnerDamaged;
    public event EventHandler OnSpawnerDied;
    public event EventHandler OnSpawnerSpawnStart;
    public event EventHandler OnSpawnerSpawned;
    public event EventHandler<OnHitPSInstatiatedEventArgs> OnHitPSInstatiated;

    public class OnHitPSInstatiatedEventArgs : EventArgs {
        public float angle;
        public float height;
    }

    protected override void Start() {
    }

    protected void Update() {
        if (dead) return;
        if (mobSpawnedList.Count >= mobAmountToSpawn) return;
        if (!canSpawnMobsAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

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

    public override void SpawnMobs(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {
            Mob mob = Instantiate(mobPrefab, spawnPosition.position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if (isCreatureSpawner) {

                Creature creature = (Creature)mob;
                if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {

                    creature.SetAsDayCreature(false);
                    CreaturesManager.Instance.AddAdditionalCreatureToNightWave(creature);

                } else {

                    mob.GetComponent<Creature>().SetAsDayCreature(true);
                }

                mob.transform.parent = SpawnedObjects.Instance.creaturesContainer;
            }

            if (mob is Worker) {
                mob.transform.parent = SpawnedObjects.Instance.workersContainer;
            }

            if (mob is Animal) {
                mob.transform.parent = SpawnedObjects.Instance.AnimalsContainer;
            }

            InvokeOnMobSpawned(mob);
        }
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
        OnHitPSInstatiated?.Invoke(this, new OnHitPSInstatiatedEventArgs {
            angle = angle,
            height = height,
        });
    }

    public void SetDead() {
        dead = true;
    }

    public void SetCanSpawnAtNight(bool canSpawn) {
        canSpawnMobsAtNight = canSpawn;
    }

}
