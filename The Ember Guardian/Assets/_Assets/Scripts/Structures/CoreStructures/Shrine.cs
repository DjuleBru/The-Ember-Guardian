using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine : Structure
{
    public event EventHandler OnShrineActivated;
    public static event EventHandler OnAnyShrineActivated;

    public enum ShrineType {
        hunterShrine,
        minerShrine,
        guardShrine,
    }

    [SerializeField] private Transform workerSpawnPosition;
    [SerializeField] private ShrineType shrineType;

    protected override void Start() {
        base.Start();
        WorkerManager.Instance.OnJoblessWorkerAmountChanged += WorkerManager_OnJoblessWorkerAmountChanged;
        RefreshShrineActivation();
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        base.PayOrbsUI_OnOrbPaymentSuccess(sender, e);
        
        Worker joblessWorker = WorkerManager.Instance.GetFirstJoblessWorker();

        if(shrineType == ShrineType.hunterShrine) {
            joblessWorker.transform.position = workerSpawnPosition.position;
            joblessWorker.GetComponent<WorkerAI>().SetJob(WorkerAI.JobTypes.hunter);
        }
        if (shrineType == ShrineType.minerShrine) {
            joblessWorker.transform.position = workerSpawnPosition.position;
            joblessWorker.GetComponent<WorkerAI>().SetJob(WorkerAI.JobTypes.miner);
        }
        if (shrineType == ShrineType.guardShrine) {
            joblessWorker.transform.position = workerSpawnPosition.position;
            joblessWorker.GetComponent<WorkerAI>().SetJob(WorkerAI.JobTypes.guard);
        }

        OnShrineActivated?.Invoke(this, EventArgs.Empty);
        OnAnyShrineActivated?.Invoke(this, EventArgs.Empty);
    }

    private void WorkerManager_OnJoblessWorkerAmountChanged(object sender, EventArgs e) {
        RefreshShrineActivation();
    }

    private void RefreshShrineActivation() {
        if (WorkerManager.Instance.GetJoblessWorkerAmount() == 0) {
            SetStructurePrimaryFunctionUnlocked(false);
        }
        else {
            SetStructurePrimaryFunctionUnlocked(true);
        }
    }

}
