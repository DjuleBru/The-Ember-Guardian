using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_SummonerAgressive : CreatureAI {
    [SerializeField] private CreatureSpawnerContinuous creatureSpawnerContinuous;
    [SerializeField] private List<Transform> spawnPositionList;

    [SerializeField] private float spawnRate;
    private float spawnTimer;

    protected override void Start() {
        base.Start();

        creatureSpawnerContinuous.SetCanSpawnAtNight(!creature.IsDayCreature());
    }

    protected override void Update() {
        base.Update();

        if (creatureAttack.GetAttacking()) return;
        if (creatureSpawnerContinuous.SpawnedMaxMobs()) return;
        if (creature.GetDead()) return;

        spawnTimer += Time.deltaTime;

        if(spawnTimer > spawnRate) {
            creatureSpawnerContinuous.StartSpawnMobs_Summoner(spawnPositionList.Count, spawnPositionList);
            spawnTimer = 0;
        }

    }

}
