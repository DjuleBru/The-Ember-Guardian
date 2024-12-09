using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMoveSpeedBoostVisual : MonoBehaviour
{

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private ParticleSystem footStepPS;
    [SerializeField] private ParticleSystem skillActivePS;

    private bool moveSpeedSkillActive;
    public event EventHandler OnMoveSpeedFootStepTriggered;

    private void Start() {
        footStepPS.Stop();
        skillActivePS.Stop();

        playerAnimator.OnFootStepTriggered += PlayerAnimator_OnFootStepTriggered;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnLeftActiveSkillDeactivated += PlayerSkills_OnLeftActiveSkillDeactivated;
        PlayerSkills.Instance.OnRightActiveSkillDeactivated += PlayerSkills_OnRightActiveSkillDeactivated;
    }

    private void PlayerSkills_OnRightActiveSkillDeactivated(object sender, System.EventArgs e) {
        if (PlayerSkills.Instance.GetActiveSkillRight().skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            moveSpeedSkillActive = false;
            skillActivePS.Stop();
        }
    }

    private void PlayerSkills_OnLeftActiveSkillDeactivated(object sender, System.EventArgs e) {
        if(PlayerSkills.Instance.GetActiveSkillLeft().skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            moveSpeedSkillActive = false;
            skillActivePS.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            moveSpeedSkillActive = true;
            skillActivePS.Play();
        }
    }

    private void PlayerAnimator_OnFootStepTriggered(object sender, System.EventArgs e) {
        if (!moveSpeedSkillActive) return;
        footStepPS.Play();
        OnMoveSpeedFootStepTriggered?.Invoke(this, EventArgs.Empty);
    }
}
