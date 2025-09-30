using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_StartLevelFire : StructureLocation {

    protected override void Awake() {
        base.Awake();
    }

    public override Structure BuildStructure(bool buildOnLoad = false) {
        InvokeOnAnyStructureBuilt(Fire.Instance, buildOnLoad);
        showTooltipOnTrigger.HideTooltipShown();
        Fire.Instance.gameObject.SetActive(true);
        Fire.Instance.ActivateInitialFire();

        StructuresManager.Instance.AddBuiltStructure(Fire.Instance);

        StartCoroutine(DestroyGameObjectAfterFrame());
        return Fire.Instance;
    }
}
