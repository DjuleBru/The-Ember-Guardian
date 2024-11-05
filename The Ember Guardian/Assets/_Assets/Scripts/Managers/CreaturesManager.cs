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

    public Creature GetClosestCreatureInRadius(Vector2 position, float radius) {

        float closestXDistance = Mathf.Infinity;
        Creature closestCreatureInRadius = null;

        foreach (Creature creature in creaturesSpawnedList) {
            float distanceToCreature = Mathf.Abs(creature.transform.position.x - position.x);
            if ((distanceToCreature) < radius && (distanceToCreature < closestXDistance)) {
                // Creature is the closest one

                closestXDistance = Mathf.Abs(creature.transform.position.x - position.x);
                closestCreatureInRadius = creature;
            }

        }

        return closestCreatureInRadius;
    }

    public void AddCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Add(creature);
    }

    public void RemoveCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Remove(creature);
    }
}
