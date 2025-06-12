using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] protected PlayerCurrencies.CurrencyType currencyType;
    [SerializeField] protected PlayerCurrencies.CurrencyCategory currencyCategory;
    [SerializeField] protected int currencyAmount;

    [SerializeField] protected Collider2D solidCollider;

    public event EventHandler OnCollectibleDestroyed;
    public event EventHandler OnCollectibleEnteredSlot;
    public static event EventHandler OnAnyCollectibleEnteredSlot;
    public event EventHandler OnCollectibleFellFromBag;

    public static event EventHandler OnAnyCollectibleTouchedFloor;
    public static event EventHandler OnAnyCollectiblePickedUpByPlayer;
    public static event EventHandler OnAnyCollectiblePickedUpByWorker;
    public static event EventHandler<OnAnyCollectiblePouffedEventArgs> OnAnyCollectiblePlouffed;
    public event EventHandler OnCollectiblePlouffed;

    public class OnAnyCollectiblePouffedEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }
    public Action onDestinationReached;

    protected float initialGravityScale;

    protected bool interactable;
    protected bool canNeverBePickedUpByWorker;
    protected bool canBePickedUpByWorker;
    protected bool droppedByPlayer;
    protected bool droppedInFire;
    protected bool playerInTriggerArea;
    protected bool aggroedByWorker;
    protected bool collected;
    protected bool touchedFloor;
    protected bool enteredPayCurrencyUISlot;

    protected bool movingForReloading;
    protected bool movingForPayment;
    protected bool destroyOnDestinationReached;
    protected float smoothTime = 5f;
    protected Transform paymentDestination;

    protected Worker aggroedWildWorker;
    protected Collider2D triggerCollider;
    protected Rigidbody2D rb;

    protected virtual void Awake() {
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
    }

    protected void Start() {
        if(!canNeverBePickedUpByWorker) {
            // Set Can be picked up by worker after 3 seconds 
            Invoke("SetCanBePickedUpByWorker", 3f);
        }

    }

    protected void Update() {
        if (movingForPayment && paymentDestination != null) {
            // Lerp vers la position locale de la destination
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, smoothTime * Time.deltaTime);

            if (movingForReloading && Vector3.Distance(transform.localPosition, Vector3.zero) < .1f) {
                PlayerShoot.Instance.AddAmmoClip(1);
                Destroy(gameObject);
            }

            if(destroyOnDestinationReached && Vector3.Distance(transform.localPosition, Vector3.zero) < .1f) {
                onDestinationReached?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (!touchedFloor && collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            touchedFloor = true;
            OnAnyCollectibleTouchedFloor?.Invoke(this, EventArgs.Empty);
        }

        if (movingForPayment) {
            // Orb Collisions with OrbTemplateWorldUI
            PayCurrencyTemplateWorldUI orbTemplateWorldUI = collision.GetComponent<PayCurrencyTemplateWorldUI>();

            if (orbTemplateWorldUI != null && orbTemplateWorldUI.transform == paymentDestination && !enteredPayCurrencyUISlot) {
                enteredPayCurrencyUISlot = true;
                OnCollectibleEnteredSlot?.Invoke(this, EventArgs.Empty);
                OnAnyCollectibleEnteredSlot?.Invoke(this, EventArgs.Empty);
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
                if (currencyType == PlayerCurrencies.CurrencyType.ember) return;
                if (currencyCategory == PlayerCurrencies.CurrencyCategory.gem) return;
                if (aggroedByWorker && worker != aggroedWildWorker) return;

                WorkerAI.JobTypes workerJob = worker.GetComponent<WorkerAI>().GetJob();

                if (workerJob == WorkerAI.JobTypes.wild && !collected && currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb && droppedByPlayer) {
                    collected = true;
                    worker.RecruitWorker();
                    Destroy(gameObject);

                    OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                    return;
                }

                if (workerJob != WorkerAI.JobTypes.jobless && workerJob != WorkerAI.JobTypes.wild && canBePickedUpByWorker && !collected && currencyCategory != PlayerCurrencies.CurrencyCategory.trap) {
                    collected = true;
                    worker.GetComponent<Worker>().CollectCurrency(currencyType);
                    Destroy(gameObject);

                    OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                    return;
                }

            }
        }

    }

    protected void OnTriggerExit2D(Collider2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null) {
            playerInTriggerArea = false;

            return;
        };

    }

    public void PlayerCollectThis(PlayerCurrencies.CurrencyType currencyType) {
        OnAnyCollectiblePickedUpByPlayer?.Invoke(this, EventArgs.Empty);

        if(currencyType != PlayerCurrencies.CurrencyType.ember) {
            UICurrencyManager.PlayerInventoryUI.AddCurrencyInBag(currencyType);
            Destroy(gameObject);

        } else {
            PlayerCurrencies.Instance.SetCarryingEmber(true);
            Destroy(gameObject);
        }
    }

    public void SetCollectibleUnInteractable(float delay) {
        interactable = false;
        StartCoroutine(SetCollectibleInteractableAfterDelay(delay));
    }

    public void SetCollectibleFellFromBag() {
        float delayToPlouf = UnityEngine.Random.Range(.7f, .9f);
        StartCoroutine(CollectibleFallsInWater(delayToPlouf));
    }

    protected IEnumerator CollectibleFallsInWater(float delayToPlouf) {
        yield return new WaitForSeconds(.1f);

        OnCollectibleFellFromBag?.Invoke(this, EventArgs.Empty);
        GetComponent<Collider2D>().enabled = false;
        solidCollider.enabled = false;

        yield return new WaitForSeconds(delayToPlouf);
        OnCollectiblePlouffed?.Invoke(this, EventArgs.Empty);
        OnAnyCollectiblePlouffed?.Invoke(this, new OnAnyCollectiblePouffedEventArgs { currencyType = currencyType});

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    protected IEnumerator SetCollectibleInteractableAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        interactable = true;

        if(playerInTriggerArea) {
            PlayerCollectThis(currencyType);
        }
    }

    protected IEnumerator SetDroppedByPlayerAfterDelay(bool droppedByPlayer, float delay) {
        this.droppedByPlayer = droppedByPlayer;
        yield return new WaitForSeconds(delay);
        this.droppedByPlayer = !droppedByPlayer;
    }

    public void SetAsCarriedEmber() {
        rb = GetComponent<Rigidbody2D>();
        transform.SetParent(PlayerCurrencies.Instance.GetEmberHoldPosition());
        transform.position = PlayerCurrencies.Instance.GetEmberHoldPosition().position;
        rb.bodyType = RigidbodyType2D.Kinematic;
        interactable = false;
        solidCollider.enabled = false;
    }

    public virtual void SetMovingForPayment(bool moving, float smoothTime = 1f, Transform destination = null, bool movingForReloading = false) {

        this.movingForPayment = moving;
        this.movingForReloading = movingForReloading;
        this.smoothTime = smoothTime;
        paymentDestination = destination;
        transform.SetParent(destination);

        if(moving) {
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
        } else {
            solidCollider.enabled = true;
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

    public void SetDroppedInFire() {
        droppedInFire = true;
    }

    public bool GetDroppedByPlayer() {
        return droppedByPlayer;
    }
    public bool GetDroppedInFire() {
        return droppedInFire;
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

    public void SetDestroyOnDestinationReached(Action callback = null) {
        destroyOnDestinationReached = true;
        onDestinationReached = callback;
    }

    public void SetCanBePickedUpByWorkerAfterDelay(float delay) {
        StartCoroutine(SetCanBePickedUpByWorkerAfterDelayCoroutine(delay));
    }

    protected IEnumerator SetCanBePickedUpByWorkerAfterDelayCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
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
    public void ApplyRandomForceRandomDir(float minForceX, float maxForceX, bool randomDirX, float minForceY, float maxForceY, bool randomDirY) {
        float forceX = UnityEngine.Random.Range(minForceX, maxForceX);
        float forceY = UnityEngine.Random.Range(minForceY, maxForceY);

        if(randomDirX) {
            float randomDirXFloat = UnityEngine.Random.Range(-1f, 1f);
            if(randomDirXFloat < 0) {
                forceX *= -1;
            }
        }
        if (randomDirY) {
            float randomDirYFloat = UnityEngine.Random.Range(-1f, 1f);
            if (randomDirYFloat < 0) {
                forceY *= -1;
            }
        }

        Vector2 forceDir = new Vector2(forceX, forceY);
        rb.AddForce(forceDir, ForceMode2D.Impulse);
    }

    public void ApplyRandomTorque(float minForce, float maxForceX) {
        float torque = UnityEngine.Random.Range(minForce, maxForceX);

        rb.AddTorque(torque);
    }

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }

    public PlayerCurrencies.CurrencyCategory GetCurrencyCategory() {
        return currencyCategory;
    }

    public void SetAggroedByWorker(bool aggroed, Worker worker) {
        aggroedByWorker = aggroed;
        aggroedWildWorker = worker;
    }

    public bool GetAggroedByWildWorker() {
        return aggroedByWorker;
    }

    public void SetCollected() {
        collected = true;
    }

    public bool GetCollected() {
        Debug.Log("GetCollected " + collected);
        return collected;
    }


    protected void OnDestroy() {
        OnCollectibleDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
