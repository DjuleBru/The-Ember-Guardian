using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;
    [SerializeField] private int currencyAmount;

    public event EventHandler OnCollectibleDestroyed;

    public static event EventHandler OnAnyCollectibleTouchedFloor;
    public static event EventHandler OnAnyCollectiblePickedUpByPlayer;
    public static event EventHandler OnAnyCollectiblePickedUpByWorker;

    private bool interactable;
    private bool canBePickedUpByWorker;
    private bool droppedByPlayer;
    private bool playerInTriggerArea;
    private bool aggroedByWildWorker;
    private bool collected;

    private Worker aggroedWildWorker;
    private Collider2D triggerCollider;
    private Rigidbody2D rb;

    private void Awake() {
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        // Set Can be picked up by worker after 3 seconds 
        Invoke("SetCanBePickedUpByWorker", 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null) {
            playerInTriggerArea = true;
            if (!interactable) return;

            PlayerCollectThis();
            return;
        };

        Worker worker = collision.gameObject.GetComponent<Worker>();

        if (worker != null) {

            if (aggroedByWildWorker && worker != aggroedWildWorker) return;

            if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.wild && !collected) {
                collected = true;
                worker.RecruitWorker();
                Destroy(gameObject);

                OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (canBePickedUpByWorker && !collected) {
                collected = true;
                worker.GetComponent<Worker>().CollectOrb();
                Destroy(gameObject);

                OnAnyCollectiblePickedUpByWorker?.Invoke(this, EventArgs.Empty);
                return;
            }

        }


        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            OnAnyCollectibleTouchedFloor?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null) {
            playerInTriggerArea = false;

            return;
        };

    }

    public void PlayerCollectThis() {
        OnAnyCollectiblePickedUpByPlayer?.Invoke(this, EventArgs.Empty);
        //PlayerCurrencies.Instance.ChangeCurrencyAmount(currencyType, currencyAmount);
        UIOrbManager.Instance.AddBlueOrb();
        Destroy(gameObject);
    }

    public void SetCollectibleUnInteractable(float delay) {
        interactable = false;
        StartCoroutine(SetCollectibleInteractableAfterDelay(delay));
    }

    private IEnumerator SetCollectibleInteractableAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        interactable = true;

        if(playerInTriggerArea) {
            PlayerCollectThis();
        }
    }

    public void SetDroppedByPlayer() {
        droppedByPlayer = true;
    }

    public bool GetDroppedByPlayer() {
        return droppedByPlayer;
    }

    public void SetCanBePickedUpByWorker() {
        canBePickedUpByWorker = true;
    }

    public bool GetCanBePickedUpByWorker() {
        return canBePickedUpByWorker;
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
