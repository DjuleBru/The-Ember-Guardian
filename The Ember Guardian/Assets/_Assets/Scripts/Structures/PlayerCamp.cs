using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private List<StructureLocation> worldStructureLocations;
    private List<StructureLocation> allStructureLocations = new List<StructureLocation>();

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
    private List<Structure> currencyStorages = new List<Structure>();
    private List<Structure> builtTowers = new List<Structure>();
    private List<Structure> builtSpecialTowers = new List<Structure>();
    private List<FastTravelTP> allFastTravelTPsBuilt = new List<FastTravelTP>();

    private bool blockStructureUnlocks;
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
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(false);
        }

        ammoCrafterBuiltAtStart = StructureStats.Instance.GetStartWithAmmoCrafter();
        researchTowerBuiltAtStart = StructureStats.Instance.GetStartWithResearchTower();
        barricades1BuiltAtStart = StructureStats.Instance.GetStartWithBarricades();

        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) {

            initialFireStructureLocation.gameObject.SetActive(false);

        } else {

            initialFireStructureLocation.UnlockStructureLocation();
            LoadCustomCampLayout();
            StartCoroutine(InitializeBuiltAtStartStructureLocations());
            InitializeWorldStructureLocations();
        }
    }

    private void LoadCustomCampLayout() {
        List<CampEditManager.StructurePlacementData> structurePlacementData = ES3.Load("campLayout", new List<CampEditManager.StructurePlacementData>());
        customLayout = structurePlacementData.Count > 0;

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
            StructureSO structureSO = StructuresManager.Instance.GetStructureSO(data.structureType);
            int position = data.positionIndex;

            float worldPositionX = LayoutToWorldPosition(position, structureSO);
            Vector3 worldPosition = new Vector3(worldPositionX, 0, 0);

            if (structureSO.structureType == StructureSO.StructureType.tent) {

                Tent.Instance.transform.position = worldPosition;

            } else {

                if (structureSO.structureLocationPrefab == null) continue;
                StructureLocation structureLocation = Instantiate(structureSO.structureLocationPrefab, customCampStructureLocationParent).GetComponent<StructureLocation>();
                structureLocation.transform.position = worldPosition;
                AddCustomCampStructureLocationsList(structureSO, structureLocation);
            }

        }
    }

    public void AddStructureLocationLoaded(StructureSO structureSO, Vector2 position, bool unlocked) {
        StructureLocation structureLocation = Instantiate(structureSO.structureLocationPrefab, customCampStructureLocationParent).GetComponent<StructureLocation>();
        structureLocation.transform.position = position;

        if(unlocked) {
            structureLocation.UnlockStructureLocation();
        }

        AddCustomCampStructureLocationsList(structureSO, structureLocation);
    }

    public void AddCustomCampStructureLocationsList(StructureSO structureSO, StructureLocation structureLocation) {

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

    private void InitializeWorldStructureLocations() {
        foreach(StructureLocation location in worldStructureLocations) {
            location.UnlockStructureLocation();
            location.SetAsWorldStructureLocation();
        }
    }

    private IEnumerator InitializeBuiltAtStartStructureLocations() {
        yield return new WaitForSeconds(.1f);
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

        // Merchants
        foreach (StructureLocation structureLocation in allStructureLocations) {
            if (structureLocation == null) continue;

            if (structureLocation.GetStructureLocationBought()) {
                if (structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.merchant_skills || structureLocation.GetStructureSOToBuild().structureType == StructureSO.StructureType.merchant_traps) {
                    initialStructureLocationsBuilt.Add(structureLocation);
                }
            }
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
            if (structureLocation.GetStructureLocationUnlocked()) continue;

            //Debug.Log(structureLocation + " " + structureLocation.transform.position + " IsWithinBarricadePosition " + IsWithinBarricadePosition(structureLocation.transform.position, minBarricadePosition, maxBarricadePosition) + " min " + minBarricadePosition + " max " + maxBarricadePosition);
            
            if (IsWithinBarricadePosition(structureLocation.transform.position, minBarricadePosition, maxBarricadePosition)) {
                structureLocation.UnlockStructureLocation();
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

        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;
        StartCoroutine(BuildStructuresUnlockedCoroutine(.5f));

        if (blockStructureUnlocks) return;

        leftBarricade1.UnlockStructureLocation();
        rightBarricade1.UnlockStructureLocation();

        TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, leftBarricade1Position, Vector3.zero);
        TryUnlockStructureLocationsBetweenBarricades(trapLocations, leftBarricade2Position, leftBarricade1Position);
        TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, Vector3.zero, rightBarricade1Position);
        TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade1Position, rightBarricade2Position);

    }

    private IEnumerator BuildStructuresUnlockedCoroutine(float delayBetweenBuilds) {

        yield return new WaitForSeconds(delayBetweenBuilds);

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(true);
            structure.BuildInitialCampStructure();
            StructuresManager.Instance.AddBuiltStructure(structure);
        }

        float delayBetweenBuildsRandomized = UnityEngine.Random.Range(delayBetweenBuilds - delayBetweenBuilds / 2, delayBetweenBuilds + delayBetweenBuilds / 2);
        yield return new WaitForSeconds(delayBetweenBuildsRandomized);

        foreach (StructureLocation location in initialStructureLocationsBuilt) {
            if (location == null) continue;
            Structure structure = location.BuildStructure();
            initialStructures.Add(structure);

            delayBetweenBuildsRandomized = UnityEngine.Random.Range(delayBetweenBuilds - delayBetweenBuilds / 2, delayBetweenBuilds + delayBetweenBuilds / 2);

            yield return new WaitForSeconds(delayBetweenBuildsRandomized);
        }

    }

    private void Tent_OnStructureUpgraded(object sender, Structure.OnStructureUpgradedEventArgs e) {
        if (e.upgradedOnLoad) return;

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

            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, -maxCampLimit, leftBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, -maxCampLimit, leftBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, -maxCampLimit, leftBarricade2Position);
            TryUnlockStructureLocationsBetweenBarricades(allStructureLocations, rightBarricade2Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(trapLocations, rightBarricade2Position, maxCampLimit);
            TryUnlockStructureLocationsBetweenBarricades(towerLocations, rightBarricade2Position, maxCampLimit);
        }
    }

    public void AddStructure(Structure structure) {
        builtStructures.Add(structure);

        if (structure is Tower) {
            builtTowers.Add(structure);
        }

        if (structure is SpecialTower) {
            builtSpecialTowers.Add(structure);
        }

        if (structure is FastTravelTP) {
            allFastTravelTPsBuilt.Add(structure as FastTravelTP);
        }
    }

    public void RemoveStructure(Structure structure) {
        builtStructures.Remove(structure);
    }

    public int GetAvailableTowers(CampZoneManager.CampSide workerCampSide) {
        int availableTowers = 0;

        foreach (Tower tower in builtTowers) {

            if (tower.GetCampSide() != workerCampSide) continue;
            if (tower.GetTowerFull()) continue;
            if (tower.GetIsWorldStructure()) continue;
            availableTowers++;
        }

        return availableTowers;
    }

    public int GetAvailableWorldTowers(Vector3 escortDestination) {
        int availableTowers = 0;

        foreach (Tower tower in builtTowers) {
            if (tower.GetTowerFull()) continue;
            if (!tower.GetIsWorldStructure()) continue;

            float distanceToEscortDestination = Mathf.Abs(escortDestination.x - tower.transform.position.x);
            if (distanceToEscortDestination > 50f) continue;

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
            if (tower.GetIsWorldStructure()) continue;

            float distanceToTower = Mathf.Abs(position.x - tower.transform.position.x);

            if (distanceToTower < closestDistanceToTower) {
                closestTower = tower;
                closestDistanceToTower = distanceToTower;
            }
        }

        return closestTower;
    }
    public Tower GetClosestAvailableWorldTower(Vector3 escortDestination, Vector3 position) {

        float closestDistanceToTower = Mathf.Infinity;
        Tower closestTower = null;

        foreach (Tower tower in builtTowers) {

            if (tower.GetTowerFull()) continue;
            if (!tower.GetIsWorldStructure()) continue;

            float distanceToTower = Mathf.Abs(position.x - tower.transform.position.x);
            float distanceToEscortDestination = Mathf.Abs(escortDestination.x - tower.transform.position.x);

            if (distanceToEscortDestination > 50f) continue;

            if (distanceToTower < closestDistanceToTower) {
                closestTower = tower;
                closestDistanceToTower = distanceToTower;
            }
        }

        return closestTower;
    }

    public void BlockStructureUnlocks() {
        blockStructureUnlocks = true;
    } 

    public Structure GetHighestPriorityAvailableEngineerStructure(bool nightOrDusk) {

        int highestPriority = 0;
        Structure highestPriorityStructure = null;

        foreach(Structure structure in builtStructures) {

            if (!nightOrDusk && !structure.GetStructureSO().engineerCanWorkByDay) continue;
            if (nightOrDusk && !structure.GetStructureSO().engineerCanWorkByNight) continue;

            if (!structure.NeedsEngineering()) continue;
            if (structure.GetEngineersWorking().Count == structure.GetMaxEngineersWorking()) continue;

            if (structure.NeedsRefill()) {
                CurrencyStorage storage = GetCurrencyStorageWithCurrencies(structure.GetRefillCurrencyTypeNeeded(), structure.GetMinimumRefillAmountRequired());
                if (storage == null && !structure.NeedsWorking()) continue;
            };


            int structurePriority = structure.GetStructureSO().engineerWorkingPriority;
            if(structurePriority > highestPriority) {
                highestPriority = structurePriority;
                highestPriorityStructure = structure;
            }
        }
        return highestPriorityStructure;
    }

    public Structure GetClosestDefensiveStructureNeedingCurrency(Vector3 engineerPosition, List<PlayerCurrencies.CurrencyType> currencyTypeList) {
        float closestDistance = Mathf.Infinity;
        Structure closestAvailableStructure = null;

        foreach (Structure structure in builtStructures) {
            if (!structure.GetStructureSO().engineerCanWorkByDay) continue;
            if (structure.GetStructureSO().structureCategory != StructureSO.StructureCategory.tower) continue;

            if (structure.NeedsRefill()) {
                if (!currencyTypeList.Contains(structure.GetRefillCurrencyTypeNeeded())) continue;
            };

            float distanceToStructure = Mathf.Abs(engineerPosition.x - structure.transform.position.x);
            if (distanceToStructure < closestDistance) {
                closestDistance = distanceToStructure;
                closestAvailableStructure = structure;
            }
        }

        return closestAvailableStructure;
    }

    public CurrencyStorage GetClosestCurrencyStorageWithSpace(Vector3 engineerPosition, PlayerCurrencies.CurrencyType currencyType) {
        float closestDistance = Mathf.Infinity;
        CurrencyStorage closestAvailableCurrencyStorage = null;

        foreach (Structure structure in currencyStorages) {

            CurrencyStorage storage = structure as CurrencyStorage;
            if(storage.GetCurrencyTypeStored() != currencyType) continue;
            if(storage.GetEngineerCanPickUpOrbs()) continue;

            float distanceToStructure = Mathf.Abs(engineerPosition.x - structure.transform.position.x);
            if (distanceToStructure < closestDistance) {
                closestDistance = distanceToStructure;
                closestAvailableCurrencyStorage = storage;
            }
        }

        return closestAvailableCurrencyStorage;
    }
    public void AddCurrencyStorage(CurrencyStorage storage) {
        currencyStorages.Add(storage);
    }

    public CurrencyStorage GetCurrencyStorageWithCurrencies(PlayerCurrencies.CurrencyType currencyType, int currencyAmountRequired) {
        CurrencyStorage storage = null;

        foreach(CurrencyStorage currencyStorage in currencyStorages) {
            if (currencyStorage is CurrencyStorage_Objective) continue;
            if(currencyStorage.GetCurrencyTypeStored() == currencyType && currencyStorage.GetCurrencyAmountStored() >= currencyAmountRequired) {
                storage = currencyStorage;
            }
        }

        return storage;
    }

    public List<FastTravelTP> GetAllFastTravelTPsBuilt() {
        List<FastTravelTP> listCopy = new List<FastTravelTP>();

        foreach(FastTravelTP tp in allFastTravelTPsBuilt) {
            listCopy.Add(tp);
        }

        return listCopy;
    }

    public float LayoutToWorldPosition(int startLayoutPosition, StructureSO structureSO) {
        float objectMidPointLayoutPosition;

        objectMidPointLayoutPosition = startLayoutPosition + structureSO.widthInCells / 2f;

        float relativeLayoutPosition = objectMidPointLayoutPosition - layoutMidPosition;

        float layoutToWorldConversionFactor = 5f / 3.5f;
        float worldLayoutPosition = relativeLayoutPosition * layoutToWorldConversionFactor;
        return worldLayoutPosition;
    }

    public void ReplaceStructureLocation(StructureLocation oldLocation, StructureLocation newLocation) {
        if (allStructureLocations.Contains(oldLocation)) {
            int index = allStructureLocations.IndexOf(oldLocation);
            allStructureLocations[index] = newLocation;
        }

        if (trapLocations.Contains(oldLocation)) {
            trapLocations.Remove(oldLocation);
            if (newLocation.GetStructureSOToBuild().structureCategory == StructureSO.StructureCategory.trap)
                trapLocations.Add(newLocation);
        }

        if (towerLocations.Contains(oldLocation)) {
            towerLocations.Remove(oldLocation);
            if (newLocation.GetStructureSOToBuild().structureCategory == StructureSO.StructureCategory.tower)
                towerLocations.Add(newLocation);
        }

        StructuresManager.Instance.RemoveStructureLocation(oldLocation);
    }

    public bool GetHasSkillMerchantInLayoutAndUnlocked() {
        foreach(StructureLocation location in allStructureLocations) {
            if(location.GetStructureSOToBuild().structureType == StructureSO.StructureType.merchant_skills) {
                if(location.GetStructureLocationBought()) {
                    return true;
                }
            }
        }

        return false;
    }
    public List<StructureLocation> GetAllStructureLocations() {
        return allStructureLocations;
    }
}
