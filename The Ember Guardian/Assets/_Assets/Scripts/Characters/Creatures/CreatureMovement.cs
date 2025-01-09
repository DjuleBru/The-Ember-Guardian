using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MobMovement
{
    protected Creature creature;
    protected float enteredLightSpeedDebuff = 2f;
    protected bool enteredLight;
    protected bool aggroMoveSpeedSet;
    protected float aggroMoveSpeedBuff = 1.5f;

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

    protected void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        enteredLight = true;
        DebuffMoveSpeed(enteredLightSpeedDebuff);
    }

    protected void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        enteredLight = false;
        BuffMoveSpeed(enteredLightSpeedDebuff);
    }
}
