using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : Structure
{
    public static Tent Instance;
    [SerializeField] private int maxLevel;

    protected override void Awake() {
        base.Awake();
        Instance = this;
    }

    protected override void Start() {
        base.Start();

        if(Player.Instance.GetHP() != PlayerStats.Instance.GetMaxHP()) {
            ActivateStructurePrimaryFunctionInteraction(true);
        }
        else {
            ActivateStructurePrimaryFunctionInteraction(false);
        }

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerHealed += Player_OnPlayerHealed;
    }

    private void Player_OnPlayerHealed(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        RefreshPlayerStructurePrimaryInteraction();
    }

    protected void Player_OnPlayerDamaged(object sender, EventArgs e) {
        ActivateStructurePrimaryFunctionInteraction(true);
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();
        Player.Instance.HealPlayer(1);
    }

    protected void RefreshPlayerStructurePrimaryInteraction() {
        if(Player.Instance.GetHP() == PlayerStats.Instance.GetMaxHP()) {
            ActivateStructurePrimaryFunctionInteraction(false);
        } else {
            ActivateStructurePrimaryFunctionInteraction(true);
        }
    }

    protected override void RefreshStructureUpgradeInteraction() {
        string saveString = structureSO.structureType.ToString() + (structureLevel+1);
        bool upgradeUnlocked = MetaProgressionManager.Instance.GetMerchantItemBought(saveString);

        if (DebugManager.Instance.GetAllStructureUpgradesUnlocked()) {
            upgradeUnlocked = true;
        }

        if (!upgradeUnlocked) {
            SetStructureUpgradableUnlocked(false);
        }
        else {
            SetStructureUpgradableUnlocked(true);
        }
    }

}
