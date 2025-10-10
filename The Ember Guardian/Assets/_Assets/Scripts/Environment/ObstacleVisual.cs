using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleVisual : MonoBehaviour
{
    [SerializeField] private GameObject obstacleVisualGameObject;
    [SerializeField] private GameObject obstacleUIGameObject;
    [SerializeField] private Animator obstacleAnimator;
    [SerializeField] private Obstacle obstacle;
    [SerializeField] private bool hideOnTriggerExit = true;

    private void Start() {
        obstacle.OnObstacleBuilt += Obstacle_OnObstacleBuilt;
        obstacle.OnPlayerTriggeredIn += Obstacle_OnPlayerTriggeredIn;
        obstacle.OnPlayerTriggeredOut += Obstacle_OnPlayerTriggeredOut;
        obstacleVisualGameObject.SetActive(false);
    }

    private void Obstacle_OnObstacleBuilt(object sender, System.EventArgs e) {
        obstacleVisualGameObject.SetActive(true);
        obstacleAnimator.SetTrigger("Build");
        obstacleUIGameObject.SetActive(false);

        Debug.Log("Obstacle_OnObstacleBuilt");
    }

    private void Obstacle_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (obstacle.GetBuilt()) return;
        if (!hideOnTriggerExit) return;
        obstacleVisualGameObject.SetActive(false);
    }

    private void Obstacle_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (obstacle.GetBuilt()) return;
        if (!hideOnTriggerExit) return;
        obstacleVisualGameObject.SetActive(true);
    }

}
