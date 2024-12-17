using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem : MonoBehaviour
{
    [SerializeField] protected string itemName;
    [SerializeField] protected int greenGemCost;
    [SerializeField] protected int redGemCost;
    [TextArea]
    [SerializeField] protected string description;
    [TextArea]
    [SerializeField] protected string unlockDescription;

    public static event EventHandler OnAnyHubMerchantItemBought;

    public bool CanBuyItem() {
        int playerGreenGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int playerRedGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;

        if (playerGreenGems >= greenGemCost && playerRedGems >= redGemCost) {

            return true;
        }
        else {
            return false;
        }
    }

    public virtual void UnlockItem() {
        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType());
    }

    public virtual void BuyItem() {
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);

        MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType());

        OnAnyHubMerchantItemBought?.Invoke(this, EventArgs.Empty);
    }

    public string GetItemName() {
        return itemName;
    }

    public string GetDescription() {
        return description;
    }

    public string GetStatDescription() {
        return unlockDescription;
    }

    public int GetGreenGemCost() {
        return greenGemCost;
    }

    public int GetRedGemCost() {
        return redGemCost;
    }

    public virtual string GetItemType() {
        return "defaultItemType";
    }

}
