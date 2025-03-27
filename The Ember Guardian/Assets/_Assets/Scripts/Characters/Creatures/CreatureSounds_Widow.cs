using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSounds_Widow : CreatureSound
{

    [SerializeField] protected CreatureAI_TarnishedWidow widow;
    [SerializeField] protected AudioClip jumpAudioClip;
    [SerializeField] protected AudioClip landAudioClip;
    private float delayToPlayJumpSFX = .5f;
    private float delayToPlayLandSFX = 0f;

    protected override void Start() {
        base.Start();

        widow.OnWidowJumpStarted += Widow_OnWidowJumpStarted;
        widow.OnWidowLanded += Widow_OnWidowLanded;
    }

    private void Widow_OnWidowLanded(object sender, System.EventArgs e) {
        PlaySFXAfterDelay(landAudioClip, delayToPlayLandSFX);
    }

    private void Widow_OnWidowJumpStarted(object sender, System.EventArgs e) {
        PlaySFXAfterDelay(jumpAudioClip, delayToPlayJumpSFX);

    }
}
