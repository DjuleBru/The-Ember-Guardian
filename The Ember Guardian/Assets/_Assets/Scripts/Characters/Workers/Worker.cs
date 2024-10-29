using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : Mob
{
    [SerializeField] private Transform blueOrbPrefab;

    private HunterJob hunterJob;
    private int orbAmount;

    private void Awake() {
        hunterJob = GetComponent<HunterJob>();
    }

    public void CollectOrb(Collectible collectible) {
        orbAmount++;
    }

    public void DropOrbs() {
        for(int i = 0; i < orbAmount; i++) {

            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(blueOrbPrefab, transform.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomFrontForce(2f, 3f);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);

        }
    }
}
