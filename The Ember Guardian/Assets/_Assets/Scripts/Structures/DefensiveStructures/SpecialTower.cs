using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower : Structure {

    [SerializeField] protected List<Transform> level1GarrisonPositions;
    [SerializeField] protected List<Transform> level2GarrisonPositions;
    [SerializeField] protected List<SpecialTower_Manner> manners;
    [SerializeField] protected GameObject level1TowerCollider;
    [SerializeField] protected GameObject level2TowerCollider;
    [SerializeField] protected int level2TowerEngineerCapacity;

    [SerializeField] private int maxAmmoClipsInStorage;
    private int currentAmmoClip;

    public event EventHandler OnAmmoClipAdded;
    public event EventHandler OnAmmoClipRemoved;
    public event EventHandler OnEngineerExitedTower;
    public event EventHandler OnEngineerEnteredTower;
    public event EventHandler OnPlayerClimberOnSpecialTower;

    protected override void Awake() {
        base.Awake();
        level2TowerCollider.gameObject.SetActive(false);
        needsRefill = true;

        foreach(SpecialTower_Manner manner in manners) {
            manner.OnMannerReloadingHandsEnded += Manner_OnMannerReloadingEnded;
        }
    }

    protected override void Start() {
        base.Start();
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);
        needsWorking = false;
    }

    protected override void TriggerStructurePrimaryFunction() {
        // Reload Special Tower
        base.TriggerStructurePrimaryFunction();

        currentAmmoClip++;
        OnAmmoClipAdded?.Invoke(this, EventArgs.Empty);

        if (currentAmmoClip == maxAmmoClipsInStorage) {
            needsRefill = false;
            ActivateStructurePrimaryFunctionInteraction(false);

            return;
        }

        if (GetHasCurrenciesToPay() && playerInteracting) {
            payCurrencyUI.SetPlayerInteractingContinuous(.2f); // Continue l'interaction
        }
        else {
            payCurrencyUI.SetPlayerInteracting(false);
        }
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        base.GameInput_OnPlayerInteractCanceled(sender, e);

        bool playerWasHoldingInteract = GameInput.Instance.GetWasHoldingInteract();

        if (!playerWasHoldingInteract && playerInTriggerArea && Player.Instance.transform.position.y < 2f) {
            MovePlayerOnTower();
        }
    }

    protected void MovePlayerOnTower() {
        Vector3 garrisonPosition = Vector3.zero;

        if (structureLevel == 1) {
            garrisonPosition = level1GarrisonPositions[0].position;
        }

        if (structureLevel == 2) {
            garrisonPosition = level2GarrisonPositions[0].position;
        }

        garrisonPosition = new Vector3(garrisonPosition.x, garrisonPosition.y, garrisonPosition.z);

        Player.Instance.transform.position = garrisonPosition;
        OnPlayerClimberOnSpecialTower?.Invoke(this, EventArgs.Empty);
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        needsWorking = true;
    }

    private void Manner_OnMannerReloadingEnded(object sender, EventArgs e) {
        if(currentAmmoClip > 0) {
            currentAmmoClip--;
            OnAmmoClipRemoved?.Invoke(this, EventArgs.Empty);
            needsRefill = true;
            ActivateStructurePrimaryFunctionInteraction(true);
        }
    }


    public override void SetEngineerWorking(EngineerJob engineer, bool working) {
        base.SetEngineerWorking(engineer, working);

        if (working) {
            OnEngineerEnteredTower?.Invoke(this, EventArgs.Empty);
            SetEngineerGarrisonPosition(engineer);
        }

        else {
            OnEngineerExitedTower?.Invoke(this, EventArgs.Empty);
        }

    }
    protected void SetEngineerGarrisonPosition(EngineerJob engineer) {
        int workerIndex = engineersAssignedWorking.IndexOf(engineer);
        Debug.Log(workerIndex);
        Vector3 garrisonPosition = new Vector3(0, 0, 0);

        if (structureLevel == 1) {
            garrisonPosition = level1GarrisonPositions[workerIndex].position;
        }

        if (structureLevel == 2) {
            garrisonPosition = level2GarrisonPositions[workerIndex].position;
        }

        engineer.transform.position = garrisonPosition;
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();

        if (structureLevel == 2) {
            level1TowerCollider.SetActive(false);
            level2TowerCollider.SetActive(true);
            maxEngineersAssignedWorking = level2TowerEngineerCapacity;

            if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                needsWorking = true;
            }
        }

    }
    public int GetCurrentAmmoClip() {
        return currentAmmoClip;
    }
    public int GetMaxAmmoClips() {
        return maxAmmoClipsInStorage;
    }

}
