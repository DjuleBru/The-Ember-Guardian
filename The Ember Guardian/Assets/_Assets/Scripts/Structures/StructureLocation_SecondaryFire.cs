using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_SecondaryFire : StructureLocation
{
    private bool secondaryFirelocationActive;
    public override Structure BuildStructure() {

        Fire secondaryFire = Instantiate(structureSOToBuild.structurePrefab, transform.position, Quaternion.identity).GetComponent<Fire>();
        secondaryFire.SetSecondaryFireStructureLocation(this);
        InvokeOnAnyStructureBuilt(secondaryFire);

        secondaryFirelocationActive = false;
        gameObject.SetActive(false);
        return secondaryFire;
    }

    public void ReActivateFireStructureLocation() {
        secondaryFirelocationActive = true;
        gameObject.SetActive(true);
    }
}
