using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure_Trap : Structure
{
    [SerializeField] protected TrapSO trapSO;
    protected StructureLocation_Trap trapStructureLocation;

    protected int trapDamage;
    protected int trapSpecial;
    protected int usesPerNight;
    protected int rearmPrice;
    protected int maxRearms;
    protected float trapCooldown;
    protected float trapActiveDuration;

    protected int currentUseIndex;
    protected int currentRearmIndex;

    protected float trapActiveTimer;
    protected float trapCooldownTimer;

    protected bool trapIsActive;
    protected bool triggeredEnded;
    protected bool trapCoolingDown;
    protected bool trapBroken;

    public event EventHandler OnTrapBroken;
    public event EventHandler OnTrapTriggered;
    public event EventHandler OnTrapTriggeredEnded;
    public event EventHandler OnTrapActiveEnded;
    public event EventHandler OnTrapDepletedUses;
    public event EventHandler OnTrapRearmed;
    public event EventHandler OnTrapMaxRearmsReached;
    public event EventHandler OnTrapRearmPriceChanged;

    protected override void Start() {
        base.Start();
        MerchantItem.OnAnyMerchantItemBought += MerchantItem_OnAnyMerchantItemBought;
        TrapManager.Instance.OnTrapUpgradeLevelsSet += TrapManager_OnTrapUpgradeLevelsSet;

        RefreshTrapStats();
        maxRearms = trapSO.maxRearmsBeforeBreaking;
        currentRearmIndex = maxRearms;
    }

    private void TrapManager_OnTrapUpgradeLevelsSet(object sender, EventArgs e) {
        RefreshTrapStats();
    }

    protected void Update() {
        if (trapBroken) return;

        if (trapIsActive) {
            trapActiveTimer -= Time.deltaTime;
            if(trapActiveTimer < 0) {
                trapIsActive = false;

                if(!GetHasUsesLeft()) {
                    OnTrapDepletedUses?.Invoke(this, EventArgs.Empty);
                } else {
                    trapCoolingDown = true;
                    OnTrapActiveEnded?.Invoke(this, EventArgs.Empty);

                }
            }
            return;
        };    

        if(trapCoolingDown) {
            trapCooldownTimer -= Time.deltaTime;

            if (trapCooldownTimer < .2f && !triggeredEnded) {
                triggeredEnded = true;
                OnTrapTriggeredEnded?.Invoke(this, EventArgs.Empty);
            }

            if (trapCooldownTimer < 0) {
                trapCoolingDown = false;
            }
        }
    }

    [Button]
    public void TryTriggerTrap() {
        if (trapIsActive) return;
        if (trapCoolingDown) return;
        if (trapBroken) return;

        if (currentUseIndex < usesPerNight) {
            currentUseIndex++;
            OnTrapTriggered?.Invoke(this, EventArgs.Empty);

            trapActiveTimer = trapActiveDuration;
            trapCooldownTimer = trapCooldown;

            trapIsActive = true;
            triggeredEnded = false;

            if (trapSO.trapBreaksAfterUses && !GetHasUsesLeft()) {
                StartCoroutine(BreakTrapAfterRandomDelay(.1f));
                return;
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
    }

    private void MerchantItem_OnAnyMerchantItemBought(object sender, System.EventArgs e) {
        TrapItem trapItem = sender as TrapItem;

        if (trapItem != null) {
            if(trapItem.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
                RefreshTrapStats();
            }
        }
    }

    protected override void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);

        if(trapSO.trapBreaksAfterRearms && currentRearmIndex == 0) {
            StartCoroutine(BreakTrapAfterRandomDelay());
            return;
        }

        if (currentUseIndex != 0) {
            OnTrapDepletedUses?.Invoke(this, EventArgs.Empty);
            ActivateStructurePrimaryFunctionInteraction(true);
            needsRefill = true;
        }
    }

    protected override void TriggerStructurePrimaryFunction() {
        currentUseIndex = 0;
        currentRearmIndex--;

        ActivateStructurePrimaryFunctionInteraction(false);

        OnTrapRearmed?.Invoke(this, EventArgs.Empty);
        OnTrapRearmed?.Invoke(this, EventArgs.Empty);
        needsRefill = false;
        base.TriggerStructurePrimaryFunction();
    }

    protected void RefreshTrapStats() {
        int trapDamageUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.damage);
        trapDamage = trapSO.trapDamage + trapDamageUpgrade;

        int trapSpecialUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.special);
        trapSpecial = trapSO.trapSpecialStat + trapSpecialUpgrade;

        int usesPerNightUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.usesPerNight);
        usesPerNight = trapSO.trapUsesPerNight + usesPerNightUpgrade;

        int rearmPriceUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.priceToReload);
        rearmPrice = trapSO.rearmPrice - rearmPriceUpgrade;
        if(rearmPriceUpgrade != 0) {
            OnTrapRearmPriceChanged?.Invoke(this, EventArgs.Empty);
        }

        int maxRearmsUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.totalUses);
        maxRearms = trapSO.maxRearmsBeforeBreaking - maxRearmsUpgrade;

        float trapCooldownUpgrade = TrapManager.Instance.GetCurrentUpgradeValue(trapSO.trapType, TrapUpgradeSO.TrapUpgradeType.cooldown);
        trapCooldown = trapSO.trapCooldown - trapCooldownUpgrade;

        trapActiveDuration = trapSO.trapActiveDuration;
    }

    public bool GetHasUsesLeft() {
        return currentUseIndex < usesPerNight;
    }

    public int GetRearmPrice() {
        return rearmPrice;
    }


    public int GetCurrentRearmIndex() {
        return currentRearmIndex;
    }
    public int GetCurrentUseIndex() {
        return currentUseIndex;
    }
    public void SetCurrentRearmIndex(int rearmIndex) {
        currentRearmIndex = rearmIndex;

        if (trapSO.trapBreaksAfterRearms && currentRearmIndex == 0) {
            StartCoroutine(BreakTrapAfterRandomDelay());
            return;
        }
    }

    public void SetCurrentUseIndex(int useIndex) {
        StartCoroutine(SetCurrentUseIndexAfterDelay(useIndex));
    }
    public IEnumerator SetCurrentUseIndexAfterDelay(int useIndex) {

        yield return new WaitForSeconds(.1f);

        currentUseIndex = useIndex;

        if (currentUseIndex != 0) {
            OnTrapDepletedUses?.Invoke(this, EventArgs.Empty);
            ActivateStructurePrimaryFunctionInteraction(true);
            needsRefill = true;
        }
    }

    public TrapSO GetTrapSO() {
        return trapSO;
    }

    [Button]
    private void BreakTrap() {
        StartCoroutine(BreakTrapAfterRandomDelay());
    }

    private IEnumerator BreakTrapAfterRandomDelay(float fixedDelay = 0) {
        trapBroken = true;
        float randomDelay = UnityEngine.Random.Range(1f, 2f);

        if(fixedDelay != 0) {
            randomDelay = 0;
        }
        yield return new WaitForSeconds(randomDelay);

        trapStructureLocation.ReActivateTrapStructureLocation();
        OnTrapBroken?.Invoke(this, EventArgs.Empty);
        PlayerCamp.Instance.RemoveStructure(this);

        StructuresManager.Instance.RemoveBuiltStructure(this);
        StructuresManager.Instance.AddStructureLocation(trapStructureLocation);

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    public void SetTrapStructureLocation(StructureLocation_Trap trapStructureLocation) {
        this.trapStructureLocation = trapStructureLocation;
    }

    public void ApplyTrapEffect(Creature creature) {
        creature.TakeDamage(trapDamage, transform);

        if (trapSO.trapType == TrapItem.TrapType.bearTrap) {
            creature.ApplyImmobilizeEffect(trapSpecial, transform.position);
        }

        if (trapSO.trapType == TrapItem.TrapType.smokeEjector) {
            creature.ApplyPoisonStackedEffect(trapSpecial, 10f);
        }

        if (trapSO.trapType == TrapItem.TrapType.fireEjector) {
            creature.ApplyBurning(trapSpecial);
        }

        if (trapSO.trapType == TrapItem.TrapType.shockerEjector) {
            creature.ApplyShockedEffect(trapSpecial);
        }
    }

    protected void OnDestroy() {
        MerchantItem.OnAnyMerchantItemBought -= MerchantItem_OnAnyMerchantItemBought;
        DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        Tent.Instance.OnStructureUpgraded -= Tent_OnStructureUpgraded;
        TrapManager.Instance.OnTrapUpgradeLevelsSet -= TrapManager_OnTrapUpgradeLevelsSet;
    }
}
