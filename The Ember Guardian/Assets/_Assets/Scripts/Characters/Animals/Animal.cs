using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Mob
{
    [SerializeField] private AnimalSO animalSO;
    [SerializeField] private Transform dropSpawnPoint;

    public event EventHandler<OnAnimalDroppedCollectibleEventArgs> OnAnimalDroppedCollectibles;

    List<Collectible> collectiblesDropped = new List<Collectible>();
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

        SpawnDroppedCurrencies(animalSO.currencyTypeDropped, animalSO.currencyDropAmount);

        if(animalSO.currency2DropAmount > 0) {
            SpawnDroppedCurrencies(animalSO.currency2TypeDropped, animalSO.currency2DropAmount);
        }

        OnAnimalDroppedCollectibles?.Invoke(this, new OnAnimalDroppedCollectibleEventArgs {
            collectibleDroppedList = collectiblesDropped
        });

        StartCoroutine(DestroyGameObjectAfterDelay(animalSO.dieAnimationTime));
    }

   private void SpawnDroppedCurrencies(PlayerCurrencies.CurrencyType currencyType, int dropAmount) {

        for (int i = 0; i < dropAmount; i++) {
            Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(currencyType);
            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(currencyPrefab, dropSpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomUpwardsForce(3, 6);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(1f);
            lastBlueOrbDroppedOnTheFloor.SetCanBePickedUpByWorker();

            collectiblesDropped.Add(lastBlueOrbDroppedOnTheFloor);
        }

    }

    public AnimalSO GetAnimalSO() {
        return animalSO;
    }

}
