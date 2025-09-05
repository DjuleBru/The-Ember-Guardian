using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNPCGemReward : MonoBehaviour
{
    [SerializeField] private HubMerchantTalkUI talkUI;
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList;
    [SerializeField] private List<int> rewardAmountList;

    private bool disableReward;
    private bool rewarded;

    public static event EventHandler<OnAnyCurrencyDroppedEventArgs> OnAnyCurrencyDropped;
    public class OnAnyCurrencyDroppedEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    private void Start() {
        talkUI.OnMerchantEndTalk += TalkUI_OnMerchantEndTalk;
    }

    private void TalkUI_OnMerchantEndTalk(object sender, System.EventArgs e) {
        if (rewarded) return;
        if (disableReward) return;
        StartCoroutine(GiveReward());
        rewarded = true;
    }

    private IEnumerator GiveReward() {
        int j = 0;

        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
            int rewardAmount = rewardAmountList[j];

            PlayerCurrencies.CurrencyType currencyTypeToReward = currencyType;
            if(currencyType == PlayerCurrencies.CurrencyType.ammo) {
                if(PlayerShoot.Instance.GetHasOnlySpecialAmmo()) {
                    currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                    rewardAmount /= 2;
                }
            }

            for (int i = 0; i < rewardAmount; i++) {
                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeToReward), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
                OnAnyCurrencyDropped?.Invoke(this, new OnAnyCurrencyDroppedEventArgs {
                    currencyType = currencyTypeToReward,
                });

                yield return new WaitForSeconds(.2f);
                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(.75f);
            }
            j++;
        }

    }

    public void DisableReward() {
        disableReward = true;
    }

    public void SetReward(List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList, List<int> rewardAmountList) {
        this.currencyTypeToRewardList = currencyTypeToRewardList;
        this.rewardAmountList = rewardAmountList;
    }
}
