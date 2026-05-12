using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private string spawnerID;

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
    [SerializeField] protected bool flying_SpawnAtSpawnPosition;

    [SerializeField] protected MobSpawner linkedMobSpawner;
    protected int eliteSpawnedAmount;
    protected int daysSinceSpawnerActive;
    protected bool isAnimalHordeModeSpawner;
    protected bool animalHordeModeSpawnerActive;
    protected float animalSpawnerActivationDistance_HordeMode = 15f;

    private float easyDifficultySpawnAmountMultiplier = 0.8f;
    private float hardDifficultySpawnAmountMultiplier = 1.15f;

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

    protected void Awake() {
        if (GetComponentInParent<Creature>() != null) return;
        SpawnersManager.Instance.AddSpawner(this);
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

        if (SavingManager_Level.Instance.GetLoadingSavedLevel() && !LevelManager.Instance.IsHordeMode()) {
            firstDawnAfterLoad = true;
            return;
        }

        if(isCreatureSpawner) {
            SettingsManager.Difficulty currentDifficulty = SettingsManager.Instance.GetDifficulty();

            if (LevelManager.Instance.IsHordeMode()) {
                currentDifficulty = SettingsManager.Instance.GetHordeDifficulty();
            }

            if (currentDifficulty == SettingsManager.Difficulty.Easy) {
                mobAmountToSpawn = (int)(mobAmountToSpawn * easyDifficultySpawnAmountMultiplier);
            }
            if (currentDifficulty == SettingsManager.Difficulty.Hard) {
                mobAmountToSpawn = (int)(mobAmountToSpawn * hardDifficultySpawnAmountMultiplier);
            }

            if (mobAmountToSpawn == 0) {
                mobAmountToSpawn = 1;
            }
        }

        if (SavingManager_Level.Instance.GetLoadingSavedLevel() && LevelManager.Instance.IsHordeMode() && isAnimalSpawner) {
            return;
        }

        SpawnMobs(mobAmountToSpawn);
    }

    protected virtual void Update() {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f) {
            checkTimer = checkInterval;

            if(isCreatureSpawner) {
                HandleMobActivationPerMob();
            }

            if(isAnimalHordeModeSpawner && !animalHordeModeSpawnerActive) {
                HandleAnimalSpawnerActivation();
            }

        }
    }
    private void HandleAnimalSpawnerActivation() {
        float distance = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        //Debug.Log(this + " distance " + distance);
        if(distance < animalSpawnerActivationDistance_HordeMode) {
            animalHordeModeSpawnerActive = true;
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

        if (isAnimalSpawner && isAnimalHordeModeSpawner && animalHordeModeSpawnerActive) {
            mobAmountToSpawnOnDawn = GetAnimalAmountToSpawn_HordeMode();
            daysSinceSpawnerActive++;
            SpawnMobs(mobAmountToSpawnOnDawn);
            return;
        }

        if (mobAmountToSpawnOnDawn > maxMobsRespawningAtDawn) {
            mobAmountToSpawnOnDawn = maxMobsRespawningAtDawn;
        }

        if ((isAnimalSpawner || isCreatureSpawner) && CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            Debug.Log(this + " Spawner is within camp zone limits ! Spawn cancelled");
            return;
        }

        SpawnMobs(mobAmountToSpawnOnDawn);


    }

    protected int GetAnimalAmountToSpawn_HordeMode() {
        float mobAmountToSpawnOnDawn = 0;

        if (daysSinceSpawnerActive == 0) {
            mobAmountToSpawnOnDawn = mobAmountToSpawn / 1.5f;
        }

        if (daysSinceSpawnerActive == 1) {
            mobAmountToSpawnOnDawn = mobAmountToSpawn / 3f;
        }

        if (daysSinceSpawnerActive == 2) {
            mobAmountToSpawnOnDawn = mobAmountToSpawn / 4.5f;
        }

        return Mathf.RoundToInt(mobAmountToSpawnOnDawn);
    }

    public virtual void SpawnMobs(int mobAmount) {
        //Debug.Log(this + " SpawnMobs " + mobAmount);
        if (mobPrefab == null) return;
        if (ambushSpawned) return;

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

                if(creatureSO.flying && !flying_SpawnAtSpawnPosition) {
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
        if (linkedMobSpawner == null) return;
        if(linkedMobSpawner.GetMobCount() == 0) {
            KillRemainingSpawnedMobs();
            OnAllMobRemoved?.Invoke(this, EventArgs.Empty);
        }
    }

    public void KillRemainingSpawnedMobs() {
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
    public int GetDaysSinceSpawnerActive() {
        return daysSinceSpawnerActive;
    }

    public Transform GetMobPrefab() {
        return mobPrefab;
    }

    public void SetSpawnerParameters_HordeMode(Transform mobPrefab, int mobAmountToSpawn, float radiusToRoamAround, int daysSinceSpawnerActive = 0) {
        this.mobPrefab = mobPrefab;
        this.mobAmountToSpawn = mobAmountToSpawn;
        this.radiusToRoamAround = radiusToRoamAround;
        this.daysSinceSpawnerActive = daysSinceSpawnerActive;

        isAnimalHordeModeSpawner = true;
    }

    public void SetMobAmountToSpawn(int mobAmountToSpawn) {
        this.mobAmountToSpawn = mobAmountToSpawn;
    }

    public void ForceNewID() {
        spawnerID = Guid.NewGuid().ToString();
    }

    public static string GenerateSpawnerID(HordeModeBlock block, string spawnerType, Vector3 localPosition) {
        return $"{spawnerType}_{localPosition.x}_{localPosition.y}";
    }
}
