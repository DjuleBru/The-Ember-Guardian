using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverLocationCollider : MonoBehaviour
{
    private bool playerCollided;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (playerCollided) return;

        if(collision.gameObject.GetComponent<Player>() != null) {
            LevelManager.Instance.ShowNewLocationUI();
            playerCollided = true;
        }
    }
}
