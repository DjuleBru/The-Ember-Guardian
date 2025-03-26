using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Special : Chest
{


    [SerializeField] private GunSO gunSOInChest;
    private bool rewardOfferedToPlayerStarted;

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if(!chestOpenedAnimationOver) {
            Debug.Log("OpenChest");
            chestOpened = true;
            StartCoroutine(OpenChest(false));
            InvokeOnChestOpened();

        } else {
            if (rewardOfferedToPlayerStarted) return;
            rewardOfferedToPlayerStarted = true;
            Debug.Log("OfferRewardToPlayer");
            StartCoroutine(OfferRewardToPlayer());

        }
      
    }

    private IEnumerator OfferRewardToPlayer() {

        StartCoroutine(MakeChestDisappear(5f));

        if (chestType == ChestType.weaponChest) {
            PlayerShoot.Instance.SetActiveGun(gunSOInChest);
        }

        yield return new WaitForSeconds(.5f);

        int j = 0;
        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
            int rewardAmount = rewardAmountList[j];

            for (int i = 0; i < rewardAmount; i++) {
                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

                InvokeOnAnyChestSpawnedCollectibles(currencyType);

                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(.75f);
                yield return new WaitForSeconds(.2f);
            }
            j++;
        }
    }
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (rewardOfferedToPlayerStarted) return;

        base.OnTriggerEnter2D(collision);
    }

}
