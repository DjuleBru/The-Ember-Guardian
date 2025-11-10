using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivaMagmaShotVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem skillActivePS;
    [SerializeField] private ParticleSystem activateSkillPS;
    [SerializeField] private ParticleSystem activateSkillPS_Burst;

    private void Start() {
        skillActivePS.Stop();
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if (e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet) {
            skillActivePS.Stop();
            activateSkillPS.Stop();
            activateSkillPS_Burst.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet) {
            skillActivePS.Play();
            activateSkillPS.Play();
            activateSkillPS_Burst.Play();
        }
    }

    private void OnDestroy() {
        PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated -= PlayerSkills_OnActiveSkillDeactivated;
    }

}
