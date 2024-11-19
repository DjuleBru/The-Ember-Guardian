using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private Transform smallOrbPrefab;
    [SerializeField] private MMF_Player dropCurrencyFeedback;

    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;

    private bool movingOrb;
    private float initialGravityScale;
    private Transform destinationTransform;
    private float smoothTime;
    private Rigidbody2D rb;
    private Collider2D currencyCOllider2D;

    private float initialTimer;
    private float maxSpeed = 5f;
    private bool initialTimerOver;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        currencyCOllider2D = GetComponent<Collider2D>();
        initialGravityScale = rb.gravityScale;
    }

    private void Update() {

        if (movingOrb) {
            // Lerp vers la destination pour un mouvement lissé
            transform.position = Vector3.Lerp(transform.position, destinationTransform.position, smoothTime * Time.deltaTime);
        }
    }

    void FixedUpdate() {
        //if (rb.velocity.magnitude > maxSpeed) {
        //    Debug.Log("limitingSpeed " + rb.velocity.magnitude);
        //    rb.velocity = rb.velocity.normalized * maxSpeed;
        //}
    }

    public void SetMoving(bool movingOrb) {

        if (movingOrb) {
            rb.gravityScale = 0;
            currencyCOllider2D.enabled = false;
        }
        else {
            rb.gravityScale = initialGravityScale;
            currencyCOllider2D.enabled = true;
        }

        this.movingOrb = movingOrb;
    }

    public void SetDestination(Transform destinationTransform, float smoothTime) {
        this.destinationTransform = destinationTransform;
        this.smoothTime = smoothTime;
    }

    public void RemoveFromBag() {
        UICurrencyManager.Instance.RemoveCurrencyUIFromInventoryList(this);
        dropCurrencyFeedback.PlayFeedbacks();
        StartCoroutine(DestroyAfterDelay(.2f));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroyCurrency")) {
            UICurrencyManager.Instance.CurrencyFellFromBag(this);
            dropCurrencyFeedback.PlayFeedbacks();
            StartCoroutine(DestroyAfterDelay(.2f));
        }
    }

    private IEnumerator DestroyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public bool GetMoving() {
        return movingOrb;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }

}
