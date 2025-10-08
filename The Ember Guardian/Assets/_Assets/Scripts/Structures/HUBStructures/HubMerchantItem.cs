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
    [SerializeField] protected int cyanGemCost;
    [TextArea]
    [SerializeField] protected string description;
    [TextArea]
    [SerializeField] protected string unlockDescription;
    [SerializeField] protected int itemLevel;
    [SerializeField] protected int maxItemLevel;
    [SerializeField] private bool isBoughtAtStart;
    [SerializeField] private bool isUnlockedAtStart;
    [SerializeField] protected bool itemUpgradeable;
    [SerializeField] protected bool unlockRequiresAllPrerequisited;

    [SerializeField] protected HubMerchantItemStatModifierSO linkedStatModifierSO;

    [SerializeField] private HubMerchant parentHubMerchant;

    private ItemButtonUI itemButtonUI;
    protected List<int> greenGemCostList;
    protected List<int> redGemCostList;
    protected List<int> blueGemCostList;
    protected List<int> yellowGemCostList;
    protected List<int> purpleGemCostList;
    protected List<int> cyanGemCostList;

    public static event EventHandler OnAnyHubMerchantItemBought;
    public static event EventHandler OnAnyHubMerchantItemUpgraded;
    public event EventHandler OnItemMustRefreshDescriptionCard;
    public event EventHandler OnHubMerchantItemBought;
    public event EventHandler OnHubMerchantItemUpgraded;
    public event EventHandler OnHubMerchantItemLoaded;
    public event EventHandler OnHubMerchantItemEquipped;
    public static event EventHandler OnAnyHubMerchantItemEquipped;
    public event EventHandler OnHubMerchantItemUnequipped;

    protected bool canBuyItem;
    protected bool itemBought;
    protected bool itemUnlocked;
    protected bool newItemUnlocked;
    protected bool itemStatusChanged;
    protected bool initializedCostList;

    protected List<string> statValues = new List<string>();
    protected List<bool> statModifiedBools = new List<bool>();
    
    protected virtual void Awake() {
        InitializeCostLists();
       
        if(itemLevel == 0) {
            itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
        }
        UpdateItemCost();
    }

    protected virtual void Start() {
        OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);

        UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
    }

    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(canBuyItem && !CanBuyItem()) {
            canBuyItem = CanBuyItem();
            OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);

        }

        if (!canBuyItem && CanBuyItem()) {
            canBuyItem = CanBuyItem();
            OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);

        }
    }

    protected void InitializeCostLists() {
        if (initializedCostList) return;

        if (linkedStatModifierSO != null) {
            maxItemLevel = linkedStatModifierSO.statModifierList.Count;

            redGemCostList = linkedStatModifierSO.redGemCostList;
            greenGemCostList = linkedStatModifierSO.greenGemCostList;
            yellowGemCostList = linkedStatModifierSO.yellowGemCostList;
            blueGemCostList = linkedStatModifierSO.blueGemCostList;
            purpleGemCostList = linkedStatModifierSO.purleGemCostList;
            cyanGemCostList = linkedStatModifierSO.cyanGemCostList;
        }

        initializedCostList = true;
    }


    public bool CanBuyItem() {
        int playerGreenGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
        int playerRedGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
        int playerCyanGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.cyanGem).Count;
        int playerBlueGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
        int playerYellowGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
        int playerPurpleGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;

        if (playerGreenGems >= greenGemCost && playerRedGems >= redGemCost && playerBlueGems >= blueGemCost && playerYellowGems >= yellowGemCost && playerPurpleGems >= purpleGemCost && playerCyanGems >= cyanGemCost) {
            return true;
        }
        else {
            return false;
        }
    }

    public virtual void UnlockItem() {
        itemUnlocked = true;
        itemStatusChanged = true;

        //Debug.Log("UnlockItem " + GetItemType());
    }

    public virtual void SetItemBought() {
        itemBought = true;
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
        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.cyanGem, cyanGemCost);

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

        if (cyanGemCostList != null && cyanGemCostList.Count > itemLevel) {
            cyanGemCost = cyanGemCostList[itemLevel];
        }

        canBuyItem = CanBuyItem();
    }

    public virtual void EquipOrUnequipItem() {
        itemStatusChanged = true;
    }

    public virtual void UnequipItem() {
        itemStatusChanged = true;
    }

    public void InvokeOnItemLoaded() {
        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnItemUnequipped() {
        OnHubMerchantItemUnequipped?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeItemMustRefreshDescriptionCard() {
        OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);
    }

    public HubMerchant GetHubMerchantParent() {
        return parentHubMerchant;
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
    public int GetCyanGemCost() {
        return cyanGemCost;
    }

    public int GetTotalGemCosts(PlayerCurrencies.CurrencyType gemType) {
        InitializeCostLists();

        // Cas 1 : Item non-upgradable et déjà acheté :plus de coût
        if (!itemUpgradeable && itemBought) {
            return 0;
        }

        int totalGemCosts = 0;

        if (linkedStatModifierSO != null) {
            List<int> costList = null;

            switch (gemType) {
                case PlayerCurrencies.CurrencyType.redGem:
                    costList = linkedStatModifierSO.redGemCostList;
                    break;
                case PlayerCurrencies.CurrencyType.blueGem:
                    costList = linkedStatModifierSO.blueGemCostList;
                    break;
                case PlayerCurrencies.CurrencyType.greenGem:
                    costList = linkedStatModifierSO.greenGemCostList;
                    break;
                case PlayerCurrencies.CurrencyType.yellowGem:
                    costList = linkedStatModifierSO.yellowGemCostList;
                    break;
                case PlayerCurrencies.CurrencyType.purpleGem:
                    costList = linkedStatModifierSO.purleGemCostList;
                    break;
                case PlayerCurrencies.CurrencyType.cyanGem:
                    costList = linkedStatModifierSO.cyanGemCostList;
                    break;
            }

            if (costList != null) {
                // Cas 2 : Item upgradable mais déjà au max :rien à ajouter
                if (itemLevel >= costList.Count) {
                    return 0;
                }

                // On ne compte que les coûts à partir du niveau actuel + 1
                for (int i = itemLevel; i < costList.Count; i++) {
                    totalGemCosts += costList[i];
                }
            }
        }
        else {
            // Cas fallback : item simple avec un coût unique
            switch (gemType) {
                case PlayerCurrencies.CurrencyType.redGem:
                    return itemBought ? 0 : redGemCost;
                case PlayerCurrencies.CurrencyType.blueGem:
                    return itemBought ? 0 : blueGemCost;
                case PlayerCurrencies.CurrencyType.greenGem:
                    return itemBought ? 0 : greenGemCost;
                case PlayerCurrencies.CurrencyType.yellowGem:
                    return itemBought ? 0 : yellowGemCost;
                case PlayerCurrencies.CurrencyType.purpleGem:
                    return itemBought ? 0 : purpleGemCost;
                case PlayerCurrencies.CurrencyType.cyanGem:
                    return itemBought ? 0 : cyanGemCost;
            }
        }
        return totalGemCosts;
    }

    public int GetRedGemsPaid() {
        InitializeCostLists();

        // Cas 1 : Item non-upgradable et déjà acheté :plus de coût
        if (!itemUpgradeable && itemBought) {
            return redGemCost;
        }

        int totalGemCosts = 0;

        if (linkedStatModifierSO != null) {
            for(int i = 0; i < itemLevel; i++) {
                totalGemCosts += linkedStatModifierSO.redGemCostList[i];
            }
        }

        return totalGemCosts;
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
    public bool GetUnlockRequiresAllPrerequisites() {
        return unlockRequiresAllPrerequisited;
    }
    public bool GetItemMaxed() {
        if(itemUpgradeable) {
            return itemLevel == maxItemLevel;
        } else {
            return false;
        }
    }

    public bool GetItemUnlocked() {
        return itemUnlocked;
    }
    public bool GetNewItemUnlocked() {
        return newItemUnlocked;
    }

    public void SetNewItemUnlocked(bool unlocked) {
        newItemUnlocked = unlocked;
        itemStatusChanged = true;
    }

    public virtual int GetItemLevel() {
        return itemLevel;
    }

    public virtual int GetMaxItemLevel() {
        return maxItemLevel;
    }

    //public void SaveItemStatus() {

    //    if (!itemStatusChanged) return;

    //    bool itemBoughtInSave = MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType());
    //    if (itemBought && !itemBoughtInSave) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), itemBought);
    //    }
    //    if (!itemBought && itemBoughtInSave) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), itemUnlocked);
    //        MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), itemBought);
    //        MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), itemLevel);
    //    }

    //    if (itemUpgradeable && MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType()) != itemLevel) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), itemLevel);
    //    }

    //    if (itemUnlocked && !MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType())) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), itemUnlocked);
    //    }

    //    if(!newItemUnlocked && MetaProgressionManager.Instance.GetHubMerchantItemNewlyUnlocked(GetItemType())) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(GetItemType(), false);
    //    }
    //    if (newItemUnlocked && !MetaProgressionManager.Instance.GetHubMerchantItemNewlyUnlocked(GetItemType())) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(GetItemType(), true);
    //    }

    //    if (itemEquipable) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemEquipped(GetItemType(), itemEquipped);
    //    }

    //}

    //public virtual void LoadItemStatus() {
    //    newItemUnlocked = MetaProgressionManager.Instance.GetHubMerchantItemNewlyUnlocked(GetItemType());

    //    if (!isBoughtAtStart) {

    //        itemBought = MetaProgressionManager.Instance.GetMerchantItemBought(GetItemType());

    //        if (!itemUnlocked) {
    //            itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType());
    //        }

    //        if (itemLevel == 0) {
    //            itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
    //        }

    //        if (itemEquipable) {
    //            itemEquipped = MetaProgressionManager.Instance.GetMerchantItemEquipped(GetItemType());
    //        }

    //    }
    //    else {
    //        itemBought = true;
    //        itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(GetItemType());
    //        if (newItemUnlocked) {
    //            itemUnlocked = true;
    //        }

    //        itemLevel = maxItemLevel;

    //        if (itemEquipable) {
    //            LoadItemEquipped();
    //        }
    //    }

    //    if (isUnlockedAtStart) {
    //        itemUnlocked = true;
    //    }

    //    OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    //}

    public void LoadItemStatus_Batch() {
        var key = GetItemType() + "_Data";

        if(isBoughtAtStart) {
            itemBought = true;
        }
        if (isUnlockedAtStart) {
            itemUnlocked = true;
        }

        if (ES3.KeyExists(key)) {
            var data = ES3.Load<Dictionary<string, object>>(key);

            if(!itemBought) {
                itemBought = data.ContainsKey("Bought") ? Convert.ToBoolean(data["Bought"]) : isBoughtAtStart;
            }

            if(!itemUnlocked) {
                itemUnlocked = data.ContainsKey("Unlocked") ? Convert.ToBoolean(data["Unlocked"]) : isUnlockedAtStart;
            }

            if(!newItemUnlocked) {
                newItemUnlocked = data.ContainsKey("NewlyUnlocked") ? Convert.ToBoolean(data["NewlyUnlocked"]) : false;
            }

            itemLevel = data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : 0;
        }

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void SaveItemStatus_Batch() {
        if (!itemStatusChanged) return;

        var dataToSave = new Dictionary<string, object> {
            ["Bought"] = itemBought,
            ["Unlocked"] = itemUnlocked,
            ["NewlyUnlocked"] = newItemUnlocked,
            ["Level"] = itemLevel
        };

        // Sauvegarde du dictionnaire complet en une seule clé
        ES3.Save(GetItemType() + "_Data", dataToSave);

        itemStatusChanged = false;
    }

    // --- RESET ---
    public void ResetItemStatus_Batch() {
        itemBought = false;
        itemLevel = 0;
        itemUnlocked = false;
        newItemUnlocked = false;

        SaveItemStatus_Batch();
    }

    //public void ResetItemStatus() {

    //    MetaProgressionManager.Instance.SetHubMerchantItemBought(GetItemType(), false);
    //    if (itemUpgradeable) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemLevel(GetItemType(), 0);
    //    }
    //    MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(GetItemType(), false);

    //    if(isBoughtAtStart) {
    //        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(GetItemType(), false);
    //    }

    //}

    private void OnDestroy() {
        UICurrencyManager.HubInventoryUI.OnCurrencyCollected -= HubInventoryUI_OnCurrencyCollected;
    }

    public virtual void ResetGunItemStatus() {
        itemBought = false;
        itemUnlocked = false;
        itemLevel = 0;

        itemStatusChanged = true;

        UpdateItemCost();
        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }
}
