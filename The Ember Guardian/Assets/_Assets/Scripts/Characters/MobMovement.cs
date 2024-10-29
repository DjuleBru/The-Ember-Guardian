using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    [SerializeField] private float mobSpeed;

    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velPower;

    private Rigidbody2D rb;
    private Vector3 targetDestination;
    private float moveDirFloat;

    private bool destinationReached;
    public event EventHandler OnDestinationReached;
    public event EventHandler OnDestinationSet;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        targetDestination = transform.position;
    }

    private void FixedUpdate() {

        //if (Mathf.Abs(targetDestination.x - transform.position.x) < .1f) {
        //    moveDirFloat = 0;

        //    if(!destinationReached) {
        //        Debug.Log("destinationReached");
        //        OnDestinationReached?.Invoke(this, EventArgs.Empty);
        //        rb.velocity = Vector3.zero;
        //        destinationReached = true;  
        //    }
        //} else {
        //    //HandleMovementForces();
        //}

        Debug.DrawLine(transform.position, targetDestination);
    }

    private void HandleMovementForces() {

        Vector3 moveDirection = targetDestination - transform.position;

        if(moveDirection.x <0) {
            moveDirFloat = -1;
        } else {
            moveDirFloat = 1;
        }

        float targetSpeed = moveDirFloat * mobSpeed;
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

        float targetSpeed = moveDirFloat * mobSpeed;
        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
    }

    public void SetMoveSpeed(float moveSpeed) {
        mobSpeed = moveSpeed;
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

    private void OnDisable() {
        rb.velocity = Vector2.zero;
    }

}
