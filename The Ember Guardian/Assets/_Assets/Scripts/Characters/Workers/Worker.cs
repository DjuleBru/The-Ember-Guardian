using BehaviorDesigner.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : Mob
{
    [SerializeField] private Transform blueOrbPrefab;
    [SerializeField] private ExternalBehavior wildBehavior;
    [SerializeField] private ExternalBehavior hunterBehavior;
    [SerializeField] private ExternalBehavior minerBehavior;
    [SerializeField] private ExternalBehavior guardBehavior;

    private BehaviorTree behaviorTree;
    private int orbAmount;

    public enum JobTypes {
        wild,
        jobless,
        hunter,
        miner,
        guard
    }

    private JobTypes currentJob;
    public event EventHandler OnJobChanged;

    private void Awake() {
        behaviorTree = GetComponent<BehaviorTree>();
    }

    public void CollectOrb() {
        orbAmount++;
    }

    public void DropOrbs() {
        for(int i = 0; i < orbAmount; i++) {

            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(blueOrbPrefab, transform.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomFrontForce(2f, 3f);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);

        }
    }
    public void RecruitWorker() {
        WorkerManager.Instance.AddRecruitedWorker(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);
        SetJob(JobTypes.jobless);
    }

    #region JOBS

    private void Start() {
        SetJob(JobTypes.hunter);
    }

    public void SetJob(JobTypes newJob) {

        currentJob = newJob;

        if (currentJob == JobTypes.wild) {
        }

        if (currentJob == JobTypes.hunter) {
            behaviorTree.ExternalBehavior = hunterBehavior;
        }

        if (currentJob == JobTypes.jobless) {
        }

        OnJobChanged?.Invoke(this, EventArgs.Empty);
    }

    public JobTypes GetJob() {
        return currentJob;
    }

    #endregion

}
