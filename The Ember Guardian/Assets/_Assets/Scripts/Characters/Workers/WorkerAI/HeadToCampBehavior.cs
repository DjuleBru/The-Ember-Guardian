using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadToCampBehavior : MonoBehaviour
{
   public static void SetDestinationToCampCenter(MobMovement mobMovement) {

        float randomXPositionInCamp = Random.Range(CampZoneManager.Instance.GetCampCenterMinLimit(), CampZoneManager.Instance.GetCampCenterMaxLimit());
        Vector3 targetDestination = new Vector3(randomXPositionInCamp, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);
   }
}
