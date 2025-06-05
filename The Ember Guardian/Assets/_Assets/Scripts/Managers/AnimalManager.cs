using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    public static AnimalManager Instance;
    private List<Animal> spawnedAnimalList = new List<Animal>();
    private float distanceToMaxHuntingLimitToAllowHunting = 8f;
    private float distanceToAnimalToAllowReassigning = 15f;

    private void Awake() {
        Instance = this;
    }

    public Animal GetClosestAnimal(Vector2 position) {
        float closestXDistance = Mathf.Infinity;
        Animal closestAnimal = null;

        foreach(Animal animal in spawnedAnimalList) {
            if((Mathf.Abs(animal.transform.position.x -  position.x)) < closestXDistance) {
                closestXDistance = Mathf.Abs(animal.transform.position.x - position.x);
                closestAnimal = animal;
            }
        }

        return closestAnimal;
    }

    public Animal GetClosestAvailableAnimalInRadius(Worker worker) {
        Vector2 hunterPos = worker.transform.position;
        CampZoneManager.CampSide campSide = worker.GetCampSideAddigned();

        float bestDist = Mathf.Infinity;
        Animal closestAnimalInRadius = null;

        foreach (Animal animal in spawnedAnimalList) {

            // Check if animal can have workers assigned
            if (!IsOnSameCampSide(animal, campSide)) continue;
            if (!IsInHuntingZone(animal)) continue;

            float dist = Vector2.Distance(hunterPos, animal.transform.position);
            if (dist < bestDist) {

                if (animal.GetMaxHuntersAssigned() && !animal.GetHunterIsAlreadyAssigned(worker)) {
                    // animal has max hunters assigned and this worker is not assigned
                    if (animal.GetFurthestWorkerDistance() < distanceToAnimalToAllowReassigning) continue;
                    if (animal.GetFurthestWorkerDistance() < dist) continue;

                };

                bestDist = dist;
                closestAnimalInRadius = animal;
            }
        }

        return closestAnimalInRadius;
    }

    private bool IsOnSameCampSide(Animal animal, CampZoneManager.CampSide campSide) {
        return (animal.transform.position.x < 0 && campSide == CampZoneManager.CampSide.left)
            || (animal.transform.position.x > 0 && campSide == CampZoneManager.CampSide.right);
    }

    private bool IsInHuntingZone(Animal animal) {
        float x = animal.transform.position.x;
        return (x < 0 && x > (CampZoneManager.Instance.GetHuntingMinZoneLimit() - distanceToMaxHuntingLimitToAllowHunting))
            || (x > 0 && x < (CampZoneManager.Instance.GetHuntingMaxZoneLimit() + distanceToMaxHuntingLimitToAllowHunting));
    }

    public List<Transform> GetAllSpawnedAnimalTransformList() {
        List<Transform> allSpawnedAnimals = new List<Transform>();
        foreach(Animal animal in spawnedAnimalList) {
            allSpawnedAnimals.Add(animal.transform);
        }

        return allSpawnedAnimals;
    }

    public void AddAnimalSpawned(Animal animal) {
        spawnedAnimalList.Add(animal);
    }

    public void RemoveAnimalSpawned(Animal animal) {
        spawnedAnimalList.Remove(animal);
    }
}
