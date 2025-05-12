using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap_EffectCollider : MonoBehaviour
{
    [SerializeField] private Structure_Trap trap;

    private int creatureTouchedIndex;

    private void Awake() {
        trap.OnTrapTriggered += Trap_OnTrapTriggered;
        trap.OnTrapActiveEnded += Trap_OnTrapActiveEnded;
    }

    private void Trap_OnTrapActiveEnded(object sender, System.EventArgs e) {

    }

    private void Trap_OnTrapTriggered(object sender, System.EventArgs e) {
        creatureTouchedIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature == null) return;
        if(!trap.GetTrapSO().trapHasAOEAttack) {
            if (creatureTouchedIndex > 0) return;
        }

        ApplyTrapEffect(creature);
        creatureTouchedIndex++;
    }

    private void ApplyTrapEffect(Creature creature) {
        creature.TakeDamage(trap.GetTrapSO().trapDamage, transform);

        if(trap.GetTrapSO().trapType == TrapItem.TrapType.bearTrap) {
            creature.ApplyImmobilizeEffect(trap.GetTrapSO().trapSpecialStat, transform.position);
        }

        if (trap.GetTrapSO().trapType == TrapItem.TrapType.smokeEjector) {
            creature.ApplyPoisonEffect(trap.GetTrapSO().trapSpecialStat);
        }

        if (trap.GetTrapSO().trapType == TrapItem.TrapType.shockerEjector) {
            creature.ApplyShockedEffect(trap.GetTrapSO().trapSpecialStat);
        }
    }
}
