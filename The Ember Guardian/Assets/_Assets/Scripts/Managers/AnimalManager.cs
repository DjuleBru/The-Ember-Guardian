using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    public static AnimalManager Instance;
    private List<Animal> spawnedAnimalList = new List<Animal>();

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

    public Animal GetClosestAvailableAnimalInRadius(Vector2 position, CampZoneManager.CampSide campSide, Worker worker) {

        float closestXDistance = Mathf.Infinity;
        Animal closestAnimalInRadius = null;

        foreach (Animal animal in spawnedAnimalList) {

            // Check if animal can have workers assigned
            if (animal.GetMaxHuntersAssigned() && !animal.GetHunterIsAlreadyAssigned(worker)) continue;

            // Check if animal is on the same side as worker
            if ((animal.transform.position.x < 0 && campSide == CampZoneManager.CampSide.left) || (animal.transform.position.x > 0 && campSide == CampZoneManager.CampSide.right)) {


                // Check if animal is within hunting limits
                if (animal.transform.position.x < 0) {
                    if ((animal.transform.position.x) > CampZoneManager.Instance.GetHuntingMinZoneLimit()) {

                        if ((Mathf.Abs(animal.transform.position.x - position.x)) < closestXDistance) {
                            // Animal is the closest one
                            closestXDistance = Mathf.Abs(animal.transform.position.x - position.x);
                            closestAnimalInRadius = animal;
                        }

                    }

                }
                else {

                    if ((animal.transform.position.x) < CampZoneManager.Instance.GetHuntingMaxZoneLimit()) {

                        if ((Mathf.Abs(animal.transform.position.x - position.x)) < closestXDistance) {
                            // Animal is the closest one
                            closestXDistance = Mathf.Abs(animal.transform.position.x - position.x);
                            closestAnimalInRadius = animal;
                        }

                    }
                }


            }
        }
        return closestAnimalInRadius;
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
