using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower : Structure {

    [SerializeField] protected List<Transform> level1GarrisonPositions;
    [SerializeField] protected List<Transform> level2GarrisonPositions;
    [SerializeField] protected GameObject level1TowerCollider;
    [SerializeField] protected GameObject level2TowerCollider;

    [SerializeField] private int shotsPerAmmoClip;
    [SerializeField] private int maxAmmoClipsInStorage;
    private int currentAmmoClip;
    private int currentShotIndex;

    public event EventHandler OnAmmoClipAdded;
    public event EventHandler OnAmmoClipRemoved;
    public event EventHandler OnPlayerClimberOnSpecialTower;

    protected override void Awake() {
        base.Awake();
        level2TowerCollider.gameObject.SetActive(false);
        needsEngineering = true;
        needsEngineerRefill = true;
    }

    protected override void Start() {
        base.Start();
    }
    protected override void UpgradeStructure() {
        base.UpgradeStructure();

        if (structureLevel == 2) {
            level1TowerCollider.SetActive(false);
            level2TowerCollider.SetActive(true);
        }
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();

        currentAmmoClip++;
        currentShotIndex = shotsPerAmmoClip;
        OnAmmoClipAdded?.Invoke(this, EventArgs.Empty);

        if (currentAmmoClip == maxAmmoClipsInStorage) {
            needsEngineerRefill = false;
            ActivateStructurePrimaryFunctionInteraction(false);

            if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
                needsEngineering = false;
            }

            return;
        }

        if (GetHasCurrenciesToPay() && playerInteracting) {
            payCurrencyUI.SetPlayerInteractingContinuous(); // Continue l'interaction
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

    public int GetCurrentAmmoClip() {
        return currentAmmoClip;
    }
    public int GetMaxAmmoClips() {
        return maxAmmoClipsInStorage;
    }
}
