using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTravelTPSounds : StructureSounds
{
    [SerializeField] private FastTravelTP fastTravelTP;
    [SerializeField] private AudioClip positionOnTPAudioClip;
    [SerializeField] private AudioClip warpAudioClip;
    [SerializeField] private AudioClip selectReceiverTPAudioClip;
    [SerializeField] private AudioClip warpStartAudioClip;

    protected override void Start() {
        base.Start();
        fastTravelTP.OnPlayerPositionedOnTP += FastTravelTP_OnPlayerPositionedOnTP;
        fastTravelTP.OnPlayerWarped += FastTravelTP_OnPlayerWarped;
        fastTravelTP.OnPlayerWarpedOut += FastTravelTP_OnPlayerWarpedOut;
        fastTravelTP.OnPlayerCanceledTP += FastTravelTP_OnPlayerCanceledTP;
        fastTravelTP.OnPlayerWarpStarted += FastTravelTP_OnPlayerWarpStarted;
        fastTravelTP.OnReceiverFastTravelTPChanged += FastTravelTP_OnReceiverFastTravelTPChanged;
        fastTravelTP.OnOtherCharacterWarped += FastTravelTP_OnOtherCharacterWarped;
    }

    private void FastTravelTP_OnOtherCharacterWarped(object sender, System.EventArgs e) {
        PlaySound2D(warpAudioClip);
    }

    private void FastTravelTP_OnReceiverFastTravelTPChanged(object sender, System.EventArgs e) {
        PlaySound2D(selectReceiverTPAudioClip);
    }


    private void FastTravelTP_OnPlayerWarpStarted(object sender, System.EventArgs e) {
        PlaySound2D(warpStartAudioClip);
    }

    private void FastTravelTP_OnPlayerCanceledTP(object sender, System.EventArgs e) {
        PlaySound2D(positionOnTPAudioClip);
    }

    private void FastTravelTP_OnPlayerWarped(object sender, System.EventArgs e) {
        PlaySound2D(warpAudioClip);
    }

    private void FastTravelTP_OnPlayerWarpedOut(object sender, System.EventArgs e) {
        PlaySound2D(warpAudioClip);
    }
    private void FastTravelTP_OnPlayerPositionedOnTP(object sender, System.EventArgs e) {
        PlaySound2D(positionOnTPAudioClip);
    }
}
