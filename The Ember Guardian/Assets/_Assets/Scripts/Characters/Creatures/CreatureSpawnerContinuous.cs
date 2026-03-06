using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnerContinuous : MobSpawner, IDamageable {

    [SerializeField] protected int mobAmountSpawnedSimultaneously = 1;
    [SerializeField] protected int spawnerHealth;
    [SerializeField] protected float spawnRate;
    [SerializeField] protected float spawnAnimationDelay;
    [SerializeField] protected bool canSpawnMobsAtNight;
    [SerializeField] protected bool blockAutoMobSpawn;

    protected float spawnTimer;

    private bool dead;
    private bool isPaused;
    protected bool loaded = true;

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
        spawnTimer = 1f;

        if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            loaded = false;
        }

        DayNightManager.Instance.OnCyclePausedByMerchantTalk += DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
    }


    protected void Update() {
        if (!loaded) return;
        if (dead) return;
        if (isPaused) return;
        if (blockAutoMobSpawn) return;
        if (!canSpawnMobsAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        spawnTimer -= Time.deltaTime;

        if (SpawnedMaxMobs()) return;

        if (spawnTimer < 0) {
            spawnTimer = spawnRate;
            StartCoroutine(SpawnCreature());
        }
    }
    private void DayNightManager_OnCycleUnpaused(object sender, EventArgs e) {
        isPaused = false;
    }

    private void DayNightManager_OnCyclePausedByMerchantTalk(object sender, EventArgs e) {
        isPaused = true;
    }

    public bool SpawnedMaxMobs() {
        return mobSpawnedList.Count >= mobAmountToSpawn;
    }

    private IEnumerator SpawnCreature() {
        OnSpawnerSpawnStart?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(spawnAnimationDelay);

        OnSpawnerSpawned?.Invoke(this, EventArgs.Empty);
        SpawnMobs(mobAmountSpawnedSimultaneously);
    }

    public override void SpawnMobs(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {
            Mob mob = Instantiate(mobPrefab, spawnPositionList[UnityEngine.Random.Range(0, spawnPositionList.Count)].position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            HandleMobSpawn(mob);
            InvokeOnMobSpawned(mob);
        }

    }

    public void StartSpawnMobs_Summoner(int mobAmount, List<Transform> spawnPositionList) {
        OnSpawnerSpawnStart?.Invoke(this, EventArgs.Empty);
        StartCoroutine(SpawnMobsCoroutine_Summoner(mobAmount, spawnPositionList));
    }

    private IEnumerator SpawnMobsCoroutine_Summoner(int mobAmount, List<Transform> spawnPositionList) {
        yield return new WaitForSeconds(spawnAnimationDelay);

        for (int i = 0; i < mobAmount; i++) {
            Mob mob = Instantiate(mobPrefab, spawnPositionList[i].position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            HandleMobSpawn(mob);
            InvokeOnMobSpawned(mob);
        }
    }

    public void SpawnMobAtPosition(Transform position) {
        Mob mob = Instantiate(mobPrefab, position.position, Quaternion.identity).GetComponent<Mob>();
        mobSpawnedList.Add(mob);
        mob.SetMobSpawner(this);

        HandleMobSpawn(mob);
        InvokeOnMobSpawned(mob);
    }

    private void HandleMobSpawn(Mob mob) {

        if (isCreatureSpawner) {

            Creature creature = (Creature)mob;
            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {

                creature.SetAsDayCreature(false);
                CreaturesManager.Instance.AddAdditionalCreatureToNightWave(creature);

            }
            else {

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
    }

    public void Die(Transform damageSource = null) {
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

    public void TakeDamage(int damage, Transform damageSource, bool crit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
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

    public void SetDead(bool setDeadFromLoadLevel = false) {
        dead = true;

        if(setDeadFromLoadLevel) {
            GetComponent<Collider2D>().enabled = false;
            OnSpawnerDied?.Invoke(this, EventArgs.Empty);
        }
    }
    public void SetLoaded() {
        loaded = true;
    }

    public void SetCanSpawnAtNight(bool canSpawn) {
        canSpawnMobsAtNight = canSpawn;
    }

    public bool GetDead() {
        return dead;
    }
    private void OnDestroy() {
        DayNightManager.Instance.OnCyclePausedByMerchantTalk -= DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused -= DayNightManager_OnCycleUnpaused;
    }
}
