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
        hugeChest
    }

    [SerializeField] private ChestType chestType;
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList;
    [SerializeField] private List<int> rewardAmountList;

    private float delayToChestUnlockAnimation;
    private float delayToSpawnCollectibles;

    private bool chestLocked = false;
    private bool chestOpened;
    public event EventHandler OnChestUnlocked;
    public event EventHandler OnChestOpened;
    public event EventHandler OnChestDisappear;
    public static event EventHandler<OnAnyChestSpawnedCollectibleEventArgs> OnAnyChestSpawnedCollectible;
    public class OnAnyChestSpawnedCollectibleEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    private void Awake() {
        if(chestType == ChestType.orbChest) {
            delayToChestUnlockAnimation = 2.5f;
            delayToSpawnCollectibles = 3.4f;
        }
        if (chestType == ChestType.gemChest) {
            delayToChestUnlockAnimation = 2.1f;
            delayToSpawnCollectibles = 2.8f;
        }
        if (chestType == ChestType.ammoChest) {
            delayToChestUnlockAnimation = 2.3f;
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
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (chestOpened) return;
        if (chestLocked) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        chestOpened = true;
        StartCoroutine(OpenChest());
        OnChestOpened?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator OpenChest() {

        yield return new WaitForSeconds(delayToChestUnlockAnimation);

        OnChestUnlocked?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToSpawnCollectibles - delayToChestUnlockAnimation);

        int j = 0;

        foreach(PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
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

        yield return new WaitForSeconds(2f);

        OnChestDisappear?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);

    }

    public ChestType GetChestType() {
        return chestType;
    }

    public void SetChestLocked(bool locked) {
        chestLocked = locked;
    }
}
