using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDamageColliderParent : MonoBehaviour {

    private bool playerJustExitedCollider;

    private float playerJustExitedColliderTimer;
    private float playerJustExitedColliderTime = .075f;

    private void Update() {
        if (!playerJustExitedCollider) return;
        playerJustExitedColliderTimer -= Time.deltaTime;
        if (playerJustExitedColliderTimer < 0) {
            playerJustExitedCollider = false;
        }
    }

    public void SetPlayerJustExitedCollider() {
        playerJustExitedColliderTimer = playerJustExitedColliderTime;
        playerJustExitedCollider = true;
    }

    public bool PlayerJustExitedCollider() {
        return playerJustExitedCollider;
    }
}
