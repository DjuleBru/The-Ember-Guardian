using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player runningFeedbacks;
    [SerializeField] private MMF_Player walkingFeedbacks;
    [SerializeField] private MMF_Player shootFeedbacks;

    [SerializeField] private PlayerAnimator playerAnimator;

    public event EventHandler OnBackpackFeedbacksPlay;
 
    private void Start() {
        //Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        //PlayerMovement.Instance.OnPlayerLanded += PlayerMovement_OnPlayerLanded;
        //PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        //PlayerShoot.Instance.OnPlayerShot += Player_OnPlayerShotProjectile;
        //playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
    }

    private void PlayerMovement_OnPlayerCrouched(object sender, System.EventArgs e) {
        runningFeedbacks.PlayFeedbacks();
        OnBackpackFeedbacksPlay?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerMovement_OnPlayerLanded(object sender, System.EventArgs e) {
        runningFeedbacks.PlayFeedbacks();
        OnBackpackFeedbacksPlay?.Invoke(this, EventArgs.Empty);
    }

    private void Player_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        shootFeedbacks.PlayFeedbacks();
        OnBackpackFeedbacksPlay?.Invoke(this, EventArgs.Empty);
    }
    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
        OnBackpackFeedbacksPlay?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (PlayerMovement.Instance.IsRunning()) {
            if(!runningFeedbacks.IsPlaying) {
                runningFeedbacks.PlayFeedbacks();
            }

            OnBackpackFeedbacksPlay?.Invoke(this, EventArgs.Empty);
        } else {
            walkingFeedbacks.PlayFeedbacks();
        }
    }
}
