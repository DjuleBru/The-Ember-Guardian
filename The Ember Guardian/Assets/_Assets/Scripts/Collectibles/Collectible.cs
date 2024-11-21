using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;
    [SerializeField] private int currencyAmount;

    [SerializeField] private Collider2D solidCollider;

    public event EventHandler OnCollectibleDestroyed;
    public event EventHandler OnCollectibleEnteredSlot;

    public static event EventHandler OnAnyCollectibleTouchedFloor;
    public static event EventHandler OnAnyCollectiblePickedUpByPlayer;
    public static event EventHandler OnAnyCollectiblePickedUpByWorker;

    private float initialGravityScale;

    private bool interactable;
    private bool canNeverBePickedUpByWorker;
    private bool canBePickedUpByWorker;
    private bool droppedByPlayer;
    private bool playerInTriggerArea;
    private bool aggroedByWildWorker;
    private bool collected;
    private bool touchedFloor;

    private bool movingForPayment;
    private float smoothTime = 5f;
    private Transform paymentDestination;

    private Worker aggroedWildWorker;
    private Collider2D triggerCollider;
    private Rigidbody2D rb;

    private void Awake() {
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
    }

    private void Start() {
        if(!canNeverBePickedUpByWorker) {
            // Set Can be picked up by worker after 3 seconds 
            Invoke("SetCanBePickedUpByWorker", 3f);
        }

    }

    private void Update() {
        if (movingForPayment && paymentDestination != null) {
            // Lerp vers la position locale de la destination
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, smoothTime * Time.deltaTime);

            if (currencyType == PlayerCurrencies.CurrencyType.ammo && Vector3.Distance(transform.localPosition, Vector3.zero) < .1f) {
                PlayerShoot.Instance.AddAmmoClip(1);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!touchedFloor && collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            touchedFloor = true;
            OnAnyCollectibleTouchedFloor?.Invoke(this, EventArgs.Empty);
        }

        if (movingForPayment) {
            // Orb Collisions with OrbTemplateWorldUI
            PayCurrencyTemplateWorldUI orbTemplateWorldUI = collision.GetComponent<PayCurrencyTemplateWorldUI>();

            if (orbTemplateWorldUI != null && orbTemplateWorldUI.transform == paymentDestination && !orbTemplateWorldUI.GetCurrencyPaid()) {
                OnCollectibleEnteredSlot?.Invoke(this, EventArgs.Empty);
                orbTemplateWorldUI.SetCurrencyPaid(true);
            }

        } else {

            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                playerInTriggerArea = true;
                if (!interactable) return;

                PlayerCollectThis(currencyType);
                return;
            };

            Worker worker = collision.gameObject.GetComponent<Worker>();

            if (worker != null) {

                if (aggroedByWildWorker && worker != aggroedWildWorker) return;

                if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.wild && !collected && currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                    collected = true;
                    worker.RecruitWorker();
                    Destroy(gameObject);

                    OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                    return;
                }

                if (canBePickedUpByWorker && !collected) {
                    collected = true;
                    worker.GetComponent<Worker>().CollectCurrency(currencyType);
                    Destroy(gameObject);

                    OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                    return;
                }

            }
        }

    }

    private void OnTriggerExit2D(Collider2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null) {
            playerInTriggerArea = false;

            return;
        };

    }

    public void PlayerCollectThis(PlayerCurrencies.CurrencyType currencyType) {
        OnAnyCollectiblePickedUpByPlayer?.Invoke(this, EventArgs.Empty);
        UICurrencyManager.Instance.AddCurrencyInBag(currencyType);

        if(currencyType != PlayerCurrencies.CurrencyType.ember) {

            Destroy(gameObject);

        } else {
            transform.SetParent(PlayerCurrencies.Instance.GetEmberHoldPosition());
            transform.position = PlayerCurrencies.Instance.GetEmberHoldPosition().position;
            rb.bodyType = RigidbodyType2D.Kinematic;
            interactable = false;
            PlayerCurrencies.Instance.SetCarryingEmber(true);
        }
    }

    public void SetCollectibleUnInteractable(float delay) {
        interactable = false;
        StartCoroutine(SetCollectibleInteractableAfterDelay(delay));
    }

    public void SetCollectibleFellFromBag() {
        GetComponent<Collider2D>().enabled = false;
        solidCollider.enabled = false;
    }

    private IEnumerator SetCollectibleInteractableAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        interactable = true;

        if(playerInTriggerArea) {
            PlayerCollectThis(currencyType);
        }
    }

    private IEnumerator SetDroppedByPlayerAfterDelay(bool droppedByPlayer, float delay) {
        this.droppedByPlayer = droppedByPlayer;
        yield return new WaitForSeconds(delay);
        this.droppedByPlayer = !droppedByPlayer;
    }

    public void SetMovingForPayment(bool moving, Transform destination = null) {

        this.movingForPayment = moving;
        paymentDestination = destination;
        transform.SetParent(destination);

        if(moving) {
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        } else {
            rb.gravityScale = initialGravityScale;
            rb.angularVelocity = 0;
            rb.bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(SetCollectibleInteractableAfterDelay(1f));
            StartCoroutine(SetDroppedByPlayerAfterDelay(true, 1f));
        }
    }

    public void SetScale(float scale) {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetDroppedByPlayer() {
        droppedByPlayer = true;
    }

    public bool GetDroppedByPlayer() {
        return droppedByPlayer;
    }

    public bool GetMovingForPayment() {
        return movingForPayment;
    }

    public bool GetInteractable() {
        return interactable;
    }

    public void SetCanBePickedUpByWorker() {
        canBePickedUpByWorker = true;
    }

    public void SetCanNeverBePickedUpByWorker() {
        canNeverBePickedUpByWorker = true;
    }

    public void ApplyRandomUpwardsForce(float minForce, float maxForce) {
        float force = UnityEngine.Random.Range(minForce, maxForce);

        Vector2 forceDir = new Vector2(UnityEngine.Random.Range(-.5f, .5f)*force, UnityEngine.Random.Range(.5f, 1f) * force);

        rb.AddForce(forceDir, ForceMode2D.Impulse);
    }

    public void ApplyRandomSidewardsForce(float minForce, float maxForce) {
        float force = UnityEngine.Random.Range(minForce, maxForce);
        float xDir = UnityEngine.Random.Range(-1, 1);

        if(xDir < 0) {
            xDir = -1;
        } else {
            xDir = 1;
        }

        Vector2 forceDir = new Vector2(UnityEngine.Random.Range(.5f, 1f) * xDir * force, UnityEngine.Random.Range(.1f, .25f) * force);

        rb.AddForce(forceDir, ForceMode2D.Impulse);
    }

    public void ApplyRandomFrontForce(float minForce, float maxForce) {
        float force = UnityEngine.Random.Range(minForce, maxForce);
   
        Vector2 forceDir = new Vector2(UnityEngine.Random.Range(.5f, 1f) * force, UnityEngine.Random.Range(.1f, .25f) * force);

        rb.AddForce(forceDir, ForceMode2D.Impulse);
    }

    public void ApplyRandomForce(float minForceX, float maxForceX, float minForceY, float maxForceY) {
        float forceX = UnityEngine.Random.Range(minForceX, maxForceX);
        float forceY = UnityEngine.Random.Range(minForceY, maxForceY);

        Vector2 forceDir = new Vector2(forceX, forceY);

        rb.AddForce(forceDir, ForceMode2D.Impulse);
    }

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }

    public void SetAggroedByWildWorker(bool aggroed, Worker worker) {
        aggroedByWildWorker = aggroed;
        aggroedWildWorker = worker;
    }

    public bool GetAggroedByWildWorker() {
        return aggroedByWildWorker;
    }

    public void SetCollected() {
        collected = true;
    }

    public bool GetCollected() {
        Debug.Log("GetCollected " + collected);
        return collected;
    }


    private void OnDestroy() {
        OnCollectibleDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
