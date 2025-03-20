using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : Structure, IDamageable {

    [SerializeField] private Transform projectileTarget;
    [SerializeField] private Transform meleeAttackPosition;
    [SerializeField] private Collider2D barricadeColliderLevel1;
    [SerializeField] private Collider2D barricadeColliderLevel2;
    [SerializeField] private Collider2D barricadeColliderLevel3;
    [SerializeField] private Collider2D barricadeColliderLevel4;
    [SerializeField] private BarricadeVisual barricadeVisual;

    private BoxCollider2D triggerCollider;
    [SerializeField] private float triggerColliderLevel1SizeY;
    [SerializeField] private float triggerColliderLevel1OffsetY;
    [SerializeField] private float triggerColliderLevel2SizeY;
    [SerializeField] private float triggerColliderLevel2OffsetY;
    [SerializeField] private float triggerColliderLevel3SizeY;
    [SerializeField] private float triggerColliderLevel3OffsetY;
    [SerializeField] private float triggerColliderLevel4SizeY;
    [SerializeField] private float triggerColliderLevel4OffsetY;

    private Collider2D currentBarricadeCollider;

    private int level1Health = 24;
    private int level2Health = 32;
    private int level3Health = 40;
    private int level4Health = 56;

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

    private bool barricadeRepairable;

    protected override void Start() {
        base.Start();
        triggerCollider = GetComponent<BoxCollider2D>();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;

        barricadeMaxHealth = level1Health;
        barricadeHealth = level1Health;
        currentBarricadeCollider = barricadeColliderLevel1;

        OnAnyBarricadeBuilt?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {
        Debug.Log("barricade destroyed !");
        OnBarricadeDestroyed?.Invoke(this, EventArgs.Empty);
        OnAnyBarricadeDestroyed?.Invoke(this, EventArgs.Empty);
        currentBarricadeCollider.enabled = false;
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false) {
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

        Vector2 triggerColliderOffset = new Vector2(0, 0);
        Vector2 triggerColliderSize = new Vector2(1.7f, 0);

        if (structureLevel == 2) {
            barricadeMaxHealth = level2Health;
            barricadeHealth = level2Health;

            triggerColliderOffset.y = triggerColliderLevel2OffsetY;
            triggerColliderSize.y = triggerColliderLevel2SizeY;

            ActivateCollider(barricadeColliderLevel2);
        }
        if (structureLevel == 3) {
            barricadeMaxHealth = level3Health;
            barricadeHealth = level3Health;

            triggerColliderOffset.y = triggerColliderLevel3OffsetY;
            triggerColliderSize.y = triggerColliderLevel3SizeY;

            ActivateCollider(barricadeColliderLevel3);

        }
        if (structureLevel == 4) {
            barricadeMaxHealth = level4Health;
            barricadeHealth = level4Health;

            triggerColliderOffset.y = triggerColliderLevel4OffsetY;
            triggerColliderSize.y = triggerColliderLevel4SizeY;

            ActivateCollider(barricadeColliderLevel4);
        }

        triggerCollider.offset = triggerColliderOffset;
        triggerCollider.size = triggerColliderSize;
    }

    private void ActivateCollider(Collider2D collider) {

        currentBarricadeCollider.gameObject.SetActive(false);
        currentBarricadeCollider = collider;
        currentBarricadeCollider.gameObject.SetActive(true);

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
        barricadeRepairable = true;
    }

    public void SetAsOuterBarricade(bool outerBarricade) {
        barricadeVisual.SetAsOuterBarricade(outerBarricade);
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

        if (collision.gameObject.GetComponent<Player>()  != null && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            if (barricadeRepairable) {
                barricadeVisual.ShowRepairStructureVisual(false);
            }
        }
    }

}
