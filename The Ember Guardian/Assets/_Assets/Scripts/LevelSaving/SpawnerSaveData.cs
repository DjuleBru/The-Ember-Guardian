using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnerSaveData
{
    public string spawnerID;   // identifiant unique de ce spawner
    public int currentMobsAlive;
    public bool ambushSpawned; // Pour ambush
    public bool dead;          // pour CreatureSpawnerContinuous
    public bool mobsCanSpawnAtDawn;
}
