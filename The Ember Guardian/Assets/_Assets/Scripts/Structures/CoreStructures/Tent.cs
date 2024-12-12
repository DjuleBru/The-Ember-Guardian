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
        if(Player.Instance.GetHP() != PlayerStats.Instance.GetInitialPlayerMaxHP()) {
            ActivateStructurePrimaryFunctionInteraction(true);
        }
        else {
            ActivateStructurePrimaryFunctionInteraction(false);
        }

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
    }

    protected void Player_OnPlayerDamaged(object sender, EventArgs e) {
        ActivateStructurePrimaryFunctionInteraction(true);
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();
        Player.Instance.RefillPlayerHealth();
        ActivateStructurePrimaryFunctionInteraction(false);
    }

    protected override void RefreshStructureUpgradeInteraction() {

        string saveString = structureSO.structureType.ToString() + (structureLevel+1);

        if (!MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            Debug.Log("Tent has NOT been bought at merchant " + saveString);
            SetStructureUpgradableUnlocked(false);
        }
        else {
            Debug.Log("Tent has been bought at merchant " + saveString);
            SetStructureUpgradableUnlocked(true);
        }
    }

}
