using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeHandler : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Color activeMagmaShotBulletVignetteColor;
    private Vignette vignette;

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;

        // Cloner le profile pour ne pas modifier l’original (facultatif mais conseillé)
        volume.profile = Instantiate(volume.profile);

        if (!volume.profile.TryGet(out vignette)) {
            Debug.LogWarning("Pas d'effet Vignette dans ce volume !");
        }
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if(e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet || e.skillTypeDeactivated == SkillItem.SkillType.activeFeedFireOnKills) {
            vignette.color.value = Color.black;
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {

        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet || e.skillItemAdded.skillType == SkillItem.SkillType.activeFeedFireOnKills) {
            vignette.color.value = activeMagmaShotBulletVignetteColor;
        }
    }
}
