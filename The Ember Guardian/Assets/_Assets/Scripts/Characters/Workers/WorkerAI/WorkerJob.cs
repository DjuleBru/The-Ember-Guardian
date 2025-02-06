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

    protected Creature closestCreature;
    protected Creature targetCreature;
    protected Creature aggroedCreature;

    protected WorkerDetectionCollider workerDetectionCollider;


    protected List<Collectible> orbsToCollect = new List<Collectible>();

    protected bool hasSetSpeed;
    protected bool isInSafeZone;
    protected bool hasSetCampDestination;

    protected float headToCampMoveSpeed = 3f;
    protected float roamMoveSpeed = 1.5f;
    protected float fleeMoveSpeed = 3.5f;
    protected float roamTimer;
    protected float roamChangeDestinationRate = 6f;
    protected float minimumDistanceToStaySafeFromCreature = 6f;
    protected float distanceToStaySafeFromCreature;
    protected float maxDistanceToPlayerWhenFollowing = 10f;
    protected float minDistanceToPlayerWhenFollowing = 5f;

    protected float checkClosestTargetTimer;
    protected float checkClosestTargetCooldown = .3f;

    protected void TargetCreature(Creature newTargetCreature) {
        if (targetCreature == newTargetCreature) return;

        if (targetCreature != null) {
            targetCreature.OnMobDied -= TargetCreature_OnMobDied;
        }

        mobMovement.SetMoveTarget(transform.position);
        workerAttack.SetAttackTarget(newTargetCreature);
        targetCreature = newTargetCreature;
        targetCreature.OnMobDied += TargetCreature_OnMobDied;
    }

    protected void TargetCreature_OnMobDied(object sender, System.EventArgs e) {
        targetCreature = null;
        workerAttack.RemoveAttackTarget();
    }

    protected bool TargetIsInRange(IDamageable iDamageable, float range) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < (range)) {
            return true;
        }
        else {
            return false;
        }
    }

    protected bool PlayerIsTooFarFromWorker() {

        float distanceFromPlayer = Mathf.Abs(Mathf.Abs(Player.Instance.transform.position.x) - Mathf.Abs(transform.position.x));
        return distanceFromPlayer > maxDistanceToPlayerWhenFollowing;

    }

    protected bool PlayerIsCloseEnoughFromWorker() {

        float distanceFromPlayer = Mathf.Abs(Mathf.Abs(Player.Instance.transform.position.x) - Mathf.Abs(transform.position.x));
        return distanceFromPlayer < minDistanceToPlayerWhenFollowing;

    }

    protected bool CheckAggroClosestCreatureSmart(Vector2 positionOrigin, float checkDistance) {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Creature newTargetCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(positionOrigin, checkDistance, workerAttack.GetAttackDamage(), false);

            if (newTargetCreature == null) {
                aggroedCreature = null;
                return false;

            }

            else {
                aggroedCreature = newTargetCreature;
                return true;
            }
        }

        return false;
    }

    public void StayAwayFromCreature(Creature closestCreature) {

        mobMovement.SetMoveSpeed(fleeMoveSpeed);

        if (closestCreature != null) {

            float direction = closestCreature.transform.position.x - transform.position.x;
            if (direction > 0) {
                direction = -1;
            }
            else {
                direction = 1;
            }

            Vector3 safePosition = new Vector3(closestCreature.transform.position.x + direction * distanceToStaySafeFromCreature, 0, 0);
            mobMovement.SetMoveTarget(safePosition);
            workerAttack.RemoveAttackTarget();

            return;
        }

    }

    public bool CreatureIsTooClose(Creature closestCreature) {
        if (closestCreature != null) {
            bool creatureIsTooClose = Mathf.Abs(closestCreature.transform.position.x) - Mathf.Abs(transform.position.x) < minimumDistanceToStaySafeFromCreature;
            return creatureIsTooClose;
        }
        return false;
    }

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
