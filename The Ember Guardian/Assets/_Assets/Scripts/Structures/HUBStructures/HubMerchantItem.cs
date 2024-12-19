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
    [SerializeField] private bool isEquippedAtStart;
    [SerializeField] private bool itemUpgradeable;

    protected List<int> greenGemCostList;
    protected List<int> redGemCostList;

    public static event EventHandler OnAnyHubMerchantItemBought;
    public static event EventHandler OnAnyHubMerchantItemUpgraded;
    public  event EventHandler OnItemMustRefreshDescriptionCard;
    public event EventHandler OnHubMerchantItemUpgraded;
    public event EventHandler OnHubMerchantItemLoaded;
    public event EventHandler OnHubMerchantItemEquipped;
    public static event EventHandler OnAnyHubMerchantItemEquipped;
    public event EventHandler OnHubMerchantItemUnequipped;

    protected bool itemBought;
    protected bool itemUnlocked;
    protected bool itemEquipped;

    protected virtual void Start() {
        LoadItemStatus();
    }

    protected void LoadItemStatus() {
        if(!isBoughtAtStart) {

            itemBought = MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType());
            itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType());
            itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
            itemEquipped = MetaProgressionManager.Instance.GetMerchantItemEquipped(GetItemType());

        } else {
            itemBought = true;
            itemUnlocked = true;

            if(isEquippedAtStart) {
                itemEquipped = true;
            }
        }

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

        if(greenGemCostList != null && greenGemCostList.Count >= itemLevel) {
            greenGemCost = greenGemCostList[itemLevel-1];
        }

        if(redGemCostList != null && redGemCostList.Count >= itemLevel) {
            redGemCost = redGemCostList[itemLevel-1];
        }
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

        OnHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
        OnAnyHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public virtual void EquipOrUnequipItem() {
    }

    public virtual void UnequipItem() {
    }

    public void InvokeOnItemEquipped() {
        OnHubMerchantItemEquipped?.Invoke(this, EventArgs.Empty);
        OnAnyHubMerchantItemEquipped?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnItemUnequipped() {
        OnHubMerchantItemUnequipped?.Invoke(this, EventArgs.Empty);
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

    public virtual bool GetConstantUnlockDescription() {
        return true;
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

    public bool GetItemUnlocked() {
        return itemUnlocked;
    }

    public virtual int GetItemLevel() {
        return itemLevel;
    }

    public virtual int GetMaxItemLevel() {
        return maxItemLevel;
    }
    public bool GetItemEquipped() {
        return itemEquipped;
    }
}
