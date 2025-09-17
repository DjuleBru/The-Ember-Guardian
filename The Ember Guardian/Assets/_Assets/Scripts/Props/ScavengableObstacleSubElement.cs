using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableObstacleSubElement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private bool applyTorque = true;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
    }

    public void Fall(float explosionForce, float torqueForce, float randomness) {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        Vector2 force = new Vector2(
            Random.Range(-randomness, randomness),
            Random.Range(0.5f * explosionForce, 1.5f * explosionForce)
        );


        float torque = Random.Range(-0.5f * torqueForce, 1.5f * torqueForce);

        rb.AddForce(force, ForceMode2D.Impulse);

        if(applyTorque) {
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }
    }

    public void TakeDamage() {
        animator.SetTrigger("Hit");
    }
}
