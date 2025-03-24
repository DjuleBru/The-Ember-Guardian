using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private MMF_Player dropCurrencyFeedback;

    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;

    private bool movingOrb;
    private Transform destinationTransform;
    private float smoothTime;
    private Rigidbody2D rb;
    private Collider2D currencyCOllider2D;
    private float bottomPositionY;
    private float topPositionY;

    private float initialTimer = 1.5f;
    private float maxSpeed = 5f;
    private float minMass = 5f;
    private float maxMass = 5000f;
    private float speedToDisableRb = 1f;
    private bool initialTimerOver;

    private int ammoTriggerAmount;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        currencyCOllider2D = GetComponent<Collider2D>();
    }

    private void Update() {
        if (!initialTimerOver) {
            initialTimer -= Time.deltaTime;
            if (initialTimer < 0) {
                initialTimerOver = true;
            }
        }

        //float positionYNormalized = (topPositionY - transform.position.y) / (topPositionY - bottomPositionY);
        //float mass = Mathf.Lerp(minMass, maxMass, positionYNormalized);
        //rb.mass = mass;

        if (movingOrb) {
            // Lerp vers la destination pour un mouvement lissé
            transform.position = Vector3.Lerp(transform.position, destinationTransform.position, smoothTime * Time.deltaTime);
        }

        if (rb.velocity.magnitude < speedToDisableRb && initialTimerOver) {
            rb.bodyType = RigidbodyType2D.Static;
            //rb.Sleep();
        }
    }

    public void RemoveFromBag(UICurrencyManager currencyManagerSender) {
        currencyManagerSender.RemoveCurrencyUIFromInventoryList(this);
        dropCurrencyFeedback.PlayFeedbacks();
        StartCoroutine(DestroyAfterDelay(.2f));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroyCurrency")) {

            if(currencyType == PlayerCurrencies.CurrencyType.ammo) {
                ammoTriggerAmount++;
                if (ammoTriggerAmount != 3) return;
            }

            UICurrencyManager.PlayerInventoryUI.CurrencyFellFromBag(this);
            dropCurrencyFeedback.PlayFeedbacks();
            StartCoroutine(DestroyAfterDelay(.2f));
        }

        Currency_UI currencyUIHit = collision.gameObject.GetComponentInParent<Currency_UI>();
        if (currencyUIHit != null) {
            currencyUIHit.SetCurrencyRbMovable();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Currency_UI currencyUIHit = collision.gameObject.GetComponentInParent<Currency_UI>();
        if (currencyUIHit != null) {
            SetCurrencyRbMovable();
        }
    }
    private IEnumerator DestroyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void DestroyCurrency() {
        Destroy(gameObject);
    }

    public bool GetMoving() {
        return movingOrb;
    }

    public void SetCurrencyRbMovable() {
        if (currencyType == PlayerCurrencies.CurrencyType.ember) return;
        //rb.WakeUp();
        rb.bodyType = RigidbodyType2D.Dynamic;
        initialTimerOver = false;
        initialTimer = 1.5f;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }

    public void SetBackpackBottomPosition(Transform bottom) {
        bottomPositionY = bottom.position.y;
        topPositionY = transform.position.y;
    } 
}
