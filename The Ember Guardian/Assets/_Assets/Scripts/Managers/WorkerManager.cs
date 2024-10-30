using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    private List<Worker> recruitedWorkers = new List<Worker>();
    private List<Worker> joblessWorkers = new List<Worker>();

    public event EventHandler OnJoblessWorkerAmountChanged;

    private void Awake() {
        Instance = this;
    }

    public Worker GetFirstJoblessWorker() {
        List<Worker> joblessWorkers = new List<Worker>();

        foreach(Worker worker in recruitedWorkers) {
            if(worker.GetJob() == Worker.JobTypes.jobless) {
                joblessWorkers.Add(worker);
            }
        }

        return joblessWorkers[0];
    }

    public int GetJoblessWorkerAmount() {
        return joblessWorkers.Count;
    }

    public void AddRecruitedWorker(Worker worker) {
        recruitedWorkers.Add(worker);
        joblessWorkers.Add(worker);

        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveJoblessWorker(Worker worker) {
        joblessWorkers.Remove(worker);

        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveRecruitedWorker(Worker worker) {
        recruitedWorkers.Remove(worker);
    }

}
