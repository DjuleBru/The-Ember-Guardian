using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem exhaustedPS;

    private void Start() {
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, System.EventArgs e) {
        exhaustedPS.Stop();
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, System.EventArgs e) {
        exhaustedPS.Play();
    }
}
