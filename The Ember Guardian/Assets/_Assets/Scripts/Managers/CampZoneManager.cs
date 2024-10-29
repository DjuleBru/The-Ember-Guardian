using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampZoneManager : MonoBehaviour
{

    public static CampZoneManager Instance;

    [SerializeField] private float campCenterMinLimit;
    [SerializeField] private float campCenterMaxLimit;

    private float minZoneLimit = 0;
    private float maxZoneLimit = 0;

    public event EventHandler OnCampZoneLimitsChanged;

    private void Awake() {
        Instance = this;

        minZoneLimit = campCenterMinLimit;
        maxZoneLimit = campCenterMaxLimit;
    }

    private void Start() {
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, System.EventArgs e) {
        StructureLocation structureLocation = sender as StructureLocation;

        if(structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.barricade) {
            if(structureLocation.transform.position.x < minZoneLimit) {
                minZoneLimit = structureLocation.transform.position.x;
                OnCampZoneLimitsChanged?.Invoke(this, EventArgs.Empty);
            }

            if (structureLocation.transform.position.x > maxZoneLimit) {
                maxZoneLimit = structureLocation.transform.position.x;
                OnCampZoneLimitsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public float GetMinZoneLimit() {
        return minZoneLimit;
    }

    public float GetMaxZoneLimit() {
        return maxZoneLimit;
    }

    public Vector3 GetClosestExteriorZoneLimit(Vector3 targetPosition) {
        Vector3 closestExteriorZoneLimit = new Vector3(0,0,0);

        if(targetPosition.x < 0) {
            closestExteriorZoneLimit.x = minZoneLimit;
        }
        else
        {
            closestExteriorZoneLimit.x = maxZoneLimit;
        }

        return closestExteriorZoneLimit;
        
    }

    public float GetCampCenterMinLimit() {
        return campCenterMinLimit;
    }

    public float GetCampCenterMaxLimit() {
        return campCenterMaxLimit;
    }
}
