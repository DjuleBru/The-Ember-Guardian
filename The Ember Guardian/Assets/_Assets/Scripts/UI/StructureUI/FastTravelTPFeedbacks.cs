using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTravelTPFeedbacks : MonoBehaviour
{
    [SerializeField] private FastTravelTP fastTravelTP;
    [SerializeField] private MMF_Player startTPFeedbacks;

    [SerializeField] private ParticleSystem stepOnTPPS;
    [SerializeField] private ParticleSystem teleportPS;

    private void Start() {
        fastTravelTP.OnPlayerWarped += FastTravelTP_OnPlayerWarped;
        fastTravelTP.OnPlayerWarpedOut += FastTravelTP_OnPlayerWarpedOut;
        fastTravelTP.OnPlayerPositionedOnTP += FastTravelTP_OnPlayerPositionedOnTP;
        fastTravelTP.OnPlayerCanceledTP += FastTravelTP_OnPlayerCanceledTP;
        fastTravelTP.OnOtherCharacterWarped += FastTravelTP_OnOtherCharacterWarped;
    }

    private void FastTravelTP_OnOtherCharacterWarped(object sender, System.EventArgs e) {
        stepOnTPPS.Play();
    }

    private void FastTravelTP_OnPlayerWarpedOut(object sender, System.EventArgs e) {
        CameraManager.Instance.ZoomIn(true);
        stepOnTPPS.Play();
    }

    private void FastTravelTP_OnPlayerCanceledTP(object sender, System.EventArgs e) {
        CameraManager.Instance.ZoomIn(true);
    }

    private void FastTravelTP_OnPlayerPositionedOnTP(object sender, System.EventArgs e) {
        CameraManager.Instance.ZoomIn(false, 1.1f, 1f);
        stepOnTPPS.Play();
    }

    private void FastTravelTP_OnPlayerWarped(object sender, System.EventArgs e) {
        startTPFeedbacks.PlayFeedbacks();
        teleportPS.Play();
    }
}
