using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant_SkillsFeedbacks : MonoBehaviour
{
    [SerializeField] private Merchant_Skills merchant;
    [SerializeField] private MMF_Player passiveBuyFeedbacks;
    [SerializeField] private MMF_Player activeBuyFeedbacks;

    private void Start() {
        merchant.OnPlayerBoughtItem += Merchant_OnPlayerBoughtItem;
    }

    private void Merchant_OnPlayerBoughtItem(object sender, Merchant.OnPlayerBoughtItemEventArgs e) {
        if(e.boughtItem.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
            passiveBuyFeedbacks.PlayFeedbacks();
        }
        if (e.boughtItem.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
            activeBuyFeedbacks.PlayFeedbacks();
        }
    }
}
