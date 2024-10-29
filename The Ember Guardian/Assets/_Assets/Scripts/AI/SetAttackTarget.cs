using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class SetAttackTarget : Action
{

    private MobAttack mobAttack;
    public SharedTransform target;
    public SharedBool attackTargetSet;

    public override void OnAwake() {
        mobAttack = GetComponent<MobAttack>();
    }

    public override TaskStatus OnUpdate() {

        if(!attackTargetSet.Value) {
            mobAttack.SetAttackTargetTransform(target.Value);
            attackTargetSet.Value = true;
        }

        return TaskStatus.Success;
    }

}
