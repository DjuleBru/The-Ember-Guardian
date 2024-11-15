using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigOrb_UI : MonoBehaviour
{
    [SerializeField] private Transform smallOrbPrefab;
    [SerializeField] private MMF_Player dropOrbsFeedback;

    private bool movingOrb;
    private float initialGravityScale;
    private Vector3 destination;
    private float smoothTime;
    private Rigidbody2D rb;
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
    }

    private void Update() {
        if (movingOrb) {
            // Lerp vers la destination pour un mouvement lissé
            transform.position = Vector3.Lerp(transform.position, destination, smoothTime * Time.deltaTime);
        }
    }

    public void SetMovingBigOrb(bool formingOrb) {
        Debug.Log("SetMovingBigOrb");
        if (formingOrb) {
            rb.gravityScale = 0;
        }
        else {
            rb.gravityScale = initialGravityScale;
        }

        this.movingOrb = formingOrb;
    }

    public void SetDestination(Vector3 destination, float smoothTime) {
        this.destination = destination;
        this.smoothTime = smoothTime;
    }

    public void DropOrb() {
        dropOrbsFeedback.PlayFeedbacks();
        StartCoroutine(DestroyAfterDelay(.2f));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroyBigOrb")) {

            UIOrbManager.Instance.CrackleBigOrb(transform.position);

            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

}
