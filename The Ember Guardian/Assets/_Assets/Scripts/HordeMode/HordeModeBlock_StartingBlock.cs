using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HordeModeBlock_StartingBlock : HordeModeBlock
{

    [SerializeField] private List<MobSpawner> animalSpawnerList_Left;
    [SerializeField] private List<MobSpawner> animalSpawnerList_Right;
    [SerializeField] private List<Transform> scavengablePositionsList_Left;
    [SerializeField] private List<Transform> scavengablePositionsList_Right;
    [SerializeField] private List<Transform> creatureSpawnerPositions;

    [SerializeField] private WorkerSpawner workerSpawnerLeft_VG;
    [SerializeField] private WorkerSpawner workerSpawnerRight_VG;
    [SerializeField] private WorkerSpawner workerSpawnerLeft_CC;
    [SerializeField] private WorkerSpawner workerSpawnerRight_CC;
    [SerializeField] private WorkerSpawner workerSpawnerLeft_LH;
    [SerializeField] private WorkerSpawner workerSpawnerRight_LH;
    [SerializeField] private WorkerSpawner workerSpawnerLeft_FD;
    [SerializeField] private WorkerSpawner workerSpawnerRight_FD;
    [SerializeField] private int centralBlockInitialWorkers = 4;

    protected override void Start() {
        base.Start();

        workerSpawnerLeft_VG.gameObject.SetActive(false);
        workerSpawnerRight_VG.gameObject.SetActive(false);

        workerSpawnerLeft_CC.gameObject.SetActive(false);
        workerSpawnerRight_CC.gameObject.SetActive(false);

        workerSpawnerLeft_LH.gameObject.SetActive(false);
        workerSpawnerRight_LH.gameObject.SetActive(false);

        workerSpawnerLeft_FD.gameObject.SetActive(false);
        workerSpawnerRight_FD.gameObject.SetActive(false);

        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;

        HandleCentralBlockResources();
        HandleCentralBlockWorkerSpawners();
    }

    protected void HandleCentralBlockResources() {
        float randomValue = UnityEngine.Random.value;
        float randomDirValue = UnityEngine.Random.value;

        bool scavengablesUnlocked = HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine);
        
        if(randomDirValue < .5f) {
            randomDirValue = -1;
        }

        if(!scavengablesUnlocked) {
            randomValue = 1;
        }

        if (randomValue > .75) {
            // Only animals
            HandleAnimalsCentralBlock(3, true, randomDirValue);
        }
        if (randomValue > .5 && randomValue <= .75) {
            // 2x Animals and  1x Scavengables

            HandleAnimalsCentralBlock(2, true, randomDirValue);
            HandleScavengablesCentralBlock(1, false, randomDirValue);
        }
        if (randomValue > .25 && randomValue <= .5) {
            // 1x Animal and 2x Scavengables

            HandleAnimalsCentralBlock(1, false, randomDirValue);
            HandleScavengablesCentralBlock(2, true, randomDirValue);
        }
        if (randomValue <= .25) {
            // Only scavengables
            HandleScavengablesCentralBlock(3, true, randomDirValue);
        }
    }

    private void HandleCentralBlockWorkerSpawners() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            workerSpawnerLeft_CC.gameObject.SetActive(true);
            workerSpawnerRight_CC.gameObject.SetActive(true);
            workerSpawnerLeft_CC.SpawnMobs(centralBlockInitialWorkers);
            workerSpawnerRight_CC.SpawnMobs(centralBlockInitialWorkers);
        }
        if (env == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            workerSpawnerLeft_VG.gameObject.SetActive(true);
            workerSpawnerRight_VG.gameObject.SetActive(true);
            workerSpawnerLeft_VG.SpawnMobs(centralBlockInitialWorkers);
            workerSpawnerRight_VG.SpawnMobs(centralBlockInitialWorkers);
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            workerSpawnerLeft_LH.gameObject.SetActive(true);
            workerSpawnerRight_LH.gameObject.SetActive(true);
            workerSpawnerLeft_LH.SpawnMobs(centralBlockInitialWorkers);
            workerSpawnerRight_LH.SpawnMobs(centralBlockInitialWorkers);
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            workerSpawnerLeft_FD.gameObject.SetActive(true);
            workerSpawnerRight_FD.gameObject.SetActive(true);
            workerSpawnerLeft_FD.SpawnMobs(centralBlockInitialWorkers);
            workerSpawnerRight_FD.SpawnMobs(centralBlockInitialWorkers);
        }
    }

    protected void HandleAnimalsCentralBlock(int spawnerAmount, bool bothSides, float dir) {
        List<MobSpawner> chosen = new List<MobSpawner>();

        if (bothSides) {
            int amountMainSide = Mathf.CeilToInt(spawnerAmount * 0.66f); // 2/3 côté prioritaire
            int amountOtherSide = spawnerAmount - amountMainSide;

            if (dir > 0) {
                chosen.AddRange(PickFromSide(animalSpawnerList_Right, amountMainSide));
                chosen.AddRange(PickFromSide(animalSpawnerList_Left, amountOtherSide));
            }
            else {
                chosen.AddRange(PickFromSide(animalSpawnerList_Left, amountMainSide));
                chosen.AddRange(PickFromSide(animalSpawnerList_Right, amountOtherSide));
            }
        }
        else {
            // Tout d’un seul côté selon dir
            if (dir > 0)
                chosen.AddRange(PickFromSide(animalSpawnerList_Right, spawnerAmount));
            else
                chosen.AddRange(PickFromSide(animalSpawnerList_Left, spawnerAmount));
        }

        // On applique les animaux aux spawners choisis
        foreach (MobSpawner spawner in chosen)
            SelectRandomAnimalForSpawner(spawner);
    }

    protected void HandleScavengablesCentralBlock(int scavAmount, bool bothSides, float dir) {
        HordeModeBlockScavengables scav = GetComponent<HordeModeBlockScavengables>();
        if (scav == null) return;

        if (bothSides && scavAmount == 2) {
            if (scav == null) return;

            if (dir > 0) {
                SpawnScavOnSide(scav, scavengablePositionsList_Right, 1);
                SpawnScavOnSide(scav, scavengablePositionsList_Left, 1);
            }
            else {
                SpawnScavOnSide(scav, scavengablePositionsList_Left, 1);
                SpawnScavOnSide(scav, scavengablePositionsList_Right, 1);
            }
            return;
        }

        int mainAmount = (bothSides ? Mathf.CeilToInt(scavAmount * 0.66f) : scavAmount);
        int otherAmount = scavAmount - mainAmount;

        if (dir > 0) {
            // droite prioritaire
            SpawnScavOnSide(scav, scavengablePositionsList_Right, mainAmount);
            if (bothSides)
                SpawnScavOnSide(scav, scavengablePositionsList_Left, otherAmount);
        }
        else {
            // gauche prioritaire
            SpawnScavOnSide(scav, scavengablePositionsList_Left, mainAmount);
            if (bothSides)
                SpawnScavOnSide(scav, scavengablePositionsList_Right, otherAmount);
        }
    }

    private void SpawnScavOnSide(HordeModeBlockScavengables scavSys, List<Transform> positions, int num) {
        for (int i = 0; i < num && i < positions.Count; i++) {
            scavSys.SpawnSingleScav(positions[i], 10, 20);
        }
    }

    private void SelectRandomAnimalForSpawner(MobSpawner spawner) {
        // Récupère toutes les valeurs de l'énum
        Array values = Enum.GetValues(typeof(AnimalSO.AnimalType));
        // Sélectionne une valeur aléatoire
        AnimalSO.AnimalType randomAnimal = (AnimalSO.AnimalType)values.GetValue(UnityEngine.Random.Range(0, values.Length));

        int animalAmountToSpawn = 0;
        if (randomAnimal == AnimalSO.AnimalType.Rat) {
            animalAmountToSpawn = 8;
        }
        if (randomAnimal == AnimalSO.AnimalType.Fox) {
            animalAmountToSpawn = 5;
        }
        if (randomAnimal == AnimalSO.AnimalType.Elk) {
            animalAmountToSpawn = 3;
        }
        if (randomAnimal == AnimalSO.AnimalType.Deer) {
            animalAmountToSpawn = 2;
        }

        float radiusToRoamAround = (blockWidth / 2f);

        animalAmountToSpawn = Mathf.RoundToInt(animalAmountToSpawn * GetSizeRewardMultiplier());

        spawner.SetSpawnerParameters(AnimalManager.Instance.GetAnimalPrefab(randomAnimal), animalAmountToSpawn, radiusToRoamAround);
    }

    public override void GenerateCreatureSpawners(List<CreatureSO> selectedCreatures, List<int> spawnAmounts, List<int> eliteAmounts, List<Vector3> positions = null) {
        Debug.Log("Central Block GenerateCreatureSpawners " + spawnAmounts.Count);

        foreach (var creatureIndex in System.Linq.Enumerable.Range(0, selectedCreatures.Count)) {

            Vector3 randomSpawnPos = creatureSpawnerPositions[UnityEngine.Random.Range(0, creatureSpawnerPositions.Count)].position;

            if (positions != null) {
                randomSpawnPos = positions[creatureIndex];
            }

            GameObject spawnerObj = Instantiate(dayCreatureSpawnerTemplate, randomSpawnPos, Quaternion.identity, transform);
            spawnerObj.SetActive(true);

            DayCreatureSpawner spawner = spawnerObj.GetComponent<DayCreatureSpawner>();
            float radiusToRoam = 10f;
            float radius = UnityEngine.Random.Range(radiusToRoam * 0.25f, radiusToRoam * 2f);

            spawner.InitializeDayCreatureSpawner(selectedCreatures[creatureIndex], spawnAmounts[creatureIndex], eliteAmounts[creatureIndex], radius);

            dayCreatureSpawnerList.Add(spawner);
        }
    }

    private List<T> PickFromSide<T>(List<T> list, int amount) {
        amount = Mathf.Min(amount, list.Count);
        List<T> result = new List<T>();

        // Shuffle léger
        List<T> temp = new List<T>(list);
        for (int i = 0; i < temp.Count; i++) {
            int rand = UnityEngine.Random.Range(i, temp.Count);
            (temp[i], temp[rand]) = (temp[rand], temp[i]);
        }

        for (int i = 0; i < amount; i++)
            result.Add(temp[i]);

        return result;
    }

    private WorkerSpawner GetActiveLeftWorkerSpawner() {
        if (workerSpawnerLeft_CC.GetMobCount() > 0) return workerSpawnerLeft_CC;
        if (workerSpawnerLeft_VG.GetMobCount() > 0) return workerSpawnerLeft_VG;
        if (workerSpawnerLeft_LH.GetMobCount() > 0) return workerSpawnerLeft_LH;
        if (workerSpawnerLeft_FD.GetMobCount() > 0) return workerSpawnerLeft_FD;
        return null;
    }

    private WorkerSpawner GetActiveRightWorkerSpawner() {
        if (workerSpawnerRight_CC.GetMobCount() > 0) return workerSpawnerRight_CC;
        if (workerSpawnerRight_VG.GetMobCount() > 0) return workerSpawnerRight_VG;
        if (workerSpawnerRight_LH.GetMobCount() > 0) return workerSpawnerRight_LH;
        if (workerSpawnerRight_FD.GetMobCount() > 0) return workerSpawnerRight_FD;
        return null;
    }

    public void LoadStartingBlock(BlockSaveData data) {

        // ANIMAUX GAUCHE
        for (int i = 0; i < data.startingBlockAnimalSpawners_Left.Count && i < animalSpawnerList_Left.Count; i++) {
            var save = data.startingBlockAnimalSpawners_Left[i];
            animalSpawnerList_Left[i].SetSpawnerParameters(save.mobPrefab, save.currentMobsAlive, 5f);
            animalSpawnerList_Left[i].SpawnMobs(save.currentMobsAlive);
        }

        // ANIMAUX DROITE
        for (int i = 0; i < data.startingBlockAnimalSpawners_Right.Count && i < animalSpawnerList_Right.Count; i++) {
            var save = data.startingBlockAnimalSpawners_Right[i];
            animalSpawnerList_Right[i].SetSpawnerParameters(save.mobPrefab, save.currentMobsAlive, 5f);
            animalSpawnerList_Right[i].SpawnMobs(save.currentMobsAlive);
        }

        // SCAVENGABLES
        HordeModeBlockScavengables scav = GetComponent<HordeModeBlockScavengables>();
        if (scav != null && data.scavengableSaveDataList != null) {
            scav.LoadScavengables(data);
        }

        // WORKERS
        WorkerSpawner activeLeft = GetActiveLeftWorkerSpawner();
        WorkerSpawner activeRight = GetActiveRightWorkerSpawner();

        if (activeLeft != null && data.startingBlockWorkers_Left.currentMobsAlive > 0) {
            activeLeft.gameObject.SetActive(true);
            activeLeft.SpawnMobs(data.startingBlockWorkers_Left.currentMobsAlive);
        }

        if (activeRight != null && data.startingBlockWorkers_Right.currentMobsAlive > 0) {
            activeRight.gameObject.SetActive(true);
            activeRight.SpawnMobs(data.startingBlockWorkers_Right.currentMobsAlive);
        }
         
    }

    public override BlockSaveData GetBlockSaveData() {
        BlockSaveData save = new BlockSaveData();

        List<SpawnerSaveData> creaturesSpawnerSaveData = new List<SpawnerSaveData>();
        foreach (MobSpawner spawner in dayCreatureSpawnerList) {
            DayCreatureSpawner creatureSpawner = spawner as DayCreatureSpawner;

            SpawnerSaveData spawnerData = new SpawnerSaveData {
                mobPrefab = creatureSpawner.GetMobPrefab(),
                currentMobsAlive = creatureSpawner.GetMobCount(),
                mobsCanSpawnAtDawn = creatureSpawner.GetMobsCanSpawnAtDawn(),
                ambushSpawned = creatureSpawner.GetAmbushSpawned(),
                creatureSO = creatureSpawner.GetCreatureSO(),
                eliteMobsAlive = creatureSpawner.GetEliteMobsAlive(),
                spawnerPosition = creatureSpawner.transform.position,
            };

            creaturesSpawnerSaveData.Add(spawnerData);
        }

        save.creatureSpawnerListSaveData = creaturesSpawnerSaveData;

        // --- ANIMAUX GAUCHE / DROITE ---
        save.startingBlockAnimalSpawners_Left = new List<SpawnerSaveData>();

        foreach (var spawner in animalSpawnerList_Left) {
            save.startingBlockAnimalSpawners_Left.Add(new SpawnerSaveData {
                mobPrefab = spawner.GetMobPrefab(),
                currentMobsAlive = spawner.GetMobCount(),
                mobsCanSpawnAtDawn = spawner.GetMobsCanSpawnAtDawn(),
            });
        }

        save.startingBlockAnimalSpawners_Right = new List<SpawnerSaveData>();
        foreach (var spawner in animalSpawnerList_Right) {
            save.startingBlockAnimalSpawners_Right.Add(new SpawnerSaveData {
                mobPrefab = spawner.GetMobPrefab(),
                currentMobsAlive = spawner.GetMobCount(),
                mobsCanSpawnAtDawn = spawner.GetMobsCanSpawnAtDawn(),
            });
        }

        // --- SCAVENGABLES ---
        HordeModeBlockScavengables scav = GetComponent<HordeModeBlockScavengables>();
        if (scav != null) {
            save.scavengableSaveDataList = scav.GetScavengableSaveData();
        }

        // --- WORKERS ---
        save.startingBlockWorkers_Left = new SpawnerSaveData();
        save.startingBlockWorkers_Right = new SpawnerSaveData();

        WorkerSpawner activeLeft = GetActiveLeftWorkerSpawner();
        WorkerSpawner activeRight = GetActiveRightWorkerSpawner();

        if (activeLeft != null)
            save.startingBlockWorkers_Left.currentMobsAlive = activeLeft.GetMobCount();

        if (activeRight != null)
            save.startingBlockWorkers_Right.currentMobsAlive = activeRight.GetMobCount();

        return save;
    }
}
