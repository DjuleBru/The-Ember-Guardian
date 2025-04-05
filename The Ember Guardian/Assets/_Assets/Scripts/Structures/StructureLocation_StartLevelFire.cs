using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_StartLevelFire : StructureLocation {

    protected override void Awake() {
        base.Awake();
    }



    public override Structure BuildStructure() {
        InvokeOnAnyStructureBuilt(Fire.Instance);
        showTooltipOnTrigger.HideTooltipShown();
        Fire.Instance.gameObject.SetActive(true);
        Fire.Instance.ActivateInitialFire();

        StartCoroutine(DestroyGameObjectAfterFrame());
        return Fire.Instance;
    }
}
