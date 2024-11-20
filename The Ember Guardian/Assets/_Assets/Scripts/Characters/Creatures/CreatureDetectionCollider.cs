using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDetectionCollider : MonoBehaviour
{
    private Creature creature;
    private CreatureAI creatureAI;
    private CreatureAttack creatureAttack;
    private List<IDamageable> iDamageablesInDetectionRange = new List<IDamageable>();

    private float refreshTargetTimer;
    private float refreshTargetcooldown = .25f;

    private void Awake() {
        creature = GetComponentInParent<Creature>();
        creatureAI = GetComponentInParent<CreatureAI>();
        creatureAttack = GetComponentInParent<CreatureAttack>();
    }

    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        if(iDamageablesInDetectionRange.Contains(Player.Instance)) {
            RemoveIDamageableInDetectionRange(Player.Instance);
        }
    }

    private void Update() {
        refreshTargetTimer -= Time.deltaTime;
        if(refreshTargetTimer < 0) {
            refreshTargetTimer = refreshTargetcooldown;
            RefreshHighestPriorityTarget();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {

        // Player
        Player player = other.GetComponent<Player>(); 

        if (player != null) {
            AddIDamageableInDetectionRange(player);
        }

        // Barricade
        Barricade barricade = other.GetComponent<Barricade>();
        if (barricade != null) {
            barricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
            AddIDamageableInDetectionRange(barricade);
        }

        // Fire
        Fire fire = other.GetComponent<Fire>();
        if (fire != null) {
            AddIDamageableInDetectionRange(fire);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null) {
            if(worker.GetRecruited() && !(worker.GetStructureAssigned() is Tower)) {
                worker.OnMobDied += Worker_OnMobDied;
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
            barricade.OnBarricadeDestroyed -= Barricade_OnBarricadeDestroyed;
            RemoveIDamageableInDetectionRange(barricade);
        }

        // Fire
        Fire fire = other.GetComponent<Fire>();
        if (fire != null) {
            RemoveIDamageableInDetectionRange(fire);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null) {
            worker.OnMobDied -= Worker_OnMobDied;
            RemoveIDamageableInDetectionRange(worker);
        }

    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        RemoveIDamageableInDetectionRange(sender as Worker);
    }

    private void Barricade_OnBarricadeDestroyed(object sender, System.EventArgs e) {
        RemoveIDamageableInDetectionRange(sender as Barricade);
    }

    private void AddIDamageableInDetectionRange(IDamageable iDamageableAdded) {
        if (iDamageablesInDetectionRange.Contains(iDamageableAdded)) return;

        iDamageablesInDetectionRange.Add(iDamageableAdded);
    }

    private void RemoveIDamageableInDetectionRange(IDamageable iDamageable) {
        iDamageablesInDetectionRange.Remove(iDamageable);

        if (iDamageablesInDetectionRange.Count == 0) {
            creatureAI.ResetAttackTargetInProximity();
        }
    }

    private void RefreshHighestPriorityTarget() {

        if (iDamageablesInDetectionRange.Count == 0) return;


        IDamageable highestPriorityTarget = null;
        int highestPriority = int.MaxValue; // Initialise à une valeur élevée

        foreach (IDamageable iDamageable in iDamageablesInDetectionRange) {
            int currentPriority = int.MaxValue;

            if (iDamageable is Worker) {

                Worker worker = (Worker)iDamageable;
                // Check if worker is out of camp
                if (!CampZoneManager.Instance.IsWithinCampZoneLimits(worker.transform.position)) {
                    currentPriority = creature.GetCreatureSO().workerTargetingPriority;
                }

            }

            if (iDamageable is Barricade) {
                Barricade barricade = (Barricade)iDamageable;
                if(barricade.GetBarricadeHealthNormalized() > 0) {
                    currentPriority = creature.GetCreatureSO().barricadeTargetingPriority;
                }
            }

            if (iDamageable is Fire) {
                currentPriority = 0;
            }

            if (iDamageable is Player) {

                // Check if player is in range in the y axis !
                if ((Player.Instance.transform.position.y - 1.5f > creature.GetCreatureSO().attackRange) && !creatureAttack.GetIsRangedAttack()) {
                    continue;
                }

                // Check if player is out of camp
                if (!CampZoneManager.Instance.IsWithinCampZoneLimits(Player.Instance.transform.position)) {
                     currentPriority = creature.GetCreatureSO().playerTargetingPriority;
                }
            }

            // Si la priorité actuelle est plus haute, on la met à jour
            if (currentPriority < highestPriority) {
                highestPriorityTarget = iDamageable;
                highestPriority = currentPriority;
            }

        }

        creatureAI.SetAttackTarget(highestPriorityTarget, iDamageablesInDetectionRange);
    }

}
