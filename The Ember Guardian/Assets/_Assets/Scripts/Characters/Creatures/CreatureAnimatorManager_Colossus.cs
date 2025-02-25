using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Colossus : CreatureAnimatorManager
{

    protected override void Awake() {
        base.Awake();
        SetUnReadyToMove();
    }

    protected override void Start() {
        base.Start();
        creatureAI.OnCreatureAggro += CreatureAI_OnCreatureAggro;
    }

    private void CreatureAI_OnCreatureAggro(object sender, System.EventArgs e) {
        animator.SetTrigger("Wake");
    }
}
