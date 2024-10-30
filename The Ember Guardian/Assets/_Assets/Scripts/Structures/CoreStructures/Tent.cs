using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : Structure
{
    [SerializeField] private int maxLevel;

    protected override void Start() {
        base.Start();
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();
        StructuresManager.Instance.UnlockNextStructureLocations();
    }

}
