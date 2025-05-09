using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player damagedFeedbacks;
    [SerializeField] private MMF_Player passiveShieldDamagedFeedbacks;
    [SerializeField] private MMF_Player activeMoveSpeedBuffStartFeedbacks;
    [SerializeField] private MMF_Player activeMoveSpeedBuffEndFeedbacks;
    [SerializeField] private MMF_Player activeShootSpeedBuffStartFeedbacks;
    [SerializeField] private MMF_Player activeShootSpeedBuffEndFeedbacks;
    [SerializeField] private MMF_Player activeMagmaShotBulletStartFeedbacks;
    [SerializeField] private MMF_Player activeMagmaShotBulletEndFeedbacks;
    [SerializeField] private MMF_Player activeHealOnKillsFeedbacks;
    [SerializeField] private MMF_Player activeTeleportationFeedbacks;
    [SerializeField] private MMF_Player aimingSightsStartFeedbacks;
    [SerializeField] private MMF_Player aimingSightsEndFeedbacks;
    [SerializeField] private MMF_Player exhaustedStartFeedbacks;
    [SerializeField] private MMF_Player exhaustedEndFeedbacks;
    [SerializeField] private MMF_Player petDogStartFeedbacks;
    [SerializeField] private MMF_Player petDogEndFeedbacks;

    private bool playerExhausted;

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionDeactivateFeedbacks += PlayerMovement_OnPlayerAlmostExhaustionDeactivateFeedbacks;
        PassiveShield.OnAnyPassiveShieldDied += PassiveShield_OnAnyPassiveShieldDied;

        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSKills_OnActiveSkillDeactivated;

        PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerEndedPettingDog += PetDog_OnPlayerEndedPettingDog;

        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAIm_OnPlayerAimSightEnded;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAim_OnPlayerAimSightStarted;
    }


    private void PetDog_OnPlayerEndedPettingDog(object sender, System.EventArgs e) {
        petDogEndFeedbacks.PlayFeedbacks();
    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        petDogStartFeedbacks.PlayFeedbacks();
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

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {

        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            activeMoveSpeedBuffStartFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            activeShootSpeedBuffStartFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeTeleportation) {
            activeTeleportationFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet || e.skillItemAdded.skillType == SkillItem.SkillType.activeFeedFireOnKills) {
            activeMagmaShotBulletStartFeedbacks.PlayFeedbacks();
        }

        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeHealOnKills) {
            activeHealOnKillsFeedbacks.PlayFeedbacks();
        }
    }

    private void PlayerSKills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {

        if (e.skillTypeDeactivated == SkillItem.SkillType.activeMoveSpeedBuff) {
            activeMoveSpeedBuffEndFeedbacks.PlayFeedbacks();
        }

        if (e.skillTypeDeactivated == SkillItem.SkillType.activeShootSpeedBuff) {
            activeShootSpeedBuffEndFeedbacks.PlayFeedbacks();
        }

        if (e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet || e.skillTypeDeactivated == SkillItem.SkillType.activeFeedFireOnKills) {
            activeMagmaShotBulletEndFeedbacks.PlayFeedbacks();
        }
    }


    private void PassiveShield_OnAnyPassiveShieldDied(object sender, System.EventArgs e) {
        passiveShieldDamagedFeedbacks.PlayFeedbacks();
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
