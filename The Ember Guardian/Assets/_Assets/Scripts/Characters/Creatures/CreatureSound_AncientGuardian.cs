using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound_AncientGuardian : CreatureSound
{
    [SerializeField] private CreatureAI_AncientGuardian ancientGuardian;
    [SerializeField] private AudioClip startTPClip;
    [SerializeField] private AudioClip endTPClip;
   protected override void Start() {
        base.Start();
        ancientGuardian.OnGuardianEndedTP += AncientGuardian_OnGuardianEndedTP;
        ancientGuardian.OnGuardianStartedTP += AncientGuardian_OnGuardianStartedTP;
    }


    private void AncientGuardian_OnGuardianStartedTP(object sender, System.EventArgs e) {
        PlaySound2D(startTPClip);
    }

    private void AncientGuardian_OnGuardianEndedTP(object sender, System.EventArgs e) {
        PlaySound2D(endTPClip);
    }
}
