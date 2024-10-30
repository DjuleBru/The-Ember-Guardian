using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampZoneManager : MonoBehaviour
{

    public enum CampSide {
        left,
        right,
    }

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

    public Vector3 GetClosestExteriorZoneLimit(Vector3 initialPosition, float distanceToSafety = 0f) {
        Vector3 closestExteriorZoneLimit = new Vector3(0,0,0);

        if(initialPosition.x < 0) {
            closestExteriorZoneLimit.x = minZoneLimit + distanceToSafety;
        }
        else
        {
            closestExteriorZoneLimit.x = maxZoneLimit - distanceToSafety;
        }

        return closestExteriorZoneLimit;
    }

    public Vector3 GetClosestExteriorZoneLimit(CampSide campSide, float distanceToSafety = 0f) {
        Vector3 closestExteriorZoneLimit = new Vector3(0, 0, 0);

        if (campSide == CampSide.left) {
            closestExteriorZoneLimit.x = minZoneLimit + distanceToSafety;
        }
        else {
            closestExteriorZoneLimit.x = maxZoneLimit - distanceToSafety;
        }

        return closestExteriorZoneLimit;
    }

    public float GetCampCenterMinLimit() {
        return campCenterMinLimit;
    }

    public float GetCampCenterMaxLimit() {
        return campCenterMaxLimit;
    }

    public CampSide AssignCampSide(Vector3 position) {
        if(position.x < 0) {
            return CampSide.left;
        }
        else {
            return CampSide.right;
        }
    }
}
