using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSpawner : MobSpawner
{

    [SerializeField] protected WorkerAI.JobTypes jobType;

    public override void SpawnMobs(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {

            float positionRandomizer = UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 spawnPositionRandomized = spawnPosition.position;
            spawnPositionRandomized.x += positionRandomizer;

            Worker worker = Instantiate(mobPrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<Worker>();
            mobSpawnedList.Add(worker);
            worker.SetMobSpawner(this);
            worker.SetWildJobType(jobType);

            worker.transform.parent = SpawnedObjects.Instance.workersContainer;
            InvokeOnMobSpawned(worker);
        }
    }
}
