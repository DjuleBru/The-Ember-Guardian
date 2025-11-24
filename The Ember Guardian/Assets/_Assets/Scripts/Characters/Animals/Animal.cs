using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Mob
{
    [SerializeField] private AnimalSO animalSO;

    private int maxHuntersAssigned;
    private List<Worker> workersAssigned = new List<Worker>();

    private float checkWorkerListTimer;
    private float checkWorkerListTime = 5f;

    private void Start() {
        health = animalSO.maxHP;
        maxHuntersAssigned = animalSO.maxHuntersAssigned;
        AnimalManager.Instance.AddAnimalSpawned(this);
    }

    public override void Die(Transform damageSource = null) {
        base.Die();

        AnimalManager.Instance.RemoveAnimalSpawned(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);

        SpawnDroppedCurrencies(animalSO.currencyTypeDroppedList, animalSO.currencyDropAmountList);
        InvokeOnMobDroppedCollectibles(collectiblesDropped);

        StartCoroutine(DestroyGameObjectAfterDelay(animalSO.dieAnimationTime));
    }

    public AnimalSO GetAnimalSO() {
        return animalSO;
    }

    public void AssignHunter(Worker worker) {
        workersAssigned.Add(worker);
    }
    public void UnAssignHunter(Worker worker) {
        workersAssigned.Remove(worker);
    }

    public bool GetHunterIsAlreadyAssigned(Worker worker) {
        return workersAssigned.Contains(worker);
    }

    public bool GetMaxHuntersAssigned() {

        foreach (Worker worker in workersAssigned) {
            if (worker == null) {
                workersAssigned.Clear();
                break;
            }
        }

        if (workersAssigned.Count == maxHuntersAssigned) return true;

        return false;
    }

    public Worker GetFurthestWorkerAssigned() {
        Worker furthestWorker = null;
        float maxDistance = -1f;

        foreach (Worker worker in workersAssigned) {
            float distance = Vector2.Distance(worker.transform.position, this.transform.position);
            if (distance > maxDistance) {
                maxDistance = distance;
                furthestWorker = worker;
            }
        }

        return furthestWorker;
    }

    public float GetFurthestWorkerDistance() {
        float maxDistance = -1f;

        foreach (Worker worker in workersAssigned) {
            float distance = Vector2.Distance(worker.transform.position, this.transform.position);
            if (distance > maxDistance) {
                maxDistance = distance;
            }
        }

        return maxDistance;

    }
}
