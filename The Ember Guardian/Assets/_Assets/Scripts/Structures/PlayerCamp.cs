using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamp : MonoBehaviour
{
    public static PlayerCamp Instance;

    private int structureLocationsUnlockedLevel = 1;

    [SerializeField] private StructureLocation initialFireStructureLocation;
    [SerializeField] private List<Structure> initialStructures;
    [SerializeField] private List<StructureLocation> initialStructureLocationsBuilt;
    [SerializeField] private List<StructureLocation> level1StructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level2StructureLocationsUnlocked;
    [SerializeField] private List<StructureLocation> level3StructureLocationsUnlocked;

    private List<Structure> builtStructures = new List<Structure>();
    private List<Structure> builtTowers = new List<Structure>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        initialFireStructureLocation.UnlockStructureLocation();

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(false);
        }
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        foreach (StructureLocation location in level1StructureLocationsUnlocked) {
            location.UnlockStructureLocation();
        }


        StartCoroutine(BuildStructuresUnlockedCoroutine(.5f));
    }

    private IEnumerator BuildStructuresUnlockedCoroutine(float delayBetweenBuilds) {

        yield return new WaitForSeconds(delayBetweenBuilds);

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(true);
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
            foreach (StructureLocation location in level2StructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }

        if (Tent.Instance.GetStructureLevel() == 3) {
            foreach (StructureLocation location in level3StructureLocationsUnlocked) {
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

    public void UnlockNextStructureLocations() {
        structureLocationsUnlockedLevel++;

        if (structureLocationsUnlockedLevel == 2) {
            foreach (StructureLocation location in level2StructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }

        if (structureLocationsUnlockedLevel == 3) {
            foreach (StructureLocation location in level3StructureLocationsUnlocked) {
                location.UnlockStructureLocation();
            }
        }
    }

    public Tower GetClosestTower(CampZoneManager.CampSide workerCampSide, Vector3 position) {

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
}
