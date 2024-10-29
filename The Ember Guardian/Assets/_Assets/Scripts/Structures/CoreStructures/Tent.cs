using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tent : Structure
{
    [SerializeField] private int maxLevel;
    [SerializeField] private List<StructureLocation> level1StructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level2StructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level3StructureLocationsUnlocked;

    protected override void Start() {
        base.Start();

        foreach(StructureLocation location in level1StructureLocationsUnlocked) {
            location.UnlockStructureLocation();
        }
    }

    protected override void UpgradeStructure() {
        base.UpgradeStructure();

        if(structureLevel == 2) {
            foreach (StructureLocation location in level2StructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }

        if (structureLevel == 3) {
            foreach (StructureLocation location in level3StructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }
    }

}
