using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI_Merchant : StructureUI {

    protected Merchant merchant;
    [SerializeField] protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected Animator merchantUIAnimator;

    [SerializeField] protected RectTransform allMerchantItemUIContainer;
    [SerializeField] protected RectTransform bigMerchantItemUIContainer;
    [SerializeField] protected RectTransform bigMerchantItemUITemplate;
    [SerializeField] protected RectTransform smallMerchantItemUITemplate;
    [SerializeField] protected RectTransform smallMerchantItemUIContainer;

    [SerializeField] protected MerchantDescriptionPanelUI merchantDescriptionPanelUI;
    [SerializeField] protected RectTransform descriptionPanelLeftPosition;
    [SerializeField] protected RectTransform descriptionPanelRightPosition;

    [SerializeField] protected bool useMajorMinorDistinction = true;

    public event EventHandler OnNewItemHovered;
    public event EventHandler OnDescriptionPanelOpened;
    public event EventHandler OnPlayerBoughtMinorItem;
    public event EventHandler OnPlayerBoughtMajorItem;

    protected MerchantItem selectedMerchantItem;
    protected int selectedItemIndex;
    protected int previousSelectedItemIndex;

    protected override void Awake() {
        base.Awake();
        merchant = GetComponentInParent<Merchant>();
        merchantUIAnimator.GetComponent<CanvasGroup>().alpha = 0;
    }

    protected override void Start() {
        base.Start();
        merchant.OnStructurePrimaryFunctionUsed += Merchant_OnStructurePrimaryFunctionUsed;
        merchant.OnPlayerOpenedMerchantShop += Merchant_OnPlayerStartedInteractedWithMerchant;
        merchant.OnPlayerClosedMerchantShop += Merchant_OnPlayerStoppedInteractedWithMerchant;
        merchant.OnPlayerBoughtItem += Merchant_OnPlayerBoughtItem1;
        GameInput.Instance.OnPlayerLeftRightDirPerformed += GameInput_OnPlayerLeftRightDirPerformed;
    }


    protected void Merchant_OnPlayerStoppedInteractedWithMerchant(object sender, System.EventArgs e) {
        selectedItemIndex = 0;
        UpdateSelectedItemUI();
        ShowItemsToSale(false);
    }

    protected void Merchant_OnPlayerStartedInteractedWithMerchant(object sender, System.EventArgs e) {
        ShowItemsToSale(true);

        selectedItemIndex = 0;
        UpdateSelectedItemUI();
        UpdateDescriptionPanelVisuals();
    }

    protected void Merchant_OnStructurePrimaryFunctionUsed(object sender, System.EventArgs e) {
        RefreshMerchantItemsUI();
        ShowItemsToSale(true);
        UpdateDescriptionPanelVisuals();
    }

    protected void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!merchant.GetShopOpen()) return;
        NavigateUIItems();
    }

    protected void NavigateUIItems() {
        float selectDir = GameInput.Instance.GetMovementFloatNormalized();

        if (selectDir != 0) {
            // Naviguer à travers tous les items
            List<MerchantItem> allItems = merchant.GetAllItemsForSale();
            int itemCount = allItems.Count;

            do {
                if (selectDir > 0) {
                    selectedItemIndex = (selectedItemIndex + 1) % itemCount;
                }
                else {
                    selectedItemIndex = (selectedItemIndex - 1 + itemCount) % itemCount;
                }
            } while (allItems[selectedItemIndex].isPurchased);

            OnNewItemHovered?.Invoke(this, EventArgs.Empty);

            UpdateSelectedItemUI();
            UpdateDescriptionPanelVisuals();
            previousSelectedItemIndex = selectedItemIndex;
        }
    }

    protected void UpdateSelectedItemUI() {
        //Ignore template
        int majorItemsCount = bigMerchantItemUIContainer.childCount -1;
        int minorItemsCount = smallMerchantItemUIContainer.childCount -1;

        // Parcourir les items majeurs
        for (int i = 1; i <= majorItemsCount; i++) {
            MerchantItemUI merchantItemUI = bigMerchantItemUIContainer.GetChild(i).GetComponent<MerchantItemUI>();

            if (i-1 == selectedItemIndex) {
                merchantItemUI.HighlightItem(true);
                payCurrencyUI.SetOrbTemplateUIList(merchantItemUI.GetPayCurrencyTemplateWorldUIList());
                selectedMerchantItem = merchantItemUI.GetMerchantItemLinked();
            }
            else {
                merchantItemUI.HighlightItem(false);
            }
        }

        // Parcourir les items mineurs
        for (int i = 1; i <= minorItemsCount; i++) {
            MerchantItemUI merchantItemUI = smallMerchantItemUIContainer.GetChild(i).GetComponent<MerchantItemUI>();

            // L'index global commence après les items majeurs
            if (i-1 + majorItemsCount == selectedItemIndex) {
                merchantItemUI.HighlightItem(true);
                payCurrencyUI.SetOrbTemplateUIList(merchantItemUI.GetPayCurrencyTemplateWorldUIList());
                selectedMerchantItem = merchantItemUI.GetMerchantItemLinked();
            }
            else {
                merchantItemUI.HighlightItem(false);
            }
        }

        // Met à jour l'état de l'item sélectionné
        merchant.SetCurrentHoveredItem(selectedMerchantItem);
        merchant.SetCurrentSelectedItemPurchased(selectedMerchantItem.isPurchased);
    }

    protected void UpdateDescriptionPanelVisuals() {
        //Check if we switched from big item to small item
        if(useMajorMinorDistinction) {

            if(selectedItemIndex == 0 && previousSelectedItemIndex == 0) {
                // First time the player opens the panel
                merchantDescriptionPanelUI.OpenPanel();
                merchantDescriptionPanelUI.SetPanelPosition(descriptionPanelLeftPosition);
                OnDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);
            }

            if (selectedItemIndex != 0 && previousSelectedItemIndex == 0) {
                merchantDescriptionPanelUI.OpenPanel();
                merchantDescriptionPanelUI.SetPanelPosition(descriptionPanelRightPosition);
                OnDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);
            }

            if (selectedItemIndex == 0 && previousSelectedItemIndex != 0) {
                merchantDescriptionPanelUI.OpenPanel();
                merchantDescriptionPanelUI.SetPanelPosition(descriptionPanelLeftPosition);
                OnDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);
            }

        }

        merchantDescriptionPanelUI.UpdateDescriptionPanelVisuals(selectedMerchantItem);

    }

    private void Merchant_OnPlayerBoughtItem1(object sender, Merchant.OnPlayerBoughtItemEventArgs e) {

        if(e.boughtItem.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
            OnPlayerBoughtMajorItem?.Invoke(this, EventArgs.Empty);
        }

        if (e.boughtItem.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
            OnPlayerBoughtMinorItem?.Invoke(this, EventArgs.Empty);
        }

        MerchantItemUI merchantItemUI = FindMerchantItemUI(e.boughtItem);
        merchantItemUI.PurchaseItem();
        merchant.SetItemSold(merchantItemUI.GetMerchantItemLinked());
    }

    private MerchantItemUI FindMerchantItemUI(MerchantItem merchantItem) {
        MerchantItemUI foundMerchantItemUI = null;

        MerchantItemUI[] minorMerchantItemUIArray = smallMerchantItemUIContainer.GetComponentsInChildren<MerchantItemUI>();
        foreach (MerchantItemUI merchantItemUI in minorMerchantItemUIArray) {

            if(merchantItemUI.GetMerchantItemLinked() ==  merchantItem) {
                foundMerchantItemUI = merchantItemUI;
            }
        }

        MerchantItemUI[] majorMerchantItemUIArray = bigMerchantItemUIContainer.GetComponentsInChildren<MerchantItemUI>();
        foreach (MerchantItemUI merchantItemUI in majorMerchantItemUIArray) {

            if (merchantItemUI.GetMerchantItemLinked() == merchantItem) {
                foundMerchantItemUI = merchantItemUI;
            }
        }

        return foundMerchantItemUI;
    }

    protected virtual void RefreshMerchantItemsUI() {
        foreach (RectTransform child in smallMerchantItemUIContainer) {
            if (child == smallMerchantItemUITemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (RectTransform child in bigMerchantItemUIContainer) {
            if (child == bigMerchantItemUITemplate) continue;
            Destroy(child.gameObject);
        }

        if (useMajorMinorDistinction) {

            // Affichage avec distinction
            List<MerchantItem> majorItemList = merchant.GetMajorItemListForSale();
            bigMerchantItemUITemplate.gameObject.SetActive(true);
            foreach (MerchantItem merchantItem in majorItemList) {
                MerchantItemUI minorMerchantItemUI = Instantiate(bigMerchantItemUITemplate, bigMerchantItemUIContainer).GetComponent<MerchantItemUI>();
                minorMerchantItemUI.SetLinkedItem(merchantItem);
            }
            bigMerchantItemUITemplate.gameObject.SetActive(false);


            List<MerchantItem> minorItemList = merchant.GetMinorItemListForSale();
            smallMerchantItemUITemplate.gameObject.SetActive(true);
            foreach (MerchantItem merchantItem in minorItemList) {
                MerchantItemUI minorMerchantItemUI = Instantiate(smallMerchantItemUITemplate, smallMerchantItemUIContainer).GetComponent<MerchantItemUI>();
                minorMerchantItemUI.SetLinkedItem(merchantItem);
            }
            smallMerchantItemUITemplate.gameObject.SetActive(false);
        }

        else {

            // Affichage uniforme
            List<MerchantItem> allItems = merchant.GetAllItemsForSale();
            for (int i = 0; i < allItems.Count; i++) {
                MerchantItemUI merchantItemUI;
                if (i == 0) {
                    merchantItemUI = bigMerchantItemUITemplate.GetComponent<MerchantItemUI>();
                }
                else {
                    merchantItemUI = Instantiate(smallMerchantItemUITemplate, smallMerchantItemUIContainer).GetComponent<MerchantItemUI>();
                }
                merchantItemUI.SetLinkedItem(allItems[i]);
            }

        }
    }

    protected void ShowItemsToSale(bool show) {

        if(show) {
            merchantUIAnimator.SetTrigger("Show");
        } else {
            merchantUIAnimator.SetTrigger("Hide");
        }

        UpdateSelectedItemUI();
    }


}
