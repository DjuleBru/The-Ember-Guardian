using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_StartLevelFire : StructureLocation
{
    protected override void BuildStructure() {
        InvokeOnAnyStructureBuilt();
        Fire.Instance.gameObject.SetActive(true);
        Fire.Instance.ActivateInitialFire();
        Destroy(gameObject);
    }
}
