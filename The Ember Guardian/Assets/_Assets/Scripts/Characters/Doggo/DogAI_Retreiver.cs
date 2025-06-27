using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DogAI_Retreiver : DogAI {

    [SerializeField] private Transform dropSpawnPoint;
    [SerializeField] private DogCurrenciesDetectionCollider currenciesDetectionCollider;

    private bool pickUpItemsUnlocked;
    private bool playerIsClose;
    private bool droppingCurrencies;
    private float playerIsCloseTimer;
    private float timeToStayClose = .7f;
    private float dropTimer;
    private float dropDelay = 0.125f;
    private Dictionary<PlayerCurrencies.CurrencyType, int> collectedCurrencies = new Dictionary<PlayerCurrencies.CurrencyType, int>();
    public static event EventHandler OnAnyOrbDroppedByDog;
    public event EventHandler OnDogCollectedCurrency;
    public event EventHandler OnDogDroppedCurrency;
    public event EventHandler OnDogDroppedAllCurrencies;

    protected override void Start() {
        base.Start();
        pickUpItemsUnlocked = DogStats.Instance.GetRetreiverPickUpItemsAbilityUnlocked();
    }

    protected override void Update() {

        if (pickUpItemsUnlocked) {

            if(state != State.pickingUpOrbs && currenciesDetectionCollider.GetClosestCollectibleToCollect() != null) {
                ChangeState(State.pickingUpOrbs);
                return;
            }

            if(state != State.droppingOrbs && CheckDropCurrenciesToPlayer()) {
                droppingCurrencies = true;
                ChangeState(State.droppingOrbs);
                return;
            }

            if (state == State.droppingOrbs) {
                DroppingOrbsUpdate();
                return;
            }

            if(state == State.pickingUpOrbs) {
                if(currenciesDetectionCollider.GetClosestCollectibleToCollect() == null) {
                    ChangeState(State.runWithPlayer);
                    return;
                }

                HeadToClosestOrb();
                return;
            }

            if (state == State.runToPlayer) {
                HeadToPlayer();
                return;
            }

        }

        base.Update();
    }

    protected override void ChangeState(State newState) {
        base.ChangeState(newState);
        if(newState == State.pickingUpOrbs || newState == State.runToPlayer) {
            dogMovement.SetMoveSpeed(runMoveSpeed);
            hasSetSpeed = true;
        }

    }

    protected void HeadToClosestOrb() {
        Collectible closestCollectible = currenciesDetectionCollider.GetClosestCollectibleToCollect();
        float destinationPositionX = closestCollectible.transform.position.x;

        Vector3 destinationPosition = new Vector3(destinationPositionX, 0, 0);
        dogMovement.SetMoveTarget(destinationPosition);

        if ((Mathf.Abs(transform.position.x - destinationPosition.x)) < .1f) {
            CollectCurrency(closestCollectible.GetCurrencyType());
            closestCollectible.RetreiverCollectThis();
            ChangeState(State.runToPlayer);
            playerStickedAroundTimer = 0;
        }
    }
    protected void HeadToPlayer() {
        float destinationPositionX = Player.Instance.transform.position.x;

        Vector3 destinationPosition = new Vector3(destinationPositionX, 0, 0);
        dogMovement.SetMoveTarget(destinationPosition);

        if ((Mathf.Abs(transform.position.x - destinationPosition.x)) < .1f) {
            ChangeState(currentBehaviorIdleState);
            playerStickedAroundTimer = 0;
        }
    }

    private bool CheckDropCurrenciesToPlayer() {
        CheckPlayerIsClose();
        if (playerIsClose && GetTotalCurrencyAmount() > 0) {
            return true;
        }
        return false;
    }

    public void DroppingOrbsUpdate() {
        HandleDroppingCurrencies();

        if (GetTotalCurrencyAmount() == 0) {
            ReturnToPreviousState();
            return;
        }

        if (PlayerIsCloseAndStayedAround()) {
            DropCurrencies();
            return;
        }

        if (!playerIsClose) {
            ReturnToPreviousState();
            return;
        }
    }

    public void DropCurrencies() {
        if (droppingCurrencies) return;

        droppingCurrencies = true; // On commence le processus de drop
        dropTimer = 0f; // Réinitialiser le timer
    }

    public void ReturnToPreviousState() {
        ChangeState(previousState);
    }

    public int GetTotalCurrencyAmount() {
        int totalAmount = 0;
        foreach (var amount in collectedCurrencies.Values) {
            totalAmount += amount;
        }
        return totalAmount;
    }

    private void HandleDroppingCurrencies() {
        // Si on est en train de dropper des currencies
        if (droppingCurrencies) {
            dropTimer += Time.deltaTime; // Ajouter le temps écoulé

            // Si le délai entre deux drops est écoulé
            if (dropTimer >= dropDelay) {
                dropTimer = 0f; // Réinitialiser le timer

                // Essayer de dropper toutes les currencies collectées
                foreach (var currency in collectedCurrencies.ToList()) {
                    if (currency.Value > 0) {
                        Transform prefabToDrop = CurrenciesManager.Instance.GetCurrencyPrefab(currency.Key);
                        Collectible droppedCurrency = Instantiate(prefabToDrop, dropSpawnPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
                        droppedCurrency.ApplyRandomFrontForce(2f, 3f);
                        droppedCurrency.SetCollectibleUnInteractable(1f);
                        OnAnyOrbDroppedByDog?.Invoke(this, EventArgs.Empty);

                        // Réduire la valeur après chaque drop
                        collectedCurrencies[currency.Key]--;
                        OnDogDroppedCurrency?.Invoke(this, EventArgs.Empty);

                        // Si la valeur atteint 0, retirer l'élément du dictionnaire
                        if (collectedCurrencies[currency.Key] <= 0) {
                            collectedCurrencies.Remove(currency.Key);
                        }
                        break; // Quitter la boucle dès qu'on a fait un drop
                    }
                }

                // Si plus rien à dropper, arrêter le processus
                if (collectedCurrencies.Count == 0) {
                    droppingCurrencies = false;
                    OnDogDroppedAllCurrencies?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public void CheckPlayerIsClose() {
        float distance = 2f;

        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) < distance) {
            playerIsClose = true;
        }
        else {
            playerIsCloseTimer = 0;
            playerIsClose = false;
        }
    }

    public bool PlayerIsCloseAndStayedAround() {
        float distance = 0.5f;

        playerIsCloseTimer += Time.deltaTime;

        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) < distance && playerIsCloseTimer > timeToStayClose) {
            return true;
        }
        else {
            return false;
        }
    }

    public void CollectCurrency(PlayerCurrencies.CurrencyType currencyType) {
        if (!collectedCurrencies.ContainsKey(currencyType)) {
            collectedCurrencies[currencyType] = 0;
        }
        collectedCurrencies[currencyType]++;
        OnDogCollectedCurrency?.Invoke(this, EventArgs.Empty);
    }

    public bool GetPickUpItemsUnlocked() {
        return pickUpItemsUnlocked;
    }

}
