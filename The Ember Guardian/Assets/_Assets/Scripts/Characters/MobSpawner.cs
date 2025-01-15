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
    [SerializeField] protected bool isAnimalSpawner;

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

        if ((isAnimalSpawner || isCreatureSpawner) && CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            Debug.Log(this + " Spawner is within camp zone limits ! Spawn cancelled");
            return;
        }

        SpawnMobs(mobAmountToSpawnOnDawn);
    }

    public virtual void SpawnMobs(int mobAmount) {
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

            InvokeOnMobSpawned(mob);
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
