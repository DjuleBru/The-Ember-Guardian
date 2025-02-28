using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MobMovement
{
    protected Creature creature;
    protected float enteredLightSpeedDebuff = 2f;
    protected bool enteredLight;
    protected bool aggroMoveSpeedSet;
    protected bool spawned;
    protected bool immobilized;
    protected float aggroMoveSpeedBuff = 1.5f;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();
        enteredLightSpeedDebuff = creature.GetCreatureSO().enteredLightMoveSpeedDebuff;
    }

    protected override void Start() {
        base.Start();
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureImmobilizedStarted += Creature_OnCreatureImmobilizedStarted;
        creature.OnCreatureImmobilizedStopped += Creature_OnCreatureImmobilizedStopped;
        creature.OnCreatureShockedStarted += Creature_OnCreatureShockedStarted;
        creature.OnCreatureShockedStopped += Creature_OnCreatureShockedStopped;
        InitializeCreatureMoveSpeed();

    }


    protected override void FixedUpdate() {
        if (!spawned) return;
        if (immobilized) return;
        base.FixedUpdate();
    }

    private void InitializeCreatureMoveSpeed() {

        if (creature.IsDayCreature()) {
            initialMobSpeed = creature.GetCreatureSO().dayMoveSpeed;
        }
        else {
            initialMobSpeed = creature.GetCreatureSO().nightMoveSpeed;
        }

        if (creature.GetIsEliteSpeedCreature()) {
            initialMobSpeed *= 1.5f;
        }

        initialMobSpeed = initialMobSpeed + Random.Range(-creature.GetCreatureSO().moveSpeedRandomizerDelta, creature.GetCreatureSO().moveSpeedRandomizerDelta);

        moveSpeed = initialMobSpeed;
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

    private void Creature_OnCreatureImmobilizedStopped(object sender, System.EventArgs e) {
        immobilized = false;
    }

    private void Creature_OnCreatureImmobilizedStarted(object sender, System.EventArgs e) {
        immobilized = true;
        rb.velocity = Vector2.zero;
    }

    private void Creature_OnCreatureShockedStopped(object sender, System.EventArgs e) {
        float debuff = 1 + creature.GetShockSlowAmount() / 100;
        BuffMoveSpeed(debuff);

    }

    private void Creature_OnCreatureShockedStarted(object sender, System.EventArgs e) {
        float debuff = 1 + creature.GetShockSlowAmount() / 100;
        DebuffMoveSpeed(debuff);
    }

    public void SetSpawned() {
        spawned = true;
    }
}
