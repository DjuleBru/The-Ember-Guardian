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

    protected override void Start() {
        Debug.Log(" HordeModeBlock_StartingBlock Start");
        base.Start();
        HandleCentralBlockResources();
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

    public override void GenerateCreatureSpawners(List<CreatureSO> selectedCreatures, List<int> spawnAmounts, List<int> eliteAmounts) {
        Debug.Log("Central Block GenerateCreatureSpawners " + spawnAmounts.Count);

        foreach (var creatureIndex in System.Linq.Enumerable.Range(0, selectedCreatures.Count)) {

            Transform randomSpawnPos = creatureSpawnerPositions[UnityEngine.Random.Range(0, creatureSpawnerPositions.Count)];

            GameObject spawnerObj = Instantiate(dayCreatureSpawnerTemplate, randomSpawnPos.position, Quaternion.identity, transform);
            spawnerObj.SetActive(true);

            DayCreatureSpawner spawner = spawnerObj.GetComponent<DayCreatureSpawner>();
            float radiusToRoam = 10f;
            float radius = UnityEngine.Random.Range(radiusToRoam * 0.25f, radiusToRoam * 2f);

            spawner.InitializeDayCreatureSpawner(selectedCreatures[creatureIndex], spawnAmounts[creatureIndex], eliteAmounts[creatureIndex], radius);
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

}
