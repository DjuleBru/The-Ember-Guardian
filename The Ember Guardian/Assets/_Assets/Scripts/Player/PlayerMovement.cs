using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runAccelerationFactor = 1.3f;

    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velPower;

    private Rigidbody2D rb;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerRunStarted += GameInput_OnPlayerRunStarted;
        GameInput.Instance.OnPlayerRunCanceled += GameInput_OnPlayerRunCanceled;
    }

    private void GameInput_OnPlayerRunCanceled(object sender, System.EventArgs e) {
        moveSpeed /= runAccelerationFactor;
    }

    private void GameInput_OnPlayerRunStarted(object sender, System.EventArgs e) {
        moveSpeed *= runAccelerationFactor;
    }

    private void FixedUpdate() {
        HandleMovementForces();
    }

    private void Update() {
        //HandleMovement();
    }

    private void HandleMovementForces() {
        float targetSpeed = GameInput.Instance.GetMovementFloatNormalized() * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }
}
