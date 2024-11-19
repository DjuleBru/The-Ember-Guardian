using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour {

    public static PlayerMovement Instance;

    [SerializeField] private float initialMoveSpeed = 5f;
    [SerializeField] private float runMaxTime = 15f;
    [SerializeField] private float exhaustionTime = 5f;
    [SerializeField] private float moveSpeedBackwardsMultiplier = .7f;
    [SerializeField] private float exhaustedSpeedFactor = 1.3f;
    [SerializeField] private float runAccelerationFactor = 1.3f;
    [SerializeField] private float runRecoverFactor = 1.5f;
    [SerializeField] private float crouchAccelerationFactor = .7f;

    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velPower;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCutMultiplier;
    [SerializeField] private float gravityScale;
    [SerializeField] private float fallGravityMultiplier;

    [SerializeField] private float castDistance;
    [SerializeField] Vector2 boxSize;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask platformLayerMask;

    private bool isRunning;
    private bool isExhausted;
    private bool isMovingBackwards;
    private bool isCrouching;
    private bool isJumping;
    private bool isJumpTop;
    private bool isJumpDown;
    private bool isLanded;
    private bool jumpInputReleased;

    private float moveSpeed;
    private float lastMoveDir = 1;
    private float lastJumpTime;
    private float runTimer;
    private float exhaustionTimer;
    private Rigidbody2D rb;

    public event EventHandler OnPlayerJumpUp;
    public event EventHandler OnPlayerJumpTop;
    public event EventHandler OnPlayerJumpDown;
    public event EventHandler OnPlayerLanded;
    public event EventHandler OnPlayerRunStarted;
    public event EventHandler OnPlayerRunStopped;
    public event EventHandler OnPlayerExhaustionStarted;
    public event EventHandler OnPlayerExhaustionStopped;

    public event EventHandler OnPlayerCrouched;
    public event EventHandler OnPlayerCrouchedEnded;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        Instance = this;
        moveSpeed = initialMoveSpeed;
    }

    private void Start() {
        GameInput.Instance.OnPlayerRunStarted += GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled += GameInput_OnPlayerRunCanceled;
        GameInput.Instance.OnPlayerJumpCanceled += GameInput_OnPlayerJumpCanceled;
        GameInput.Instance.OnPlayerJumpStarted += GameInput_OnPlayerJumpStarted;
    }

    private void FixedUpdate() {
        HandleMovementForces();
    }

    private void Update() {
        HandleCrouch();
        HandleMovingBackwards();
        HandleRunningAndExhaustion();

        // LAST MOVE DIR
        float lastMoveInput = GameInput.Instance.GetMovementFloatNormalized();
        if (lastMoveInput != 0) {
            lastMoveDir = lastMoveInput;
        }

        // GRAVITY FALL
        if (!isJumpTop && rb.velocity.y < 1 && rb.velocity.y > 0) {
            // Y velocity is low : reach top of jump
            OnPlayerJumpTop?.Invoke(this, EventArgs.Empty);
            isJumpTop = true;
            //Debug.Log("isJumpTop");
        }


        if(!isJumpDown && rb.velocity.y < 0) {
            OnPlayerJumpDown?.Invoke(this, EventArgs.Empty);
            isJumpDown = true;
            //Debug.Log("isJumpDown");
        }

        if(!isLanded && rb.velocity.y < 2 && IsGrounded()) {
            OnPlayerLanded?.Invoke(this, EventArgs.Empty);
            isLanded = true;
            isJumping = false;
            //Debug.Log("isLanded");
        }

        if (rb.velocity.y < -0.1) {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else {
            rb.gravityScale = gravityScale;
        }
    }

    private void GameInput_OnPlayerJumpStarted(object sender, System.EventArgs e) {
        if (isJumping) return;

        if(GetPlatformStanding() != null) {
            if (GameInput.Instance.GetJumpDirNormalized() <= -.5) {
                PlatformJumpDown();
            } else {
                StartJumping();
            }
        } else {
            StartJumping();
        }
    }

    private void GameInput_OnPlayerJumpCanceled(object sender, System.EventArgs e) {
        if(rb.velocity.y > 0 && isJumping) {
            // Reduce current y velocity by amount
            rb.AddForce(Vector2.down * rb.velocity * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        lastJumpTime = 0;
        jumpInputReleased = true;
    }

    private void GameInput_OnPlayerRunCanceled(object sender, System.EventArgs e) {
        if (!isRunning) return;

        StopRunning();
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        if (isExhausted) return;

        StartRunning();
    }

    private void HandleMovingBackwards() {

        if((lastMoveDir > 0 && PlayerAim.Instance.GetAimDir().x < 0) || (lastMoveDir < 0 && PlayerAim.Instance.GetAimDir().x > 0)) {

            isMovingBackwards = true;

        } else {

            isMovingBackwards = false;

        }
    }

    private void HandleCrouch() {
        if (isJumping) return;
        if (GameInput.Instance.GetJumpDirNormalized() <= -.5) {
            if(!isCrouching) {
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
        jumpInputReleased = false;

        OnPlayerCrouchedEnded?.Invoke(this, EventArgs.Empty);
        OnPlayerJumpUp?.Invoke(this, EventArgs.Empty);
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

        if (isMovingBackwards) {
            targetSpeed *= moveSpeedBackwardsMultiplier;
        }

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    private void HandleRunningAndExhaustion() {

        if(isExhausted) {
            exhaustionTimer += Time.deltaTime;

            if(exhaustionTimer >= exhaustionTime) {
                StopExhausted();
            }
        }

        if(isRunning && (moveSpeed != 0)) {
            runTimer += Time.deltaTime;

            if(runTimer > runMaxTime) {
                StartExhausted();
                StopRunning();
            }

        } else {
            // Recover only when not exhausted anymore
            if(!isExhausted && runTimer > 0) {
                runTimer -= Time.deltaTime * runRecoverFactor;
            }
        }
    }

    private void StartRunning() {
        isRunning = true;
        BuffMoveSpeed(runAccelerationFactor);
        OnPlayerRunStarted?.Invoke(this, EventArgs.Empty);
    }

    private void StopRunning() {
        isRunning = false;
        DebuffMoveSpeed(runAccelerationFactor);
        OnPlayerRunStopped?.Invoke(this, EventArgs.Empty);
    }

    private void StartExhausted() {
        isExhausted = true;
        DebuffMoveSpeed(exhaustedSpeedFactor);
        OnPlayerExhaustionStarted?.Invoke(this, EventArgs.Empty);
    }

    private void StopExhausted() {
        isExhausted = false;
        exhaustionTimer = 0;
        BuffMoveSpeed(exhaustedSpeedFactor);
        OnPlayerExhaustionStopped?.Invoke(this, EventArgs.Empty);
    }

    public void BuffMoveSpeed(float buffAmount) {
        moveSpeed *= buffAmount;
        Debug.Log("buffMoveSpeed " + buffAmount + " new move speed " + moveSpeed);
    }

    public void DebuffMoveSpeed(float buffAmount) {
        moveSpeed /= buffAmount;
        Debug.Log("DebuffMoveSpeed " + buffAmount + " new move speed " + moveSpeed);
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

    public float GetLastMoveDir() {
        return lastMoveDir;
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

    public float GetMoveSpeed() {
        return rb.velocity.x;
    }

    public float GetMoveSpeedNormalized() {
        Debug.Log(moveSpeed / initialMoveSpeed);
        return moveSpeed / initialMoveSpeed;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position-transform.up*castDistance, boxSize);
    }
}
