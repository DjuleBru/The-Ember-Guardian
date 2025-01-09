using QFSW.QC.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement_Flying : CreatureMovement {

    [SerializeField] private float noiseAmplitude = 0.5f; // Amplitude du bruit
    [SerializeField] private float noiseFrequency = 1f;   // Fréquence du bruit
    private Vector3 noiseOffset;


    protected override void FixedUpdate() {
        Debug.DrawLine(transform.position, targetDestination, Color.yellow);

        if (Vector3.Distance(targetDestination, transform.position) < .3f) {
            moveDirFloat = 0;

            if (!destinationReached) {
                InvokeOnDestinationReached();
                rb.velocity = Vector3.zero;
                destinationReached = true;
            }
        }
        else {
            if (!canMove) return;
            HandleMovementForces();
        }
    }

    protected override void HandleMovementForces() {  
        Vector2 moveDirection = (targetDestination - transform.position).normalized;

        // Calcul de la vitesse cible en fonction de la direction
        targetSpeed = moveSpeed;
        float speedDifX = targetSpeed * moveDirection.x - rb.velocity.x; // Différence de vitesse sur X
        float speedDifY = targetSpeed * moveDirection.y - rb.velocity.y; // Différence de vitesse sur Y
        
        // Calculer le taux d'accélération
        float accelRateX = (Mathf.Abs(speedDifX) > 0.01f) ? acceleration : deceleration;
        float accelRateY = (Mathf.Abs(speedDifY) > 0.01f) ? acceleration : deceleration;

        // Calcul des forces nécessaires sur X et Y
        float forceX = Mathf.Pow(Mathf.Abs(speedDifX) * accelRateX, velPower) * Mathf.Sign(speedDifX);
        float forceY = Mathf.Pow(Mathf.Abs(speedDifY) * accelRateY, velPower) * Mathf.Sign(speedDifY);

        if (moveDirection.x < 0) {
            moveDirFloat = -1;
            lastMoveDir = -1;
        }
        else {
            moveDirFloat = 1;
            lastMoveDir = 1;
        }

        // Ajouter la force au Rigidbody2D
        Vector2 forceToAdd = new Vector2(forceX, forceY);
        rb.AddForce(forceToAdd * rb.mass);
    }

}
