using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList;
    [SerializeField] private List<int> rewardAmountList;

    [SerializeField] private float delayToChestUnlockAnimation;
    [SerializeField] private float delayToSpawnCollectibles;

    private bool chestOpened;
    public event EventHandler OnChestUnlocked;
    public event EventHandler OnChestOpened;
    public event EventHandler OnChestDisappear;
    public static event EventHandler<OnAnyChestSpawnedCollectibleEventArgs> OnAnyChestSpawnedCollectible;
    public class OnAnyChestSpawnedCollectibleEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (chestOpened) return;
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
                    currencyType = currencyType
                });

                yield return new WaitForSeconds(.2f);
                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(1f);
            }
            j++;
        }

        yield return new WaitForSeconds(2f);

        OnChestDisappear?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);

    }
}
