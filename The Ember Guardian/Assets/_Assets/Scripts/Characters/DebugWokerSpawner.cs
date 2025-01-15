using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWokerSpawner : MobSpawner
{

    [SerializeField] protected WorkerAI.JobTypes jobType;

    protected override void Start() {
        SpawnWorkers(mobAmountToSpawn);
    }

    public virtual void SpawnWorkers(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {

            Worker worker = Instantiate(mobPrefab, transform.position, Quaternion.identity).GetComponent<Worker>();
            worker.SetMobSpawner(this);

            worker.transform.parent = SpawnedObjects.Instance.workersContainer;
            worker.RecruitWorker();
            worker.GetComponent<WorkerAI>().SetDebugSpawn();
            worker.GetComponent<WorkerAI>().SetJob(jobType);
        }
    }
}
