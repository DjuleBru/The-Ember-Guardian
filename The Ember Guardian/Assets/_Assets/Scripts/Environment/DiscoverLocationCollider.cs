using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverLocationCollider : MonoBehaviour
{
    public static bool playerCollided;

    private void Awake() {
        playerCollided = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (playerCollided) return;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;
        }

        if(collision.gameObject.GetComponent<Player>() != null) {
            LevelManager.Instance.ShowNewLocationUI();
            playerCollided = true;
        }
    }
}
