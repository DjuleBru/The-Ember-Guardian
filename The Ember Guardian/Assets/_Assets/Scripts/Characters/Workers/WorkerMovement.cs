using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerMovement : MobMovement
{

    private WorkerAI workerAI;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    protected bool followingPlayer;
    protected Vector3 finalDestination;  // Objectif final (utile si on prend un TP)
    protected FastTravelTP departureTP;
    protected bool teleporting;

    public event EventHandler OnTPStarted;
    public event EventHandler OnTPEnded;

    protected override void Awake() {
        base.Awake();
        workerAI = GetComponent<WorkerAI>();
        hunterJob = GetComponent<HunterJob>();
        guardJob = GetComponent<GuardJob>();    
    }

    protected override void Start() {
        base.Start();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;
    }

    protected void Update() {
        if (!followingPlayer) return;

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            if (hunterJob.GetState() == HunterJob.HunterState.escortAttackCreature) return;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            if (guardJob.GetState() == GuardJob.GuardState.escorting) return;
        }

        float minSpeed = 2f;  // Vitesse minimale
        float maxSpeed = PlayerMovement.Instance.GetTargetMoveSpeed()*1.25f; // Vitesse max = celle du joueur

        float distance = Vector3.Distance(transform.position, targetDestination);
        float speedFactor = Mathf.Clamp01(distance / 5f); // Distance max considérée pour le scaling

        float newSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedFactor);
        SetMoveSpeed(newSpeed);
    }

    protected override void FixedUpdate() {
        if (teleporting) return;

        if(!destinationReached) {
            if (IsAtTeleporter(transform.position) && finalDestination != targetDestination) {
                TeleportToClosestTPNear(finalDestination);
            }
        }

        base.FixedUpdate();
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, System.EventArgs e) {
        followingPlayer = workerAI.GetFollowingPlayer();
    }

    public override void SetMoveSpeed(float moveSpeed) {

        float moveSpeedBuff = 0f;
        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            moveSpeedBuff = WorkerStats.Instance.GetHunterMoveSpeedBuff();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            moveSpeedBuff = WorkerStats.Instance.GetGuardMoveSpeedBuff();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            //Debug.Log(gameObject + " SetMoveSpeed " + moveSpeed);
            moveSpeedBuff = WorkerStats.Instance.GetMinerMoveSpeedBuff();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.engineer) {
            moveSpeedBuff = WorkerStats.Instance.GetEngineerMoveSpeedBuff();
        }

        this.moveSpeed = moveSpeed * (1+moveSpeedBuff);
    }

    public override void SetMoveTarget(Vector3 moveTarget) {
        finalDestination = moveTarget;
        Vector3 optimizedTarget = EvaluateOptimizedPath(transform.position, moveTarget);
        base.SetMoveTarget(optimizedTarget);
    }

    private Vector3 EvaluateOptimizedPath(Vector3 currentPos, Vector3 targetPos) {
        float directDist = Vector3.Distance(currentPos, targetPos);
        Vector3 bestDestination = targetPos;
        float bestTotalDist = directDist;

        foreach (var tpFrom in PlayerCamp.Instance.GetAllFastTravelTPsBuilt()) {
            float distToTP = Vector3.Distance(currentPos, tpFrom.transform.position);

            foreach (var tpTo in PlayerCamp.Instance.GetAllFastTravelTPsBuilt()) {
                if (tpFrom == tpTo) continue;

                float distFromTP = Vector3.Distance(tpTo.transform.position, targetPos);
                float totalDistance = distToTP + distFromTP;

                if (totalDistance < bestTotalDist) {
                    bestTotalDist = totalDistance;
                    departureTP = tpFrom;
                    bestDestination = tpFrom.transform.position;
                }
            }
        }

        return bestDestination;
    }

    private bool IsAtTeleporter(Vector3 pos) {
        if (departureTP == null) return false;

        if (Vector3.Distance(departureTP.transform.position, pos) < 0.5f) return true;

        return false;
    }
    private void TeleportToClosestTPNear(Vector3 destination) {
        FastTravelTP bestTP = null;
        float bestDist = float.MaxValue;

        foreach (var tp in PlayerCamp.Instance.GetAllFastTravelTPsBuilt()) {
            float dist = Vector3.Distance(tp.transform.position, destination);
            if (dist < bestDist) {
                bestDist = dist;
                bestTP = tp;
            }
        }

        if (bestTP != null) {
            StartCoroutine(Teleport(bestTP));
        }
    }

    private IEnumerator Teleport(FastTravelTP destinationTP) {
        teleporting = true;

        departureTP.StartTeleporting(this, .6f);
        yield return new WaitForSeconds(.2f);

        OnTPStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(.4f);

        OnTPEnded?.Invoke(this, EventArgs.Empty);
        destinationTP.ReceiveTeleporting(this);
        targetDestination = finalDestination;
        teleporting = false;
    }

}
