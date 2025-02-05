using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerJob : MonoBehaviour
{
    protected Worker worker;
    protected WorkerAI workerAI;
    protected MobMovement mobMovement;
    protected WorkerAnimatorManager workerAnimatorManager;
    protected MobAttack workerAttack;
    protected Animal targetAnimal;
    protected Creature targetCreature;
    protected WorkerDetectionCollider workerDetectionCollider;


    protected List<Collectible> orbsToCollect = new List<Collectible>();

    protected bool hasSetSpeed;
    protected bool isInSafeZone;
    protected bool hasSetCampDestination;

    protected float headToCampMoveSpeed = 3f;
    protected float roamMoveSpeed = 1.5f;
    protected float roamTimer;
    protected float roamChangeDestinationRate = 6f;

    public void HeadToCampCenter() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        if (!hasSetCampDestination) {

            float randomXPositionInCamp = UnityEngine.Random.Range(CampZoneManager.Instance.GetCampCenterMinLimit(), CampZoneManager.Instance.GetCampCenterMaxLimit());
            Vector3 targetDestination = new Vector3(randomXPositionInCamp, 0, 0);
            mobMovement.SetMoveTarget(targetDestination);

            hasSetCampDestination = true;
            hasSetSpeed = false;
        }
    }

    public void RoamInCampCenter() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamInCampCenter(mobMovement);
        }
    }

    public void Roam(float roamDistance, Vector3 positionToRoamAround) {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(mobMovement, roamDistance, positionToRoamAround, false);
        }
    }

    protected bool CheckDropCurrenciesToPlayer() {
        if (worker.GetPlayerIsClose() && worker.GetTotalCurrencyAmount() > 0) {
            return true;
        }
        return false;
    }

    protected bool CheckOrbsToCollect() {
        if (orbsToCollect.Count > 0) {
            return true;
        }
        return false;
    }

    public virtual void InitializeJob() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();

        mobMovement = GetComponentInChildren<MobMovement>();
        workerAnimatorManager = GetComponentInChildren<WorkerAnimatorManager>();
        workerAttack = GetComponent<MobAttack>();
        worker = GetComponent<Worker>();
        workerAI = GetComponent<WorkerAI>();

        mobMovement.SetMoveTarget(mobMovement.transform.position);
    }
}
