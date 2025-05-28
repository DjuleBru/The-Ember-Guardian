using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamp : MonoBehaviour
{
    public static PlayerCamp Instance;

    [SerializeField] private Transform initialStructureLocationsParent;
    [SerializeField] private Transform customCampStructureLocationParent;
    [SerializeField] private StructureLocation initialFireStructureLocation;
    [SerializeField] private List<Structure> initialStructures;

    [SerializeField] private List<StructureLocation> trapLocations;
    [SerializeField] private List<StructureLocation> towerLocations;
    [SerializeField] private List<StructureLocation> initialStructureLocationsBuilt;
    private List<StructureLocation> allStructureLocations;

    [SerializeField] private StructureLocation ammoCrafter1Location;
    [SerializeField] private StructureLocation researchTowerLocation;
    [SerializeField] private StructureLocation leftBarricade1;
    [SerializeField] private StructureLocation rightBarricade1;
    [SerializeField] private StructureLocation leftBarricade2;
    [SerializeField] private StructureLocation rightBarricade2;
    [SerializeField] private StructureLocation leftBarricade3;
    [SerializeField] private StructureLocation rightBarricade3;

    private float layoutMidPosition = 56.5f;
    private float maxCampLimitPosition = 100f;

    private Vector3 leftBarricade1Position;
    private Vector3 rightBarricade1Position;
    private Vector3 leftBarricade2Position;
    private Vector3 rightBarricade2Position;
    private Vector3 leftBarricade3Position;
    private Vector3 rightBarricade3Position;

    private List<Structure> builtStructures = new List<Structure>();
    private List<Structure> builtTowers = new List<Structure>();

    private bool customLayout;
    private bool ammoCrafterBuiltAtStart;
    private bool researchTowerBuiltAtStart;
    private bool barricades1BuiltAtStart;

    private bool initialFireLit;
    private void Awake() {
        Instance = this;
        InitializeBarricadePositions();
    }

    private void Start() {
        initialFireStructureLocation.UnlockStructureLocation();

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(false);
        }

        ammoCrafterBuiltAtStart = StructureStats.Instance.GetStartWithAmmoCrafter();
        researchTowerBuiltAtStart = StructureStats.Instance.GetStartWithResearchTower();
        barricades1BuiltAtStart = StructureStats.Instance.GetStartWithBarricades();

        LoadCustomCampLayout();
        InitializeBuiltAtStartStructureLocations();
    }

    private void LoadCustomCampLayout() {
        allStructureLocations = new List<StructureLocation>();
        List<CampEditManager.StructurePlacementData> structurePlacementData = ES3.Load("campLayout", new List<CampEditManager.StructurePlacementData>());
        customLayout = structurePlacementData.Count > 0;

        Debug.Log("structurePlacementData.Count " + structurePlacementData.Count);
        // Camp has never been customized
        if (!customLayout) {
            foreach (StructureLocation location in initialStructureLocationsParent.GetComponentsInChildren<StructureLocation>()) {
                allStructureLocations.Add(location);
            }
            return;
        }

        trapLocations.Clear();
        towerLocations.Clear();

        foreach (StructureLocation location in initialStructureLocationsParent.GetComponentsInChildren<StructureLocation>()) {
            location.gameObject.SetActive(false);
        }

        foreach (CampEditManager.StructurePlacementData data in structurePlacementData) {
            StructureSO structureSO = data.structureSO;
            int position = data.positionIndex;

            float worldPositionX = LayoutToWorldPosition(position, structureSO);
            Vector3 worldPosition = new Vector3(worldPositionX, 0, 0);

            if (structureSO.structureType == StructureSO.StructureType.tent) {
                Tent.Instance.transform.position = worldPosition;
            } else {
                if (structureSO.structureLocationPrefab == null) continue;
                StructureLocation structureLocation = Instantiate(structureSO.structureLocationPrefab, customCampStructureLocationParent).GetComponent<StructureLocation>();
                structureLocation.transform.position = worldPosition;
                FillCustomCampStructureLocationsList(structureSO, structureLocation);
            }

        }

    }

    private void FillCustomCampStructureLocationsList(StructureSO structureSO, StructureLocation structureLocation) {

        allStructureLocations.Add(structureLocation);

        if (structureSO.structureCategory == StructureSO.StructureCategory.trap) {
            trapLocations.Add(structureLocation);
        }
        if (structureSO.structureCategory == StructureSO.StructureCategory.tower) {
            towerLocations.Add(structureLocation);
        }

        if(structureSO.structureType == StructureSO.StructureType.ammoCrafter && ammoCrafterBuiltAtStart) {
            ammoCrafter1Location = structureLocation;
        }
        if (structureSO.structureType == StructureSO.StructureType.ammoCrafter && researchTowerBuiltAtStart) {
            researchTowerLocation = structureLocation;
        }
    }

    private void InitializeBuiltAtStartStructureLocations() {
        if (ammoCrafterBuiltAtStart) {
            initialStructureLocationsBuilt.Add(ammoCrafter1Location);
        }
        if (researchTowerBuiltAtStart) {
            initialStructureLocationsBuilt.Add(researchTowerLocation);
        }
        if (barricades1BuiltAtStart) {
            initialStructureLocationsBuilt.Add(rightBarricade1);
            initialStructureLocationsBuilt.Add(leftBarricade1);
        }
    }

    private void InitializeBarricadePositions() {
        leftBarricade1Position = leftBarricade1.transform.position;
        rightBarricade1Position = rightBarricade1.transform.position;
        leftBarricade2Position = leftBarricade2.transform.position;
        rightBarricade2Position = rightBarricade2.transform.position;
        leftBarricade3Position = leftBarricade3.transform.position;
        rightBarricade3Position = rightBarricade3.transform.position;
    }

    private void TryUnlockStructureLocationsBetweenBarricades(List<StructureLocation> structureLocations, Vector3 minBarricadePosition, Vector3 maxBarricadePosition) {
        if (!initialFireLit) return;

        foreach (StructureLocation structureLocation in structureLocations) {
            if (structureLocation == null) continue;
            if (IsWithinBarricadePosition(structureLocation.transform.position, minBarricadePosition, maxBarricadePosition)) {
                if (!structureLocation.GetStructureLocationUnlocked()) {
                    structureLocation.UnlockStructureLocation();
                }
            }

        }
    }

    private bool IsWithinBarricadePosition(Vector3 position, Vector3 minBarricadePosition, Vector3 maxBarricadePosition) {
        if (position.x >= minBarricadePosition.x && position.x <= maxBarricadePosition.x) {
            return true;
        }
        else {
            return false;
        }
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        initialFireLit = true;

        leftBarricade1.UnlockStructureLocation();
        rightBarricade1.UnlockStructureLocation();

        TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, leftBarricade1Position, Vector3.zero);
        TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade2Position, leftBarricade1Position);
        TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, Vector3.zero, rightBarricade1Position);
        TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade1Position, rightBarricade2Position);

        StartCoroutine(BuildStructuresUnlockedCoroutine(.5f));
    }

    private IEnumerator BuildStructuresUnlockedCoroutine(float delayBetweenBuilds) {

        yield return new WaitForSeconds(delayBetweenBuilds);

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(true);
            structure.BuildInitialCampStructure();
        }

        float delayBetweenBuildsRandomized = UnityEngine.Random.Range(delayBetweenBuilds - delayBetweenBuilds / 2, delayBetweenBuilds + delayBetweenBuilds / 2);
        yield return new WaitForSeconds(delayBetweenBuildsRandomized);

        foreach (StructureLocation location in initialStructureLocationsBuilt) {
            Structure structure = location.BuildStructure();
            initialStructures.Add(structure);

            delayBetweenBuildsRandomized = UnityEngine.Random.Range(delayBetweenBuilds - delayBetweenBuilds / 2, delayBetweenBuilds + delayBetweenBuilds / 2);

            yield return new WaitForSeconds(delayBetweenBuildsRandomized);
        }

    }

    private void Tent_OnStructureUpgraded(object sender, EventArgs e) {
        Vector3 maxCampLimit = new Vector3(maxCampLimitPosition, 0, 0);

        if (Tent.Instance.GetStructureLevel() == 2) {
            leftBarricade2.UnlockStructureLocation();
            rightBarricade2.UnlockStructureLocation();

            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, leftBarricade2Position, leftBarricade1Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade3Position, leftBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, leftBarricade3Position, leftBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, rightBarricade1Position, rightBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade2Position, rightBarricade3Position);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, rightBarricade2Position, rightBarricade3Position);
        }

        if (Tent.Instance.GetStructureLevel() == 3) {
            leftBarricade3.UnlockStructureLocation();
            rightBarricade3.UnlockStructureLocation();

            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, leftBarricade3Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade3Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, leftBarricade3Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, rightBarricade3Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade3Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, rightBarricade3Position, maxCampLimit);
        }
    }

    public void AddStructure(Structure structure) {
        builtStructures.Add(structure);

        if (structure is Tower) {
            builtTowers.Add(structure);
        }
    }
    public int GetAvailableTowers(CampZoneManager.CampSide workerCampSide) {
        int availableTowers = 0;

        foreach (Tower tower in builtTowers) {

            if (tower.GetCampSide() != workerCampSide) continue;
            if (tower.GetTowerFull()) continue;
            availableTowers++;
        }

        return availableTowers;
    }
    public Tower GetClosestAvailableTower(CampZoneManager.CampSide workerCampSide, Vector3 position) {

        float closestDistanceToTower = Mathf.Infinity;
        Tower closestTower = null;

        foreach (Tower tower in builtTowers) {

            if (tower.GetCampSide() != workerCampSide) continue;
            if (tower.GetTowerFull()) continue;

            float distanceToTower = Mathf.Abs(position.x - tower.transform.position.x);

            if (distanceToTower < closestDistanceToTower) {
                closestTower = tower;
                closestDistanceToTower = distanceToTower;
            }
        }

        return closestTower;
    }

    public float LayoutToWorldPosition(int startLayoutPosition, StructureSO structureSO) {
        float objectMidPointLayoutPosition;

        objectMidPointLayoutPosition = startLayoutPosition + structureSO.widthInCells / 2f;

        float relativeLayoutPosition = objectMidPointLayoutPosition - layoutMidPosition;

        float layoutToWorldConversionFactor = 5f / 3.5f;
        float worldLayoutPosition = relativeLayoutPosition * layoutToWorldConversionFactor;
        return worldLayoutPosition;
    }
}
