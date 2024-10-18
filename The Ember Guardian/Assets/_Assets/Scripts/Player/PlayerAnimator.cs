using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator gunAnimator;

    private float previousMoveDir = 1f;
    private float moveDir;

    private bool moving;

    private void Start() {
        GameInput.Instance.OnPlayerRunStarted += GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled += GameInput_OnPlayerRunCanceled;
    }

    private void GameInput_OnPlayerRunCanceled(object sender, System.EventArgs e) {
        playerAnimator.SetBool("Running", false);
        gunAnimator.SetBool("Running", false);
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        playerAnimator.SetBool("Walking", true);
        playerAnimator.SetBool("Running", true);
        gunAnimator.SetBool("Walking", true);
        gunAnimator.SetBool("Running", true);
    }

    private void Update() {
        moveDir = GameInput.Instance.GetMovementFloatNormalized();

        HandleXScale();
        HandleAnimatorMovementBool();

    }

    private void HandleAnimatorMovementBool() {

        if (moveDir != 0) {

            if (!moving) {
                playerAnimator.SetBool("Walking", true);
                gunAnimator.SetBool("Walking", true);
            }
            moving = true;

        }
        else {

            if (moving) {
                playerAnimator.SetBool("Walking", false);
                gunAnimator.SetBool("Walking", false);
            }
            moving = false;

        }
    }

    private void HandleXScale() {

        if(moveDir < 0 && previousMoveDir > 0) {
            previousMoveDir = moveDir;
            Vector3 newScale = new Vector3(-1,1,1);
            transform.localScale = newScale;
        }

        if(moveDir > 0 && previousMoveDir < 0) {
            previousMoveDir = moveDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            transform.localScale = newScale;
        }
    }
}
