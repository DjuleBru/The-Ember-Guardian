using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public enum ChestType {
        orbChest,
        gemChest,
        ammoChest,
        initialChest,
        hugeChest,
        weaponChest,
    }

    [SerializeField] protected ChestType chestType;
    [SerializeField] protected Transform orbSpawnPosition;
    [SerializeField] protected List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList;
    [SerializeField] protected List<int> rewardAmountList;
    [SerializeField] protected bool chestDisappearsAutomaticallyAfterOpened;

    protected float delayToChestUnlockAnimation;
    protected float delayToSpawnCollectibles;

    protected bool chestLocked = false;
    protected bool chestOpened;
    protected bool chestOpenedAnimationOver;
    protected bool playerInTriggerArea;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnChestUnlocked;
    public event EventHandler OnChestOpened;
    public event EventHandler OnChestOpenedAnimationOver;
    public event EventHandler OnChestDisappear;


    public static event EventHandler<OnAnyChestSpawnedCollectibleEventArgs> OnAnyChestSpawnedCollectible;
    public class OnAnyChestSpawnedCollectibleEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    protected void Awake() {
        if(chestType == ChestType.orbChest) {
            delayToChestUnlockAnimation = 2.5f;
            delayToSpawnCollectibles = 3.4f;
        }
        if (chestType == ChestType.gemChest) {
            delayToChestUnlockAnimation = 2.1f;
            delayToSpawnCollectibles = 2.8f;
        }
        if (chestType == ChestType.ammoChest) {
            delayToChestUnlockAnimation = 3.8f;
            delayToSpawnCollectibles = 4.8f;
        }
        if (chestType == ChestType.initialChest) {
            delayToChestUnlockAnimation = 1.9f;
            delayToSpawnCollectibles = 2.6f;
        }
        if (chestType == ChestType.hugeChest) {
            delayToChestUnlockAnimation = 2.2f;
            delayToSpawnCollectibles = 3.2f;
        }
        if (chestType == ChestType.weaponChest) {
            delayToChestUnlockAnimation = 3.8f;
            delayToSpawnCollectibles = 4.8f;
        }
    }

    protected void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
    }


    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (chestOpened) return;

        chestOpened = true;
        StartCoroutine(OpenChest(true));
        OnChestOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (chestDisappearsAutomaticallyAfterOpened && chestOpened) return;
        if (chestLocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        playerInTriggerArea = true;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (chestDisappearsAutomaticallyAfterOpened && chestOpened) return;
        if (chestLocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        playerInTriggerArea = false;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
    }

    protected IEnumerator OpenChest(bool spawnCollectibles) {
        yield return new WaitForEndOfFrame();

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        yield return new WaitForSeconds(delayToChestUnlockAnimation);

        OnChestUnlocked?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToSpawnCollectibles - delayToChestUnlockAnimation);

        if(spawnCollectibles) {
            int j = 0;

            foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
                int rewardAmount = rewardAmountList[j];

                for (int i = 0; i < rewardAmount; i++) {
                    Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

                    OnAnyChestSpawnedCollectible?.Invoke(this, new OnAnyChestSpawnedCollectibleEventArgs {
                        currencyType = currencyType,
                    });

                    collectible.ApplyRandomUpwardsForce(5, 8);
                    collectible.SetCollectibleUnInteractable(.75f);
                    yield return new WaitForSeconds(.2f);
                }
                j++;
            }
        }

        chestOpenedAnimationOver = true;
        OnChestOpenedAnimationOver?.Invoke(this, EventArgs.Empty);

        if (!chestDisappearsAutomaticallyAfterOpened) yield break;

        yield return new WaitForSeconds(2f);

        StartCoroutine(MakeChestDisappear(3f));
    }

    protected IEnumerator MakeChestDisappear(float delayToDisappear) {

        OnChestDisappear?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToDisappear);
        Destroy(gameObject);
    }

    public ChestType GetChestType() {
        return chestType;
    }

    public void InvokeOnChestOpened() {
        OnChestOpened?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnAnyChestSpawnedCollectibles(PlayerCurrencies.CurrencyType currencyType) {
        OnAnyChestSpawnedCollectible?.Invoke(this, new OnAnyChestSpawnedCollectibleEventArgs {
            currencyType = currencyType,
        });
    }

    public bool GetChestDisappearsAutomatically() {
        return chestDisappearsAutomaticallyAfterOpened;
    } 

    public void SetChestLocked(bool locked) {
        chestLocked = locked;
    }
}
