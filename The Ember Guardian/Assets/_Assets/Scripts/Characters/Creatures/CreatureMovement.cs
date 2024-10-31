using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MobMovement
{
    private Creature creature;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();
    }

}
