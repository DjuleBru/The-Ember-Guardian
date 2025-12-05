using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogWorkerDetectionCollider : MonoBehaviour
{
    private List<Worker> workersInTriggerArea = new List<Worker>();
    private List<SpecialTower_Manner> mannersBuffed = new List<SpecialTower_Manner>();
    private CircleCollider2D circleCollider;

    private bool retreiverSelected;
    private bool buffWorkersUnlocked;

    private void Start() {
        circleCollider = GetComponent<CircleCollider2D>();

        retreiverSelected = Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver;
        buffWorkersUnlocked = DogStats.Instance.GetRetreiverBuffWorkersAbilityUnlocked();
        circleCollider.radius = DogStats.Instance.GetRetreiverBuffWorkersRadius();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!retreiverSelected || !buffWorkersUnlocked) return;

        Worker worker = collision.GetComponent<Worker>();

        if (worker != null && !workersInTriggerArea.Contains(worker)) {

            workersInTriggerArea.Add(worker);
            worker.GetComponent<WorkerAttack>().BuffAttackSpeed(DogStats.Instance.GetRetreiverBuffWorkersAmount() / 100f);
            circleCollider.radius = DogStats.Instance.GetRetreiverBuffWorkersRadius();
        }


        SpecialTower_Manner manner = collision.GetComponent<SpecialTower_Manner>();
        if (manner != null && !mannersBuffed.Contains(manner)) {
            manner.BuffCooldownTime(DogStats.Instance.GetRetreiverBuffWorkersAmount() / 100f);
            mannersBuffed.Add(manner);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!retreiverSelected || !buffWorkersUnlocked) return;

        Worker worker = collision.GetComponent<Worker>();
        if (worker != null && workersInTriggerArea.Contains(worker)) {

            workersInTriggerArea.Remove(worker);
            worker.GetComponent<WorkerAttack>().DebuffAttackSpeed(DogStats.Instance.GetRetreiverBuffWorkersAmount() / 100f);

        }


        SpecialTower_Manner manner = collision.GetComponent<SpecialTower_Manner>();
        if (manner != null && mannersBuffed.Contains(manner)) {
            manner.DebuffCooldownTime(DogStats.Instance.GetRetreiverBuffWorkersAmount() / 100f);
            mannersBuffed.Remove(manner);
        }
    }
}
