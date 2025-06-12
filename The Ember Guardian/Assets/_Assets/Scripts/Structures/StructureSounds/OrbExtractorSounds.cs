using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExtractorSounds : StructureSounds {

    [SerializeField] private OrbExtractor orbExtractor;
    [SerializeField] private AudioClip[] pullLevelAudioClip;
    [SerializeField] private AudioClip[] drillAudioClip;
    [SerializeField] private AudioClip garrisonedEngineerAudioClip;

    private float delayBetweenDrillStartAndPullLever= .05f;
    private float delayBetweenLevelAndDrillSFX = .3f;

    protected override void Awake() {
        base.Awake();

        orbExtractor.OnExtractorStartedDrilling += OrbExtractor_OnExtractorStartedDrilling;
        orbExtractor.OnEngineerStartedWorking += OrbExtractor_OnEngineerStartedWorking;
        orbExtractor.OnEngineerStoppedWorking += OrbExtractor_OnEngineerStoppedWorking;
    }

    private void OrbExtractor_OnEngineerStoppedWorking(object sender, System.EventArgs e) {
        PlaySound2D(garrisonedEngineerAudioClip);
    }

    private void OrbExtractor_OnEngineerStartedWorking(object sender, System.EventArgs e) {
        PlaySound2D(garrisonedEngineerAudioClip, 2f);
    }

    private void OrbExtractor_OnExtractorStartedDrilling(object sender, System.EventArgs e) {
        PlaySFXAfterDelay(pullLevelAudioClip, delayBetweenDrillStartAndPullLever);

        PlaySFXAfterDelay(drillAudioClip, delayBetweenLevelAndDrillSFX);
    }
}
