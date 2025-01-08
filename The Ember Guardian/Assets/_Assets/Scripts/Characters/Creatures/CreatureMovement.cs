using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MobMovement
{
    private Creature creature;
    private float enteredLightSpeedDebuff = 2f;
    private bool enteredLight;
    private bool aggroMoveSpeedSet;
    private float aggroMoveSpeedBuff = 1.5f;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();
        enteredLightSpeedDebuff = creature.GetCreatureSO().enteredLightMoveSpeedDebuff;

        initialMobSpeed = creature.GetCreatureSO().moveSpeed + Random.Range(-creature.GetCreatureSO().moveSpeedRandomizerDelta, creature.GetCreatureSO().moveSpeedRandomizerDelta);
        moveSpeed = initialMobSpeed;
    }

    protected override void Start() {
        base.Start();
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
    }

    public void SetCreatureAggroMoveSpeed(bool aggroMoveSpeed) {
        if (enteredLight) return;
        if(!aggroMoveSpeed && aggroMoveSpeedSet) {
            DebuffMoveSpeed(aggroMoveSpeedBuff);
        }

        if(aggroMoveSpeed && !aggroMoveSpeedSet) {
            BuffMoveSpeed(aggroMoveSpeedBuff);
        }

        aggroMoveSpeedSet = aggroMoveSpeed;
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        enteredLight = true;
        DebuffMoveSpeed(enteredLightSpeedDebuff);
    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        enteredLight = false;
        BuffMoveSpeed(enteredLightSpeedDebuff);
    }
}
