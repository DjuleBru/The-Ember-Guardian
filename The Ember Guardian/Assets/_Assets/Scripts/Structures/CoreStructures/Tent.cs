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


}
