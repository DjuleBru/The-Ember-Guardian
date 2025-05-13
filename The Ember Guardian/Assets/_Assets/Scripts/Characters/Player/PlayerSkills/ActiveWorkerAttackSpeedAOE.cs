using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWorkerAttackSpeedAOE : MonoBehaviour
{

    private List<Worker> workersBuffed = new List<Worker>();

    private void OnTriggerEnter2D(Collider2D collision) {
        Worker worker = collision.GetComponent<Worker>();
        WorkerDetectionCollider workerDetectionCollider = collision.GetComponent<WorkerDetectionCollider>();
        if (workerDetectionCollider != null) return;
        if (worker != null && !workersBuffed.Contains(worker)) {
            worker.GetComponent<WorkerAttack>().BuffAttackSpeed(PlayerSkills.Instance.GetWorkerAttackSpeedBuffAmount() / 100f);
            workersBuffed.Add(worker);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Worker worker = collision.GetComponent<Worker>();
        WorkerDetectionCollider workerDetectionCollider = collision.GetComponent<WorkerDetectionCollider>();
        if (workerDetectionCollider != null) return;

        if (worker != null && workersBuffed.Contains(worker)) {
            worker.GetComponent<WorkerAttack>().DebuffAttackSpeed(PlayerSkills.Instance.GetWorkerAttackSpeedBuffAmount() / 100f);
            workersBuffed.Remove(worker);
        }
    }


}
