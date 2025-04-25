using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamp : MonoBehaviour
{
    public static PlayerCamp Instance;

    [SerializeField] private StructureLocation initialFireStructureLocation;
    [SerializeField] private List<Structure> initialStructures;

    [SerializeField] private List<StructureLocation> structureLocationsLockedBeforeBuildingBarricade;
    [SerializeField] private List<StructureLocation> trapLocations;
    [SerializeField] private List<StructureLocation> initialStructureLocations;
    [SerializeField] private List<StructureLocation> initialStructureLocationsBuilt;
    [SerializeField] private List<StructureLocation> level1DefensiveStructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level2DefensiveStructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level3DefensiveStructureLocationsUnlocked;

    [SerializeField] private StructureLocation ammoCrafter1Location;
    [SerializeField] private StructureLocation researchTowerLocation;
    [SerializeField] private StructureLocation leftBarricade1;
    [SerializeField] private StructureLocation rightBarricade1;
    [SerializeField] private StructureLocation leftBarricade2;
    [SerializeField] private StructureLocation rightBarricade2;
    [SerializeField] private StructureLocation leftBarricade3;
    [SerializeField] private StructureLocation rightBarricade3;

    private Vector3 leftBarricade1Position;
    private Vector3 rightBarricade1Position;
    private Vector3 leftBarricade2Position;
    private Vector3 rightBarricade2Position;
    private Vector3 leftBarricade3Position;
    private Vector3 rightBarricade3Position;

    private List<Structure> builtStructures = new List<Structure>();
    private List<Structure> builtTowers = new List<Structure>();

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
        //CampZoneManager.Instance.OnCampZoneLimitsChanged += CampZoneManager_OnCampZoneLimitsChanged;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(false);
        }

        InitializeBuiltAtStartStructureLocations();
    }

    private void InitializeBuiltAtStartStructureLocations() {
        ammoCrafterBuiltAtStart = StructureStats.Instance.GetStartWithAmmoCrafter();
        researchTowerBuiltAtStart = StructureStats.Instance.GetStartWithResearchTower();
        barricades1BuiltAtStart = StructureStats.Instance.GetStartWithBarricades();

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

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        StructureLocation structureLocation = (StructureLocation)sender;

        if (!(structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.barricade)) return;

        if(structureLocation == leftBarricade1) {
            TryUnlockStructureLocationsBetweenBarricades(structureLocationsLockedBeforeBuildingBarricade, leftBarricade1Position, Vector3.zero);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade2Position, leftBarricade1Position);
        }
        if (structureLocation == leftBarricade2) {
            TryUnlockStructureLocationsBetweenBarricades(structureLocationsLockedBeforeBuildingBarricade, leftBarricade2Position, leftBarricade1Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade3Position, leftBarricade2Position);
        }

        if (structureLocation == rightBarricade1) {
            TryUnlockStructureLocationsBetweenBarricades(structureLocationsLockedBeforeBuildingBarricade, Vector3.zero, rightBarricade1Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade1Position, rightBarricade2Position);
        }
        if (structureLocation == rightBarricade2) {
            TryUnlockStructureLocationsBetweenBarricades(structureLocationsLockedBeforeBuildingBarricade, rightBarricade1Position, rightBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade2Position, rightBarricade3Position);
        }
    }

    private void CampZoneManager_OnCampZoneLimitsChanged(object sender, EventArgs e) {
        if (!initialFireLit) return;

        foreach (StructureLocation structureLocation in structureLocationsLockedBeforeBuildingBarricade) {

            if(structureLocation ==  null) continue;
            if (CampZoneManager.Instance.IsWithinCampZoneLimits(structureLocation.transform.position)) {
                if (!structureLocation.GetStructureLocationUnlocked()) {
                    structureLocation.UnlockStructureLocation();
                }
            }

        }

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

        foreach (StructureLocation location in initialStructureLocations) {
            location.UnlockStructureLocation();
        }

        foreach (StructureLocation location in level1DefensiveStructureLocationsUnlocked) {
            location.UnlockStructureLocation();
        }

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
        if (Tent.Instance.GetStructureLevel() == 2) {
            foreach (StructureLocation location in level2DefensiveStructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }

        if (Tent.Instance.GetStructureLevel() == 3) {
            foreach (StructureLocation location in level3DefensiveStructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
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

    public bool GetStructureBuiltAtStart(Structure structure) {
        return initialStructures.Contains(structure);
    }

    private void OnDestroy() {
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
    }
}
