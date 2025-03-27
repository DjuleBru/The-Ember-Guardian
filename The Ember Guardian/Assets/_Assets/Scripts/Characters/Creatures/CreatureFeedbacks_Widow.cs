using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFeedbacks_Widow : CreatureFeedbacks
{
    [SerializeField] protected CreatureAI_TarnishedWidow widow;
    [SerializeField] protected MMF_Player landFeedbacks;
    
    protected override void Awake() {
        base.Awake();
        widow.OnWidowLanded += Widow_OnWidowLanded;
    }

    private void Widow_OnWidowLanded(object sender, System.EventArgs e) {
        landFeedbacks.PlayFeedbacks();
    }
}
