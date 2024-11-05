using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
