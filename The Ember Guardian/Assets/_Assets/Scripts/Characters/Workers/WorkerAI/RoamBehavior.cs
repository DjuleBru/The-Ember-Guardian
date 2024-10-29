using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoamBehavior
{

    public static void Roam(MobMovement mobMovement, float roamRadius) {

        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {


            float randomRoamPoint = Random.Range(mobMovement.transform.position.x - roamRadius, mobMovement.transform.position.x + roamRadius);


            Vector3 randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }
    }

    public static void RoamAroundPoint(MobMovement mobMovement, float roamRadius, Vector3 point) {

        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {


            float randomRoamPoint = Random.Range(point.x - roamRadius, point.x + roamRadius);

            Vector3 randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }
    }

    public static void RoamInCampCenter(MobMovement mobMovement) {

        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {

            float randomRoamPoint = Random.Range(CampZoneManager.Instance.GetCampCenterMinLimit(), CampZoneManager.Instance.GetCampCenterMaxLimit());

            Vector3 randomMoveTarget = new Vector3(randomRoamPoint, 0, 0);
            mobMovement.SetMoveTarget(randomMoveTarget);
        }
    }
}
