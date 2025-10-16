using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerVisual : StructureVisual
{
    [SerializeField] private Transform level2InteractIconPosition;
    [SerializeField] private Transform level3InteractIconPosition;
    [SerializeField] private Transform level4InteractIconPosition;

    protected override void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        int structureLevel = structure.GetStructureLevel();
        structureSpriteRenderer.sprite = structure.GetStructureSO().buildingUpgradeSpriteList[structureLevel - 1];

        if (structure.GetStructureLevel() == 2 ) {
            structureInteractionIconGO.transform.position = level2InteractIconPosition.position;
        }
        if (structure.GetStructureLevel() == 3) {
            structureInteractionIconGO.transform.position = level2InteractIconPosition.position;
        }
        if (structure.GetStructureLevel() == 4) {
            structureInteractionIconGO.transform.position = level2InteractIconPosition.position;
        }
    }
}
