using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobodogMissile_RotateTowardsVelocity : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float rotationOffsetDeg = 90f; // selon l'orientation du sprite (0 si le sprite "regarde" vers la droite à l'origine)
    [SerializeField] private float minVelocityToRotate = 0.05f;

    private float currentAngle;

    private void Awake() {
        if (rb == null) {
            rb = GetComponentInParent<Rigidbody2D>();
        }
        currentAngle = transform.eulerAngles.z;
    }

    private void LateUpdate() {
        Vector2 velocity = rb.velocity;

        if (velocity.magnitude < minVelocityToRotate) {
            return;
        }

        float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + rotationOffsetDeg;
        currentAngle = targetAngle;

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}
