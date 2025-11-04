using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Colossus : CreatureAnimatorManager
{
    private CreatureAI_Colossus colossus;
    protected override void Awake() {
        base.Awake();
        colossus = creatureAI.GetComponent<CreatureAI_Colossus>();
        colossus.OnColossusWake += Colossus_OnColossusWake;
        animator.enabled = false;
    }

    private void Colossus_OnColossusWake(object sender, System.EventArgs e) {
        animator.enabled = true;
        animator.SetTrigger("Spawn");
    }
}
