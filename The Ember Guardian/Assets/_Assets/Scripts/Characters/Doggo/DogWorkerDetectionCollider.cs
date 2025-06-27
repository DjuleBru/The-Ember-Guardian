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
    private float buffWorkersAmount;
    private float buffWorkersRadius;

    private void Start() {
        circleCollider = GetComponent<CircleCollider2D>();

        retreiverSelected = Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver;
        buffWorkersUnlocked = DogStats.Instance.GetRetreiverBuffWorkersAbilityUnlocked();
        buffWorkersAmount = DogStats.Instance.GetRetreiverBuffWorkersAmount()/100f;
        buffWorkersRadius = DogStats.Instance.GetRetreiverBuffWorkersRadius();

        circleCollider.radius = buffWorkersRadius;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!retreiverSelected || !buffWorkersUnlocked) return;

        Worker worker = collision.GetComponent<Worker>();

        if (worker != null && !workersInTriggerArea.Contains(worker)) {

            workersInTriggerArea.Add(worker);
            worker.GetComponent<WorkerAttack>().BuffAttackSpeed(buffWorkersAmount);
        }


        SpecialTower_Manner manner = collision.GetComponent<SpecialTower_Manner>();
        if (manner != null && !mannersBuffed.Contains(manner)) {
            manner.BuffCooldownTime(buffWorkersAmount);
            mannersBuffed.Add(manner);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!retreiverSelected || !buffWorkersUnlocked) return;

        Worker worker = collision.GetComponent<Worker>();
        if (worker != null && workersInTriggerArea.Contains(worker)) {

            workersInTriggerArea.Remove(worker);
            worker.GetComponent<WorkerAttack>().DebuffAttackSpeed(buffWorkersAmount);

        }


        SpecialTower_Manner manner = collision.GetComponent<SpecialTower_Manner>();
        if (manner != null && mannersBuffed.Contains(manner)) {
            manner.DebuffCooldownTime(buffWorkersAmount);
            mannersBuffed.Remove(manner);
        }
    }
}
