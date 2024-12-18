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
    [SerializeField] protected int itemLevel;
    [SerializeField] protected int maxItemLevel;
    [SerializeField] private bool isBoughtAtStart;
    [SerializeField] private bool itemUpgradeable;

    protected List<int> greenGemCostList;
    protected List<int> redGemCostList;

    public static event EventHandler OnAnyHubMerchantItemBought;
    public  event EventHandler OnItemMustRefreshDescriptionCard;
    public event EventHandler OnHubMerchantItemUpgraded;
    public event EventHandler OnHubMerchantItemLoaded;

    protected bool itemBought;
    protected bool itemUnlocked;
    protected bool itemBuyable;

    protected virtual void Start() {
        LoadItemStatus();
    }

    protected void LoadItemStatus() {
        itemBought = MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType());
        itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType());
        itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
        RefreshItemBuyable();

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

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
        itemUnlocked = true;
        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType());
    }

    public virtual void BuyItem() {
        itemLevel++;
        itemBought = true;

        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);

        MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType());
        MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), GetItemLevel());

        OnAnyHubMerchantItemBought?.Invoke(this, EventArgs.Empty);

        if(greenGemCostList.Count >= itemLevel) {
            greenGemCost = greenGemCostList[itemLevel-1];
        }

        if(redGemCostList.Count >= itemLevel) {
            redGemCost = redGemCostList[itemLevel-1];
        }

        RefreshItemBuyable();
    }

    public virtual void UpgradeItem() {
        itemLevel++;

        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);

        MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), GetItemLevel());

        if (greenGemCostList.Count >= itemLevel) {
            greenGemCost = greenGemCostList[itemLevel - 1];
        }

        if (redGemCostList.Count >= itemLevel) {
            redGemCost = redGemCostList[itemLevel - 1];
        }

        RefreshItemBuyable();

        OnHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshItemBuyable() {

        if(!itemUpgradeable) {

            if(itemBought) {
                itemBuyable = false;
            } else {
                itemBuyable = true;
            }

        } else {

            if (itemLevel == maxItemLevel) {
                itemBuyable = false;
            }
            else {
                itemBuyable = true;
            }
        }

        Debug.Log(gameObject + " " + itemBuyable);

    }

    public void InvokeItemMustRefreshDescriptionCard() {
        OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);
    }

    public string GetItemName() {
        return itemName;
    }

    public virtual string GetDescription() {
        return description;
    }

    public virtual List<string> GetStatDescription() {
        List<string> statDescriptionList = new List<string> {
            unlockDescription
        };

        return statDescriptionList;
    }

    public virtual List<string> GetStatValues() {
        return null;
    }

    public virtual List<bool> GetStatModifierBools() {
        return null;
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

    public bool GetItemUpgradeable() {
        return itemUpgradeable;
    }

    public bool GetItemBoughtAtStart() {
        return isBoughtAtStart;
    }

    public bool GetItemBought() {
        return itemBought;
    }
    public bool GetItemBuyable() {
        return itemBuyable;
    }

    public bool GetItemUnlocked() {
        return itemUnlocked;
    }

    public virtual int GetItemLevel() {
        return itemLevel;
    }

    public virtual int GetMaxItemLevel() {
        return maxItemLevel;
    }
}
