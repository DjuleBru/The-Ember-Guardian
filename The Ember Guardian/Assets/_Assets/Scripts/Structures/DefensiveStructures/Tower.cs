using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Structure
{
    private List<Worker> assignedWorkersList = new List<Worker>();
    private int maxWorkersAssigned = 1;

    [SerializeField] private GameObject level1TowerCollider;
    [SerializeField] private GameObject level2TowerCollider;
    [SerializeField] private GameObject level3TowerCollider;
    [SerializeField] private GameObject level4TowerCollider;

    [SerializeField] private List<Transform> level1GarrisonPositions;
    [SerializeField] private List<Transform> level2GarrisonPositions;
    [SerializeField] private List<Transform> level3GarrisonPositions;
    [SerializeField] private List<Transform> level4GarrisonPositions;

    protected override void Start() {
        base.Start();
        DisableAllGarrisonColliders();
        level1TowerCollider.SetActive(true);

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    private void DisableAllGarrisonColliders() {
        level1TowerCollider.SetActive(false);
        level2TowerCollider.SetActive(false);
        level3TowerCollider.SetActive(false);
        level4TowerCollider.SetActive(false);
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();
        DisableAllGarrisonColliders();

        if (structureLevel == 2) {
            level2TowerCollider.SetActive(true);
        }

        if (structureLevel == 3) {
            level3TowerCollider.SetActive(true);
            maxWorkersAssigned = 2;
        }

        if (structureLevel == 4) {
            level4TowerCollider.SetActive(true);
            maxWorkersAssigned = 3;
        }

        foreach(Worker worker in assignedWorkersList) {
            SetWorkerGarrisonPosition(worker);
        }
    }

    public void AssignWorker(Worker worker) {
        assignedWorkersList.Add(worker);
        SetWorkerGarrisonPosition(worker);
    }

    private void SetWorkerGarrisonPosition(Worker worker) {
        int workerIndex = assignedWorkersList.IndexOf(worker);
        Vector3 garrisonPosition = new Vector3(0, 0, 0);

        if (structureLevel == 1) {
            garrisonPosition = level1GarrisonPositions[workerIndex].position;
        }

        if (structureLevel == 2) {
            garrisonPosition = level2GarrisonPositions[workerIndex].position;
        }

        if (structureLevel == 3) {
            garrisonPosition = level3GarrisonPositions[workerIndex].position;
        }

        if (structureLevel == 4) {
            garrisonPosition = level4GarrisonPositions[workerIndex].position;
        }

        worker.transform.position = garrisonPosition;
        worker.GetComponent<HunterJob>().SetGarrisoned(garrisonPosition, this);
    }

    public bool GetTowerFull() {
        return assignedWorkersList.Count >= maxWorkersAssigned;
    }

    public bool GetWorkerAssigned(Worker worker) {
        if (assignedWorkersList.Contains(worker)) return true;
        return false;
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        StartCoroutine(UnGarrisonWorkersCoroutine());
    }

    private IEnumerator UnGarrisonWorkersCoroutine() {

        foreach (Worker worker in assignedWorkersList) {

            Vector3 groundPosition = new Vector3(transform.position.x, 1, 0);
            worker.transform.position = groundPosition;
            worker.AssignStructure(null);
            yield return new WaitForSeconds(.2f);

        }

        assignedWorkersList.Clear();
    }

}
