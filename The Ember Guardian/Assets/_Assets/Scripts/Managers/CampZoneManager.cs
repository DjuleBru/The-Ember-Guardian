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

    private List<Barricade> functionalBarricades = new List<Barricade>();

    private float minZoneLimit = 0;
    private float maxZoneLimit = 0;

    [SerializeField] private HuntingFlag huntingFlagMax;
    [SerializeField] private HuntingFlag huntingFlaxMin;

    private float maxAnimalTargetingDistanceToCampOuterPoint = 60f;

    public event EventHandler OnCampZoneLimitsChanged;

    private void Awake() {
        Instance = this;

        Barricade.OnAnyBarricadeBuilt += Barricade_OnAnyBarricadeBuilt;
        Barricade.OnAnyBarricadeDestroyed += Barricade_OnAnyBarricadeDestroyed;
        Barricade.OnAnyBarricadeRepaired += Barricade_OnAnyBarricadeRepaired;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;

        huntingFlagMax.gameObject.SetActive(false);
        huntingFlaxMin.gameObject.SetActive(false);
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = sender as StructureLocation;

        if(structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.hunterShrine) {
            huntingFlagMax.gameObject.SetActive(true);
            huntingFlaxMin.gameObject.SetActive(true);
        }

        if(structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.fire) {
            RefreshCampZoneLimits();
        }
    }

    private void Start() {
        RefreshCampZoneLimits();
    }

    private void Barricade_OnAnyBarricadeBuilt(object sender, EventArgs e) {
        functionalBarricades.Add((sender as Barricade));
        RefreshCampZoneLimits();
    }

    private void Barricade_OnAnyBarricadeRepaired(object sender, EventArgs e) {
        functionalBarricades.Add((sender as Barricade));
        RefreshCampZoneLimits();
    }

    private void Barricade_OnAnyBarricadeDestroyed(object sender, EventArgs e) {
        functionalBarricades.Remove((sender as Barricade));
        RefreshCampZoneLimits();
    }

    private void RefreshCampZoneLimits() {
        float minZoneLimit = campCenterMinLimit;
        float maxZoneLimit = campCenterMaxLimit;

        foreach(Barricade barricade in functionalBarricades) {

            if (barricade.transform.position.x < minZoneLimit) {
                minZoneLimit = barricade.transform.position.x;
            }

            if (barricade.transform.position.x > maxZoneLimit) {
                maxZoneLimit = barricade.transform.position.x;
            }
        }

        this.minZoneLimit = minZoneLimit;
        this.maxZoneLimit = maxZoneLimit;

        foreach(Barricade barricade in functionalBarricades) {
            if (barricade.transform.position.x == this.minZoneLimit || barricade.transform.position.x == this.maxZoneLimit) {
                barricade.SetAsOuterBarricade(true);
            }
            else {
                barricade.SetAsOuterBarricade(false);
            }

        }

        huntingFlaxMin.TrySetCampHuntingLimit(new Vector3(minZoneLimit - maxAnimalTargetingDistanceToCampOuterPoint, 0, 0));
        huntingFlagMax.TrySetCampHuntingLimit(new Vector3(maxZoneLimit + maxAnimalTargetingDistanceToCampOuterPoint, 0, 0));

        OnCampZoneLimitsChanged?.Invoke(this, EventArgs.Empty);
    }


    public float GetMinZoneLimit() {
        return minZoneLimit;
    }

    public float GetMaxZoneLimit() {
        return maxZoneLimit;
    }

    public bool IsWithinCampZoneLimits(Vector3 position) {
        if(position.x >= minZoneLimit && position.x <= maxZoneLimit) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsWithinHuntingLimits(Vector3 position) {
        if (position.x >= GetHuntingMinZoneLimit() && position.x <= GetHuntingMaxZoneLimit()) {
            return true;
        }
        else {
            return false;
        }
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
    public Vector3 GetExteriorZoneCenterPoint(CampSide campSide) {
        Vector3 exteriorZoneCenterPoint = new Vector3(0, 0, 0);

        if (campSide == CampSide.left) {
            exteriorZoneCenterPoint.x = minZoneLimit + (GetHuntingMinZoneLimit() - minZoneLimit) / 2;
        }
        else {
            exteriorZoneCenterPoint.x = maxZoneLimit + (GetHuntingMaxZoneLimit() - maxZoneLimit)/2;
        }

        return exteriorZoneCenterPoint;
    }
    public Vector3 GetClosestHuntingLimit(Vector3 initialPosition, float distanceToSafety = 0f) {
        Vector3 closestExteriorZoneLimit = new Vector3(0, 0, 0);

        if (initialPosition.x < 0) {
            closestExteriorZoneLimit.x = GetHuntingMinZoneLimit() + distanceToSafety;
        }
        else {
            closestExteriorZoneLimit.x = GetHuntingMaxZoneLimit() - distanceToSafety;
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

    public float GetHuntingMaxZoneLimit() {
        return huntingFlagMax.GetCampHuntingLimit();
    }

    public float GetHuntingMinZoneLimit() {
        return huntingFlaxMin.GetCampHuntingLimit();
    }

    private void OnDestroy() {
        Barricade.OnAnyBarricadeBuilt -= Barricade_OnAnyBarricadeBuilt;
        Barricade.OnAnyBarricadeDestroyed -= Barricade_OnAnyBarricadeDestroyed;
        Barricade.OnAnyBarricadeRepaired -= Barricade_OnAnyBarricadeRepaired;
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
    }
}
