using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerJob : WorkerJob {

    private MinerState state;
    private MinerState previousState;

    private Creature closestCreature;
    private Scavengable assignedScavengable;
    private Transform assignedScavengableMiningPosition;

    private float headToMineMoveSpeed = 2.5f;

    private bool isNightOrDusk;
    private bool followingPlayer;

    public enum MinerState {
        idle,
        droppingOrbs,
        followPlayerIdle,
        followPlayerAttackCreature,
        blockedByCreatures,
        headToSafety,
        headingToMine,
        pickingUpOrbs,
        Mining,
    }

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();
    }

    private void Update() {
        closestCreature = workerDetectionCollider.GetClosestCreature();

        if (CheckDropCurrenciesToPlayer()) {
            ChangeState(MinerState.droppingOrbs);
        }

        if (followingPlayer) {

        } else {

            switch (state) {
                case MinerState.idle:
                    RoamInCampCenter();
                    CheckAvailableScavengables();

                    if(assignedScavengable != null && !isNightOrDusk) {
                        ChangeState(MinerState.headingToMine);
                    }

                    break;

                case MinerState.headToSafety:
                    if (isInSafeZone) {
                        RoamInCampCenter();
                    }
                    else {
                        HeadToCampCenter();
                    }
                break;

                case MinerState.headingToMine:
                    HeadToMine();
                break;

                case MinerState.Mining:
                    Mine();
                    if(CheckOrbsToCollect()) {
                        ChangeState(MinerState.pickingUpOrbs);
                    }
                break;

                case MinerState.pickingUpOrbs:
                    HeadToPickUpClosestOrb();
                break;

                case MinerState.droppingOrbs:

                    if (worker.GetTotalCurrencyAmount() == 0) {
                        ChangeState(previousState);
                        return;
                    }

                    if (worker.PlayerIsCloseAndStayedAround()) {
                        worker.DropCurrencies();
                        return;
                    }

                    if (!worker.GetPlayerIsClose()) {
                        ChangeState(previousState);
                        return;
                    }

                break;
            }
        }

    }

    public void HeadToPickUpClosestOrb() {

        if (orbsToCollect.Count == 0) {
            ChangeState(MinerState.idle);
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);

    }

    private void Mine() {
        workerAttack.SetAttackTarget(assignedScavengable);
    }

    private void HeadToMine() {
        float distanceToScavengable = Mathf.Abs(transform.position.x - assignedScavengableMiningPosition.position.x);

        if(distanceToScavengable < .5f) {
            ChangeState(MinerState.Mining);
        } else {
            mobMovement.SetMoveTarget(assignedScavengableMiningPosition.position);
        }
    }

    private void CheckAvailableScavengables() {
        Scavengable assignableScavengable = ScavengableManager.Instance.GetClosestScavengableToScavenge(transform.position);

        if(assignableScavengable != null) {
            AssignScavengable(assignableScavengable);
        }

    }

    public void AssignScavengable(Scavengable scavengable) {
        assignedScavengable = scavengable;
        scavengable.OnScavengableSpawnedCurrency += AssignedScavengable_OnScavengableSpawnedCurrency;
        scavengable.AssignMiner(this);
        assignedScavengableMiningPosition = scavengable.GetMeleeAttackPosition();
    }

    public void UnAssignScavengable() {
        assignedScavengable.OnScavengableSpawnedCurrency -= AssignedScavengable_OnScavengableSpawnedCurrency;
        assignedScavengable = null;
        workerAttack.RemoveAttackTarget();
        ChangeState(MinerState.idle);
    }

    private void AssignedScavengable_OnScavengableSpawnedCurrency(object sender, Scavengable.OnStavengableSpawnedCurrencyEventArgs e) {
        orbsToCollect.Add(e.collectibleSpawned);
        e.collectibleSpawned.OnCollectibleDestroyed += CollectibleSpawned_OnCollectibleDestroyed;
    }
    private void CollectibleSpawned_OnCollectibleDestroyed(object sender, System.EventArgs e) {
        Collectible collectible = sender as Collectible;
        RemoveOrbToCollect(collectible);
    }

    public void RemoveOrbToCollect(Collectible collectible) {
        if (orbsToCollect.Contains(collectible)) {
            orbsToCollect.Remove(collectible);
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
        isNightOrDusk = true;

        if (followingPlayer) return;
        ChangeState(MinerState.headToSafety);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        isNightOrDusk = false;

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

        if(state == MinerState.idle) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
        }
        if (state == MinerState.headingToMine) {
            workerAttack.RemoveAttackTarget();
            mobMovement.SetMoveSpeed(headToMineMoveSpeed);
        }
        if (state == MinerState.pickingUpOrbs) {
            workerAttack.RemoveAttackTarget();
        }
        if (state == MinerState.headToSafety) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
        }
    }

    public MinerState GetState() {
        return state;
    }
}
