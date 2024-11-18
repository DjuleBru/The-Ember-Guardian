using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player runningFeedbacks;
    [SerializeField] private MMF_Player shootFeedbacks;

    [SerializeField] private PlayerAnimator playerAnimator;

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        PlayerShoot.Instance.OnPlayerShotProjectile += Player_OnPlayerShotProjectile;
        playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
    }


    private void Player_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        shootFeedbacks.PlayFeedbacks();
    }
    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (PlayerMovement.Instance.IsRunning()) {
            runningFeedbacks.PlayFeedbacks();
        }
    }
}
