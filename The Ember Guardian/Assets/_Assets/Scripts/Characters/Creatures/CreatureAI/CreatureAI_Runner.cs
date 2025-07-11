using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Runner : CreatureAI
{
    [SerializeField] private float runSpeedBuff = 4f;
    [SerializeField] private float normalizedHealthToStartRunning = .7f;
    [SerializeField] private float runDuration = 5f;
    [SerializeField] private float delayBetweenRuns;
    [SerializeField] private float startRunDelay;

    private bool running;
    private float runTimer;
    private bool justRunned;
    private float justRunnedTimer;

    private float probabilityToTriggerRun;

    public event EventHandler OnRunStarted;
    public event EventHandler OnRunStopped;

    protected override void Start() {
        base.Start();

        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
    }

    protected override void Update() {
        if(running) {
            runTimer += Time.deltaTime;
            if(runTimer >= runDuration) {
                StopRunning();
            }
        }

        if(justRunned) {
            justRunnedTimer += Time.deltaTime;
            if(justRunnedTimer >= delayBetweenRuns) {
                justRunned = false;
            }
        }

        base.Update();
    }

    protected override void ChangeState(State newState) {
        base.ChangeState(newState);

        if(newState != State.moveToTarget) {
            if(running) {
                StopRunning();
            }
        }
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        if (justRunned) return;
        if (running) return;

        float healthNormalized = (float)creature.GetCreatureHealth() / (float)creature.GetCreatureMaxHealth();

        if(healthNormalized < normalizedHealthToStartRunning) {
            StartCoroutine(StartRunning());
        }
    }
    private IEnumerator StartRunning() {
        OnRunStarted?.Invoke(this, EventArgs.Empty);
        running = true;

        yield return new WaitForSeconds(startRunDelay);

        runTimer = 0;
        creatureMovement.BuffMoveSpeed(runSpeedBuff, false);

    }

    private void StopRunning() {
        running = false;
        justRunned = true;
        justRunnedTimer = 0;
        creatureMovement.DebuffMoveSpeed(runSpeedBuff, false);

        OnRunStopped?.Invoke(this, EventArgs.Empty);
    }
}
