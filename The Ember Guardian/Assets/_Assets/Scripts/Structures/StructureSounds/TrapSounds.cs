using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapSounds : SoundObject
{
    [SerializeField] private TrapAnimatorTriggerEvents trapAnimatorEvents;

    [SerializeField] private float trapTriggerVolumeMultiplier = 1f;
    [SerializeField] private float trapRearmVolumeMultiplier = 1f;

    private Structure_Trap trap;

    protected void Awake() {
        trap = GetComponentInParent<Structure_Trap>();
        trapAnimatorEvents.OnTrapTriggerSFX += TrapVisual_OnTrapTriggerSFX;
    }

    private void TrapVisual_OnTrapTriggerSFX(object sender, System.EventArgs e) {
        PlaySound2D(trap.GetTrapSO().triggerTrapAudioClip, trapTriggerVolumeMultiplier);
    }
}
