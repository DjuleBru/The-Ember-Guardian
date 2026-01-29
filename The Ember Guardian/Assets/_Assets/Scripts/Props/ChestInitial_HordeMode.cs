using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInitial_HordeMode : Chest
{
    protected override void Start() {
        base.Start();
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (DayNightManager.Instance.GetCurrentDay() == 0) return;

        currencyTypeToRewardList = new List<PlayerCurrencies.CurrencyType>();
        currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);
        rewardAmountList.Clear();
        rewardAmountList.Add(DayNightManager.Instance.GetCurrentDay() +1);

        chestOpened = false;
        SetChestOpenable();
    }

    protected override void OpenChest(bool spawnCollectibles) {
        chestOpened = true;
        StartCoroutine(OpenThenCloseChestCoroutine(true));
        InvokeOnChestOpened(true);
    }

    protected IEnumerator OpenThenCloseChestCoroutine(bool spawnCollectibles, bool triggerSFX = true) {
        yield return new WaitForSeconds(.1f);

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        yield return new WaitForSeconds(delayToChestUnlockAnimation - .1f);

        playerPayingCurrencies = false;
        InvokeOnChestUnlocked(true);

        yield return new WaitForSeconds(delayToSpawnCollectibles - delayToChestUnlockAnimation - .1f);

        if (spawnCollectibles) {
            int j = 0;

            foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
                int rewardAmount = rewardAmountList[j];

                PlayerCurrencies.CurrencyType currencyTypeToReward = currencyType;

                if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
                    bool secondaryGunUnlocked = PlayerShoot.Instance.GetSecondaryGunSO() != null;
                    bool hasOnlySpecialAmmo = PlayerShoot.Instance.GetHasOnlySpecialAmmo();
                    bool hasBothAmmoTypes = PlayerShoot.Instance.GetHasBothAmmoTypes();

                    if (hasOnlySpecialAmmo) {

                        // Player has 2 special ammo weapons OR only 1 weapon with special ammo:
                        currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                        rewardAmount /= 2;

                    }
                    else if (hasBothAmmoTypes) {

                        // Player has both ammo types
                        float randomFloat = UnityEngine.Random.value;
                        if (randomFloat < 0.5f) {
                            currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                            rewardAmount /= 2;
                        }

                    }
                }

                for (int i = 0; i < rewardAmount; i++) {

                    Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeToReward), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
                    InvokeOnAnyChestSpawnedCollectibles(currencyType);

                    collectible.ApplyRandomUpwardsForce(5, 8);
                    collectible.SetCollectibleUnInteractable(.75f);
                    yield return new WaitForSeconds(.2f);
                }
                j++;
            }
        }

        chestOpenedAnimationOver = true;
        InvokeOnAnyChestOpenedAnimationOver();

        yield return new WaitForSeconds(2f);

        InvokeOnChestClosed();
        SetChestLocked(true);
    }
}
