using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeToReward;
    [SerializeField] private int rewardAmount;

    [SerializeField] private float delayToChestUnlockAnimation;
    [SerializeField] private float delayToSpawnCollectibles;

    private bool chestOpened;
    public event EventHandler OnChestUnlocked;
    public event EventHandler OnChestOpened;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (chestOpened) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Debug.Log(collision.gameObject);
        chestOpened = true;
        StartCoroutine(OpenChest());
        OnChestOpened?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator OpenChest() {

        yield return new WaitForSeconds(delayToChestUnlockAnimation);

        OnChestUnlocked?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToSpawnCollectibles - delayToChestUnlockAnimation);

        for(int i = 0; i < rewardAmount; i++) {
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeToReward), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

            yield return new WaitForSeconds(.1f);
            collectible.ApplyRandomUpwardsForce(5, 8);
            collectible.SetCollectibleUnInteractable(1f);
        }

    }
}
