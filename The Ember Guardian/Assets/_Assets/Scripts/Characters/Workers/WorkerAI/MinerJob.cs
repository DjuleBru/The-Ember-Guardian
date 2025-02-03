using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerJob : MonoBehaviour
{
    private Worker worker;
    private WorkerAI workerAI;
    private MobMovement mobMovement;
    private WorkerAnimatorManager workerAnimatorManager;
    private WorkerDetectionCollider workerDetectionCollider;

    private MinerState state;
    private MinerState previousState;

    private bool followingPlayer;

    public enum MinerState {
        idle,
        blockedByCreatures,
        headToSafety,
    }
    public event EventHandler OnMinerChangedState;

    public void InitializeMinerJob() {
        mobMovement = GetComponentInChildren<MobMovement>();
        workerAnimatorManager = GetComponentInChildren<WorkerAnimatorManager>();
        worker = GetComponent<Worker>();
        workerAI = GetComponent<WorkerAI>();
        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        mobMovement = GetComponent<MobMovement>();
        mobMovement.SetMoveTarget(mobMovement.transform.position);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(MinerState.idle);
        }
        else {

        }
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        //ChangeState(HunterState.headingToGuard);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        //minerAttack.SetHomingProjectile(true);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        //ChangeState(HunterState.idle);
    }

    private void ChangeState(MinerState newState) {
        if (newState == state) return;
        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;


        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnMinerChangedState?.Invoke(this, EventArgs.Empty);
    }

    public MinerState GetState() {
        return state;
    }
}
