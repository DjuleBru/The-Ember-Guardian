using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer sceneViewSpawnerSpriteRenderer;
    [SerializeField] protected Transform mobPrefab;
    [SerializeField] protected Transform spawnPosition;
    [SerializeField] protected float spawnPositionRandomizer;
    [SerializeField] protected int mobAmountToSpawn;
    [SerializeField] protected int maxMobsRespawningAtDawn;

    [SerializeField] protected bool isCreatureSpawner;
    [SerializeField] protected bool isAnimalSpawner;
    [SerializeField] protected bool blockSpawningOnStart;

    [SerializeField] protected bool canSpawnEliteCreatures;
    [SerializeField] protected float eliteSpawnProbability;
    [SerializeField] protected int eliteSpawnAmount;
    protected int eliteSpawnedAmount;

    protected List<Mob> mobSpawnedList = new List<Mob>();

    public event EventHandler<OnMobSpawnedEventArgs> OnMobSpawned;
    public event EventHandler<OnMobSpawnedEventArgs> OnMobRemoved;

    protected bool mobsCanSpawnAtDawn = true;

    public class OnMobSpawnedEventArgs : EventArgs {
        public Mob mob;
    }

    protected virtual void Start() {
        if(sceneViewSpawnerSpriteRenderer != null) {
            sceneViewSpawnerSpriteRenderer.enabled = false;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        if (blockSpawningOnStart) return;
        SpawnMobs(mobAmountToSpawn);
    }

    public virtual void RemoveMobFromMobSpawnedList(Mob mob) {
        mobSpawnedList.Remove(mob);
        OnMobRemoved?.Invoke(this, new OnMobSpawnedEventArgs {
            mob = mob,
        });
    }

    protected void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (!mobsCanSpawnAtDawn) return;
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
        for (int i = 0; i < mobAmount; i++) {
            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPosition.position;
            spawnPositionRandomized.x += positionRandomizer;

            Mob mob = Instantiate(mobPrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if(isCreatureSpawner) {
                mob.GetComponent<Creature>().SetAsDayCreature(true);
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
    
    public IEnumerator SpawnMobsCoroutine(float delayBetweenMobs) {
        for (int i = 0; i < mobAmountToSpawn; i++) {
            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPosition.position;
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

    public void SetMobsCanSpawnAtDawn(bool canSpawn) {
        mobsCanSpawnAtDawn = canSpawn;
    }
}
