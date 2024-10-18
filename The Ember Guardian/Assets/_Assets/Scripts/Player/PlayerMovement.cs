using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour {

    public static PlayerMovement Instance;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runAccelerationFactor = 1.3f;

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

    private bool isCrouching;
    private bool isJumping;
    private bool isJumpTop;
    private bool isJumpDown;
    private bool isLanded;
    private bool jumpInputReleased;
    private float lastJumpTime;
    private Rigidbody2D rb;

    public event EventHandler OnPlayerJumpUp;
    public event EventHandler OnPlayerJumpTop;
    public event EventHandler OnPlayerJumpDown;
    public event EventHandler OnPlayerLanded;

    public event EventHandler OnPlayerCrouched;
    public event EventHandler OnPlayerCrouchedEnded;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        Instance = this;
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

        if (rb.velocity.y < 0) {
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

        //if(GameInput.Instance.GetJumpDirNormalized() >= -.5) {
        //    StartJumping();
        //} else {
        //    PlatformJumpDown();
        //}
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
        moveSpeed /= runAccelerationFactor;
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        moveSpeed *= runAccelerationFactor;
    }

    private void HandleCrouch() {
        if (isJumping) return;
        if (GameInput.Instance.GetJumpDirNormalized() <= -.5) {
            if(!isCrouching) {
                isCrouching = true;
                OnPlayerCrouched?.Invoke(this, EventArgs.Empty);
            }

        } else {

            if(isCrouching) {
                isCrouching = false;
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
        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    public bool IsGrounded() {
        if(Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayerMask)) {
            return true;
        } else {
            return false;
        }
    }

    public Platform GetPlatformStanding() {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayerMask);

        if (hit.collider != null) {
            Platform platformHit = hit.collider.GetComponent<Platform>();
            if (platformHit != null) {
                return platformHit; // Récupère la plateforme touchée
            }
        }

        return null; // Pas de plateforme touchée
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position-transform.up*castDistance, boxSize);
    }
}
