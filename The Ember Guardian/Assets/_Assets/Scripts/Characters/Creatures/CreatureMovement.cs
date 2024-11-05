using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MobMovement
{
    private Creature creature;
    private float enteredLightSpeedDebuff = 1.6f;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();

        initialMobSpeed = creature.GetCreatureSO().moveSpeed + Random.Range(-creature.GetCreatureSO().moveSpeedRandomizerDelta, creature.GetCreatureSO().moveSpeedRandomizerDelta);
        moveSpeed = initialMobSpeed;
    }

    protected void Start() {
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        DebuffMoveSpeed(enteredLightSpeedDebuff);
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        BuffMoveSpeed(enteredLightSpeedDebuff);
    }
}
