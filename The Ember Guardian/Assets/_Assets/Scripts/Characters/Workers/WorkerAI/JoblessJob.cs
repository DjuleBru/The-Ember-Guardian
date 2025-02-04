using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoblessJob : WorkerJob, IJobBehavior {
    private bool blockedByCreatures;
    public event EventHandler OnJoblessBlockedByCreatures;
    public event EventHandler OnJoblessNotBlockedByCreatures;

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
    }

    private void Update() {

        if (CheckBlockedByCreature()) {
            mobMovement.SetMoveTarget(mobMovement.transform.position);
            return;
        }

        if (isInSafeZone) {
            RoamInCampCenter();
        }
        else {
            HeadToCampCenter();
        }
    }

    private bool CheckBlockedByCreature() {
        if (workerDetectionCollider.CreaturesInDetectionCollider()) {
            Vector3 creaturePosition = workerDetectionCollider.GetClosestCreature().transform.position;

            if(Mathf.Abs(creaturePosition.x) > Mathf.Abs(transform.position.x)) {
                // Creature is not in the way
                if (blockedByCreatures) {
                    blockedByCreatures = false;
                    OnJoblessNotBlockedByCreatures?.Invoke(this, EventArgs.Empty);
                }
                return false;
            }

            if (!blockedByCreatures) {
                blockedByCreatures = true;
                OnJoblessBlockedByCreatures?.Invoke(this, EventArgs.Empty);
            }
            return true;
        }
       
        if(!workerDetectionCollider.CreaturesInDetectionCollider()) {
            if(blockedByCreatures) {
                blockedByCreatures = false;
                OnJoblessNotBlockedByCreatures?.Invoke(this, EventArgs.Empty);
            }
        }

        return false;
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {
        if(!isInSafeZone && hasSetCampDestination) {
            isInSafeZone = true;
        }
    }

    private void OnEnable() {
        mobMovement = GetComponent<MobMovement>();
        mobMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
    }

    private void OnDisable() {
        hasSetSpeed = false;
        mobMovement.OnDestinationReached -= WorkerMovement_OnDestinationReached;
    }
}
