using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual : MonoBehaviour
{
    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Instance_OnPlayerRespawned;
    }

    private void Instance_OnPlayerRespawned(object sender, System.EventArgs e) {
        gameObject.SetActive(true);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }
}
