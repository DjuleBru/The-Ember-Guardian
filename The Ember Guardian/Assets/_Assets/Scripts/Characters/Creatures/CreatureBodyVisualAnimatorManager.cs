using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBodyVisualAnimatorManager : BodyVisualAnimatorManager
{
    private Creature creature;

    protected override void Start() {
        base.Start();

        creature = mob as Creature;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;


        if (!creature.GetCreatureSO().hasCustomSpawnAnimation) {
            animator.SetTrigger("Spawn");
        }
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        animator.SetBool("EnteredLight", false);
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        animator.SetTrigger("EnteringLight");
        animator.SetBool("EnteredLight", true);
    }
}
