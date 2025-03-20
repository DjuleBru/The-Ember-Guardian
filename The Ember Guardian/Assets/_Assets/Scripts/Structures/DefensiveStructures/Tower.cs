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

    private float level1RangeMultiplier = 1.25f;
    private float level2RangeMultiplier = 1.5f;
    private float level3RangeMultiplier = 1.5f;
    private float level4RangeMultiplier = 2f;

    private float level1DamageMultiplier = 1.25f;
    private float level2DamageMultiplier = 1.5f;
    private float level3DamageMultiplier = 1.5f;
    private float level4DamageMultiplier = 2f;

    public event EventHandler OnHunterAssigned;
    public event EventHandler OnPlayerClimbedOnTower;
    public static event EventHandler OnPlayerClimbedOnAnyTower;

    protected override void Start() {
        base.Start();
        DisableAllGarrisonColliders();
        level1TowerCollider.SetActive(true);

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;
    }

    private void Worker_OnAnyWorkerDied(object sender, EventArgs e) {
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
        OnHunterAssigned?.Invoke(this, EventArgs.Empty);
    }

    private void SetWorkerGarrisonPosition(Worker worker) {
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

    private IEnumerator UnGarrisonWorkersCoroutine() {

        for (int i = assignedWorkersList.Count - 1; i >= 0; i--) {
            Worker worker = assignedWorkersList[i];
            Vector3 groundPosition = new Vector3(transform.position.x, 1, 0);
            worker.transform.position = groundPosition;
            worker.AssignStructure(null);
            worker.GetComponent<HunterJob>().ResetRangeBuff();
            worker.GetComponent<HunterJob>().ResetDamageBuff();
            yield return new WaitForSeconds(.2f);
        }

        assignedWorkersList.Clear();
    }

    private void MovePlayerOnTower() {
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

    private void OnDestroy() {
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;
    }

}
