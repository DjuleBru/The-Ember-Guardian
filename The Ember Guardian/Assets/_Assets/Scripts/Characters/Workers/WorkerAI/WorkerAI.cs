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

    public enum JobTypes {
        wild,
        jobless,
        hunter,
        miner,
        guard
    }

    private JobTypes currentJob;

    public event EventHandler OnJobChanged;

    protected virtual void Awake() {
        worker = GetComponent<Worker>();
        workerMovement = GetComponent<MobMovement>();

        wildJob = GetComponent<WildJob>();
        joblessJob = GetComponent<JoblessJob>();
        hunterJob = GetComponent<HunterJob>();
    }

    private void Start() {
        SetJob(JobTypes.wild);
    }

    public void SetJob(JobTypes newJob) {
        SetAllJobTypesInactive();

        currentJob = newJob;

        if(currentJob == JobTypes.wild) {
            wildJob.enabled = true;
        }

        if (currentJob == JobTypes.hunter) {
            hunterJob.enabled = true;
        }

        if (currentJob == JobTypes.jobless) {
            joblessJob.enabled = true;
        }

        OnJobChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RecruitWorker() {
        WorkerManager.Instance.AddRecruitedWorker(worker);
        worker.GetMobSpawner().RemoveMobFromMobSpawnedList(worker);
        SetJob(JobTypes.jobless);
    }

    private void SetAllJobTypesInactive() {
        wildJob.enabled = false;

        joblessJob.enabled = false;

        hunterJob.enabled = false;
    }

    public JobTypes GetJob() {
        return currentJob;
    }
}
