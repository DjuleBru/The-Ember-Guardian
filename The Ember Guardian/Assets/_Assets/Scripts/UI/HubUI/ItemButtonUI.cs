using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using Plugins.Animate_UI_Materials;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButtonUI : ButtonUI {

    [SerializeField] private HubMerchant parentHubMerchant;
    [SerializeField] private List<ItemButtonUI> lockingItemButtonUIList;
    [SerializeField] private ItemDescriptionCardUI descriptionCard;
    [SerializeField] private GameObject newUnlockedItemGO;
    private List<ItemButtonUI> initialLockingItemButtonUIList = new List<ItemButtonUI>();

    [SerializeField] private Color outlineUnlockedBuyableColor;
    [SerializeField] private Color outlineUnlockedButNotBuyableColor;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject lockedFromOtherMerchantImage;
    [SerializeField] private ItemButtonUI_Visual itemButtonUI_Visual;
    [SerializeField] private Color boughtOutlineColor;
    [SerializeField] private Color boughtBackgroundColor;
    [SerializeField] private List<Image> outputLinkUnlockedImageList;
    [SerializeField] private GameObject itemLevelBackgroundGameObject;
    [SerializeField] private TextMeshProUGUI itemMaxedLevelText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemMaxLevelText;
    [SerializeField] private TextMeshProUGUI buyItemFromOtherMerchantText;
    [SerializeField] private Animator buyItemFromOtherMerchantTextAnimator;

    [SerializeField] private Sprite outlineImageBoughtSprite;
    [SerializeField] private Sprite defaultOutlineSprite;
    [SerializeField] private Sprite itemEquippedOutlineSprite;
    [SerializeField] private Sprite itemMaxedOutlineSprite;

    [SerializeField] private bool showItemLevel;
    [SerializeField] private bool hideItemMaxLevel;
    [SerializeField] private bool lockHoverInteractions;
    [SerializeField] private bool itemLockedInDemo;
    [SerializeField] private bool hideItemIconUntilUnlocked;
    [SerializeField] private bool itemUnlockableOnlyInLevel;

    [SerializeField] private List<ItemButtonUI> itemsToForceUnlockWhenBought = new List<ItemButtonUI>();
    [SerializeField] private ItemButtonUI_ChildTreeShowHide treeShowHide;
    [SerializeField] private bool isTreeParent;
    [SerializeField] private bool isTreeChild;

    private HubMerchantItem hubMerchantItem;
    private Button button;

    private bool itemSelected;
    private bool itemHovered;

    public static event EventHandler OnAnyOutputLinkUnlocked;
    public static event EventHandler OnAnyLockedButtonTryPress;
    public static event EventHandler OnAnyHubMerchantItemFailedBuy;
    public static event EventHandler OnAnyHubMerchantItemRefunded;
    public event EventHandler OnHubMerchantItemRefunded;
    public static event EventHandler OnAnyHubMerchantItemTryBuyMaxedItem;

    public void InitializeItemButtonUI() {
        button = GetComponent<Button>();
        hubMerchantItem = GetComponent<HubMerchantItem>();

        foreach (ItemButtonUI buttonUI in lockingItemButtonUIList) {
            initialLockingItemButtonUIList.Add(buttonUI);
        }

        if(newUnlockedItemGO != null ) {
            newUnlockedItemGO.SetActive(false);
        }

        iconImage.material = new Material(iconImage.material);
        outlineImage.material = new Material(outlineImage.material);

        backgroundImage.color = Color.black;

        if (!showItemLevel) {
            itemLevelBackgroundGameObject.SetActive(false);
        }

        OnAnyOutputLinkUnlocked += ItemButtonUI_OnAnyOutputLinkUnlocked;
        hubMerchantItem.OnHubMerchantItemBought += HubMerchantItem_OnHubMerchantItemBought;
        hubMerchantItem.OnHubMerchantItemLoaded += HubMerchantItem_OnHubMerchantItemLoaded;
        hubMerchantItem.OnHubMerchantItemUpgraded += HubMerchantItem_OnHubMerchantItemUpgraded;
        hubMerchantItem.OnItemMustRefreshDescriptionCard += HubMerchantItem_OnItemMustRefreshDescriptionCard;

        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;

        if (hubMerchantItem is HUBMerchantItem_GunMerchantItem) {
            HUBMerchantItem_GunMerchantItem gunItem = hubMerchantItem as HUBMerchantItem_GunMerchantItem;
            if(gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.newGun) {
                GameInput.Instance.OnRefundGunPerformed += GameInput_OnRefundGunPerformed;
            }  
        }

    }

    protected override void Start() {
        base.Start();
        UICurrencyManager.HubInventoryUI.OnCurrencyRemovedFromBag += HubInventoryUI_OnCurrencyRemovedFromBag;
        UICurrencyManager.HubInventoryUI.OnCurrencyCollected += HubInventoryUI_OnCurrencyCollected;

        if(DebugManager.Instance.GetAllItemsUnlockedInDemo()) {
            itemLockedInDemo = false;
            lockHoverInteractions = false;
            itemUnlockableOnlyInLevel = false;
        }

        TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();
        itemMaxedLevelText.font = font;
        itemLevelText.font = font;
        itemMaxLevelText.font = font;
        buyItemFromOtherMerchantText.font = font;

        RefreshItemStatusVisuals();
    }

    private void HubInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        RefreshItemStatusVisuals();
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {
        itemHovered = false;
        itemSelected = false;
    }

    private void GameInput_OnRefundGunPerformed(object sender, EventArgs e) {
        // ONLY SUBBED FOR GUNS
        bool isUsingGamepad = GameInput.Instance.IsUsingGamepad();

        if (!itemHovered && !isUsingGamepad) return;
        if (!itemSelected && isUsingGamepad) return;
        if (!GetItemRefundable()) return;

        int redGemsToRefund = GetTotalRedGunGemsToRefund();
        RefundGunItems();

        ResetMajorGunItemAfterRefund();

        UICurrencyManager.HubInventoryUI.AddCurrencyAmount(PlayerCurrencies.CurrencyType.redGem, redGemsToRefund, .15f);
    }

    public void ResetMajorGunItemAfterRefund() {
        if (!hubMerchantItem.GetItemBought()) return;

        HUBMerchantItem_GunMerchantItem gunItem = hubMerchantItem as HUBMerchantItem_GunMerchantItem;

        if(gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.newGun) {
            StartBuyItemVisuals();
        }


        if (outputLinkUnlockedImageList.Count != 0) {
            foreach (Image image in outputLinkUnlockedImageList) {
                image.gameObject.SetActive(true);
                StartCoroutine(UnlockOutputLink(image));
            }
        }

    }

    public bool GetItemRefundable() {
        if (!hubMerchantItem.GetItemBought()) return false;
        int redGemsToRefund = GetTotalRedGunGemsToRefund();
        if (redGemsToRefund == 0) return false;

        return true;
    }

    public int GetTotalRedGunGemsToRefund() {
        int totalGems = 0;

        HubMerchantItem[] childItems = treeShowHide.GetComponentsInChildren<HubMerchantItem>();
        foreach (HubMerchantItem item in childItems) {
            HUBMerchantItem_GunMerchantItem gunItem = item as HUBMerchantItem_GunMerchantItem;
            if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.gunAbility) continue;

            totalGems += item.GetRedGemsPaid();
        }

        return totalGems;
    }

    private void RefundGunItems() {
        HubMerchantItem[] childItems = treeShowHide.GetComponentsInChildren<HubMerchantItem>();
        foreach(HubMerchantItem item in childItems) {

            HUBMerchantItem_GunMerchantItem gunItem = item as HUBMerchantItem_GunMerchantItem;
            if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.gunAbility) continue;

            item.GetComponent<ItemButtonUI>().ResetItemStatusVisuals();
        }

        foreach (HubMerchantItem item in childItems) {

            HUBMerchantItem_GunMerchantItem gunItem = item as HUBMerchantItem_GunMerchantItem;
            if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.gunAbility) continue;

            item.ResetGunItemStatus();
        }

        foreach (HubMerchantItem item in childItems) {

            HUBMerchantItem_GunMerchantItem gunItem = item as HUBMerchantItem_GunMerchantItem;
            if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.gunAbility) {
                gunItem.GetComponent<ItemButtonUI>().ResetMajorGunItemAfterRefund();
            };
        }

        hubMerchantItem.ResetGunItemStatus();
        OnAnyHubMerchantItemRefunded?.Invoke(this, EventArgs.Empty);
        OnHubMerchantItemRefunded?.Invoke(this, EventArgs.Empty);
    }


    private void HubInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        RefreshItemStatusVisuals();
    }

    private void HubMerchantItem_OnItemMustRefreshDescriptionCard(object sender, EventArgs e) {
        RefreshDescriptionCard();
        RefreshItemLevelUI();
    }

    private void HubMerchantItem_OnHubMerchantItemBought(object sender, EventArgs e) {
        RefreshDescriptionCard(); 
        
        if (hubMerchantItem.GetItemUpgradeable()) {
            RefreshItemLevelUI();
        }
    }

    private void HubMerchantItem_OnHubMerchantItemUpgraded(object sender, EventArgs e) {
        if (hubMerchantItem.GetItemUpgradeable() && hubMerchantItem.GetItemMaxed()) {
            outlineImage.sprite = itemMaxedOutlineSprite;
        }
        else {
            outlineImage.sprite = outlineImageBoughtSprite;
        }

        StartUpgradeItemAnimation();
        RefreshItemLevelUI();
        RefreshDescriptionCard();
    }

    private void HubMerchantItem_OnHubMerchantItemLoaded(object sender, EventArgs e) {
        if (hubMerchantItem.GetItemUnlocked()) {
            SetItemUnlocked();
            if (hubMerchantItem.GetItemBought()) {
                SetOutputLinksBought();
            }
        }

        //Debug.Log("Loaded " + hubMerchantItem.GetItemType() + " GetNewItemUnlocked " + hubMerchantItem.GetNewItemUnlocked() + " GetItemUnlocked " + hubMerchantItem.GetItemUnlocked());
        if (hubMerchantItem.GetNewItemUnlocked() && hubMerchantItem.GetItemUnlocked()) {
            newUnlockedItemGO.SetActive(true);
        }

        // Check if it force unlocks another item :
        if(itemsToForceUnlockWhenBought.Count != 0) {
            if(hubMerchantItem.GetItemBought()) {
                foreach (ItemButtonUI itemButtonUI in itemsToForceUnlockWhenBought) {
                    itemButtonUI.SetItemUnlocked();
                    itemButtonUI.SetItemBought();
                }
            }
        }

        RefreshItemStatusVisuals();
        RefreshItemLevelUI();
    }

    private void RefreshDescriptionCard() {

        string itemName = hubMerchantItem.GetItemName();
        string itemDescription = hubMerchantItem.GetDescription();

        List<string> itemStatDescription = hubMerchantItem.GetStatDescription();
        List<string> itemStatValues = hubMerchantItem.GetStatValues();
        List<bool> itemStatModifierValues = hubMerchantItem.GetStatModifierBools();
        bool constantUnlockDescription = hubMerchantItem.GetConstantUnlockDescription();

        int greenGemCost = hubMerchantItem.GetGreenGemCost();
        int redGemCost = hubMerchantItem.GetRedGemCost();
        int blueGemCost = hubMerchantItem.GetBlueGemCost();
        int yellowGemCost = hubMerchantItem.GetYellowGemCost();
        int purpleGemCost = hubMerchantItem.GetPurpleGemCost();
        int cyanGemCost = hubMerchantItem.GetCyanGemCost();

        descriptionCard.SetDescriptionCardFonts();
        descriptionCard.SetDescriptionCardText(itemName, constantUnlockDescription, itemStatDescription, itemDescription, itemStatValues, itemStatModifierValues);
        descriptionCard.SetDescriptionCardCost(greenGemCost, redGemCost, blueGemCost, yellowGemCost, purpleGemCost, cyanGemCost);
        descriptionCard.ChestDescriptionCardFree(greenGemCost, redGemCost, blueGemCost, yellowGemCost, purpleGemCost, cyanGemCost);

        if(itemLockedInDemo && HUBManager.Instance.GetIsDemo()) {
            descriptionCard.SetDescriptionCardItemLockedInDemo();
            return;
        }

        if (!hubMerchantItem.GetItemBought() && itemUnlockableOnlyInLevel) {
            descriptionCard.SetDescriptionCardItemUnlockableOnlyInLevel();
            return;
        }

        if (!hubMerchantItem.GetItemUpgradeable()) {
            
            if (hubMerchantItem.GetItemBought()) {
                descriptionCard.SetDescriptionCardBought();
                return;
            }

        } else {

            if (hubMerchantItem.GetItemLevel() == hubMerchantItem.GetMaxItemLevel()) {
                descriptionCard.SetDescriptionCardMaxlevel();
            }
        }

    }

    private void RefreshDescriptionCardCosts() {
        if (hubMerchantItem.GetItemBought()) return;
        if (itemLockedInDemo) return;
        int greenGemCost = hubMerchantItem.GetGreenGemCost();
        int redGemCost = hubMerchantItem.GetRedGemCost();
        int blueGemCost = hubMerchantItem.GetBlueGemCost();
        int yellowGemCost = hubMerchantItem.GetYellowGemCost();
        int purpleGemCost = hubMerchantItem.GetPurpleGemCost();
        int cyanGemCost = hubMerchantItem.GetCyanGemCost();

        descriptionCard.SetDescriptionCardCost(greenGemCost, redGemCost, blueGemCost, yellowGemCost, purpleGemCost, cyanGemCost);
    }

    private void ItemButtonUI_OnAnyOutputLinkUnlocked(object sender, EventArgs e) {
        ItemButtonUI itemButtonUI = (ItemButtonUI)sender;

        if (lockingItemButtonUIList.Contains(itemButtonUI)) {
            SetLockingItemBought(itemButtonUI);
            CheckNewItemUnlocked(itemButtonUI);
            RefreshItemStatusVisuals();
        }
    }

    public void BuyItem() {
        if (!hubMerchantItem.GetItemUnlocked() || (itemLockedInDemo && HUBManager.Instance.GetIsDemo()) || (!hubMerchantItem.GetItemBought() && itemUnlockableOnlyInLevel)) {
            OnAnyLockedButtonTryPress?.Invoke(this, EventArgs.Empty);
            CheckItemLockedFromOtherMerchantItem();
            return;
        }

        if (hubMerchantItem.GetItemMaxed()) {
            // Fail buy
            OnAnyHubMerchantItemTryBuyMaxedItem?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (!hubMerchantItem.GetItemBought()) {
            // Item is not bought

            if (!hubMerchantItem.CanBuyItem()) {
                // Fail buy
                OnAnyHubMerchantItemFailedBuy?.Invoke(this, EventArgs.Empty);
                return;
            }

            hubMerchantItem.BuyItem();
            StartBuyItemVisuals();

            if (hubMerchantItem.GetItemUpgradeable()) {
                RefreshItemLevelUI();
            }

            if (outputLinkUnlockedImageList.Count != 0) {
                foreach(Image image in outputLinkUnlockedImageList) {
                    image.gameObject.SetActive(true);
                    StartCoroutine(UnlockOutputLink(image));
                }
            }

            return;
        }
        else {
            // Item is already bought

            if (hubMerchantItem.GetItemUpgradeable()) {
                // Upgrade

                if (!hubMerchantItem.CanBuyItem()) {
                    // Fail buy
                    OnAnyHubMerchantItemFailedBuy?.Invoke(this, EventArgs.Empty);
                    return;
                }

                hubMerchantItem.UpgradeItem();

            } else {
                hubMerchantItem.EquipOrUnequipItem();
            }

        }

    }

    private void CheckItemLockedFromOtherMerchantItem() {
        foreach(ItemButtonUI itemButtonUI in lockingItemButtonUIList) {
            HubMerchant lockingItemHubMerchantParent = itemButtonUI.GetHubMerchantParent();

            if (lockingItemHubMerchantParent != null && lockingItemHubMerchantParent != hubMerchantItem.GetHubMerchantParent()) {

                string itemNameLocalized = LocalizationManager.Instance.GetLocalizedText(itemButtonUI.GetHubMerchantItem().GetItemName());
                string hubMerchantParentLocalized = LocalizationManager.Instance.GetLocalizedText(lockingItemHubMerchantParent.GetHubMerchantName());

                buyItemFromOtherMerchantText.text = "Buy " + itemNameLocalized + " from " + hubMerchantParentLocalized + " first";
                buyItemFromOtherMerchantTextAnimator.SetTrigger("Show");
            }
        }
    }

    private bool ItemLockedFromOtherMerchantItem() {
        foreach (ItemButtonUI itemButtonUI in lockingItemButtonUIList) {
            HubMerchant lockingItemHubMerchantParent = itemButtonUI.GetHubMerchantParent();

            if (lockingItemHubMerchantParent != null && lockingItemHubMerchantParent != hubMerchantItem.GetHubMerchantParent()) {
                return true;
            }
        }
        return false;
    }

    private void StartBuyItemVisuals() {
        if (hubMerchantItem.GetItemUpgradeable() && hubMerchantItem.GetItemMaxed()) {
            outlineImage.sprite = itemMaxedOutlineSprite;
        }
        else {
            outlineImage.sprite = outlineImageBoughtSprite;
        }
        outlineImage.color = boughtOutlineColor;
        backgroundImage.color = boughtBackgroundColor;

        iconImage.material = new Material(iconImage.material);
        iconImage.material.SetFloat("_GreyscaleBlend", 0);
        itemButtonUI_Visual.StartBuyAnimation(hubMerchantItem.GetRedGemCost(), hubMerchantItem.GetGreenGemCost(), hubMerchantItem.GetBlueGemCost(), hubMerchantItem.GetYellowGemCost(), hubMerchantItem.GetPurpleGemCost(), hubMerchantItem.GetCyanGemCost());
    }

    private void SetItemBoughtVisuals() {
        if (hubMerchantItem.GetItemUpgradeable() && hubMerchantItem.GetItemMaxed()) {
            outlineImage.sprite = itemMaxedOutlineSprite;
        } else {
            outlineImage.sprite = outlineImageBoughtSprite;
        }

        outlineImage.color = boughtOutlineColor;
        backgroundImage.color = boughtBackgroundColor;
        iconImage.material = new Material(iconImage.material);
        iconImage.material.SetFloat("_GreyscaleBlend", 0);

        itemButtonUI_Visual.SetItemLoadedBought();
    }

    private void SetOutputLinksBought() {
        foreach (Image image in outputLinkUnlockedImageList) {
            image.gameObject.SetActive(true);
            image.color = boughtOutlineColor;
            image.fillAmount = 1f;

            OnAnyOutputLinkUnlocked?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RefreshItemLevelUI() {
        if(showItemLevel) {

            itemLevelBackgroundGameObject.SetActive(true);

        } else {
            if(!hubMerchantItem.GetItemBought()) {

                itemLevelBackgroundGameObject.SetActive(false);

            } else {
                // Item bought

                if(hubMerchantItem.GetItemUpgradeable()) {
                    itemLevelBackgroundGameObject.SetActive(true);
                }

            }

        }

        if (hubMerchantItem.GetItemUpgradeable() && hubMerchantItem.GetItemMaxed()) {
            itemMaxedLevelText.text = "MAX";
            itemLevelText.text = "";
            itemMaxLevelText.text = "";
        } else {
            if(hideItemMaxLevel) {
                itemMaxedLevelText.text = hubMerchantItem.GetItemLevel().ToString(); 
                itemLevelText.text = "";
                itemMaxLevelText.text = "";
            } else {
                if(itemMaxedLevelText != null) {
                    itemMaxedLevelText.text = "";
                    itemLevelText.text = hubMerchantItem.GetItemLevel().ToString();
                    itemMaxLevelText.text = "/" + hubMerchantItem.GetMaxItemLevel().ToString();
                }
            }
        }

        if(showItemLevel && hideItemMaxLevel) {
            SetItemLevelRomanNumber(hubMerchantItem.GetItemLevel());
        }
    }

    private void SetItemLevelRomanNumber(int level) {
        if(level == 1) {
            itemMaxedLevelText.text = "I";
        }
        if (level == 2) {
            itemMaxedLevelText.text = "II";
        }
        if (level == 3) {
            itemMaxedLevelText.text = "III";
        }
        if (level == 4) {
            itemMaxedLevelText.text = "IV";
        }

    }

    public void StartUpgradeItemAnimation() {
        itemButtonUI_Visual.StartBuyAnimation(hubMerchantItem.GetRedGemCost(), hubMerchantItem.GetGreenGemCost(), hubMerchantItem.GetBlueGemCost(), hubMerchantItem.GetYellowGemCost(), hubMerchantItem.GetPurpleGemCost(), hubMerchantItem.GetCyanGemCost());
    }

    public void SetLockingItemBought(ItemButtonUI itemButtonUI) {
        lockingItemButtonUIList.Remove(itemButtonUI);

        if (lockingItemButtonUIList.Count != 0 && hubMerchantItem.GetUnlockRequiresAllPrerequisites()) return;
        SetItemUnlocked();
    }

    private void CheckNewItemUnlocked(ItemButtonUI itemButtonUI) {
        bool hasLockedItemFromOtherMerchant = false;
        foreach(ItemButtonUI bttonUI in initialLockingItemButtonUIList) {
            if(itemButtonUI.GetHubMerchantParent() != GetHubMerchantParent()) {
                hasLockedItemFromOtherMerchant = true;
            }
        }

        if (!hasLockedItemFromOtherMerchant) return;

        //Debug.Log(itemButtonUI.GetHubMerchantParent());
        //Debug.Log(GetHubMerchantParent());

        if (itemButtonUI.GetHubMerchantParent() != GetHubMerchantParent()) {

            bool lockingItemBought = MetaProgressionManager.Instance.GetMerchantItemBought(itemButtonUI.GetHubMerchantItem().GetItemType());
            bool lockingItemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(itemButtonUI.GetHubMerchantItem().GetItemType());

            //Debug.Log(itemButtonUI.GetHubMerchantItem().GetItemType() + " lockingItemBought " + lockingItemBought);
            //Debug.Log(itemButtonUI.GetHubMerchantItem().GetItemType() + " lockingItemUnlocked " + lockingItemUnlocked);

            //if (!lockingItemBought && lockingItemUnlocked) {
            //    hubMerchantItem.SetNewItemUnlocked(true);
            //}; 
            
            if (!lockingItemBought) {
                hubMerchantItem.SetNewItemUnlocked(true);
            };
        }

        //Debug.Log(hubMerchantItem.GetItemType() + " hubMerchantItem.GetNewItemUnlocked() " + hubMerchantItem.GetNewItemUnlocked());
        if (hubMerchantItem.GetNewItemUnlocked()) {
            //Debug.Log(hubMerchantItem.GetItemType() + " lockingItemButtonUIList.Count " + lockingItemButtonUIList.Count);

            if (lockingItemButtonUIList.Count != 0 && hubMerchantItem.GetUnlockRequiresAllPrerequisites()) return;

            newUnlockedItemGO.SetActive(true);
            //Debug.Log(hubMerchantItem.GetItemType() + parentHubMerchant);
            parentHubMerchant.SetMerchantHasNewItems();
        }
    }

    public void SetItemUnlocked() {
        if (itemLockedInDemo && HUBManager.Instance.GetIsDemo()) return;
        if (hubMerchantItem.GetItemBought()) return;

        hubMerchantItem.UnlockItem();
        RefreshItemStatusVisuals();
    }

    public void SetItemBought() {
        if (itemLockedInDemo && HUBManager.Instance.GetIsDemo()) return;
        if (hubMerchantItem.GetItemBought()) return;

        hubMerchantItem.SetItemBought();
        hubMerchantItem.UnlockItem();
        RefreshItemStatusVisuals();
    }

    private void RefreshItemStatusVisuals() {
        if (!hubMerchantItem.GetItemUnlocked()) {

            if (ItemLockedFromOtherMerchantItem() || hideItemIconUntilUnlocked) {
                if (lockedFromOtherMerchantImage.gameObject != null) {
                    lockedFromOtherMerchantImage.gameObject.SetActive(true);
                }

                Color semiTransparentColor = Color.white;
                semiTransparentColor.a = .5f;
                iconImage.color = semiTransparentColor;

                lockHoverInteractions = true;
            }
            else {
                if (lockedFromOtherMerchantImage.gameObject != null) {
                    lockedFromOtherMerchantImage.gameObject.SetActive(false);
                }
                iconImage.color = Color.white;

                iconImage.gameObject.SetActive(true);

                if(!HUBManager.Instance.GetIsDemo()) {
                    lockHoverInteractions = false;
                }
            }

        }
        else {
            if(lockedFromOtherMerchantImage.gameObject != null) {
                lockedFromOtherMerchantImage.gameObject.SetActive(false);
            }
            iconImage.color = Color.white;
            iconImage.gameObject.SetActive(true);
            lockHoverInteractions = false;
        }

        if (!hubMerchantItem.GetItemUnlocked() || (itemLockedInDemo && HUBManager.Instance.GetIsDemo())) {
            outlineImage.color = Color.grey;
            return;
        }

        if ((!hubMerchantItem.GetItemBought() && itemUnlockableOnlyInLevel)) {
            outlineImage.color = Color.grey;
            return;
        }

        if (hubMerchantItem.GetItemBought()) {
            SetItemBoughtVisuals();
            return;
        }

        if (!hubMerchantItem.CanBuyItem()) {
            outlineImage.color = outlineUnlockedButNotBuyableColor;
            return;
        }
        else {
            outlineImage.color = outlineUnlockedBuyableColor;
        }

    }

    public void ResetItemStatusVisuals() {
        outlineImage.color = Color.grey;
        iconImage.material = new Material(iconImage.material);
        iconImage.material.SetFloat("_GreyscaleBlend", 1);
        backgroundImage.color = Color.black;
        itemButtonUI_Visual.DisableAnimator();

        foreach (Image image in outputLinkUnlockedImageList) {
            image.gameObject.SetActive(false);
            image.fillAmount = 0f;
        }

        lockingItemButtonUIList.Clear();
        foreach (ItemButtonUI itemButtonUI in initialLockingItemButtonUIList) {
            lockingItemButtonUIList.Add(itemButtonUI);
        }

    }

    private IEnumerator UnlockOutputLink(Image image) {
        float fillRate = .1f;
        image.color = boughtOutlineColor;
        image.fillAmount = 0f;

        while(image.fillAmount < .99f) {
            image.fillAmount += fillRate;
            yield return new WaitForSeconds(.01f);
        }

        OnAnyOutputLinkUnlocked?.Invoke(this, EventArgs.Empty);
        yield return null;
    }

    #region NAVIGATION
    protected override void ButtonUI_OnAnyButtonHovered(object sender, EventArgs e) {
        if (lockHoverInteractions) return;
        ItemButtonUI itemButtonUI = sender as ItemButtonUI;
        if (itemButtonUI == null) return;

        if (this == itemButtonUI) {
            itemHovered = true;
            RefreshDescriptionCardCosts();
            descriptionCard.gameObject.SetActive(true);

            if (hubMerchantItem.GetNewItemUnlocked() && hubMerchantItem.GetItemUnlocked()) {
                newUnlockedItemGO.SetActive(false);
                hubMerchantItem.SetNewItemUnlocked(false);
            }

            if (treeShowHide != null) {
                descriptionCard.transform.SetParent(treeShowHide.transform.parent);
            } else {
                descriptionCard.transform.SetParent(transform.parent);
            }


            descriptionCard.transform.SetAsLastSibling(); // Amène la carte au-dessus

            if(isTreeParent) {
                treeShowHide.ShowTree();
            }
            return;
        }

        if (this != itemButtonUI && itemHovered) {
            itemHovered = false;
            descriptionCard.gameObject.SetActive(false);
        }

        if(treeShowHide != null && this != itemButtonUI) {
            if (!itemButtonUI.GetIsTreeChild() && isTreeParent) {
                treeShowHide.HideTree();
            }

            if (itemButtonUI.GetIsTreeParent() && isTreeParent) {
                treeShowHide.HideTree();
            }
        }
    }

    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (lockHoverInteractions) return;
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ItemButtonUI itemButtonUI = sender as ItemButtonUI;

        if (this == itemButtonUI) {
            itemSelected = true;
            descriptionCard.gameObject.SetActive(true);

            if (hubMerchantItem.GetNewItemUnlocked() && hubMerchantItem.GetItemUnlocked()) {
                newUnlockedItemGO.SetActive(false);
                hubMerchantItem.SetNewItemUnlocked(false);
            }

            if (treeShowHide != null) {
                descriptionCard.transform.SetParent(treeShowHide.transform.parent);
            }
            else {
                descriptionCard.transform.SetParent(transform.parent);
            }

            if (isTreeParent) {
                treeShowHide.ShowTree();
            }
            return;
        }

        if (this != itemButtonUI && itemSelected) {
            itemSelected = false;
            descriptionCard.gameObject.SetActive(false);
        }

        if (treeShowHide != null && this != itemButtonUI) {
            if (!itemButtonUI.GetIsTreeChild() && isTreeParent) {
                treeShowHide.HideTree();
            }

            if (itemButtonUI.GetIsTreeParent() && isTreeParent) {
                treeShowHide.HideTree();
            }
        }
    }

    public override void OnPointerExit(PointerEventData eventData) {
        if (lockHoverInteractions) return;

        itemHovered = false;
        descriptionCard.gameObject.SetActive(false);
    }

    #endregion

    public HubMerchantItem GetHubMerchantItem() {
        hubMerchantItem = GetComponent<HubMerchantItem>();
        return hubMerchantItem;
    }
    public HubMerchant GetHubMerchantParent() {
        return parentHubMerchant;
    }
    public bool GetIsTreeChild() {
        return isTreeChild;
    }
    public bool GetIsTreeParent() {
        return isTreeParent;
    }

    public bool GetHideItemIconUntilUnlocked() {
        return hideItemIconUntilUnlocked;
    }

    public void SetParentHubMerchant(HubMerchant hubMerchant) {
        parentHubMerchant = hubMerchant;
    }

    public Vector2 GetLocalPosition() {
        if(isTreeChild) {
            return treeShowHide.GetComponent<RectTransform>().localPosition;
        } else {
            return GetComponent<RectTransform>().localPosition;
        }
    }
    protected override void OnDestroy() {
        base.OnDestroy();

        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;

        OnAnyOutputLinkUnlocked -= ItemButtonUI_OnAnyOutputLinkUnlocked;
        OnAnyButtonHovered -= ButtonUI_OnAnyButtonHovered;
        OnAnyButtonSelected -= ButtonUI_OnAnyButtonSelected;
    }
}
