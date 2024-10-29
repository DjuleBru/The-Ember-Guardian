using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoblessJob : MonoBehaviour, IJobBehavior {

    private MobMovement mobMovement;

    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField]  private float headToCampMoveSpeed = 2f;
    [SerializeField]  private float roamChangeDestinationRate = 5f;

    private float roamTimer;

    private bool hasSetSpeed;
    private bool hasSetCampDestination;

    private bool isInSafeZone;

    private void Update() {

        if (isInSafeZone) {
            Roam();
        }
        else {
            HeadToCampCenter();
        }

    }

    public void HeadToCampCenter() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        if (!hasSetCampDestination) {
           HeadToCampBehavior.SetDestinationToCampCenter(mobMovement);
            hasSetCampDestination = true;
            hasSetSpeed = false;
        }
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {
        if(!isInSafeZone && hasSetCampDestination) {
            isInSafeZone = true;
        }
    }

    public void Roam() {

        if(!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamInCampCenter(mobMovement);
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
