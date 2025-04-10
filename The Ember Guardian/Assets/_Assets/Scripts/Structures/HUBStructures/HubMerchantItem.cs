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
    [SerializeField] private bool isUnlockedAtStart;
    [SerializeField] protected bool itemUpgradeable;
    [SerializeField] private bool itemEquipable;

    [SerializeField] protected HubMerchantItemStatModifierSO linkedStatModifierSO;

    protected List<int> greenGemCostList;
    protected List<int> redGemCostList;
    protected List<int> blueGemCostList;
    protected List<int> yellowGemCostList;
    protected List<int> purpleGemCostList;

    public static event EventHandler OnAnyHubMerchantItemBought;
    public static event EventHandler OnAnyHubMerchantItemUpgraded;
    public event EventHandler OnItemMustRefreshDescriptionCard;
    public event EventHandler OnHubMerchantItemBought;
    public event EventHandler OnHubMerchantItemUpgraded;
    public event EventHandler OnHubMerchantItemLoaded;
    public event EventHandler OnHubMerchantItemEquipped;
    public static event EventHandler OnAnyHubMerchantItemEquipped;
    public event EventHandler OnHubMerchantItemUnequipped;

    protected bool itemBought;
    protected bool itemUnlocked;
    protected bool itemEquipped;
    protected bool itemStatusChanged;

    protected List<string> statValues = new List<string>();
    protected List<bool> statModifiedBools = new List<bool>();
    
    protected virtual void Awake() {
        if (linkedStatModifierSO != null) {
            maxItemLevel = linkedStatModifierSO.statModifierList.Count;

            redGemCostList = linkedStatModifierSO.redGemCostList;
            greenGemCostList = linkedStatModifierSO.greenGemCostList;
            yellowGemCostList = linkedStatModifierSO.yellowGemCostList;
            blueGemCostList = linkedStatModifierSO.blueGemCostList;
            purpleGemCostList = linkedStatModifierSO.purleGemCostList;
        }

        if(itemLevel == 0) {
            itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
        }

        UpdateItemCost();
    }

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
                LoadItemEquipped();
            }
        }

        if (isUnlockedAtStart) {
            itemUnlocked = true;
        }

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void LoadItemEquipped() {
        itemEquipped = MetaProgressionManager.Instance.GetMerchantItemEquipped(GetItemType());
    }

    public bool CanBuyItem() {
        int playerGreenGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int playerRedGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
        int playerBlueGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
        int playerYellowGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
        int playerPurpleGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;

        if (playerGreenGems >= greenGemCost && playerRedGems >= redGemCost && playerBlueGems >= blueGemCost && playerYellowGems >= yellowGemCost && playerPurpleGems >= purpleGemCost) {

            return true;
        }
        else {
            return false;
        }
    }

    public virtual void UnlockItem() {
        itemUnlocked = true;
        itemStatusChanged = true;
    }

    public virtual void BuyItem() {

        if (itemUpgradeable) {
            itemLevel++;
        }

        itemBought = true;
        itemStatusChanged = true;

        PayGemPrice();
        UpdateItemCost();

        OnHubMerchantItemBought?.Invoke(this, EventArgs.Empty);
    }

    public virtual void UpgradeItem() {
        itemLevel++;
        PayGemPrice();
        UpdateItemCost();

        OnHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
        OnAnyHubMerchantItemUpgraded?.Invoke(this, EventArgs.Empty);
        itemStatusChanged = true;
    }

    protected void PayGemPrice() {

        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);
        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.blueGem, blueGemCost);
        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.yellowGem, yellowGemCost);
        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.purpleGem, purpleGemCost);

        OnAnyHubMerchantItemBought?.Invoke(this, EventArgs.Empty);

    }

    protected void UpdateItemCost() {

        if (greenGemCostList != null && greenGemCostList.Count > itemLevel) {
            greenGemCost = greenGemCostList[itemLevel];
        }

        if (redGemCostList != null && redGemCostList.Count > itemLevel) {
            redGemCost = redGemCostList[itemLevel];
        }

        if (yellowGemCostList != null && yellowGemCostList.Count > itemLevel) {
            yellowGemCost = yellowGemCostList[itemLevel];
        }

        if (blueGemCostList != null && blueGemCostList.Count > itemLevel) {
            blueGemCost = blueGemCostList[itemLevel];
        }

        if (purpleGemCostList != null && purpleGemCostList.Count > itemLevel) {
            purpleGemCost = purpleGemCostList[itemLevel];
        }
    }

    public virtual void EquipOrUnequipItem() {
        itemStatusChanged = true;
    }

    public virtual void UnequipItem() {
        itemStatusChanged = true;
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
        return statValues;
    }

    public virtual List<bool> GetStatModifierBools() {
        return statModifiedBools;
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

    public int GetBlueGemCost() {
        return blueGemCost;
    }

    public int GetYellowGemCost() {
        return yellowGemCost;
    }

    public int GetPurpleGemCost() {
        return purpleGemCost;
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
        Debug.Log(itemStatusChanged + " " + GetItemType()); 

        if (!itemStatusChanged) return;

        if(itemBought && !MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType())) {
            MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), itemBought);
        }

        if (itemUpgradeable && MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType()) != itemLevel) {
            MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), itemLevel);
        }

        if (itemUnlocked && !MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType())) {
            MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), itemUnlocked);
        }

        if(itemEquipable) {
            MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), itemEquipped);
        }

    }

    public void ResetItemStatus() {
        MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), false);
        if (itemUpgradeable) {
            MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), 0);
        }
        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), false);
        if (itemEquipable) {
            MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), false);
        }
    }
}
