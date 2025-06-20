using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator gunBodyAnimator;
    [SerializeField] private Animator armBodyAnimator;
    [SerializeField] private Animator emberBodyAnimator;
    [SerializeField] private GameObject breatheVisual;
    [SerializeField] private ParticleSystem respawnPS;
    [SerializeField] private ParticleSystem diePS;
    [SerializeField] private ParticleSystem runDustPS;

    public event EventHandler OnFootStepTriggered;

    private float walkAnimationSpeed = 1f;
    private float previousMoveDir = 1f;
    private float moveDir;

    private bool dead;
    private bool moving;
    private bool running;
    private bool isAlmostExhausted;


    private void Awake() {
        Portal.OnAnyPlayerTeleported += Portal_OnPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerSHoor_OnPlayerSwappedGun;
    }


    private void Start() {
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
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

        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerSetupLMGStarted += PlayerShoot_OnPlayerSetupLMGStarted;
        PlayerShoot.Instance.OnPlayerResetLMGBipod += PlayerShoot_OnPlayerResetLMGBipod;

        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAIm_OnPlayerAimSightEnded;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAim_OnPlayerAimSightStarted;

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerDamagedRecentlyEnded += Player_OnPlayerDamagedRecentlyEnded;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;

        PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerStoppedPettingDog += PetDogf_OnPlayerStoppedPettingDog;

        breatheVisual.SetActive(false);
    }


    private void Update() {
        if (dead) return;
        moveDir = GameInput.Instance.GetMovementFloatNormalized();

        //HandleXScale();
        if (PlayerMovement.Instance.IsMovingBackwards()) {
            playerAnimator.SetFloat("WalkAnimationSpeed", -walkAnimationSpeed);
            playerAnimator.SetFloat("RollAnimationSpeed", -1f);

            if (running && playerAnimator.GetBool("Running")) {
                playerAnimator.SetBool("Running", false);
            }

        }
        else {
            playerAnimator.SetFloat("WalkAnimationSpeed", walkAnimationSpeed);
            playerAnimator.SetFloat("RollAnimationSpeed", 1f);

            if (running && !playerAnimator.GetBool("Running")) {
                playerAnimator.SetBool("Running", true);
            }

        }
        HandleAnimatorMovementBool();
    }

    private void PetDogf_OnPlayerStoppedPettingDog(object sender, EventArgs e) {
        playerAnimator.SetTrigger("PetDogEnd");
    }
    private void PetDog_OnPlayerStartedPettingDog(object sender, EventArgs e) {
        playerAnimator.SetTrigger("PetDogStart");
    }

    private void PlayerShoot_OnPlayerResetLMGBipod(object sender, EventArgs e) {
        playerAnimator.SetBool("Crouching", false);
    }


    private void PlayerShoot_OnPlayerSetupLMGStarted(object sender, EventArgs e)
    {
        playerAnimator.SetBool("Crouching", true);

    }

    private void PlayerSHoor_OnPlayerSwappedGun(object sender, EventArgs e) {
        gunBodyAnimator = PlayerShoot.Instance.GetHeldGun().GetGunBodyAnimator();
        armBodyAnimator = PlayerShoot.Instance.GetHeldGun().GetArmBodyAnimator();
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStopped(object sender, EventArgs e) {
        isAlmostExhausted = false;
        breatheVisual.SetActive(false);
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStarted(object sender, EventArgs e) {
        isAlmostExhausted = true;
        breatheVisual.SetActive(true);
    }

    private void PlayerShoot_OnPlayerReloadEnded(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }
    private void PlayerShoot_OnPlayerReload(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }
    private void Player_OnPlayerDamagedRecentlyEnded(object sender, EventArgs e) {
        bodyAnimator.SetBool("DamagedRecently", false);
        gunBodyAnimator.SetBool("DamagedRecently", false);
        armBodyAnimator.SetBool("DamagedRecently", false);
        emberBodyAnimator.SetBool("DamagedRecently", false);
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        playerAnimator.Play("Idle");

        bodyAnimator.SetTrigger("Respawn");
        gunBodyAnimator.SetTrigger("Respawn");
        armBodyAnimator.SetTrigger("Respawn");
        respawnPS.Play();
        dead = false;
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        playerAnimator.SetTrigger("Die");
        bodyAnimator.SetTrigger("Die");
        gunBodyAnimator.SetTrigger("Die");
        armBodyAnimator.SetTrigger("Die");
        diePS.Play();
        dead = true;
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        bodyAnimator.SetTrigger("Hit");
        gunBodyAnimator.SetTrigger("Hit");
        armBodyAnimator.SetTrigger("Hit");

        if (Player.Instance.GetDead()) return;
        bodyAnimator.SetBool("DamagedRecently", true);
        gunBodyAnimator.SetBool("DamagedRecently", true);
        armBodyAnimator.SetBool("DamagedRecently", true);
        emberBodyAnimator.SetBool("DamagedRecently", true);
    }

    private void Portal_OnPlayerTeleported(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport");
        gunBodyAnimator.SetTrigger("Teleport");
        armBodyAnimator.SetTrigger("Teleport");
        emberBodyAnimator.SetTrigger("Teleport");
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport_Out");
        gunBodyAnimator.SetTrigger("Teleport_Out");
        armBodyAnimator.SetTrigger("Teleport_Out");
        emberBodyAnimator.SetTrigger("Teleport_Out");
    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport_Out");
        gunBodyAnimator.SetTrigger("Teleport_Out");
        armBodyAnimator.SetTrigger("Teleport_Out");
        emberBodyAnimator.SetTrigger("Teleport_Out");
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, EventArgs e) {
        bodyAnimator.SetTrigger("Teleport");
        gunBodyAnimator.SetTrigger("Teleport");
        armBodyAnimator.SetTrigger("Teleport");
        emberBodyAnimator.SetTrigger("Teleport");
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


    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (PlayerMovement.Instance.IsCrouching()) {
            playerAnimator.SetBool("Crouching", false);
        }

        playerAnimator.ResetTrigger("RollFinished");
        playerAnimator.SetTrigger("Roll");
        breatheVisual.SetActive(false);
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        if(PlayerMovement.Instance.IsCrouching()) {
            playerAnimator.SetBool("Crouching", true);
        }

        playerAnimator.SetTrigger("RollFinished");
        breatheVisual.SetActive(isAlmostExhausted);
    }
    private void PlayerAim_OnPlayerAimSightStarted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();

    }

    private void PlayerAIm_OnPlayerAimSightEnded(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();

    }

    private void PlayerMovement_OnPlayerRunStopped(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
        playerAnimator.SetBool("Running", false);
        running = false;
    }

    private void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
        playerAnimator.SetBool("Running", true);
        running = true;
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, EventArgs e) {
        walkAnimationSpeed = PlayerMovement.Instance.GetMoveSpeedNormalized();
    }

    private void HandleAnimatorMovementBool() {
        if(!Player.Instance.GetPlayerControlInputsEnabled() || PlayerShoot.Instance.GetHoldingStationaryGun()) {
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

        if(PlayerMovement.Instance.GetRunning()) {
            int randomParticles = UnityEngine.Random.Range(0, 5);
            runDustPS.Emit(randomParticles);
        }
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

        PlayerShoot.Instance.OnPlayerReload -= PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded -= PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted -= PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerSetupLMGStarted -= PlayerShoot_OnPlayerSetupLMGStarted;
        PlayerShoot.Instance.OnPlayerResetLMGBipod -= PlayerShoot_OnPlayerResetLMGBipod;

        Player.Instance.OnPlayerDamaged -= Player_OnPlayerDamaged;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
        Player.Instance.OnPlayerDamagedRecentlyEnded -= Player_OnPlayerDamagedRecentlyEnded;
        Portal.OnAnyPlayerTeleported -= Portal_OnPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarped -= FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut -= FastTravelTP_OnAnyPlayerWarpedOut;
    }

    private void OnDisable() {
        playerAnimator.SetBool("Walking", false);
        moving = false;
    }

}
