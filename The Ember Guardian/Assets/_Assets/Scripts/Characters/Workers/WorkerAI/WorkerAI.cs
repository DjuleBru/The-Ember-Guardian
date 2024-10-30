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
            WorkerManager.Instance.RemoveJoblessWorker(worker);
            hunterJob.enabled = true;
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
    }

    public JobTypes GetJob() {
        return currentJob;
    }
}
