using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator_LMG : GunAnimator {

    private bool bipodEnabled;

    protected override void PlayerSHoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        if(PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
            bipodEnabled = !bipodEnabled;
            if (bipodEnabled) {
                animator.SetTrigger("DeployBipod");
            }
            else {
                animator.SetTrigger("ResetBipod");
            }
        }

        if(PlayerShoot.Instance.GetSecondSecondaryAbilityEquipped()) {
            animator.SetTrigger("SwitchFireMode");

            if(PlayerShoot.Instance.GetBlastingLMGModeActive()) {
                animator.SetBool("BlastModeActive", true);
            } else {
                animator.SetBool("BlastModeActive", false);
            }
        }
      
    }
}
