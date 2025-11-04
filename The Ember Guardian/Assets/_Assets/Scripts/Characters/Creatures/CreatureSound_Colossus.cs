using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSound_Colossus : CreatureSound
{
    [SerializeField] private AudioClip wakeAudioClip;
    private CreatureAI_Colossus colossus;

    protected override void Start() {
        base.Start();
        colossus = creatureAI.GetComponent<CreatureAI_Colossus>();
        colossus.OnColossusWake += Colossus_OnColossusWake;
    }

    private void Colossus_OnColossusWake(object sender, System.EventArgs e) {
        PlaySound2D(wakeAudioClip);
    }
}
