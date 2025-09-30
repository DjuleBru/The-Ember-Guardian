using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_SecondaryFire : StructureLocation
{
    private bool secondaryFirelocationActive;
    public override Structure BuildStructure(bool buildOnLoad = false) {

        Fire secondaryFire = Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity).GetComponent<Fire>();
        secondaryFire.SetSecondaryFireStructureLocation(this);
        InvokeOnAnyStructureBuilt(secondaryFire, buildOnLoad);

        StructuresManager.Instance.RemoveStructureLocation(this);
        StructuresManager.Instance.AddBuiltStructure(secondaryFire);

        secondaryFirelocationActive = false;
        gameObject.SetActive(false);
        return secondaryFire;
    }

    public void ReActivateFireStructureLocation() {
        secondaryFirelocationActive = true;
        gameObject.SetActive(true);
    }
}
