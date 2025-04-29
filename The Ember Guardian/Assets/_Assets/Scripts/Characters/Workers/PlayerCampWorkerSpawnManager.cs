using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCampWorkerSpawnManager : MonoBehaviour
{
    private int initialWorkersSpawned;
    private int emberlingsArrivals;

    private int dayIndex = 0;

    [SerializeField] private DebugWokerSpawner debugWorkerSpawner;

    private void Start() {
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;

        initialWorkersSpawned = WorkerStats.Instance.GetInitialEmberlings();
        emberlingsArrivals = (int)WorkerStats.Instance.GetEmberlingsArrivalsNumber();

        for(int i = 0; i < initialWorkersSpawned; i++) {
            debugWorkerSpawner.SpawnWorker(Tent.Instance.transform.position);
        }
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        dayIndex++;
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if(dayIndex >= 2) {
            dayIndex = 0;
            StartCoroutine(SpawnArrivalEmberlings());
        }
    }
    private IEnumerator SpawnArrivalEmberlings() {
        for(int i = 0; i < emberlingsArrivals; i++) {
            float randomizer = Random.Range(-1f, 1f);
            Vector3 positionRandomized = Vector3.zero;

            if(randomizer <0) {
                positionRandomized.x = CampZoneManager.Instance.GetMinZoneLimit() - 20f;
            } else {
                positionRandomized.x = CampZoneManager.Instance.GetMaxZoneLimit() + 20f;
            }

            debugWorkerSpawner.SpawnWorker(positionRandomized);
            yield return new WaitForSeconds(.75f);
        }
    }
}
