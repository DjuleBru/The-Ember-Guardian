using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeBehavior : MonoBehaviour
{
    public static Vector3 GetFleeFromTargetDestination(MobMovement mobMovement, Vector3 positionToFleeFrom, float distanceToFlee) {

        float directionToFlee = positionToFleeFrom.x - mobMovement.transform.position.x;


        if(directionToFlee > 0) {
            directionToFlee = 1f;
        } else {
            directionToFlee = -1f;
        }

        float targetXFleePosition = directionToFlee * distanceToFlee;
        Vector3 targetDestination = new Vector3(mobMovement.transform.position.x - targetXFleePosition, 0, 0);

        return targetDestination;
    }
}
