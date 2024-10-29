using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

public class MoveTowards : Action
{
    public SharedTransform target;
    private MobMovement mobMovement;

    public override void OnAwake() {
        mobMovement = GetComponent<MobMovement>();
    }

    public override TaskStatus OnUpdate() {

        if(target == null) {
            return TaskStatus.Failure;
        }

        if(Mathf.Abs(transform.position.x - target.Value.position.x) < .1f) {
            return TaskStatus.Success;
        }

        mobMovement.HeadToDestination(target.Value.transform.position);
        return TaskStatus.Running;
    }

}
