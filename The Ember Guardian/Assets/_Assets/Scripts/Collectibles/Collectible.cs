using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private PlayerCurrencies.CurrencyType currencyType;
    [SerializeField] private int currencyAmount;

    public event EventHandler OnCollectibleDestroyed;

    private bool interactable;
    private bool canBePickedUpByWorker;
    private bool droppedByPlayer;
    private bool playerInTriggerArea;
    private bool aggroedByWildWorker;

    private Collider2D triggerCollider;
    private Rigidbody2D rb;

    private void Awake() {
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
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

            if(worker.GetJob() == Worker.JobTypes.wild) {
                worker.RecruitWorker();
                Destroy(gameObject);
                return;
            }

            if(canBePickedUpByWorker) {
                worker.GetComponent<Worker>().CollectOrb();
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null) {
            playerInTriggerArea = false;

            if (!interactable) {
                interactable = true;
            }
            return;
        };

    }

    public void PlayerCollectThis() {
        PlayerCurrencies.Instance.ChangeCurrencyAmount(currencyType, currencyAmount);
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

    public PlayerCurrencies.CurrencyType GetCurrencyType() {
        return currencyType;
    }

    public void SetAggroedByWildWorker() {
        aggroedByWildWorker = true;
    }

    public bool GetAggroedByWildWorker() {
        return aggroedByWildWorker;
    }

    private void OnDestroy() {
        OnCollectibleDestroyed?.Invoke(this, EventArgs.Empty);
    }

}
