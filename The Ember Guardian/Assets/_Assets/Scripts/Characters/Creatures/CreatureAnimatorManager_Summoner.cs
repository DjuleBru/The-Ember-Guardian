using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_Summoner : CreatureAnimatorManager
{
    [SerializeField] private CreatureSpawnerContinuous spawner;

    protected override void Start() {
        base.Start();
        spawner.OnSpawnerSpawnStart += Spawner_OnSpawnerSpawnStart;
    }

    private void Spawner_OnSpawnerSpawnStart(object sender, System.EventArgs e) {
        animator.SetTrigger("Special");

    }
}
