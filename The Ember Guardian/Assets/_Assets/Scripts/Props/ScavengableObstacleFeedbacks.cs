using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableObstacleFeedbacks : MonoBehaviour
{
    [SerializeField] private ScavengableObstacle scavengableObstacle;
    [SerializeField] private MMF_Player creatureSpawnedFeedbacks;

    private void Start() {
        scavengableObstacle.OnCreatureSpawned += ScavengableObstacle_OnCreatureSpawned;
    }

    private void ScavengableObstacle_OnCreatureSpawned(object sender, System.EventArgs e) {
        creatureSpawnedFeedbacks.PlayFeedbacks();
    }
}
