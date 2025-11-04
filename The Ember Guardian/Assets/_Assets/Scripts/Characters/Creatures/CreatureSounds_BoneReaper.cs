using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSounds_BoneReaper : CreatureSound
{
    [SerializeField] private AudioClip enrageAudioClip;
    private CreatureAI_BoneReaper boneReaper;

    protected override void Start() {
        base.Start();
        boneReaper = creatureAI.GetComponent<CreatureAI_BoneReaper>();

        boneReaper.OnBoneReaperEnraged += BoneReaper_OnBoneReaperEnraged;
        boneReaper.OnBoneReaperSpawned += BoneReaper_OnBoneReaperEnraged;
    }

    private void BoneReaper_OnBoneReaperEnraged(object sender, System.EventArgs e) {
        PlaySound2D(enrageAudioClip, 3f);
    }
}
