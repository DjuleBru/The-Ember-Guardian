using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWokerSpawner : MobSpawner
{

    [SerializeField] protected WorkerAI.JobTypes jobType;
    [SerializeField] protected CampZoneManager.CampSide campSide;

    protected override void Start() {
        StartCoroutine(SpawnWorkersCoroutine(mobAmountToSpawn));
    }

    public IEnumerator SpawnWorkersCoroutine(int mobAmount) {
        for (int i = 0; i < mobAmount; i++) {

            float xPositionRandomized = transform.position.x + UnityEngine.Random.Range(-spawnPositionRandomizer, spawnPositionRandomizer);
            Vector3 positionRandomized = new Vector3(xPositionRandomized, transform.position.y, 0);

            Worker worker = Instantiate(mobPrefab, positionRandomized, Quaternion.identity).GetComponent<Worker>();


            worker.SetMobSpawner(this);

            worker.transform.parent = SpawnedObjects.Instance.workersContainer;
            worker.RecruitWorker(false);
            worker.GetComponent<WorkerAI>().SetDebugSpawn();
            WorkerManager.Instance.AssignSideToHunter(worker, campSide);
            worker.GetComponent<WorkerAI>().SetJob(jobType);

            yield return new WaitForSeconds(.5f);
        }
    }
}
