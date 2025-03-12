using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerAI : MonoBehaviour
{
    protected Worker worker;
    protected MobMovement workerMovement;

    private WildJob wildJob;
    private JoblessJob joblessJob;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    private MinerJob minerJob;

    private bool followingPlayer;

    public enum JobTypes {
        wild,
        jobless,
        hunter,
        miner,
        guard
    }

    private JobTypes currentJob;
    private bool debugSpawn;

    public event EventHandler OnJobChanged;
    public event EventHandler OnWorkerFollowPlayerChanged;
    public static event EventHandler OnAnyWorkerFollowPlayerStarted;
    public static event EventHandler OnAnyWorkerFollowPlayerStopped;

    protected virtual void Awake() {
        worker = GetComponent<Worker>();
        workerMovement = GetComponent<MobMovement>();

        wildJob = GetComponent<WildJob>();
        joblessJob = GetComponent<JoblessJob>();
        hunterJob = GetComponent<HunterJob>();
        guardJob = GetComponent<GuardJob>();
        minerJob = GetComponent<MinerJob>();
    }

    private void Start() {
        if (debugSpawn) return;
        SetJob(JobTypes.wild);
    }

    public void SetJob(JobTypes newJob) {
        SetAllJobTypesInactive();

        currentJob = newJob;

        if(currentJob == JobTypes.wild) {
            wildJob.enabled = true;
        }

        if (currentJob == JobTypes.hunter) {
            WorkerManager.Instance.RemoveJoblessWorker(worker);
            hunterJob.InitializeJob();
            hunterJob.enabled = true;
        }
        if (currentJob == JobTypes.miner) {
            WorkerManager.Instance.RemoveJoblessWorker(worker);
            minerJob.InitializeJob();
            minerJob.enabled = true;
        }
        if (currentJob == JobTypes.guard) {
            WorkerManager.Instance.RemoveJoblessWorker(worker);
            guardJob.InitializeJob();
            guardJob.enabled = true;
        }

        if (currentJob == JobTypes.jobless) {
            wildJob.UnAggroBlueOrb();
            joblessJob.enabled = true;
        }

        OnJobChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetAllJobTypesInactive() {
        wildJob.enabled = false;
        joblessJob.enabled = false;
        hunterJob.enabled = false;
        minerJob.enabled = false;
        guardJob.enabled = false;
    }

    public JobTypes GetJob() {
        return currentJob;
    }

    public void SetFollowingPlayer(bool followingPlayer, bool triggerSFX) {
        this.followingPlayer = followingPlayer;
        OnWorkerFollowPlayerChanged?.Invoke(this, EventArgs.Empty);

        if (followingPlayer) {
            WorkerFollowPlayerHandler.Instance.AddFollowingWorker(worker);

            if(triggerSFX) {
                OnAnyWorkerFollowPlayerStarted?.Invoke(this, EventArgs.Empty);
            }
        } else {
            if(triggerSFX) {
                OnAnyWorkerFollowPlayerStopped?.Invoke(this, EventArgs.Empty);
            }
        }
    }


    public bool GetFollowingPlayer() {
        return followingPlayer;
    } 

    public void SetDebugSpawn() {
        debugSpawn = true;
    }
    public bool GetDebugSpawn() {
        return debugSpawn;
    }
}
