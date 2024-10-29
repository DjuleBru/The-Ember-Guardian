using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

public class WithinRange : Conditional {

    private MobAttack mobAttack;

    public float targetingRange;
    public SharedTransform target;
    public SharedBool attackTargetSet;

    public override void OnAwake() {
        mobAttack = GetComponent<MobAttack>();
    }

    public override TaskStatus OnUpdate() {


        if (Mathf.Abs(target.Value.transform.position.x - transform.position.x) < targetingRange) {

            return TaskStatus.Success;

        } else {
            // Remove attack target if out of range

            if(attackTargetSet.Value) {
                mobAttack.RemoveAttackTarget();
                attackTargetSet.Value = false;
            }
        }

        return TaskStatus.Failure;
    }
}
