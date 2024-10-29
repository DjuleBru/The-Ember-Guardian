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

    public Animal GetClosestAnimalInRadius(Vector2 position, float radius) {
        float closestXDistance = Mathf.Infinity;
        Animal closestAnimalInRadius = null;

        foreach (Animal animal in spawnedAnimalList) {

            if ((Mathf.Abs(animal.transform.position.x - position.x)) < closestXDistance) {

                // Check if animal is within distance from closest exterior zone limit
                if(Mathf.Abs(animal.transform.position.x - CampZoneManager.Instance.GetClosestExteriorZoneLimit(animal.transform.position).x) < radius) {

                    closestXDistance = Mathf.Abs(animal.transform.position.x - position.x);
                    closestAnimalInRadius = animal;

                }
            }

        }

        return closestAnimalInRadius;
    }

    public void AddAnimalSpawned(Animal animal) {
        spawnedAnimalList.Add(animal);
    }

    public void RemoveAnimalSpawned(Animal animal) {
        spawnedAnimalList.Remove(animal);
    }
}
