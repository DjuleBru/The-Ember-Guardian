using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallOrb_UI : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool formingOrb;
    private float initialGravityScale;
    private Vector3 destination;
    private float smoothTime = 1f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
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
            rb.gravityScale = 0;
        } else {
            rb.gravityScale = initialGravityScale;
        }

        this.formingOrb = formingOrb;
    }

    public void SetDestination(Vector3 destination, float smoothTime) {
        this.destination = destination;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroySmallOrb")) {
            Destroy(gameObject);
        }
    }
}
