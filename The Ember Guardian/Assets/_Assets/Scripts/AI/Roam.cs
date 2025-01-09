using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class Roam : Action
{
    private Mob mob;

    private MobMovement mobMovement;
    private Vector3 positionToRoamAmound;
    private Vector3 destinationPoint;

    private float roamTimer;
    public float roamCooldown;
    public float roamRadius;

    public override void OnAwake() {
        mobMovement = GetComponent<MobMovement>();
        mob = GetComponent<Mob>();
    }

    public override void OnStart() {
        positionToRoamAmound = CampZoneManager.Instance.GetClosestExteriorZoneLimit(transform.position);
        destinationPoint = positionToRoamAmound;

    }

    public override TaskStatus OnUpdate() {
        roamTimer -= Time.deltaTime;

        if(roamTimer < 0) {
            roamTimer = roamCooldown;
            destinationPoint = GetRoamDestinationAroundPoint(mobMovement, roamRadius, positionToRoamAmound);
        }

        //mobMovement.HeadToDestination(destinationPoint);
        return TaskStatus.Running;
    }

    private void DynamicRoam(MobMovement mobMovement, float roamRadius) {

        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {


            float randomRoamPoint = Random.Range(mobMovement.transform.position.x - roamRadius, mobMovement.transform.position.x + roamRadius);


            Vector3 randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }
    }

    private Vector3 GetRoamDestinationAroundPoint(MobMovement mobMovement, float roamRadius, Vector3 point) {

        Vector3 randomMoveTarget = destinationPoint;

        if (Mathf.Abs(mobMovement.transform.position.x - destinationPoint.x) < .1f) {

            float randomRoamPoint = Random.Range(point.x - roamRadius, point.x + roamRadius);

            randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }

        return randomMoveTarget;
    }

    private void RoamInCampCenter(MobMovement mobMovement) {

        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {

            float randomRoamPoint = Random.Range(CampZoneManager.Instance.GetCampCenterMinLimit(), CampZoneManager.Instance.GetCampCenterMaxLimit());

            Vector3 randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }
    }
}
