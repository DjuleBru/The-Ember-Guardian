using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    private List<Worker> recruitedWorkers = new List<Worker>();
    private List<Worker> joblessWorkers = new List<Worker>();

    private List<Worker> leftSideAssignedWorkers = new List<Worker>();
    private List<Worker> rightSideAssignedWorkers = new List<Worker>();

    public event EventHandler OnJoblessWorkerAmountChanged;

    private void Awake() {
        Instance = this;
    }

    public Worker GetFirstJoblessWorker() {
        List<Worker> joblessWorkers = new List<Worker>();

        foreach(Worker worker in recruitedWorkers) {
            if(worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.jobless) {
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

        AssignSideToWorker(worker);
        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AssignSideToWorker(Worker worker) {

        if(leftSideAssignedWorkers.Count < rightSideAssignedWorkers.Count) {

            leftSideAssignedWorkers.Add(worker);
            worker.AssignSide(CampZoneManager.CampSide.left);

        } else {

            rightSideAssignedWorkers.Add(worker);
            worker.AssignSide(CampZoneManager.CampSide.right);

        }

    }

    public void RemoveJoblessWorker(Worker worker) {
        if(joblessWorkers.Contains(worker)) {
            joblessWorkers.Remove(worker);
        }

        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveRecruitedWorker(Worker worker) {
        recruitedWorkers.Remove(worker);
    }

    public void RemoveWorker(Worker worker) {
        if (joblessWorkers.Contains(worker)) {
            joblessWorkers.Remove(worker);
        }
        if(recruitedWorkers.Contains(worker)) {
            recruitedWorkers.Remove(worker);
        }
        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

}
