using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer sceneViewSpawnerSpriteRenderer;
    [SerializeField] protected Transform mobPrefab;
    [SerializeField] protected Transform spawnPosition;
    [SerializeField] protected int mobAmountToSpawn;
    [SerializeField] protected int maxMobsRespawningAtDawn;

    [SerializeField] protected bool isCreatureSpawner;

    protected List<Mob> mobSpawnedList = new List<Mob>();

    public event EventHandler<OnMobSpawnedEventArgs> OnMobSpawned;
    public event EventHandler<OnMobSpawnedEventArgs> OnMobRemoved;

    public class OnMobSpawnedEventArgs : EventArgs {
        public Mob mob;
    }

    protected void Start() {
        sceneViewSpawnerSpriteRenderer.enabled = false;
        SpawnMobs(mobAmountToSpawn);
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    public void RemoveMobFromMobSpawnedList(Mob mob) {
        mobSpawnedList.Remove(mob);
        OnMobRemoved?.Invoke(this, new OnMobSpawnedEventArgs {
            mob = mob,
        });
    }

    protected void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        int mobAmountToSpawnOnDawn = mobAmountToSpawn - mobSpawnedList.Count;

        if(mobAmountToSpawnOnDawn > maxMobsRespawningAtDawn) {
            mobAmountToSpawnOnDawn = maxMobsRespawningAtDawn;
        }

        SpawnMobs(mobAmountToSpawnOnDawn);
    }

    protected void SpawnMobs(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {
            Mob mob = Instantiate(mobPrefab, spawnPosition.position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);

            if(isCreatureSpawner) {
                mob.GetComponent<Creature>().SetAsDayCreature(true);
                mob.transform.parent = SpawnedObjects.Instance.creaturesContainer;
            }

            if(mob is Worker) {
                mob.transform.parent = SpawnedObjects.Instance.workersContainer;
            }

            if (mob is Animal) {
                mob.transform.parent = SpawnedObjects.Instance.AnimalsContainer;
            }

            OnMobSpawned?.Invoke(this, new OnMobSpawnedEventArgs {
                mob = mob,
            });
        }
    }
}
