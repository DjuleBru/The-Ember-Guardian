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

    private bool barricadeRepairable;

    protected override void Start() {
        base.Start();

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

        if(structureLevel == 2) {
            barricadeMaxHealth = level2Health;
            barricadeHealth = level2Health;

            ActivateCollider(barricadeColliderLevel2);
        }
        if (structureLevel == 3) {
            barricadeMaxHealth = level3Health;
            barricadeHealth = level3Health;

            ActivateCollider(barricadeColliderLevel3);

        }
        if (structureLevel == 4) {
            barricadeMaxHealth = level4Health;
            barricadeHealth = level4Health;
            ActivateCollider(barricadeColliderLevel4);
        }
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
