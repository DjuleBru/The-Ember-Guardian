using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator gunBodyAnimator;
    [SerializeField] private Animator emberBodyAnimator;
    [SerializeField] private GameObject breatheVisual;

    public event EventHandler OnFootStepTriggered;

    private float walkAnimationSpeed = 1f;
    private float previousMoveDir = 1f;
    private float moveDir;

    private bool dead;
    private bool moving;


    private void Awake() {
        Portal.OnAnyPlayerTeleported += Portal_OnPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerSHoor_OnPlayerSwappedGun;
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerJumpUp += PlayerMovement_OnPlayerJumpUp;
        PlayerMovement.Instance.OnPlayerJumpTop += PlayerMovement_OnPlayerJumpTop;
        PlayerMovement.Instance.OnPlayerJumpDown += PlayerMovement_OnPlayerJumpDown;
        PlayerMovement.Instance.OnPlayerLanded += PlayerMovement_OnPlayerLanded;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped += PlayerMovement_OnPlayerRunStopped;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStopped += PlayerMovement_OnPlayerAlmostExhaustionStopped;

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        Player.Instance.OnPlayerDamagedRecentlyEnded += Player_OnPlayerDamagedRecentlyEnded;

        breatheVisual.SetActive(false);
    }

    private void PlayerSHoor_OnPlayerSwappedGun(object sender, EventArgs e) {
        gunBodyAnimator = PlayerShoot.Instance.GetHeldGun().GetGunBodyAnimator();
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStopped(object sender, EventArgs e) {
        breatheVisual.SetActive(false);
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStarted(object sender, EventArgs e) {
        breatheVisual.SetActive(true);
    }

    private void Player_OnPlayerDamagedRecentlyEnded(object sender, EventArgs e) {
        bodyAnimator.SetBool("DamagedRecently", false);
        gunBodyAnimator.SetBool("DamagedRecently", false);
        emberBodyAnimator.SetBool("DamagedRecently", false);
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

        if (Player.Instance.GetDead()) return;
        bodyAnimator.SetBool("DamagedRecently", true);
        gunBodyAnimator.SetBool("DamagedRecently", true);
        emberBodyAnimator.SetBool("DamagedRecently", true);
    }

    private void Portal_OnPlayerTeleported(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport");
        gunBodyAnimator.SetTrigger("Teleport");
        emberBodyAnimator.SetTrigger("Teleport");
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport_Out");
        gunBodyAnimator.SetTrigger("Teleport_Out");
        emberBodyAnimator.SetTrigger("Teleport_Out");
    }

    private void Update() {
        if (dead) return;
        moveDir = GameInput.Instance.GetMovementFloatNormalized();

        //HandleXScale();
        if (PlayerMovement.Instance.IsMovingBackwards()) {
            playerAnimator.SetFloat("WalkAnimationSpeed", -walkAnimationSpeed);
        } else {
            playerAnimator.SetFloat("WalkAnimationSpeed", walkAnimationSpeed);
        }
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

    private void PlayerMovement_OnPlayerRunStopped(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void HandleAnimatorMovementBool() {
        if(!Player.Instance.GetCanMove()) {
            if(moving) {
                playerAnimator.SetBool("Walking", false);
                moving = false;
            }
            return;
        }

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

    public void FootStepEvent() {
        OnFootStepTriggered?.Invoke(this, EventArgs.Empty);
    }


    private void OnDestroy() {
        PlayerMovement.Instance.OnPlayerJumpUp -= PlayerMovement_OnPlayerJumpUp;
        PlayerMovement.Instance.OnPlayerJumpTop -= PlayerMovement_OnPlayerJumpTop;
        PlayerMovement.Instance.OnPlayerJumpDown -= PlayerMovement_OnPlayerJumpDown;
        PlayerMovement.Instance.OnPlayerLanded -= PlayerMovement_OnPlayerLanded;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;
        PlayerMovement.Instance.OnPlayerRunStarted -= PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped -= PlayerMovement_OnPlayerRunStopped;
        PlayerMovement.Instance.OnPlayerExhaustionStarted -= PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped -= PlayerMovement_OnPlayerExhaustionStopped;

        Player.Instance.OnPlayerDamaged -= Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
        Player.Instance.OnPlayerDamagedRecentlyEnded -= Player_OnPlayerDamagedRecentlyEnded;
        Portal.OnAnyPlayerTeleported -= Portal_OnPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
    }

    private void OnDisable() {
        playerAnimator.SetBool("Walking", false);
        moving = false;
    }

}
