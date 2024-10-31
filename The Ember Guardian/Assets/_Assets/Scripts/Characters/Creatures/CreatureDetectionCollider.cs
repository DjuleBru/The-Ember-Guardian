using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDetectionCollider : MonoBehaviour
{
    private CreatureAI creatureAI;
    private List<IDamageable> iDamageablesInDetectionRange = new List<IDamageable>();

    private void Awake() {
        creatureAI = GetComponentInParent<CreatureAI>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        // Player
        Player player = other.GetComponent<Player>(); 
        if (player != null) {
            Debug.Log("player collide entered");
            AddIDamageableInDetectionRange(player);
        }

        // Barricade
        Barricade barricade = other.GetComponent<Barricade>();
        if (barricade != null) {
            Debug.Log("barricade collide entered");
            AddIDamageableInDetectionRange(barricade);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null) {
            if(worker.GetRecruited()) {
                Debug.Log("worker collide entered");
                AddIDamageableInDetectionRange(worker);
            }
        }

    }

    void OnTriggerExit2D(Collider2D other) {

        // Player
        Player player = other.GetComponent<Player>();
        if (player != null) {
            RemoveIDamageableInDetectionRange(player);
        }

        // Barricade
        Barricade barricade = other.GetComponent<Barricade>();
        if (barricade != null) {
            RemoveIDamageableInDetectionRange(barricade);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null) {
            RemoveIDamageableInDetectionRange(worker);
        }
    }

    private void AddIDamageableInDetectionRange(IDamageable iDamageableAdded) {
        iDamageablesInDetectionRange.Add(iDamageableAdded);
        SetHighestPriorityTarget();

    }

    private void RemoveIDamageableInDetectionRange(IDamageable iDamageable) {

        iDamageablesInDetectionRange.Remove(iDamageable);

        if (iDamageablesInDetectionRange.Count == 0) {
            creatureAI.ResetAttackTargetInProximity();
        }
        else {
            SetHighestPriorityTarget();
        }
    }

    private void SetHighestPriorityTarget() {

        IDamageable highestPriorityTarget = iDamageablesInDetectionRange[0];
        int highestPriority = int.MaxValue; // Initialise à une valeur élevée

        foreach (IDamageable iDamageable in iDamageablesInDetectionRange) {
            int currentPriority = int.MaxValue;

            if (iDamageable is Worker) {
                currentPriority = 1;
            }

            if (iDamageable is Barricade) {
                currentPriority = 2;
            }

            if (iDamageable is Player) {
                currentPriority = 3;
            }

            // Si la priorité actuelle est plus haute, on la met à jour
            if (currentPriority < highestPriority) {
                highestPriorityTarget = iDamageable;
                highestPriority = currentPriority;
            }

            creatureAI.SetAttackTarget(highestPriorityTarget);

        }
    }

}
