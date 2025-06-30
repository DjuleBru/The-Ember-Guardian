using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner_Ambush : MobSpawner {

    [SerializeField] protected List<Transform> mobPrefabList;
    [SerializeField] protected List<Transform> spawnPositionsList;
    [SerializeField] protected List<int> mobAmountList;
    [SerializeField] protected float delayBetweenMobSpawn = .1f;
    [SerializeField] protected float minSpawnPositionFromPlayer;
    [SerializeField] protected float maxSpawnPositionFromPlayer;


    private bool ambushSpawned;

    protected override void Start() {
        if (sceneViewSpawnerSpriteRenderer != null) {
            sceneViewSpawnerSpriteRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (ambushSpawned) return;

        if(collision.gameObject.GetComponent<Player>() != null) {
            StartCoroutine(SpawnAmbush());
            ambushSpawned = true;
        }
    }
    public override void RemoveMobFromMobSpawnedList(Mob mob) {
        base.RemoveMobFromMobSpawnedList(mob);
        if(mobSpawnedList.Count == 0) {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator SpawnAmbush() {
        int i = 0;
        GetComponent<Collider2D>().enabled = false;

        foreach(Transform mob in mobPrefabList) {
            int mobAmount = mobAmountList[i];

            for(int j = 0; j < mobAmount; j++) {
                Vector3 randomizedSpawnPosition = GetRandomizedSpawnPosition(i);

                SpawnMobs(1, mob, randomizedSpawnPosition);

                float delayBetweenMobSpawnRandomized = Random.Range(delayBetweenMobSpawn - delayBetweenMobSpawn / 3, delayBetweenMobSpawn + delayBetweenMobSpawn / 3);
                yield return new WaitForSeconds(delayBetweenMobSpawnRandomized);
            }
            i++;
        }
    }

    private Vector3 GetRandomizedSpawnPosition(int creatureIndex) {

        Transform spawnPosition = spawnPositionsList[creatureIndex];
            
        float xRandomizer = Random.Range(minSpawnPositionFromPlayer, maxSpawnPositionFromPlayer);
        float directionRandomizer = Random.Range(-1f, 1f);
        if (directionRandomizer <= 0) {
            directionRandomizer = -1;
        } else {
            directionRandomizer = 1;
        }
        xRandomizer *= directionRandomizer;

        return new Vector3(spawnPosition.position.x + xRandomizer, spawnPosition.position.y, 0);

    }
}
