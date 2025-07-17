using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnOnDamageThreshold {
    [Range(0f, 1f)] public float healthThresholdNormalized; // ex : 0.75 pour 75% de vie restante
    public List<CreatureSpawnData> creaturesToSpawn;
    [HideInInspector] public bool hasTriggered; // interne, ne pas exposer
}

[Serializable]
public class CreatureSpawnData {
    public CreatureSO creatureType;
    public int amount;
    public MobSpawner mobSpawner;
}