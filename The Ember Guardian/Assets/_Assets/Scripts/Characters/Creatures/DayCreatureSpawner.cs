using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCreatureSpawner : MobSpawner
{
    private CreatureSO creatureSO;

    protected override void Start() {
        if (sceneViewSpawnerSpriteRenderer != null) {
            sceneViewSpawnerSpriteRenderer.enabled = false;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        if (blockSpawningOnStart) return;
    }

    public void InitializeDayCreatureSpawner(CreatureSO creatureSO, int amountToSpawn, int eliteSpawnAmount, float radiusToRoamAround) {
        this.creatureSO = creatureSO;
        this.radiusToRoamAround = radiusToRoamAround;
        mobPrefab = creatureSO.creaturePrefab.transform;
        mobAmountToSpawn = amountToSpawn;
        this.eliteSpawnAmount = eliteSpawnAmount;

        SpawnCreatures(creatureSO, mobAmountToSpawn);
    }

    public CreatureSO GetCreatureSO() {
        Debug.Log("GetCreatureSO " + creatureSO);
        return creatureSO;
    }

    public int GetEliteMobsAlive() {
        int elites = 0;

        foreach(Mob mob in mobSpawnedList) {
            Creature creature = mob as Creature;
            if (creature.GetIsEliteCreature()) elites++;
        }
        return elites;
    }
}
