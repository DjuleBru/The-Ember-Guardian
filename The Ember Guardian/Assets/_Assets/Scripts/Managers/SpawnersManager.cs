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
    }

    public void AddSpawner(MobSpawner spawner) {
        if (mobSpawners.Contains(spawner)) return;
        mobSpawners.Add(spawner);
    }

    public List<MobSpawner> GetAllSpawners() {
        return mobSpawners;
    }

   
}
