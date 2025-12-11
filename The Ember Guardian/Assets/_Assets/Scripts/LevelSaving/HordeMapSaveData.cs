using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HordeMapSaveData {
    public List<BlockSaveData> leftBlocks = new();
    public List<BlockSaveData> rightBlocks = new();

    // Pour la cohérence : état interne de la génération
    public List<BlockSize> sizeHistory = new();
    public int distSinceLastWeaponChest;
    public int nextWeaponChestThreshold;
    public int distSinceLastTrapChest;
    public int nextTrapChestThreshold;

    public int distSinceLastWorkerSpawner;
    public int nextWorkerSpawnerThreshold;

    public float lastFastTravelRightX;
    public float lastFastTravelLeftX;

    public int nightsSinceLastExtreme;
    public int nightsSinceLastPeaceful;
    public int nextExtremeAt;
    public int nextPeacefulAt;

    public Dictionary<int, ShopType> shopsPerDistanceCache = new();
}

[System.Serializable]
public class BlockSaveData {
    public BlockSize size;
    public string prefabName; // On utilise le nom du prefab pour l'instancier
    public Vector3 position;
    public float direction;

    public bool obstacleBuilt;

    public List<BlockType> blockTypes;
    public bool hasShop;
    public ShopType shopType;

    public bool resourceChestOpened;
    public bool weaponChestOpened;
    public bool trapChestOpened;

    public SpawnerSaveData animalSpawnerData;
    public SpawnerSaveData workerSpawnerData;

    public List<SpawnerSaveData> creatureSpawnerListSaveData;
    public List<HordeScavengableSaveData> scavengableSaveDataList = new();
    public HordeScavengableSaveData mineSaveData;

    public int decorIndex;
}

public class HordeScavengableSaveData {
    public string prefabName;
    public int health;
    public int hitsTaken;
    public int currenciesToCollect;
    public int hitsToCollect;
    public float timeToMineOneResource;
    public bool markedToScavenge;
    public bool scavengingActive;
    public PlayerCurrencies.CurrencyType currencyTypeCollected;
    public int hordeModeBlockIndex;
}