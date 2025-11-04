using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFeedbacks_Colossus : CreatureFeedbacks
{

    [SerializeField] private MMF_Player wakeFeedbacks;
    private CreatureAI_Colossus colossus;

    protected override void Awake() {
        base.Awake();
        colossus = creatureAI.GetComponent<CreatureAI_Colossus>();
        colossus.OnColossusWake += Colossus_OnColossusWake;
    }

    private void Colossus_OnColossusWake(object sender, System.EventArgs e) {
        wakeFeedbacks.PlayFeedbacks();
    }
}
