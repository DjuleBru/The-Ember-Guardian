using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player passiveShieldDamagedFeedbacks;

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;
    }

    private void PassiveShield_OnAnyPassiveShieldDied(object sender, System.EventArgs e) {
        passiveShieldDamagedFeedbacks.PlayFeedbacks();
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
