using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTravelTPVisual : MonoBehaviour
{
    [SerializeField] private Animator tpAnimator;
    [SerializeField] private FastTravelTP fastTravelTP;

    private void Start() {
        fastTravelTP.OnPlayerWarped += FastTravelTP_OnPlayerWarped;
        fastTravelTP.OnPlayerWarpedOut += FastTravelTP_OnPlayerWarpedOut;
        fastTravelTP.OnOtherCharacterWarped += FastTravelTP_OnOtherCharacterWarped;
    }

    private void FastTravelTP_OnOtherCharacterWarped(object sender, System.EventArgs e) {
        tpAnimator.SetTrigger("Warp");
    }

    private void FastTravelTP_OnPlayerWarpedOut(object sender, System.EventArgs e) {
        tpAnimator.SetTrigger("Warp");
    }

    private void FastTravelTP_OnPlayerWarped(object sender, System.EventArgs e) {
        tpAnimator.SetTrigger("Warp");
    }
}
