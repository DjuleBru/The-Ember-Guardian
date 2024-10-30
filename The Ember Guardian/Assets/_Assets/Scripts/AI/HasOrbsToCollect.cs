using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasOrbsToCollect : Conditional {

    private HunterJob hunterJob;
    public SharedTransform target;

    public override void OnAwake() {
        hunterJob = GetComponent<HunterJob>();
    }

    public override TaskStatus OnUpdate() {
        if(hunterJob.GetOrbsToCollectList().Count == 0) {
            return TaskStatus.Failure;
        } else {

            Collectible orbToCollect = hunterJob.GetOrbsToCollectList()[0];
            target.Value = orbToCollect.transform;

            return TaskStatus.Success;
        }
    }
}
