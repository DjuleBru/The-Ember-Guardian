using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sceneViewSpawnerSpriteRenderer;
    [SerializeField] private Transform mobPrefab;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private int mobAmountToSpawn;

    private List<Mob> mobSpawnedList = new List<Mob>();

    private void Start() {
        sceneViewSpawnerSpriteRenderer.enabled = false;
        SpawnMobs(mobAmountToSpawn);
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    public void RemoveMobFromMobSpawnedList(Worker worker) {
        mobSpawnedList.Remove(worker);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        int workerAmountToSpawnOnDawn = mobAmountToSpawn - mobSpawnedList.Count;

        SpawnMobs(workerAmountToSpawnOnDawn);
    }

    private void SpawnMobs(int workerAmount) {
        for (int i = 0; i < workerAmount; i++) {
            Mob mob = Instantiate(mobPrefab, spawnPosition.position, Quaternion.identity).GetComponent<Mob>();
            mobSpawnedList.Add(mob);
            mob.SetMobSpawner(this);
        }
    }
}
