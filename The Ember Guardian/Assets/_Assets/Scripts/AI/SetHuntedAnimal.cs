using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHuntedAnimal : Action
{
    public SharedTransform target;

    private HunterJob hunterJob;
    private Animal targetAnimal;

    public override void OnAwake() {
        hunterJob = GetComponent<HunterJob>();
    }

    public override TaskStatus OnUpdate() {

        if (target != null) {
            targetAnimal = target.Value.GetComponent<Animal>();
        }

        hunterJob.TargetAnimal(targetAnimal);

        return TaskStatus.Success;
    }

}
