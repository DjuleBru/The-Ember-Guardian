using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallOrb_UI : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D smallOrbCollider2D;
    private bool formingOrb;
    private float initialGravityScale;
    private Vector3 destination;
    private float smoothTime = 1f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        smallOrbCollider2D = GetComponent<Collider2D>();
        initialGravityScale = rb.gravityScale;
    }

    private void Update() {
        if (formingOrb) {
            // Lerp vers la destination pour un mouvement lissé
            transform.position = Vector3.Lerp(transform.position, destination, smoothTime * Time.deltaTime);
        }
    }

    public void SetFormingBigOrb(bool formingOrb) {
        if(formingOrb) {
            smallOrbCollider2D.enabled = false;
            rb.gravityScale = 0;
        } else {
            smallOrbCollider2D.enabled = true;
            rb.gravityScale = initialGravityScale;
        }

        this.formingOrb = formingOrb;
    }

    public void SetDestination(Vector3 destination, float smoothTime) {
        this.destination = destination;
        this.smoothTime = smoothTime;
    }

    public bool GetFormingOrb() {
        return formingOrb;
    }
}
