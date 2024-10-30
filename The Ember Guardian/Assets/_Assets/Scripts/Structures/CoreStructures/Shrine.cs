using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine : Structure
{
    public event EventHandler OnShrineActivated;

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
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        base.PayOrbsUI_OnOrbPaymentSuccess(sender, e);

        Worker joblessWorker = WorkerManager.Instance.GetFirstJoblessWorker();
        if(shrineType == ShrineType.hunterShrine) {
            joblessWorker.transform.position = workerSpawnPosition.position;
            joblessWorker.GetComponent<Worker>().SetJob(Worker.JobTypes.hunter);
        }

        OnShrineActivated?.Invoke(this, EventArgs.Empty);
    }

    private void WorkerManager_OnJoblessWorkerAmountChanged(object sender, EventArgs e) {
        if(WorkerManager.Instance.GetJoblessWorkerAmount() == 0) {
            SetStructureFunctionLocked();
        } else {
            SetStructureFunctionUnlocked();
        }
    }


}
