using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Structure
{
    protected List<Worker> assignedWorkersList = new List<Worker>();
    protected int maxWorkersAssigned = 1;

    [SerializeField] protected GameObject level1TowerCollider;
    [SerializeField] protected GameObject level2TowerCollider;
    [SerializeField] protected GameObject level3TowerCollider;
    [SerializeField] protected GameObject level4TowerCollider;

    [SerializeField] protected List<Transform> level1GarrisonPositions;
    [SerializeField] protected List<Transform> level2GarrisonPositions;
    [SerializeField] protected List<Transform> level3GarrisonPositions;
    [SerializeField] protected List<Transform> level4GarrisonPositions;

    protected float level1RangeMultiplier = 1.25f;
    protected float level2RangeMultiplier = 1.5f;
    protected float level3RangeMultiplier = 1.5f;
    protected float level4RangeMultiplier = 2f;

    protected float level1DamageMultiplier = 1.25f;
    protected float level2DamageMultiplier = 1.5f;
    protected float level3DamageMultiplier = 1.5f;
    protected float level4DamageMultiplier = 2f;

    public event EventHandler OnHunterGarrisoned;
    public event EventHandler OnPlayerClimbedOnTower;
    public static event EventHandler OnPlayerClimbedOnAnyTower;

    protected override void Start() {
        base.Start();
        DisableAllGarrisonColliders();
        level1TowerCollider.SetActive(true);

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;
    }

    protected void Worker_OnAnyWorkerDied(object sender, EventArgs e) {
        Worker worker = (Worker)sender;

        if(assignedWorkersList.Contains(worker)) {
            assignedWorkersList.Remove(worker);
        }
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        base.GameInput_OnPlayerInteractCanceled(sender, e);

        bool playerWasHoldingInteract = GameInput.Instance.GetWasHoldingInteract();

        if(!playerWasHoldingInteract && playerInTriggerArea && Player.Instance.transform.position.y <2f) {
            MovePlayerOnTower();
        }

    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        if ((collision.gameObject.GetComponent<Player>() != null)) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);
        if ((collision.gameObject.GetComponent<Player>() != null)) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        }
    }

    protected void DisableAllGarrisonColliders() {
        level1TowerCollider.SetActive(false);

        if(level2TowerCollider != null) {
            level2TowerCollider.SetActive(false);
        }
        if (level3TowerCollider != null) {
            level3TowerCollider.SetActive(false);
        }
        if (level4TowerCollider != null) {
            level4TowerCollider.SetActive(false);
        }
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();
        DisableAllGarrisonColliders();

        if (structureLevel == 2) {
            level2TowerCollider.SetActive(true);
            maxWorkersAssigned = 2;
        }

        if (structureLevel == 3) {
            level3TowerCollider.SetActive(true);
            maxWorkersAssigned = 3;
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
    }

    public void GarrisonWorker(Worker worker) {
        SetWorkerGarrisonPosition(worker);
        OnHunterGarrisoned?.Invoke(this, EventArgs.Empty);
    }

    protected void SetWorkerGarrisonPosition(Worker worker) {
        int workerIndex = assignedWorkersList.IndexOf(worker);
        Vector3 garrisonPosition = new Vector3(0, 0, 0);
        float rangeBuff = 1f;
        float damageBuff = 1f;

        if (structureLevel == 1) {
            garrisonPosition = level1GarrisonPositions[workerIndex].position;
            rangeBuff = level1RangeMultiplier;
            damageBuff = level1DamageMultiplier;
        }

        if (structureLevel == 2) {
            garrisonPosition = level2GarrisonPositions[workerIndex].position;
            rangeBuff = level2RangeMultiplier;
            damageBuff = level2DamageMultiplier;
        }

        if (structureLevel == 3) {
            garrisonPosition = level3GarrisonPositions[workerIndex].position;
            rangeBuff = level3RangeMultiplier;
            damageBuff = level3DamageMultiplier;
        }

        if (structureLevel == 4) {
            garrisonPosition = level4GarrisonPositions[workerIndex].position;
            if(workerIndex == 1) {
                rangeBuff = level4RangeMultiplier;
                damageBuff = level4DamageMultiplier;
            } else {
                rangeBuff = level3RangeMultiplier;
                damageBuff = level3DamageMultiplier;
            }
        }

        worker.transform.position = garrisonPosition;
        worker.GetComponent<HunterJob>().SetGarrisoned(garrisonPosition, this);
        worker.GetComponent<HunterJob>().BuffDamage(damageBuff);
        worker.GetComponent<HunterJob>().BuffRange(rangeBuff);
    }

    public bool GetTowerFull() {
        return assignedWorkersList.Count >= maxWorkersAssigned;
    }

    public bool GetWorkerAssigned(Worker worker) {
        if (assignedWorkersList.Contains(worker)) return true;
        return false;
    }

    protected override void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);

        StartCoroutine(UnGarrisonWorkersCoroutine());
    }

    protected IEnumerator UnGarrisonWorkersCoroutine() {

        for (int i = assignedWorkersList.Count - 1; i >= 0; i--) {
            Worker worker = assignedWorkersList[i];
            Vector3 groundPosition = new Vector3(transform.position.x, 1, 0);
            worker.transform.position = groundPosition;
            worker.AssignDefensiveStructure(null);
            worker.GetComponent<HunterJob>().ResetRangeBuff();
            worker.GetComponent<HunterJob>().ResetDamageBuff();
            yield return new WaitForSeconds(.2f);
        }

        assignedWorkersList.Clear();
    }

    protected void MovePlayerOnTower() {
        Vector3 garrisonPosition = Vector3.zero;

        if (structureLevel == 1) {
            garrisonPosition = level1GarrisonPositions[0].position;
        }

        if (structureLevel == 2) {
            garrisonPosition = level2GarrisonPositions[0].position;
        }

        if (structureLevel == 3) {
            garrisonPosition = level3GarrisonPositions[0].position;
        }

        if (structureLevel == 4) {
            garrisonPosition = level4GarrisonPositions[0].position;
        }

        garrisonPosition = new Vector3(garrisonPosition.x, garrisonPosition.y, garrisonPosition.z);

        Player.Instance.transform.position = garrisonPosition;
        OnPlayerClimbedOnAnyTower?.Invoke(this, EventArgs.Empty);
        OnPlayerClimbedOnTower?.Invoke(this, EventArgs.Empty);
    }

    protected void OnDestroy() {
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;
    }

}
