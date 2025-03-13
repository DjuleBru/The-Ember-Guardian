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
        StartCoroutine(GiveReward());
        rewarded = true;
    }

    private IEnumerator GiveReward() {
        int j = 0;

        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
            int rewardAmount = rewardAmountList[j];

            for (int i = 0; i < rewardAmount; i++) {
                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
                OnAnyCurrencyDropped?.Invoke(this, new OnAnyCurrencyDroppedEventArgs {
                    currencyType = currencyType,
                });

                yield return new WaitForSeconds(.2f);
                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(.75f);
            }
            j++;
        }

    }
}
