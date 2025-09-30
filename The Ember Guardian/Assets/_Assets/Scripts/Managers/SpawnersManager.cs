using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpawnersManager : MonoBehaviour
{
    public static SpawnersManager Instance;
    private List<MobSpawner> mobSpawners = new List<MobSpawner>();
    private void Awake() {
        Instance = this;

        InitializeSpawners();
    }

    public void InitializeSpawners() {
        MobSpawner[] mobSpawnerChilds = GetComponentsInChildren<MobSpawner>();
        foreach(MobSpawner spawner in mobSpawnerChilds) {
            mobSpawners.Add(spawner);
        }
    }

    public List<MobSpawner> GetAllSpawners() {
        return mobSpawners;
    }

   
}
