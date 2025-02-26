using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap_CreatureTriggerCollider : MonoBehaviour
{
    private Structure_Trap trap;

    private void Start() {
        trap = GetComponentInParent<Structure_Trap>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Creature>() != null) {
            trap.TryTriggerTrap();
        }
    }
}
