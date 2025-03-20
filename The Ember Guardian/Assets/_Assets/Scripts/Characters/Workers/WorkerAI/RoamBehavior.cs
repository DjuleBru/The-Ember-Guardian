using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public static class RoamBehavior
{

    public static void RoamAroundPoint(MobMovement mobMovement, float roamRadius, Vector3 point, bool flying) {
        Vector3 targetDestination = mobMovement.GetTargetDestination();

        if (Mathf.Abs(mobMovement.transform.position.x - targetDestination.x) < .1f) {

            float randomRoamPointX = Random.Range(point.x - roamRadius, point.x + roamRadius);

            Vector3 randomMoveTarget = new Vector3(randomRoamPointX, 0, 0);

            if (flying) {
                Creature creature = mobMovement.GetComponent<Creature>();
                float minFlightAltitude = creature.GetCreatureSO().flightMinAltitude;
                float maxFlightAltitude = creature.GetCreatureSO().flightMaxAltitude;

                float randomRoamPointY = Random.Range(minFlightAltitude, maxFlightAltitude);

                randomMoveTarget.y = Mathf.Abs(randomRoamPointY);
            }

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
