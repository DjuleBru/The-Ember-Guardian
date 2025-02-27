using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player passiveShieldDamagedFeedbacks;
    [SerializeField] private MMF_Player activeMoveSpeedBuffFeedbacks;
    [SerializeField] private MMF_Player activeShootSpeedBuffFeedbacks;
    [SerializeField] private MMF_Player activeTeleportationFeedbacks;
    [SerializeField] private MMF_Player aimingSightsStartFeedbacks;
    [SerializeField] private MMF_Player aimingSightsEndFeedbacks;
    [SerializeField] private MMF_Player exhaustedStartFeedbacks;
    [SerializeField] private MMF_Player exhaustedEndFeedbacks;

    private bool playerExhausted;

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionDeactivateFeedbacks += PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks;
        PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillAdded += PlayerSkills_OnActiveSkillAdded;

        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAIm_OnPlayerAimSightEnded;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAim_OnPlayerAimSightStarted;
    }

    private void PlayerAim_OnPlayerAimSightStarted(object sender, System.EventArgs e) {
        aimingSightsStartFeedbacks.PlayFeedbacks();
    }

    private void PlayerAIm_OnPlayerAimSightEnded(object sender, System.EventArgs e) {
        aimingSightsStartFeedbacks.StopFeedbacks();
        aimingSightsEndFeedbacks.PlayFeedbacks();
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, System.EventArgs e) {
        playerExhausted = false;
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, System.EventArgs e) {
        playerExhausted = true;
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks(object sender, System.EventArgs e) {
        if (playerExhausted) return;
        exhaustedStartFeedbacks.StopFeedbacks();
        exhaustedEndFeedbacks.PlayFeedbacks();
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStarted(object sender, System.EventArgs e) {
        exhaustedStartFeedbacks.PlayFeedbacks();
    }

    private void PlayerSkills_OnActiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            activeMoveSpeedBuffFeedbacks.GetFeedbackOfType<MMF_LensDistortion_URP>().Duration = e.skillItemAdded.skillSO.activeSkillEffect.GetValueAtLevel(e.skillItemAdded.currentLevel);
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            activeShootSpeedBuffFeedbacks.GetFeedbackOfType<MMF_Vignette_URP>().Duration = PlayerSkills.Instance.GetShootSpeedBuffDuration();
            activeShootSpeedBuffFeedbacks.GetFeedbackOfType<MMF_CameraOrthographicSize>().Duration = PlayerSkills.Instance.GetShootSpeedBuffDuration();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {

        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            activeMoveSpeedBuffFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            activeShootSpeedBuffFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeTeleportation) {
            activeTeleportationFeedbacks.PlayFeedbacks();
        }
    }

    private void PassiveShield_OnAnyPassiveShieldDied(object sender, System.EventArgs e) {
        passiveShieldDamagedFeedbacks.PlayFeedbacks();
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
