using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField, HideInInspector] private string spawnerID;

    public string GetSpawnerID() => spawnerID;

#if UNITY_EDITOR
    private void OnValidate() {
        // Vérifie si on est dans une scène (pas dans un prefab)
        if (string.IsNullOrEmpty(spawnerID) && gameObject.scene.IsValid()) {
            spawnerID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }

#endif

    [SerializeField] protected SpriteRenderer sceneViewSpawnerSpriteRenderer;
    [SerializeField] protected Transform mobPrefab;
    [SerializeField] protected List<Transform> spawnPositionList;
    [SerializeField] protected float spawnPositionRandomizer;
    [SerializeField] protected float radiusToRoamAround;
    [SerializeField] protected int mobAmountToSpawn;
    [SerializeField] protected int maxMobsRespawningAtDawn;

    [SerializeField] protected bool isCreatureSpawner;
    [SerializeField] protected bool isAnimalSpawner;
    [SerializeField] protected bool blockSpawningOnStart;

    [SerializeField] protected bool canSpawnEliteCreatures;
    [SerializeField] protected float eliteSpawnProbability;
    [SerializeField] protected int eliteSpawnAmount;

    [SerializeField] protected MobSpawner linkedMobSpawner;
    protected int eliteSpawnedAmount;

    private float creatureActivationDistance = 50f;
    private float checkTimer;
    private float checkInterval = 2f;

    protected List<Mob> mobSpawnedList = new List<Mob>();

    public event EventHandler<OnMobSpawnedEventArgs> OnMobSpawned;
    public event EventHandler<OnMobSpawnedEventArgs> OnMobRemoved;
    public event EventHandler OnAllMobRemoved;

    protected bool mobsCanSpawnAtDawn = true;
    protected bool ambushSpawned;
    protected bool firstDawnAfterLoad;

    public class OnMobSpawnedEventArgs : EventArgs {
        public Mob mob;
    }

    protected virtual void Start() {
        if(sceneViewSpawnerSpriteRenderer != null) {
            sceneViewSpawnerSpriteRenderer.enabled = false;
        }

        if(linkedMobSpawner != null) {
            linkedMobSpawner.OnAllMobRemoved += LinkedMobSpawner_OnAllMobRemoved;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        if (blockSpawningOnStart) return;

        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            firstDawnAfterLoad = true;
            return;
        }
        SpawnMobs(mobAmountToSpawn);
    }

    protected virtual void Update() {
        if (!isCreatureSpawner) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f) {
            checkTimer = checkInterval;
            HandleMobActivationPerMob();
        }
    }

    private void HandleMobActivationPerMob() {

        for (int i = 0; i < mobSpawnedList.Count; i++) {
            Creature creature = mobSpawnedList[i].GetComponent<Creature>();
            if (creature == null) continue;

            float distance = Mathf.Abs(Player.Instance.transform.position.x - creature.transform.position.x);
            bool shouldBeActive = distance <= creatureActivationDistance;

            creature.SetCreatureActive(shouldBeActive);
        }
    }

    private void LinkedMobSpawner_OnAllMobRemoved(object sender, EventArgs e) {
        KillRemainingSpawnedMobs();
    }

    public virtual void RemoveMobFromMobSpawnedList(Mob mob) {
        mobSpawnedList.Remove(mob);
        OnMobRemoved?.Invoke(this, new OnMobSpawnedEventArgs {
            mob = mob,
        });

        if (mobSpawnedList.Count == 0) {
            OnAllMobRemoved?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (!mobsCanSpawnAtDawn) return;
        if (firstDawnAfterLoad) {
            firstDawnAfterLoad = false;
            return;
        }

        int mobAmountToSpawnOnDawn = mobAmountToSpawn - mobSpawnedList.Count;

        if(mobAmountToSpawnOnDawn > maxMobsRespawningAtDawn) {
            mobAmountToSpawnOnDawn = maxMobsRespawningAtDawn;
        }

        if ((isAnimalSpawner || isCreatureSpawner) && CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            Debug.Log(this + " Spawner is within camp zone limits ! Spawn cancelled");
            return;
        }

        SpawnMobs(mobAmountToSpawnOnDawn);
    }

    public virtual void SpawnMobs(int mobAmount) {
        Debug.Log(this + " SpawnMobs " + mobAmount);
        for (int i = 0; i < mobAmount; i++) {

            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPositionList[0].position;
            spawnPositionRandomized.x += positionRandomizer;

            Mob mob = Instantiate(mobPrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if(isCreatureSpawner) {
                CreatureSO creatureSO = mob.GetComponent<Creature>().GetCreatureSO();
                mob.GetComponent<Creature>().SetAsDayCreature(true);

                if(creatureSO.flying) {
                    float yPositionRandomized = UnityEngine.Random.Range(creatureSO.flightMaxAltitude, creatureSO.flightMaxAltitude);
                    spawnPositionRandomized.y += yPositionRandomized;
                    mob.transform.position = spawnPositionRandomized;
                }

                mob.transform.parent = SpawnedObjects.Instance.creaturesContainer;
                if(canSpawnEliteCreatures) {
                    HandleEliteSpawn(mob.GetComponent<Creature>());
                }
            }

            if(mob is Worker) {
                mob.transform.parent = SpawnedObjects.Instance.workersContainer;
            }

            if (mob is Animal) {
                mob.transform.parent = SpawnedObjects.Instance.AnimalsContainer;
            }

            InvokeOnMobSpawned(mob);
        }
    }

    public virtual void SpawnCreatures(CreatureSO creatureSO, int mobAmount, bool agressiveDayCreature = false) {
        for (int i = 0; i < mobAmount; i++) {

            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPositionList[0].position;
            spawnPositionRandomized.x += positionRandomizer;

            if (creatureSO.flying) {
                float yPositionRandomized = UnityEngine.Random.Range(creatureSO.flightMinAltitude, creatureSO.flightMaxAltitude);
                spawnPositionRandomized.y += yPositionRandomized;
            }

            if(spawnPositionRandomized.x > LevelManager.Instance.GetMaxLevelLimitAbsolute()) {
                spawnPositionRandomized.x = LevelManager.Instance.GetMaxLevelLimitAbsolute() - 10f;
            }
            if (spawnPositionRandomized.x < LevelManager.Instance.GetMinLevelLimitAbsolute()) {
                spawnPositionRandomized.x = LevelManager.Instance.GetMinLevelLimitAbsolute() + 10f;
            }

            Mob mob = Instantiate(creatureSO.creaturePrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);
            mob.GetComponent<Creature>().SetAsDayCreature(true);

            if(agressiveDayCreature) {
                mob.GetComponent<Creature>().SetAsAgressiveDayCreature();
            }

            mob.transform.parent = SpawnedObjects.Instance.creaturesContainer;
            if (canSpawnEliteCreatures) {
                HandleEliteSpawn(mob.GetComponent<Creature>());
            }
            

            InvokeOnMobSpawned(mob);
        }
    }
    public IEnumerator SpawnMobsCoroutine(float delayBetweenMobs) {
        for (int i = 0; i < mobAmountToSpawn; i++) {
            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPositionList[0].position;
            spawnPositionRandomized.x += positionRandomizer;

            Mob mob = Instantiate(mobPrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if (isCreatureSpawner) {
                mob.GetComponent<Creature>().SetAsDayCreature(true);
                mob.transform.parent = SpawnedObjects.Instance.creaturesContainer;
                if (canSpawnEliteCreatures) {
                    HandleEliteSpawn(mob.GetComponent<Creature>());
                }
            }

            if (mob is Worker) {
                mob.transform.parent = SpawnedObjects.Instance.workersContainer;
            }

            if (mob is Animal) {
                mob.transform.parent = SpawnedObjects.Instance.AnimalsContainer;
            }

            InvokeOnMobSpawned(mob);

            yield return new WaitForSeconds(delayBetweenMobs);
        }
    }

    public void SpawnMobs(int mobAmount, Transform mobPrefabToSpawn, Vector3 position) {
        for (int i = 0; i < mobAmount; i++) {
            Mob mob = Instantiate(mobPrefabToSpawn, position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if (isCreatureSpawner) {
                mob.GetComponent<Creature>().SetAsDayCreature(true);
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

    private void HandleEliteSpawn(Creature creature) {
        if(eliteSpawnAmount != 0) {
            eliteSpawnedAmount++;
            if(eliteSpawnedAmount <= eliteSpawnAmount) {
                creature.SetAsEliteCreature();
            }
        }

        if(eliteSpawnProbability != 0) {
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat < eliteSpawnProbability) {
                creature.SetAsEliteCreature();
            }
        }

    }

    protected void InvokeOnMobSpawned(Mob mob) {

        OnMobSpawned?.Invoke(this, new OnMobSpawnedEventArgs {
            mob = mob,
        });
    }

    public void LoadLinkedMobSpawner() {
        if(linkedMobSpawner.GetMobCount() == 0) {
            KillRemainingSpawnedMobs();
            OnAllMobRemoved?.Invoke(this, EventArgs.Empty);
        }
    }

    private void KillRemainingSpawnedMobs() {
        List<Mob> mobListCopy = new List<Mob>();
        foreach (Mob mob in mobSpawnedList) {
            mobListCopy.Add(mob);
        }

        foreach (Mob mob in mobListCopy) {
            RemoveMobFromMobSpawnedList(mob);
            mob.Die();
        }
    }

    public void SetMobsCanSpawnAtDawn(bool canSpawn) {
        mobsCanSpawnAtDawn = canSpawn;
    }
    public void SetAmbushSpawned(bool ambushSpawned) {
        this.ambushSpawned = ambushSpawned;
    }
    public int GetMaxMobAmountSpawnedAtDawn() {
        return maxMobsRespawningAtDawn;
    }

    public float GetRadiusToRoamAround() {
        return radiusToRoamAround;
    }

    public int GetMobCount() {
        return mobSpawnedList.Count;
    }

    public bool GetMobsCanSpawnAtDawn() {
        return mobsCanSpawnAtDawn;
    }

    public bool GetAmbushSpawned() {
        return ambushSpawned;
    }
    public void ForceNewID() {
        spawnerID = Guid.NewGuid().ToString();
    }
}
