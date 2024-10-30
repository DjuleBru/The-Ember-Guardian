using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager Instance;

    private List<Creature> creaturePoolList = new List<Creature>();
    private List<Creature> creaturesSpawnedList = new List<Creature>();

    private void Awake() {
        Instance = this;
    }

    public void AddCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Add(creature);
    }

    public void RemoveCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Remove(creature);
    }
}
