using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator gunBodyAnimator;

    private float previousMoveDir = 1f;
    private float moveDir;

    private bool dead;
    private bool moving;

    private void Start() {
        GameInput.Instance.OnPlayerRunStarted += GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled += GameInput_OnPlayerRunCanceled;

        PlayerMovement.Instance.OnPlayerJumpUp += PlayerMovement_OnPlayerJumpUp;
        PlayerMovement.Instance.OnPlayerJumpTop += PlayerMovement_OnPlayerJumpTop;
        PlayerMovement.Instance.OnPlayerJumpDown += PlayerMovement_OnPlayerJumpDown;
        PlayerMovement.Instance.OnPlayerLanded += PlayerMovement_OnPlayerLanded;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        playerAnimator.Play("Idle");
        dead = false;
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("Die");
        dead = true;
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        bodyAnimator.SetTrigger("Hit");
        gunBodyAnimator.SetTrigger("Hit");
    }

    private void Update() {
        if (dead) return;
        moveDir = GameInput.Instance.GetMovementFloatNormalized();

        HandleXScale();
        HandleAnimatorMovementBool();
    }

    private void PlayerMovement_OnPlayerCrouchedEnded(object sender, System.EventArgs e) {
        playerAnimator.SetBool("Crouching", false);
    }

    private void PlayerMovement_OnPlayerCrouched(object sender, System.EventArgs e) {
        playerAnimator.SetBool("Crouching", true);
    }

    private void PlayerMovement_OnPlayerLanded(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("Land");
    }

    private void PlayerMovement_OnPlayerJumpDown(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("JumpDown");
    }

    private void PlayerMovement_OnPlayerJumpTop(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("JumpTop");
    }

    private void PlayerMovement_OnPlayerJumpUp(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("JumpUp");

        playerAnimator.ResetTrigger("JumpDown");
        playerAnimator.ResetTrigger("JumpTop");
        playerAnimator.ResetTrigger("Land");
    }

    private void GameInput_OnPlayerRunCanceled(object sender, System.EventArgs e) {
        playerAnimator.SetBool("Running", false);
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        if (!moving) return;

        playerAnimator.SetBool("Walking", true);
        playerAnimator.SetBool("Running", true);
    }

    private void HandleAnimatorMovementBool() {

        if (moveDir != 0) {
            if (!moving) {
                playerAnimator.SetBool("Walking", true);
            }
            moving = true;

        }
        else {
            if (moving) {
                playerAnimator.SetBool("Walking", false);
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
