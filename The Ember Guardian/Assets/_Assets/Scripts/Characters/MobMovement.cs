using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    [SerializeField] protected float initialMobSpeed;

    [SerializeField] protected float acceleration;
    [SerializeField] protected float deceleration;
    [SerializeField] protected float velPower;

    protected Rigidbody2D rb;
    protected Vector3 targetDestination;
    protected float moveDirFloat;

    protected bool destinationReached;
    public event EventHandler OnDestinationReached;
    public event EventHandler OnDestinationSet;

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();

        targetDestination = transform.position;
    }

    protected void FixedUpdate() {
        if (Mathf.Abs(targetDestination.x - transform.position.x) < .1f) {
            moveDirFloat = 0;

            if (!destinationReached) {
                OnDestinationReached?.Invoke(this, EventArgs.Empty);
                rb.velocity = Vector3.zero;
                destinationReached = true;
            }
        }
        else {
            HandleMovementForces();
        }
    }

    protected void HandleMovementForces() {

        Vector3 moveDirection = targetDestination - transform.position;

        if(moveDirection.x <0) {
            moveDirFloat = -1;
        } else {
            moveDirFloat = 1;
        }

        float targetSpeed = moveDirFloat * initialMobSpeed * rb.mass;
        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    public void HeadToDestination(Vector3 targetDestination) {

        // Check if destination reached;
        if (Mathf.Abs(targetDestination.x - transform.position.x) < .1f) {
            moveDirFloat = 0;

            if (!destinationReached) {
                Debug.Log("destinationReached");
                OnDestinationReached?.Invoke(this, EventArgs.Empty);
                rb.velocity = Vector3.zero;
                destinationReached = true;
            }

            return;
        }

        Vector3 moveDirection = targetDestination - transform.position;

        if (moveDirection.x < 0) {
            moveDirFloat = -1;
        }
        else {
            moveDirFloat = 1;
        }

        float targetSpeed = moveDirFloat * initialMobSpeed;
        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    public void SetMoveSpeed(float moveSpeed) {
        initialMobSpeed = moveSpeed;
    }

    public void SetMoveTarget(Vector3 moveTarget) {
        
        OnDestinationSet?.Invoke(this, EventArgs.Empty);
        this.targetDestination = moveTarget;
        destinationReached = false;
    }

    public Vector3 GetTargetDestination() {
        return targetDestination;
    }

    public float GetMoveDirFloat() {
        return moveDirFloat;
    }

    protected void OnDisable() {
        rb.velocity = Vector2.zero;
    }

}
