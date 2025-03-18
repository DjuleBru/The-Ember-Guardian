using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCreatureSpawner : MobSpawner
{
    protected override void Start() {
        if (sceneViewSpawnerSpriteRenderer != null) {
            sceneViewSpawnerSpriteRenderer.enabled = false;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        if (blockSpawningOnStart) return;
    }

    public void InitializeDayCreatureSpawner(CreatureSO creatureSO, int amountToSpawn, int eliteSpawnAmount, float radiusToRoamAround) {
        this.radiusToRoamAround = radiusToRoamAround;
        mobPrefab = creatureSO.creaturePrefab.transform;
        mobAmountToSpawn = amountToSpawn;
        this.eliteSpawnAmount = eliteSpawnAmount;

        SpawnCreatures(creatureSO, mobAmountToSpawn);
    }
}
