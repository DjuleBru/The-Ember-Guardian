using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private MMF_Player dropCurrencyFeedback;

    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;
    [SerializeField] private PlayerCurrencies.CurrencyCategory currencyCategory;
    [SerializeField] private Animator purifyAnimator;
    [SerializeField] private Sprite purifiedSprite;
    [SerializeField] private Image gemImage;

    private bool movingOrb;
    private bool currencyJustLoaded;
    private float currencyLoadedTimer;
    private float currencyLoadedTime = 2f;
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
    private bool fellFromBag;
    private bool isBeingDestroyed = false;

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

        if(currencyJustLoaded) {
            currencyLoadedTime += Time.deltaTime;
            if(currencyLoadedTime > currencyLoadedTimer) {
                currencyJustLoaded = false;
            }
        }

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

        if (isBeingDestroyed) return;

        dropCurrencyFeedback.PlayFeedbacks();
        StartCoroutine(DestroyAfterDelay(.2f));
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroyCurrency")) {

            if(currencyType == PlayerCurrencies.CurrencyType.ammo || currencyType == PlayerCurrencies.CurrencyType.ammo_special) {
                ammoTriggerAmount++;
                if (ammoTriggerAmount != 3) return;
            }

            UICurrencyManager.PlayerInventoryUI.CurrencyFellFromBag(this);
            dropCurrencyFeedback.PlayFeedbacks();
            StartCoroutine(DestroyAfterDelay(.2f));
        }


        if (fellFromBag) return;
        Currency_UI currencyUIHit = collision.gameObject.GetComponentInParent<Currency_UI>();
        if (currencyUIHit != null) {
            if (currencyJustLoaded) return;
            currencyUIHit.SetCurrencyRbMovable(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Currency_UI currencyUIHit = collision.gameObject.GetComponentInParent<Currency_UI>();
        if (currencyUIHit != null) {
            if (currencyUIHit.GetFellFromBag()) return;
            SetCurrencyRbMovable(true);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay) {
        isBeingDestroyed = true;

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void DestroyCurrency() {
        Destroy(gameObject);
    }

    public bool GetMoving() {
        return movingOrb;
    }

    public void SetCurrencyRbMovable(bool movable) {
        if (currencyType == PlayerCurrencies.CurrencyType.ember) return;

        if(movable) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            initialTimerOver = false;
            initialTimer = 1.5f;
        }

    }

    public void SetCurrencyLoaded() {
        currencyJustLoaded = true;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void PurifyGem() {
        purifyAnimator.SetTrigger("Purify");
        gemImage.sprite = purifiedSprite;
    }
    public void SetPurifiedGem() {
        gemImage.sprite = purifiedSprite;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }
    public PlayerCurrencies.CurrencyCategory GetCurrencyCategory() {
        return currencyCategory;
    }
    public void SetFellFromBag() {
        fellFromBag = true;
    }
    public bool GetFellFromBag() {
        return fellFromBag;
    }

    public void SetBackpackBottomPosition(Transform bottom) {
        bottomPositionY = bottom.position.y;
        topPositionY = transform.position.y;
    } 
}
