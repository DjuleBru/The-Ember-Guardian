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

    public Dictionary<int, ShopType> shopsPerDistanceCache = new();
}

[System.Serializable]
public class BlockSaveData {
    public BlockSize size;
    public string prefabName; // On utilise le nom du prefab pour l'instancier
    public Vector3 position;
    public float direction;

    public List<BlockType> blockTypes;
    public bool hasShop;
    public ShopType shopType;

    public SpawnerSaveData animalSpawnerData;

    public List<SpawnerSaveData> creatureSpawnerListSaveData;
}