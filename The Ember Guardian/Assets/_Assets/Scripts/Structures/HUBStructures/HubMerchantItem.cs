using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem : MonoBehaviour
{
    [SerializeField] protected string itemName;
    [SerializeField] protected int greenGemCost;
    [SerializeField] protected int redGemCost;
    [SerializeField] protected int blueGemCost;
    [SerializeField] protected int yellowGemCost;
    [SerializeField] protected int purpleGemCost;
    [TextArea]
    [SerializeField] protected string description;
    [TextArea]
    [SerializeField] protected string unlockDescription;
    [SerializeField] protected int itemLevel;
    [SerializeField] protected int maxItemLevel;
    [SerializeField] private bool isBoughtAtStart;
    [SerializeField] private bool itemUpgradeable;
    [SerializeField] private bool itemEquipable;
    protected bool isEquippedAtStart;

    protected List<int> greenGemCostList;
    protected List<int> redGemCostList;
    protected List<int> blueGemCostList;
    protected List<int> yellowGemCostList;
    protected List<int> purpleGemCostList;

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
            
            if(!itemUnlocked) {
                itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType());
            }

            if(itemLevel == 0) {
                itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
            }

            if(itemEquipable) {
                itemEquipped = MetaProgressionManager.Instance.GetMerchantItemEquipped(GetItemType());
            }

        } else {
            itemBought = true;
            itemUnlocked = true;
            itemLevel = maxItemLevel;

            if(itemEquipable) {
                itemEquipped = MetaProgressionManager.Instance.GetMerchantItemEquipped(GetItemType());
                isEquippedAtStart = MetaProgressionManager.Instance.GetMerchantItemEquippedAtStart(GetItemType());

                if (isEquippedAtStart || itemEquipped) {
                    itemEquipped = true;
                }
                else {
                    itemEquipped = false;
                }
            }
        }

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    public bool CanBuyItem() {
        int playerGreenGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int playerRedGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
        int playerBlueGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
        int playerYellowGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
        int playerPurpleGems = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;

        if (playerGreenGems >= greenGemCost && playerRedGems >= redGemCost && playerBlueGems >= blueGemCost && playerYellowGems >= redGemCost && playerPurpleGems >= purpleGemCost) {

            return true;
        }
        else {
            return false;
        }
    }

    public virtual void UnlockItem() {
        itemUnlocked = true;
    }

    public virtual void BuyItem() {

        if (itemUpgradeable) {
            itemLevel++;
        }

        itemBought = true;

        PayGemPrice();
        UpdateItemCost();
    }

    public virtual void UpgradeItem() {
        itemLevel++;
        PayGemPrice();
        UpdateItemCost();

        OnHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
        OnAnyHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
    }

    private void PayGemPrice() {

        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.blueGem, blueGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.yellowGem, yellowGemCost);
        UICurrencyManager.Instance.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.purpleGem, purpleGemCost);

        OnAnyHubMerchantItemBought?.Invoke(this, EventArgs.Empty);

    }

    private void UpdateItemCost() {

        if (greenGemCostList != null && greenGemCostList.Count >= itemLevel) {
            greenGemCost = greenGemCostList[itemLevel - 1];
        }

        if (redGemCostList != null && redGemCostList.Count >= itemLevel) {
            redGemCost = redGemCostList[itemLevel - 1];
        }

        if (yellowGemCostList != null && yellowGemCostList.Count >= itemLevel) {
            yellowGemCost = yellowGemCostList[itemLevel - 1];
        }

        if (blueGemCostList != null && blueGemCostList.Count >= itemLevel) {
            blueGemCost = blueGemCostList[itemLevel - 1];
        }

        if (purpleGemCostList != null && purpleGemCostList.Count >= itemLevel) {
            purpleGemCost = purpleGemCostList[itemLevel - 1];
        }
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

    public bool GetItemMaxed() {
        if(itemUpgradeable) {
            return itemLevel == maxItemLevel;
        } else {
            return false;
        }
    }
    public bool GetitemEquipable() {
        return itemEquipable;
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

    public void SaveItemStatus() {
        if(itemBought && !MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType())) {
            MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), itemBought);

            if (itemUpgradeable) {
                MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), itemLevel);
            }
        }

        if(itemUnlocked && !MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType())) {
            MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), itemUnlocked);
        }

        if(itemEquipable) {
            MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), itemEquipped);
        }

    }
}
