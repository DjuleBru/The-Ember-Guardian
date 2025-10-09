using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_ActivateShieldBounceOnHit : CreatureAnimatorManager
{
    [SerializeField] private Creature_ActivateShieldOnBounce creatureActivateShield;

    protected override void Start() {
        base.Start();
        creatureActivateShield.OnShieldActivated += CreatureActivateShield_OnShieldActivated;
    }

    private void CreatureActivateShield_OnShieldActivated(object sender, System.EventArgs e) {
        animator.SetTrigger("ActivateBounceShield");
    }
}
