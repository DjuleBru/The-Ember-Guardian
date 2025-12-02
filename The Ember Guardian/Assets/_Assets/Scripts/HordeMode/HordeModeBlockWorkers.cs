using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeBlockWorkers : MonoBehaviour
{
    [SerializeField] private Transform workerSpawner_VG;
    [SerializeField] private Transform workerSpawner_CC;
    [SerializeField] private Transform workerSpawner_LH;
    [SerializeField] private Transform workerSpawner_FD;

    [SerializeField] private Transform workerSpawnerSpawnPos;

    private void Start() {
        workerSpawnerSpawnPos.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void SetBlockAsWorkerSpawner() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        Transform workerSpawner = workerSpawner_VG;
        if(env == LevelSO.LevelEnvironment.CorruptedCity) {
            workerSpawner = workerSpawner_CC;
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            workerSpawner = workerSpawner_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            workerSpawner = workerSpawner_FD;
        }

        WorkerSpawner spawner = Instantiate(workerSpawner, workerSpawnerSpawnPos.position, Quaternion.identity, workerSpawnerSpawnPos).GetComponent<WorkerSpawner>();
    }
}
