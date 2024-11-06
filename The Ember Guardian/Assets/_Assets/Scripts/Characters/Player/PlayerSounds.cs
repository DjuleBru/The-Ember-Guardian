using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource playerMovementAudioSource;
    [SerializeField] private AnimationCurve pitchWithSpeedAnimationCurve;

    private float minSpeedToPlaySound = .3f;
    private bool moveAudioPaused;

    private void Update() {
        HandleMovementAudio();
    }

    private void HandleMovementAudio() {
        float moveSpeed = Mathf.Abs(PlayerMovement.Instance.GetMoveSpeed());

        float pitch = pitchWithSpeedAnimationCurve.Evaluate(moveSpeed);
        playerMovementAudioSource.pitch = pitch;

        if (moveSpeed < minSpeedToPlaySound && !moveAudioPaused) {
            moveAudioPaused = true;
            playerMovementAudioSource.Pause();
        }

        if(moveSpeed > minSpeedToPlaySound &&  moveAudioPaused) {
            moveAudioPaused = false;
            playerMovementAudioSource.Play();
        }


    }

}
