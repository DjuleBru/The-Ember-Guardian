using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimator_LMG : GunAnimator {

    private bool bipodEnabled;

    protected override void PlayerSHoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        bipodEnabled = !bipodEnabled;
        if(bipodEnabled) {
            animator.SetTrigger("DeployBipod");
        } else {
            animator.SetTrigger("ResetBipod");
        }
    }
}
