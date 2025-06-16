using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : Structure, IDamageable {

    [SerializeField] private Transform projectileTarget;
    [SerializeField] private Transform meleeAttackPosition;
    [SerializeField] private BarricadeVisual barricadeVisual;

    private int level1CrateAmount = 3;
    private int level2CrateAmount = 4;
    private int level3CrateAmount = 5;
    private int level4CrateAmount = 7;
    private int healthPerCrate;

    private int barricadeMaxHealth;
    private int barricadeHealth;

    public event EventHandler OnBarricadeDamageTaken;
    public event EventHandler OnBarricadeDestroyed;
    public static event EventHandler OnAnyBarricadeDestroyed;
    public event EventHandler OnBarricadeRepaired;
    public static event EventHandler OnAnyBarricadeRepaired;
    public static event EventHandler OnAnyBarricadeBuilt;
    public event EventHandler OnFireLightTriggeredIn;
    public event EventHandler OnFireLightTriggeredOut;
    public event EventHandler OnBarricadeLightSwitched;
    public event EventHandler OnBarricadeBreached;

    private bool barricadeSpiked;
    private int spikeDamage = 3;
    private bool isInnerBarricade;
    private bool barricadeRepairable;

    protected override void Start() {
        base.Start();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;

        barricadeSpiked = StructureStats.Instance.GetBarricadesSpiked();
        healthPerCrate = StructureStats.Instance.GetBarricadeHealthPerCrate();
        barricadeMaxHealth = level1CrateAmount * healthPerCrate;
        barricadeHealth = level1CrateAmount * healthPerCrate;

        OnAnyBarricadeBuilt?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {
        OnBarricadeDestroyed?.Invoke(this, EventArgs.Empty);
        OnAnyBarricadeDestroyed?.Invoke(this, EventArgs.Empty);
        OnBarricadeBreached?.Invoke(this, EventArgs.Empty);
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    [Button]
    public void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if (barricadeHealth <= 0) return;
        barricadeHealth -= damage;

        if(barricadeHealth <= 0) {
            Die();
        }

        OnBarricadeDamageTaken?.Invoke(this, EventArgs.Empty);
        RefreshBarricadeRepair();
    }

    public Transform GetMeleeAttackPosition() {
        return meleeAttackPosition;
    }

    public float GetBarricadeHealthNormalized() {
        return (float)barricadeHealth/ (float)barricadeMaxHealth;
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();

        if (structureLevel == 2) {
            barricadeMaxHealth = level2CrateAmount * healthPerCrate;
            barricadeHealth = level2CrateAmount * healthPerCrate;
        }
        if (structureLevel == 3) {
            barricadeMaxHealth = level3CrateAmount * healthPerCrate;
            barricadeHealth = level3CrateAmount * healthPerCrate;

        }
        if (structureLevel == 4) {
            barricadeMaxHealth = level4CrateAmount * healthPerCrate;
            barricadeHealth = level4CrateAmount * healthPerCrate;
        }
    }

    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);
        RefreshBarricadeRepair();

        if(!barricadeRepairable) {
            barricadeHealth = barricadeMaxHealth;
        }
    }

    private void RefreshBarricadeRepair() {
        if(!barricadeVisual.GetBarricadeHasAllSprites() && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            // At least 1 sprite fell
            SetStructurePrimaryFunctionUnlocked(true);
            SetStructureUpgradableUnlocked(false);
            barricadeRepairable = true;
            needsRefill = true;
        }
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        payCurrencyUI.SetPlayerInteracting(false);

        if (currentStructureInteractionType == StructureInteractionType.primaryFunction) {
            RepairBarricade();
            return;
        }

        if (currentStructureInteractionType == StructureInteractionType.upgrade) {
            UpgradeStructure();
            return;
        }
    }

    private void RepairBarricade() {
        barricadeHealth = barricadeMaxHealth;
        OnBarricadeRepaired?.Invoke(this, EventArgs.Empty);
        OnAnyBarricadeRepaired?.Invoke(this, EventArgs.Empty);
        SetStructurePrimaryFunctionUnlocked(false);
        RefreshStructureUpgradeInteraction();
        barricadeRepairable = false;
        needsRefill = false;
    }

    public void SetAsOuterBarricade(bool outerBarricade) {
        barricadeVisual.SetAsOuterBarricade(outerBarricade);
    }

    public void SetAsInnerBarricade(bool innerBarricade) {
        isInnerBarricade = innerBarricade;
    }

    public bool GetIsInnerBarricade() {
        return isInnerBarricade;
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        base.GameInput_OnPlayerInteractCanceled(sender, e);

        bool playerWasHoldingInteract = GameInput.Instance.GetWasHoldingInteract();

        if (!playerWasHoldingInteract && playerInTriggerArea) {
            TurnOnOrOffLights();
        }

    }

    private void TurnOnOrOffLights() {
        OnBarricadeLightSwitched?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        
        if (collision.gameObject.GetComponentInParent<Fire>()) {
            OnFireLightTriggeredIn?.Invoke(this, EventArgs.Empty);
        }

        if ((collision.gameObject.GetComponent<Player>() != null)) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
        }

        if (collision.gameObject.GetComponent<Player>() != null && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            if (barricadeRepairable) {
                barricadeVisual.ShowRepairStructureVisual(true);
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject.GetComponentInParent<Fire>()) {
            OnFireLightTriggeredOut?.Invoke(this, EventArgs.Empty);
        }

        if ((collision.gameObject.GetComponent<Player>() != null)) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        }

        if (collision.gameObject.GetComponent<Player>()  != null && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            if (barricadeRepairable) {
                barricadeVisual.ShowRepairStructureVisual(false);
            }
        }
    }

    public bool GetBarricadeSpiked() {
        return barricadeSpiked;
    }
    public int GetSpikeDamage() {
        return spikeDamage;
    }

}
