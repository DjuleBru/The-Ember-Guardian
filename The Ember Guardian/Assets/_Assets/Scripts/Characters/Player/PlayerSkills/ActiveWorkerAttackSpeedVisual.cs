using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWorkerAttackSpeedVisual : MonoBehaviour
{
    [SerializeField] private Animator shieldAnimator;
    [SerializeField] private SpriteRenderer AOESpriteRenderer;

    private bool shieldActive;

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;

        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;
        Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
    }

    private void Portal_OnAnyPlayerTeleported(object sender, System.EventArgs e) {
        if (shieldActive) {
            AOESpriteRenderer.enabled = false;
        }
    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, System.EventArgs e) {
        if (shieldActive) {
            AOESpriteRenderer.enabled = true;
        }
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, System.EventArgs e) {
        if (shieldActive) {
            AOESpriteRenderer.enabled = false;
        }
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if (e.skillTypeDeactivated == SkillItem.SkillType.activeWorkerAttackSpeedBuff) {
            shieldAnimator.SetTrigger("Die");
            shieldActive = false;
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeWorkerAttackSpeedBuff) {
            shieldAnimator.SetTrigger("Activate");
            shieldActive = true;
        }
    }


    private void OnDestroy() {
        FastTravelTP.OnAnyPlayerWarped -= FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut -= FastTravelTP_OnAnyPlayerWarpedOut;
        Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
    }
}
