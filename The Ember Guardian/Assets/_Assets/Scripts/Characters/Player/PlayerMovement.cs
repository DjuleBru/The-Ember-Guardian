using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour {

    public static PlayerMovement Instance;

    [SerializeField] private float moveSpeedBackwardsMultiplier = .7f;
    [SerializeField] private float exhaustedSpeedFactor = 1.4f;
    [SerializeField] private float aimingSightDecelerationFactor = .7f;
    [SerializeField] private float crouchAccelerationFactor = .7f;
    private float gunWeightAccelerationFactor;

    private bool isRecoveringFast;
    private float runRecoverFactor = 1.3f;
    private float standingStillTRecoverFactor = 3f;

    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velPower;

    [SerializeField] private float jumpForce;
    [SerializeField] private float rollForce;
    [SerializeField] private float jumpCutMultiplier;
    [SerializeField] private float gravityScale;
    [SerializeField] private float fallGravityMultiplier;

    [SerializeField] private float castDistance;
    [SerializeField] Vector2 boxSize;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask platformLayerMask;

    private bool holdToRun;
    private bool isRunning;
    private bool isAlmostExhausted;
    private bool isAlmostExhaustedFeedbacksActive;
    private bool isExhausted;
    private bool isMovingBackwards;
    private bool isCrouching;
    private bool isJumping;
    private bool isRolling;
    private bool isJumpTop;
    private bool isJumpDown;
    private bool isLanded;
    private bool isHubScene;

    private float moveSpeed;
    private float lastMoveDir = 1;
    private float lastJumpTime;
    private float staminaTimer;
    private float rollExhaustionAmount = 2f;
    private float rollAnimationDuration = .6f;
    private float exhaustionTimer;
    private float runTimePercentageBeforeWarningExhaustion = .75f;
    private Rigidbody2D rb;

    private bool isMoving;

    public event EventHandler OnPlayerMovespeedChanged;
    public event EventHandler OnPlayerRoll;
    public event EventHandler OnPlayerRollEnded;
    public event EventHandler OnPlayerJumpUp;
    public event EventHandler OnPlayerJumpTop;
    public event EventHandler OnPlayerJumpDown;
    public event EventHandler OnPlayerLanded;
    public event EventHandler OnPlayerMoveStarted;
    public event EventHandler OnPlayerMoveStopped;
    public event EventHandler OnPlayerRunStarted;
    public event EventHandler OnPlayerRunStopped;
    public event EventHandler OnPlayerAlmostExhaustionStarted;
    public event EventHandler OnPlayerAlmostExhaustionStopped;
    public event EventHandler OnPlayerAlmostExhaustionDeactivateFeedbacks;
    public event EventHandler OnPlayerExhaustionStarted;
    public event EventHandler OnPlayerExhaustionStopped;
    public event EventHandler OnPlayerRecoverStaminaFastStarted;
    public event EventHandler OnPlayerRecoverStaminaFastStopped;

    public event EventHandler OnPlayerCrouched;
    public event EventHandler OnPlayerCrouchedEnded;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        Instance = this;
    }

    private void Start() {
        moveSpeed = PlayerStats.Instance.GetInitialMoveSpeed();

        GameInput.Instance.OnPlayerRunPerformed += GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled += GameInput_OnPlayerRunCanceled;
        GameInput.Instance.OnPlayerRollCanceled += GameInput_OnPlayerJumpCanceled;
        GameInput.Instance.OnPlayerRollPerformed += GameInput_OnPlayerJumpStarted;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAIm_OnPlayerAimSightStarted;
        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAim_OnPlayerAimSightEnded;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadHandEnded += PlayerShoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded += PlayerShoot_OnPlayerReloadInterruptedEnded;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;

        PlayerStats.Instance.OnMoveSpeedChanged += PlayerState_OnMoveSpeedChanged;

        SettingsManager.Instance.OnHoldToggleRunChanged += SettingsManager_OnHoldToggleRunChanged;
        holdToRun = SettingsManager.Instance.GetHoldToRun();

        isHubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;
    }


    private void FixedUpdate() {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PlayerShoot.Instance.GetHoldingStationaryGun()) return;
        HandleMovementForces();
    }

    private void Update() {
        HandleMovingBackwards();
        HandleRunningAndExhaustion();
        DetectMovement();

        if (Player.Instance.GetPlayerControlInputsEnabled()) {
            HandleCrouch();

            // LAST MOVE DIR
            float lastMoveInput = GameInput.Instance.GetMovementFloatNormalized();
            if (lastMoveInput != 0) {
                lastMoveDir = lastMoveInput;
            }
        };

        // GRAVITY FALL
        if (isJumping && !isJumpTop && rb.velocity.y < 1 && rb.velocity.y > 0) {
            // Y velocity is low : reach top of jump
            OnPlayerJumpTop?.Invoke(this, EventArgs.Empty);
            isJumpTop = true;
            //Debug.Log("isJumpTop");
        }


        if(isJumping && !isJumpDown && rb.velocity.y < 0) {
            OnPlayerJumpDown?.Invoke(this, EventArgs.Empty);
            isJumpDown = true;
            //Debug.Log("isJumpDown");
        }

        if(isJumping && !isLanded && rb.velocity.y < 2 && IsGrounded()) {
            OnPlayerLanded?.Invoke(this, EventArgs.Empty);
            isLanded = true;
            isJumping = false;
        }

        if (rb.velocity.y < -0.1) {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else {
            rb.gravityScale = gravityScale;
        }
    }

    private void DetectMovement() {
        if (GameInput.Instance.GetMovementFloatNormalized() != 0 && !isMoving) {
            isMoving = true;
            OnPlayerMoveStarted?.Invoke(this, EventArgs.Empty);
        }

        if (GameInput.Instance.GetMovementFloatNormalized() == 0 && isMoving) {
            isMoving = false;
            OnPlayerMoveStopped?.Invoke(this, EventArgs.Empty);
        }

    }

    private void SettingsManager_OnHoldToggleRunChanged(object sender, EventArgs e) {
        holdToRun = SettingsManager.Instance.GetHoldToRun();
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if(gunWeightAccelerationFactor == 0) {
            // Gun weight initialization

            gunWeightAccelerationFactor = PlayerShoot.Instance.GetGunWeightAccelerationFactor();
            BuffMoveSpeed(gunWeightAccelerationFactor);

        } else {
            // Changing gun

            DebuffMoveSpeed(gunWeightAccelerationFactor);
            gunWeightAccelerationFactor = PlayerShoot.Instance.GetGunWeightAccelerationFactor();
            BuffMoveSpeed(gunWeightAccelerationFactor);
        }
    }

    private void PlayerShoot_OnPlayerReloadHandEnded(object sender, EventArgs e) {
        float reloadAccelerationFactor = PlayerShoot.Instance.GetGunReloadAccelerationFactor();
        DebuffMoveSpeed(reloadAccelerationFactor);
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, EventArgs e) {
        float reloadAccelerationFactor = PlayerShoot.Instance.GetGunReloadAccelerationFactor();
        DebuffMoveSpeed(reloadAccelerationFactor);
    }

    private void PlayerShoot_OnPlayerReloadInterruptedEnded(object sender, EventArgs e) {
        float reloadAccelerationFactor = PlayerShoot.Instance.GetGunReloadAccelerationFactor();
        BuffMoveSpeed(reloadAccelerationFactor);
    }

    private void PlayerShoot_OnPlayerReload(object sender, EventArgs e) {
        float reloadAccelerationFactor = PlayerShoot.Instance.GetGunReloadAccelerationFactor();
        BuffMoveSpeed(reloadAccelerationFactor);
    }

    private void PlayerState_OnMoveSpeedChanged(object sender, EventArgs e) {
        moveSpeed = PlayerStats.Instance.GetMoveSpeed();
    }

    private void GameInput_OnPlayerJumpStarted(object sender, System.EventArgs e) {

        if (isRolling) return;
        if (isExhausted) return;
        if (PauseMenuUI.Instance.isPaused) return;
        if (PlayerShoot.Instance.GetHoldingStationaryGun()) return;
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.roll) || PlayerShoot.Instance.GetHeldGun().GetGunJustJammed()) return;

        StartRolling();
        return;

        if (GetPlatformStanding() != null) {
            if (GameInput.Instance.GetJumpDirNormalized() <= -.5) {
                PlatformJumpDown();
            } else {
                //StartJumping();
            }
        } else {
            //StartJumping();
        }
    }

    private void GameInput_OnPlayerJumpCanceled(object sender, System.EventArgs e) {
        if(rb.velocity.y > 0 && isJumping) {
            // Reduce current y velocity by amount
            rb.AddForce(Vector2.down * rb.velocity * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        lastJumpTime = 0;
    }

    private void PlayerAim_OnPlayerAimSightEnded(object sender, EventArgs e) {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.isPaused) return;

        BuffMoveSpeed(PlayerStats.Instance.GetAimingSightDecelerationFactor());
    }

    private void PlayerAIm_OnPlayerAimSightStarted(object sender, EventArgs e) {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.isPaused) return;

        DebuffMoveSpeed(PlayerStats.Instance.GetAimingSightDecelerationFactor());
    }

    private void GameInput_OnPlayerRunCanceled(object sender, System.EventArgs e) {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.isPaused) return;

        if (!holdToRun) return;
        if (!isRunning) return;

        StopRunning();
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.isPaused) return;

        if(holdToRun) {
            if (isExhausted) return;
            StartRunning();
        } else {
            if(!isRunning) {
                StartRunning();
            } else {
                StopRunning();
            }
        }
    }

    private void HandleMovingBackwards() {

        if((lastMoveDir > 0 && PlayerAim.Instance.GetAimDir().x < 0) || (lastMoveDir < 0 && PlayerAim.Instance.GetAimDir().x > 0)) {

            if(!isMovingBackwards) {

                isMovingBackwards = true;
                BuffMoveSpeed(moveSpeedBackwardsMultiplier);
            }

        } else {

            if(isMovingBackwards) {
                isMovingBackwards = false;
                DebuffMoveSpeed(moveSpeedBackwardsMultiplier);
            }

        }
    }

    private void HandleCrouch() {
        if (isJumping) return;
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.isPaused) return;
        if (PlayerShoot.Instance.GetHoldingStationaryGun()) return;

        if (GameInput.Instance.GetJumpDirNormalized() <= -.5) {
            if (!isCrouching) {
                isCrouching = true;
                moveSpeed *= crouchAccelerationFactor;
                OnPlayerCrouched?.Invoke(this, EventArgs.Empty);
            }

        } else {

            if(isCrouching) {
                isCrouching = false;
                moveSpeed /= crouchAccelerationFactor;
                OnPlayerCrouchedEnded?.Invoke(this, EventArgs.Empty);
            }

        }
    }

    private void StartJumping() {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = 0;

        isJumping = true;
        isJumpTop = false;
        isJumpDown = false;
        isLanded = false;
        isCrouching = false;

        OnPlayerCrouchedEnded?.Invoke(this, EventArgs.Empty);
        OnPlayerJumpUp?.Invoke(this, EventArgs.Empty);
    }

    private void StartRolling() {
        float rollDir = 1f;

        if(GameInput.Instance.GetMovementFloatNormalized() != 0) {
            rollDir = lastMoveDir;
        } else {
            rollDir = PlayerAim.Instance.GetAimDirFloat();
        }

        float rollForceMetaBuff = rollForce * PlayerStats.Instance.GetRollForcePercentBuff_Meta() / 100f;
        Vector2 force = new Vector2(rollDir * (rollForce + rollForceMetaBuff), 2f);
        rb.AddForce(force, ForceMode2D.Impulse);
        isJumping = false;
        isRolling = true;
        isJumpTop = false;
        isJumpDown = false;

        if(!isHubScene) {
            float rollExhaustionAmountBuff = rollExhaustionAmount * PlayerStats.Instance.GetRollStaminaDepletionPercentBuff_Meta() / 100;
            staminaTimer += rollExhaustionAmount - rollExhaustionAmountBuff;
        }

        OnPlayerRoll?.Invoke(this, EventArgs.Empty);
        Invoke("EndRoll", .6f);
    }

    private void EndRoll() {
        OnPlayerRollEnded?.Invoke(this, EventArgs.Empty);
        isRolling = false;
    }

    private void PlatformJumpDown() {
        Platform platformStandingOn = GetPlatformStanding();

        if (platformStandingOn != null) {
            platformStandingOn.DisablePlatformCollider();

            OnPlayerJumpDown?.Invoke(this, EventArgs.Empty);
            isLanded = false;
            isJumping = true;
        }

    }

    private void HandleMovementForces() {
        float targetSpeed = GameInput.Instance.GetMovementFloatNormalized() * moveSpeed;

        if(GameInput.Instance.GetMovementFloatNormalized() * WindManager.Instance.GetWindDir() > 0) {
            // Player is moving in the same dir as wind
            targetSpeed *= WindManager.Instance.GetWindStrengthImpactOnSpeed();
        } else {
            // Player is moving in the opposite dir as wind
            targetSpeed /= WindManager.Instance.GetWindStrengthImpactOnSpeed();
        }

        //if (isMovingBackwards) {
        //    targetSpeed *= moveSpeedBackwardsMultiplier;
        //}

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    private void HandleRunningAndExhaustion() {
        // Handle run toggle : if move input = 0, stop running
        if(!holdToRun) {
            if(isRunning && GameInput.Instance.GetMovementFloatNormalized() == 0) {
                StopRunning();
            }
        }

        if(isExhausted) {
            exhaustionTimer += Time.deltaTime;

            if(exhaustionTimer >= PlayerStats.Instance.GetExhaustionTime()) {
                StopExhausted();
            }

            return;
        }

        // Recover only when not exhausted anymore
        if (!isRunning && !isExhausted && !isRolling && staminaTimer > 0) {
            if(staminaTimer > PlayerStats.Instance.GetMaxStamina()) {
                staminaTimer = PlayerStats.Instance.GetMaxStamina();
            }

            float recoverFactor = runRecoverFactor;
            if (!isMoving && !IsMovingBackwards()) {
                // Player is standing still
                recoverFactor = standingStillTRecoverFactor;

                if(!isRecoveringFast && staminaTimer > PlayerStats.Instance.GetMaxStamina()/2) {
                    // Trigger recovering fast feedbacks when actually tired
                    OnPlayerRecoverStaminaFastStarted?.Invoke(this, EventArgs.Empty);
                    isRecoveringFast = true;
                }

                if(isRecoveringFast && staminaTimer < .1f) {
                    OnPlayerRecoverStaminaFastStopped?.Invoke(this, EventArgs.Empty);
                    isRecoveringFast = false;
                }

            } else {
                // Player is moving
                if (isRecoveringFast) {
                    OnPlayerRecoverStaminaFastStopped?.Invoke(this, EventArgs.Empty);
                    isRecoveringFast = false;
                }
            }

            staminaTimer -= recoverFactor * Time.deltaTime;
        }

        if (isAlmostExhausted) {
            if (staminaTimer <= 0) {
                isAlmostExhausted = false;
                // Remove breathing animation
                OnPlayerAlmostExhaustionStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        if (isAlmostExhaustedFeedbacksActive && staminaTimer < PlayerStats.Instance.GetMaxStamina() * runTimePercentageBeforeWarningExhaustion) {
            isAlmostExhaustedFeedbacksActive = false;
            // Remove almost exhausted feedbacks & sound
            OnPlayerAlmostExhaustionDeactivateFeedbacks?.Invoke(this, EventArgs.Empty);
        }

        if (isRunning && (moveSpeed != 0) && !isHubScene) {
            staminaTimer += Time.deltaTime * (1 - PlayerStats.Instance.GetRunStaminaDepletionPercentBuff_Meta()/100f);

            if (isRecoveringFast) {
                OnPlayerRecoverStaminaFastStopped?.Invoke(this, EventArgs.Empty);
                isRecoveringFast = false;
            }

        }

        // Almost exhausted
        if (staminaTimer > PlayerStats.Instance.GetMaxStamina() * runTimePercentageBeforeWarningExhaustion) {

            if (!isAlmostExhausted) {
                isAlmostExhausted = true;
            }

            if(!isAlmostExhaustedFeedbacksActive) {
                // Activate almost exhausted feedbacks & sound
                isAlmostExhaustedFeedbacksActive = true;
                OnPlayerAlmostExhaustionStarted?.Invoke(this, EventArgs.Empty);
            }

        }


        // Exhausted
        if (staminaTimer > PlayerStats.Instance.GetMaxStamina() && !isExhausted) {
            StartExhausted();

            if(isRunning) {
                StopRunning();
            }
        }

    }

    public float GetStaminaTimerNormalized() {
        return staminaTimer / PlayerStats.Instance.GetMaxStamina();
    }

    private void StartRunning() {
        isRunning = true;
        BuffMoveSpeed(PlayerStats.Instance.GetRunAccelerationFactor());
        OnPlayerRunStarted?.Invoke(this, EventArgs.Empty);
    }

    private void StopRunning() {
        isRunning = false;
        DebuffMoveSpeed(PlayerStats.Instance.GetRunAccelerationFactor());
        OnPlayerRunStopped?.Invoke(this, EventArgs.Empty);
    }

    private void StartExhausted() {
        isExhausted = true;
        DebuffMoveSpeed(exhaustedSpeedFactor);

        OnPlayerExhaustionStarted?.Invoke(this, EventArgs.Empty);
    }

    private void StopExhausted() {
        isExhausted = false;

        staminaTimer = 0;
        exhaustionTimer = 0;
        BuffMoveSpeed(exhaustedSpeedFactor);
        OnPlayerExhaustionStopped?.Invoke(this, EventArgs.Empty);
        OnPlayerAlmostExhaustionStopped?.Invoke(this, EventArgs.Empty);
    }

    public void BuffMoveSpeed(float buffAmount) {
        //Debug.Log("BuffMoveSpeed " + buffAmount);
        moveSpeed *= buffAmount;
        OnPlayerMovespeedChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DebuffMoveSpeed(float buffAmount) {
        //Debug.Log("DebuffMoveSpeed " + buffAmount);
        moveSpeed /= buffAmount;
        OnPlayerMovespeedChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsMovingBackwards() {
        return isMovingBackwards;
    }
    public bool IsGrounded() {
        if(Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayerMask) || Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, platformLayerMask)) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsRunning() {
        return isRunning;
    }

    public bool IsCrouching() {
        return isCrouching;
    } 
    public float GetLastMoveDir() {
        return lastMoveDir;
    }

    public float GetCurrentMoveDir() {

        if(GameInput.Instance.GetMovementFloatNormalized() > 0.01) {
            return 1f;
        }

        if (GameInput.Instance.GetMovementFloatNormalized() < -0.01) {
            return -1f;
        }

        return 0;
    }

    public Platform GetPlatformStanding() {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, platformLayerMask);

        if (hit.collider != null) {
            Platform platformHit = hit.collider.GetComponent<Platform>();
            if (platformHit != null) {
                return platformHit; // Récupère la plateforme touchée
            }
        }

        return null; // Pas de plateforme touchée
    }

    public bool GetRunning() {
        return isRunning;
    }

    public float GetMoveSpeed() {
        return rb.velocity.x;
    }
    public float GetTargetMoveSpeed() {
        return moveSpeed;
    }
    public float GetMoveSpeedNormalized() {
        return moveSpeed / PlayerStats.Instance.GetMoveSpeed();
    }

    public void StopMovement() {
        rb.velocity = Vector2.zero;
       
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position-transform.up*castDistance, boxSize);
    }
    private void OnDestroy() {

        GameInput.Instance.OnPlayerRunPerformed -= GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled -= GameInput_OnPlayerRunCanceled;
        GameInput.Instance.OnPlayerRollCanceled -= GameInput_OnPlayerJumpCanceled;
        GameInput.Instance.OnPlayerRollPerformed -= GameInput_OnPlayerJumpStarted;
        PlayerAim.Instance.OnPlayerAimSightStarted -= PlayerAIm_OnPlayerAimSightStarted;
        PlayerAim.Instance.OnPlayerAimSightEnded -= PlayerAim_OnPlayerAimSightEnded;

        PlayerStats.Instance.OnMoveSpeedChanged -= PlayerState_OnMoveSpeedChanged;
    }
}
