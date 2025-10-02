using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureSaveData {

    public float posX, posY;
    public StructureSO.StructureType structureType;
    public bool structureBuilt;
    public bool structureUnlocked;
    public int structureLevel;
    public bool isWorldStructure;
    public float worldStructureScaleX;
}

public class TrapSaveData {

    public int currentUseIndex;
    public int currentRearmIndex;

}
public class FireSaveData {

    public float currentFuelLevel;
    public bool isMainFire;
    public bool isSecondaryFire;
    public bool isEndLevelFire;

}
public class CurrencyCrafterSaveData {
    public int currentBatches;
    public float currencyCraftTimer;
    public bool craftingCurrency;
    public bool craftedCurrency;
    public PlayerCurrencies.CurrencyType currencyTypeCrafted;
}
public class BarricadeSaveData {

    public int barricadeHealth;

}
public class SpecialTowerSaveData {

    public int currentAmmoClips;

}
public class CurrencyStorageSaveData {

    public int currencyAmountStored;
    public int maxCurrencyStorageIndex;

}

public class ObstacleSaveData {
    public string obstacleID;
    public bool built;
}
public class TrialAreaSaveData {
    public string trialAreaID;
    public bool trialCompleted;
}
public class ScavengableSaveData {
    public string scavengableID;
    public int health;
    public int hitsTaken;
    public float timeToMineOneResource;
    public bool markedToScavenge;
    public bool scavengingActive;
}

public class ScavengableObstacleSaveData {
    public string scavObstacleID;
    public bool markedToScavenge;
    public int health;
    public int hitsTaken;
    public List<bool> thresholdsTriggered;
}

public class ChestSaveData {
    public string chestID;
    public bool opened;
    public bool chestLocked;
    public bool chestPricePaid;
    public bool rewardOfferedToPlayer;
}
public class CollectibleSaveData {
    public float posX, posY;
    public Quaternion rotation;
    public PlayerCurrencies.CurrencyType currencyType;
}