using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack_Blinding : CreatureAttack
{
    public override void DealDamage() {
        base.DealDamage();
        Debug.Log(attackTargetIDamageable is Player);
        if(attackTargetIDamageable is Player) {
            Player.Instance.BlindPlayer();
        }
    }
}
