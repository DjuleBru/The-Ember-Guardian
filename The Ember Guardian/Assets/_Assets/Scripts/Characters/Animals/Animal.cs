using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Mob
{
    [SerializeField] private AnimalSO animalSO;

    private int maxHuntersAssigned;
    private List<Worker> workersAssigned = new List<Worker>();

    public event EventHandler OnAnimalPaused;
    public event EventHandler OnAnimalUnpaused;

    private void Start() {
        health = animalSO.maxHP;
        maxHuntersAssigned = animalSO.maxHuntersAssigned;
        AnimalManager.Instance.AddAnimalSpawned(this);

        DayNightManager.Instance.OnCyclePausedByMerchantTalk += DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
        rb = GetComponent<Rigidbody2D>();
    }
    private void DayNightManager_OnCycleUnpaused(object sender, EventArgs e) {
        isPaused = false;
        rb.simulated = true;
        OnAnimalUnpaused?.Invoke(this, EventArgs.Empty);
    }

    private void DayNightManager_OnCyclePausedByMerchantTalk(object sender, EventArgs e) {
        isPaused = true;
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        OnAnimalPaused?.Invoke(this, EventArgs.Empty);
    }

    [Button]
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


    private void OnDestroy() {
        DayNightManager.Instance.OnCyclePausedByMerchantTalk -= DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused -= DayNightManager_OnCycleUnpaused;
    }
}
