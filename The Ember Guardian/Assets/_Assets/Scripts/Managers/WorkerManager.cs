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

    private bool minerUnlocked;
    private bool guardUnlocked;
    private bool engineerUnlocked;

    public event EventHandler OnJoblessWorkerAmountChanged;
    public event EventHandler OnRecruitedWorkerDied;
    public event EventHandler<OnClosestWorkerChangedEventArgs> OnClosestWorkerChanged;

    public class OnClosestWorkerChangedEventArgs : EventArgs {
        public Worker newClosestWorker;
    }

    private void Awake() {
        Instance = this;

        minerUnlocked = ES3.Load("minerUnlocked", false);
        guardUnlocked = ES3.Load("guardUnlocked", false);
        engineerUnlocked = ES3.Load("engineerUnlocked", false);
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

        CheckUnlockNewWorkerType(worker);

        OnJoblessWorkerAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CheckUnlockNewWorkerType(Worker worker) {
        if(!minerUnlocked) {
            if(worker.GetWildJobType() == WorkerAI.JobTypes.miner) {
                minerUnlocked = true;
                ES3.Save("minerUnlocked", true);
                MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(StructureSO.StructureType.minerShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(StructureSO.StructureType.minerShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.WorkerMerchant, true);
            }
        }

        if (!guardUnlocked) {
            if (worker.GetWildJobType() == WorkerAI.JobTypes.guard) {
                guardUnlocked = true;
                ES3.Save("guardUnlocked", true);
                MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(StructureSO.StructureType.guardShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(StructureSO.StructureType.guardShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.WorkerMerchant, true);
            }
        }
        if (!engineerUnlocked) {
            if (worker.GetWildJobType() == WorkerAI.JobTypes.engineer) {
                engineerUnlocked = true;
                ES3.Save("minerUnlocked", true);
                MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(StructureSO.StructureType.engineerShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(StructureSO.StructureType.engineerShrine + "1", true);
                MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.WorkerMerchant, true);
            }
        }
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

    public void AssignSideToHunter(Worker worker, CampZoneManager.CampSide campSide) {
        if(leftSideAssignedHunters.Contains(worker)) {
            leftSideAssignedHunters.Remove(worker);
        }
        if (rightSideAssignedHunters.Contains(worker)) {
            rightSideAssignedHunters.Remove(worker);
        }

        if(campSide == CampZoneManager.CampSide.left) {
            rightSideAssignedHunters.Add(worker);
            worker.AssignSide(CampZoneManager.CampSide.left);
        }
        if (campSide == CampZoneManager.CampSide.right) {
            rightSideAssignedHunters.Add(worker);
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
