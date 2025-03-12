using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : Mob
{
    [SerializeField] private AnimalSO animalSO;

    public event EventHandler OnAnimalHitObstacle;

    private void Start() {
        health = animalSO.maxHP;
        AnimalManager.Instance.AddAnimalSpawned(this);
    }

    public override void Die() {
        base.Die();

        AnimalManager.Instance.RemoveAnimalSpawned(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);

        SpawnDroppedCurrencies(animalSO.currencyTypeDroppedList, animalSO.currencyDropAmountList);
        InvokeOnMobDroppedCollectibles(collectiblesDropped);

        StartCoroutine(DestroyGameObjectAfterDelay(animalSO.dieAnimationTime));
    }

   
    public AnimalSO GetAnimalSO() {
        return animalSO;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Obstacle>() != null) {
            OnAnimalHitObstacle?.Invoke(this, EventArgs.Empty); 
        }
    }

}
