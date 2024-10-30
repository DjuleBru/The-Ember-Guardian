using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Mob
{
    [SerializeField] private AnimalSO animalSO;
    [SerializeField] private Transform dropSpawnPoint;

    public event EventHandler<OnAnimalDroppedCollectibleEventArgs> OnAnimalDroppedCollectibles;

    public class OnAnimalDroppedCollectibleEventArgs {
        public List<Collectible> collectibleDroppedList;
    }

    private void Start() {
        health = animalSO.maxHP;
        AnimalManager.Instance.AddAnimalSpawned(this);
    }

    public override void Die() {
        base.Die();

        AnimalManager.Instance.RemoveAnimalSpawned(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);

        List<Collectible> collectiblesDropped = new List<Collectible>();

        for (int i = 0; i < animalSO.currencyDropAmount; i++) {
            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(animalSO.currencyPrefab, dropSpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomUpwardsForce(3,6);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(1f);
            lastBlueOrbDroppedOnTheFloor.SetCanBePickedUpByWorker();

            collectiblesDropped.Add(lastBlueOrbDroppedOnTheFloor);
        }

        OnAnimalDroppedCollectibles?.Invoke(this, new OnAnimalDroppedCollectibleEventArgs {
            collectibleDroppedList = collectiblesDropped
        });

        StartCoroutine(DestroyGameObjectAfterDelay(animalSO.dieAnimationTime));
    }

    public AnimalSO GetAnimalSO() {
        return animalSO;
    }

}
