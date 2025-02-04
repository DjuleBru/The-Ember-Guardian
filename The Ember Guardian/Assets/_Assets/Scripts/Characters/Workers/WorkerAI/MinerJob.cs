using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerJob : WorkerJob {

    private MinerState state;
    private MinerState previousState;

    private Creature closestCreature;

    private bool followingPlayer;

    public enum MinerState {
        idle,
        droppingOrbs,
        followPlayerIdle,
        followPlayerAttackCreature,
        blockedByCreatures,
        headToSafety,
        headingToMine,
        Mining,
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            if (CheckDropCurrenciesToPlayer()) {
                ChangeState(MinerState.droppingOrbs);
            }
            if (CheckOrbsToCollect()) {
                ChangeState(MinerState.Mining);
            };
        }

        if(followingPlayer) {

        } else {

            switch (state) {
                case MinerState.idle:
                    RoamBehavior.RoamInCampCenter(mobMovement);

                    break;

                case MinerState.headToSafety:
                    if (isInSafeZone) {
                        RoamInCampCenter();
                    }
                    else {
                        HeadToCampCenter();
                    }
                    break;
            }
        }

    }

    public event EventHandler OnMinerChangedState;

    public override void InitializeJob() {
        base.InitializeJob();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(MinerState.idle);
        }
        else {
            ChangeState(MinerState.headToSafety);
        }
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (followingPlayer) return;
        ChangeState(MinerState.headToSafety);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (followingPlayer) return;
        ChangeState(MinerState.idle);
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
