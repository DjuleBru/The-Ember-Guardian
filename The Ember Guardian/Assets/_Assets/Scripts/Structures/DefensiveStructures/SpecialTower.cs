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
    [SerializeField] protected bool playerCanClimbOnTower = true;

    [SerializeField] private int maxAmmoClipsInStorage;
    private int currentAmmoClip;

    public event EventHandler OnAmmoClipAdded;
    public event EventHandler OnAmmoClipRemoved;
    public event EventHandler OnAmmoClipsLoaded;
    public event EventHandler<OnEngineerEnteredTowerEventArgs> OnEngineerExitedTower;
    public event EventHandler<OnEngineerEnteredTowerEventArgs> OnEngineerEnteredTower;
    public event EventHandler OnPlayerClimberOnSpecialTower;

    public class OnEngineerEnteredTowerEventArgs : EventArgs {
        public EngineerJob engineerJob;
    }

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

        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            needsWorking = true;
        } 
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
            ActivateStructurePrimaryFunctionInteractionAfterFrame(false);

            return;
        }

        if (GetHasCurrenciesToPay() && playerInteracting) {
            StartCoroutine(SetPlayerInteractingAfterFrame());
        }
        else {
            payCurrencyUI.SetPlayerInteracting(false);
        }
    }

    public IEnumerator SetPlayerInteractingAfterFrame() {
        yield return new WaitForEndOfFrame();
        payCurrencyUI.SetPlayerInteracting(true); // Continue l'interaction
    }

    public void SetCurrentAmmoClips(int ammoClips) {
        StartCoroutine(SetAmmoClipsAfterFrame(ammoClips));
    }

    private IEnumerator SetAmmoClipsAfterFrame(int ammoClips) {
        yield return new WaitForEndOfFrame();
        this.currentAmmoClip = ammoClips;

        if (currentAmmoClip == maxAmmoClipsInStorage) {
            needsRefill = false;
            ActivateStructurePrimaryFunctionInteraction(false);
        }

        OnAmmoClipsLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        base.GameInput_OnPlayerInteractCanceled(sender, e);

        bool playerWasHoldingInteract = GameInput.Instance.GetWasHoldingInteract();

        if (playerCanClimbOnTower && !playerWasHoldingInteract && playerInTriggerArea && Player.Instance.transform.position.y < 2f) {
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
            OnEngineerEnteredTower?.Invoke(this, new OnEngineerEnteredTowerEventArgs {
                engineerJob = engineer
            });
            AssignEngineerToNextAvailableManner(engineer);
            SetEngineerGarrisonPosition(engineer);
            engineer.GetComponent<Worker>().AssignDefensiveStructure(this);
            engineer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            engineersGarrisoned++;

            if(engineersGarrisoned == maxEngineersAssignedWorking) {
                needsWorking = false;
            }
        }

        else {
            OnEngineerExitedTower?.Invoke(this, new OnEngineerEnteredTowerEventArgs {
                engineerJob = engineer
            });
            engineer.GetComponent<Worker>().AssignDefensiveStructure(null);
            engineer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            engineersGarrisoned--;
        }

    }
    private void AssignEngineerToNextAvailableManner(EngineerJob engineer) {
        foreach (var manner in manners) {
            if (manner.HasAvailableSlot()) {
                manner.AssignEngineer(engineer);
                return;
            }
        }

    }

    protected void SetEngineerGarrisonPosition(EngineerJob engineer) {
        int workerIndex = engineersAssignedWorking.IndexOf(engineer);
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

            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                needsWorking = true;
            }
        }

    }

    protected override void RefreshMaxEngineersAssignedAndWorking() {
        if (structureLevel == 2) {
            maxEngineersAssignedWorking = level2TowerEngineerCapacity;
        }
    }

    public int GetCurrentAmmoClip() {
        return currentAmmoClip;
    }
    public int GetMaxAmmoClips() {
        return maxAmmoClipsInStorage;
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

}
