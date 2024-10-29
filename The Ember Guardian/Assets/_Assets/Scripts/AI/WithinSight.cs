using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;

public class WithinSight : Conditional
{
    public enum TargetType {
        animal,
        mob,

    }

    public TargetType targetType;
    public float maxAnimalTargetingDistanceToCampOuterPoint = 20f;
    public SharedTransform target;

    public override TaskStatus OnUpdate() {

        if(targetType == TargetType.animal) {

            Animal closestHuntableAnimal = AnimalManager.Instance.GetClosestAnimalInRadius(transform.position, maxAnimalTargetingDistanceToCampOuterPoint);
            if(closestHuntableAnimal != null ) {
                target.Value = closestHuntableAnimal.transform;
                return TaskStatus.Success;
            }

        }

        return TaskStatus.Failure;
    }
}
