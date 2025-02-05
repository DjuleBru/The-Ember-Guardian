using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    private List<Worker> recruitedWorkers = new List<Worker>();
    private List<Worker> joblessWorkers = new List<Worker>();

    private List<Worker> leftSideAssignedHunters = new List<Worker>();
    private List<Worker> rightSideAssignedHunters = new List<Worker>();

    private List<Worker> leftSideAssignedGuards = new List<Worker>();
    private List<Worker> rightSideAssignedGuards = new List<Worker>();

    private List<Worker> leftSideAssignedMiners = new List<Worker>();
    private List<Worker> rightSideAssignedMiners = new List<Worker>();

    private List<Worker> workersInPlayerInteractionArea = new List<Worker>();
    private Worker closestInteractableWorkerFromPlayer;

    public event EventHandler OnJoblessWorkerAmountChanged;
    public event EventHandler OnRecruitedWorkerDied;
    public event EventHandler<OnClosestWorkerChangedEventArgs> OnClosestWorkerChanged;

    public class OnClosestWorkerChangedEventArgs : EventArgs {
        public Worker newClosestWorker;
    }

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        RefreshClosestInteractableWorkerFromPlayer();
    }

    private void RefreshClosestInteractableWorkerFromPlayer() {
        if (workersInPlayerInteractionArea.Count == 0) return;

        Worker closestWorker = FindClosestWorkerInPlayerInteractionArea();

        if (closestWorker != null) {

            if (closestInteractableWorkerFromPlayer != closestWorker) {
                OnClosestWorkerChanged?.Invoke(this, new OnClosestWorkerChangedEventArgs {
                    newClosestWorker = closestWorker,
                });
            }
            

            closestInteractableWorkerFromPlayer = closestWorker;
        }
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

        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AutoAssignSideToWorker(Worker worker) {
        float randomFloat = UnityEngine.Random.Range(0.0f, 1.0f);
        bool equalGoesLeft = false;
        if (randomFloat < 0.5f) {
            equalGoesLeft = true;
        }

        if(worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.hunter) {
            if (leftSideAssignedHunters.Count == rightSideAssignedHunters.Count) {

                if(equalGoesLeft) {
                    leftSideAssignedHunters.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.left);
                } else {
                    rightSideAssignedHunters.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.right);
                }
                return;
            }

            if (leftSideAssignedHunters.Count < rightSideAssignedHunters.Count) {

                leftSideAssignedHunters.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.left);

            }
            else {

                rightSideAssignedHunters.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.right);

            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.miner) {

            if (leftSideAssignedMiners.Count == rightSideAssignedMiners.Count) {

                if (equalGoesLeft) {
                    leftSideAssignedMiners.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.left);
                }
                else {
                    rightSideAssignedMiners.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.right);
                }
                return;
            }


            if (leftSideAssignedMiners.Count < rightSideAssignedMiners.Count) {

                leftSideAssignedMiners.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.left);

            }
            else {

                rightSideAssignedMiners.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.right);

            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.guard) {

            if (leftSideAssignedGuards.Count == rightSideAssignedGuards.Count) {

                if (equalGoesLeft) {
                    leftSideAssignedGuards.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.left);
                }
                else {
                    rightSideAssignedGuards.Add(worker);
                    worker.AssignSide(CampZoneManager.CampSide.right);
                }
                return;
            }

            if (leftSideAssignedGuards.Count < rightSideAssignedGuards.Count) {

                leftSideAssignedGuards.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.left);

            }
            else {

                rightSideAssignedGuards.Add(worker);
                worker.AssignSide(CampZoneManager.CampSide.right);

            }
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

        CampZoneManager.CampSide sideAssigned = worker.GetCampSideAddigned();
        if(sideAssigned == CampZoneManager.CampSide.left) {
            leftSideAssignedHunters.Remove(worker);
        } else {
            rightSideAssignedHunters.Remove(worker);
        }

        OnRecruitedWorkerDied?.Invoke(this, EventArgs.Empty);
        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddWorkerToPlayerInteractionArea(Worker worker) {
        workersInPlayerInteractionArea.Add(worker);

        Player.Instance.SetHoveringWorker(true);
    }

    public void RemoveWorkerFromPlayerInteractionArea(Worker worker, bool triggeredOut) {
        if (!workersInPlayerInteractionArea.Contains(worker)) return;
        workersInPlayerInteractionArea.Remove(worker);

        worker.HoverWorker(false);

        if (workersInPlayerInteractionArea.Count == 0) {
            closestInteractableWorkerFromPlayer = null;
            if(triggeredOut) {
                Player.Instance.SetHoveringWorker(false);
            } else {
                Player.Instance.ResetHoveringWorkerAfterInteractCanceled();
            }
        }
    }

    public Worker FindClosestWorkerInPlayerInteractionArea() {
        List<Worker> workersCurrentlyInPlayerInteractionArea = workersInPlayerInteractionArea;

        float closestDistance = Mathf.Infinity;
        Worker closestWorker = null;

        foreach(Worker worker in workersCurrentlyInPlayerInteractionArea) {
            float distanceToPlayer = Mathf.Abs(worker.transform.position.x - Player.Instance.transform.position.x);
                if(distanceToPlayer < closestDistance) {
                    closestDistance = distanceToPlayer;
                    closestWorker = worker;
                }
        }
        
        return closestWorker;
    }
    
    public Worker GetClosestWorkerInPlayerInteractionArea() {
        return closestInteractableWorkerFromPlayer;
    }

    public List<Worker> GetRecruitedWorkers() {
        return recruitedWorkers;
    }

}
