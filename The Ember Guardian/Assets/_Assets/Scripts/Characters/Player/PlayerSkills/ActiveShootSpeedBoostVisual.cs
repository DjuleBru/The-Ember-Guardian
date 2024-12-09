using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveShootSpeedBoostVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem skillActivePS;

    private void Start() {
        skillActivePS.Stop();
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnLeftActiveSkillDeactivated += PlayerSkills_OnLeftActiveSkillDeactivated;
        PlayerSkills.Instance.OnRightActiveSkillDeactivated += PlayerSkills_OnRightActiveSkillDeactivated;
    }

    private void PlayerSkills_OnRightActiveSkillDeactivated(object sender, System.EventArgs e) {
        if (PlayerSkills.Instance.GetActiveSkillRight().skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            skillActivePS.Stop();
        }
    }

    private void PlayerSkills_OnLeftActiveSkillDeactivated(object sender, System.EventArgs e) {
        if (PlayerSkills.Instance.GetActiveSkillLeft().skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            skillActivePS.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            skillActivePS.Play();
        }
    }
}
