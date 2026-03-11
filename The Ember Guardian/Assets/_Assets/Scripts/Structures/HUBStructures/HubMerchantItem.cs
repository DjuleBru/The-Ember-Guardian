using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] protected DLCManager.DLCType linkedDLCType = DLCManager.DLCType.None;

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
    public static event EventHandler OnAnyHubMerchantItemArchitectTableUnlocks;
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
            if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
                // Not horde mode
                itemLevel = MetaProgressionManager.Instance.GetHubMerchantItemLevel(GetItemType());
            };
        }

        UpdateItemCost();
    }

    protected virtual void Start() {
        StartCoroutine(RefreshDescriptionCardAfterFrame());

        if(parentHubMerchant.GetIsHordeModeNPC()) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
        } else {
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;
        }

    }

    private IEnumerator RefreshDescriptionCardAfterFrame() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        OnItemMustRefreshDescriptionCard?.Invoke(this, EventArgs.Empty);
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

        if(parentHubMerchant.GetIsHordeModeNPC()) {
            // Trouver la liste avec le plus grand Count
            var allLists = new List<List<int>> {
            redGemCostList,
            greenGemCostList,
            yellowGemCostList,
            blueGemCostList,
            purpleGemCostList,
            cyanGemCostList
            };

            // Filtrer les listes nulles
            var validLists = allLists.Where(l => l != null).ToList();

            var biggestList = validLists
                .OrderByDescending(l => l.Count)
                .FirstOrDefault();

            redGemCostList = biggestList ?? new List<int>();

            greenGemCostList = new List<int>();
            yellowGemCostList = new List<int>();
            blueGemCostList = new List<int>();
            purpleGemCostList = new List<int>();
            cyanGemCostList = new List<int>();

        }

        initializedCostList = true;
    }

    public bool CanBuyItem() {
        if(!DLCManager.Instance.HasDLC(linkedDLCType)) return false;

        int playerGreenGems = 0;
        int playerRedGems = 0;
        int playerCyanGems = 0; 
        int playerBlueGems = 0;
        int playerYellowGems = 0;
        int playerPurpleGems = 0;

        if (parentHubMerchant.GetIsHordeModeNPC()) {
            playerGreenGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
            playerRedGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
            playerCyanGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.cyanGem).Count;
            playerBlueGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
            playerYellowGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
            playerPurpleGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;
        } else {
            playerGreenGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
            playerRedGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
            playerCyanGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.cyanGem).Count;
            playerBlueGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
            playerYellowGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
            playerPurpleGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;
        }


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

        if(parentHubMerchant.GetIsHordeModeNPC()) {
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.blueGem, blueGemCost);
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.yellowGem, yellowGemCost);
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.purpleGem, purpleGemCost);
            UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.cyanGem, cyanGemCost);
        } else {
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.greenGem, greenGemCost);
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.redGem, redGemCost);
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.blueGem, blueGemCost);
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.yellowGem, yellowGemCost);
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.purpleGem, purpleGemCost);
            UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.cyanGem, cyanGemCost);
        }

        OnAnyHubMerchantItemBought?.Invoke(this, EventArgs.Empty);
    }

    protected void UpdateItemCost() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level && LevelManager.Instance.IsHordeMode()) {
            if(greenGemCostList.Count == 0 && redGemCostList.Count == 0 && yellowGemCostList.Count == 0 && blueGemCostList.Count == 0 && purpleGemCostList.Count == 0 && cyanGemCostList.Count == 0) {
                redGemCost = Mathf.Max(greenGemCost, redGemCost, yellowGemCost, blueGemCost, purpleGemCost, cyanGemCost);
                greenGemCost = 0;
                yellowGemCost = 0;
                blueGemCost = 0;
                purpleGemCost = 0;
                cyanGemCost = 0;
                canBuyItem = CanBuyItem();
                return;
            }
        }

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

    public bool GetItemLockedByDLC() {
        return !DLCManager.Instance.HasDLC(linkedDLCType);
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

    public virtual void SetNewItemUnlocked(bool unlocked) {
        //Debug.Log(GetItemType() + "SetNewItemUnlocked " + unlocked);
        newItemUnlocked = unlocked;
        itemStatusChanged = true;
    }

    public virtual int GetItemLevel() {
        return itemLevel;
    }

    public virtual int GetMaxItemLevel() {
        return maxItemLevel;
    }

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
                if(newItemUnlocked) {
                    SetNewItemUnlocked(true);
                }

            }

            itemLevel = data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : 0;

            if(linkedStatModifierSO != null) {
                // Balancing Security 
                int maxItemLevel = Mathf.Max(linkedStatModifierSO.blueGemCostList.Count, linkedStatModifierSO.redGemCostList.Count, linkedStatModifierSO.yellowGemCostList.Count, linkedStatModifierSO.purleGemCostList.Count, linkedStatModifierSO.cyanGemCostList.Count, linkedStatModifierSO.greenGemCostList.Count);
                if(itemLevel > maxItemLevel) { itemLevel = maxItemLevel; }
            }
        }

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    public virtual void LoadItemStatus_HodeMode() {
        var key = GetItemType() + "_HordeModeData";

        if (isBoughtAtStart) {
            itemBought = true;
        }

        if (isUnlockedAtStart) {
            itemUnlocked = true;
        }

        if (ES3.KeyExists(key)) {
            var data = ES3.Load<Dictionary<string, object>>(key);

            if (!itemBought) {
                itemBought = data.ContainsKey("Bought") ? Convert.ToBoolean(data["Bought"]) : isBoughtAtStart;
            }

            if (!itemUnlocked) {
                itemUnlocked = data.ContainsKey("Unlocked") ? Convert.ToBoolean(data["Unlocked"]) : isUnlockedAtStart;
            }

            if (!newItemUnlocked) {
                newItemUnlocked = data.ContainsKey("NewlyUnlocked") ? Convert.ToBoolean(data["NewlyUnlocked"]) : false;
                if (newItemUnlocked) {
                    SetNewItemUnlocked(true);
                }

            }

            itemLevel = data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : 0;

            if (linkedStatModifierSO != null) {
                // Balancing Security 
                int maxItemLevel = Mathf.Max(linkedStatModifierSO.blueGemCostList.Count, linkedStatModifierSO.redGemCostList.Count, linkedStatModifierSO.yellowGemCostList.Count, linkedStatModifierSO.purleGemCostList.Count, linkedStatModifierSO.cyanGemCostList.Count, linkedStatModifierSO.greenGemCostList.Count);
                if (itemLevel > maxItemLevel) { itemLevel = maxItemLevel; }
            }
        }

        OnHubMerchantItemLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnAnyHubMerchantItemArchitectTableUnlocks() {
        OnAnyHubMerchantItemArchitectTableUnlocks?.Invoke(this, EventArgs.Empty);
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
        string key = GetItemType() + "_Data";
        if (parentHubMerchant.GetIsHordeModeNPC()) {
            key = GetItemType() + "_HordeModeData";
        }
        ES3.Save(key, dataToSave);

        itemStatusChanged = false;
    }

    // --- RESET ---
    public void ResetHordeItemStatus_Batch() {
        var key = GetItemType() + "_HordeModeData";
        ES3.DeleteKey(key);

        //if (!isBoughtAtStart) {
        //    itemBought = false;
        //}

        //itemLevel = 0;

        //if(!isUnlockedAtStart) {
        //    itemUnlocked = false;
        //}

        //SaveItemStatus_Batch();
    }

    private void OnDestroy() {
        if(HUBManager.Instance != null) {
            UICurrencyManager.HubInventoryUI.OnCurrencyCollected -= HubInventoryUI_OnCurrencyCollected;
        } else {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= HubInventoryUI_OnCurrencyCollected;
        }
 
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
