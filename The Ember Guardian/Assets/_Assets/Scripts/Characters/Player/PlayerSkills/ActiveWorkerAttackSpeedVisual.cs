using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWorkerAttackSpeedVisual : MonoBehaviour
{
    [SerializeField] private Animator shieldAnimator;

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if (e.skillTypeDeactivated == SkillItem.SkillType.activeWorkerAttackSpeedBuff) {
            shieldAnimator.SetTrigger("Die");
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeWorkerAttackSpeedBuff) {
            shieldAnimator.SetTrigger("Activate");
        }
    }
}
