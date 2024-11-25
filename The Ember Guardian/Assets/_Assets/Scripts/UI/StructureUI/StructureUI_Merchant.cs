using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI_Merchant : StructureUI {

    private Merchant merchant;
    [SerializeField] protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected Animator merchantUIAnimator;

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
        merchant.OnPlayerBoughtItem += Merchant_OnPlayerBoughtItem;
        GameInput.Instance.OnPlayerLeftRightDirPerformed += GameInput_OnPlayerLeftRightDirPerformed;
    }

    private void Merchant_OnPlayerStoppedInteractedWithMerchant(object sender, System.EventArgs e) {
        UpdateSelectedItemUI();

        selectedItemIndex = 0;
        UpdateSelectedItemUI();
        ShowItemsToSale(false);
    }

    private void Merchant_OnPlayerStartedInteractedWithMerchant(object sender, System.EventArgs e) {
        ShowItemsToSale(true);

        selectedItemIndex = 0;
        UpdateSelectedItemUI();
        UpdateDescriptionPanelVisuals();
    }

    private void Merchant_OnStructurePrimaryFunctionUsed(object sender, System.EventArgs e) {
        RefreshMerchantItemsUI();
        ShowItemsToSale(true);
        UpdateDescriptionPanelVisuals();
    }

    private void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!merchant.GetShopOpen()) return;
        NavigateUIItems();
    }

    private void NavigateUIItems() {
        float selectDir = GameInput.Instance.GetMovementFloatNormalized();

        if (selectDir != 0) {
            // Naviguer à travers tous les items
            List<MerchantItem> allItems = merchant.GetAllItemsForSale();
            int itemCount = allItems.Count;

            if (selectDir > 0) {
                // Naviguer à droite
                selectedItemIndex = (selectedItemIndex + 1) % itemCount;
            }
            else {
                // Naviguer à gauche
                selectedItemIndex = (selectedItemIndex - 1 + itemCount) % itemCount;
            }
            OnNewItemHovered?.Invoke(this, EventArgs.Empty);

            UpdateSelectedItemUI();
            UpdateDescriptionPanelVisuals();
            previousSelectedItemIndex = selectedItemIndex;
        }
    }

    private void UpdateSelectedItemUI() {
        List<MerchantItem> allItems = merchant.GetAllItemsForSale();

        for (int i = 0; i < allItems.Count; i++) {
            MerchantItemUI merchantItemUI = bigMerchantItemUITemplate.GetComponent<MerchantItemUI>();

            if (i != 0) {
                merchantItemUI = smallMerchantItemUIContainer.GetChild(i).GetComponent<MerchantItemUI>();
            }

            // Appliquer la mise en surbrillance de l'élément sélectionné
            if(i == selectedItemIndex) {

                merchantItemUI.HighlightItem(true);
                payCurrencyUI.SetOrbTemplateUIList(merchantItemUI.GetPayCurrencyTemplateWorldUIList());
                selectedMerchantItem = merchantItemUI.GetMerchantItemLinked();

            } else {

                merchantItemUI.HighlightItem(false);

            }
        }

        merchant.SetCurrentSelectedItemPurchased(selectedMerchantItem.isPurchased);
    }

    private void UpdateDescriptionPanelVisuals() {
        //Check if we switched from big item to small item
        if(useMajorMinorDistinction) {
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

    private void Merchant_OnPlayerBoughtItem(object sender, System.EventArgs e) {
        MerchantItemUI merchantItemUI = bigMerchantItemUITemplate.GetComponent<MerchantItemUI>();

        if (selectedItemIndex != 0) {
            merchantItemUI = smallMerchantItemUIContainer.GetChild(selectedItemIndex).GetComponent<MerchantItemUI>();
            OnPlayerBoughtMajorItem?.Invoke(this, EventArgs.Empty);
        } else {
            OnPlayerBoughtMinorItem?.Invoke(this, EventArgs.Empty);
        }

        merchantItemUI.PurchaseItem();
    }

    private void RefreshMerchantItemsUI() {
        Debug.Log("RefreshMerchantItemsUI");

        if (useMajorMinorDistinction) {
            // Affichage avec distinction
            MerchantItem majorItem = merchant.GetMajorItemForSale();
            bigMerchantItemUITemplate.GetComponent<MerchantItemUI>().SetLinkedItem(majorItem);

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

    private void ShowItemsToSale(bool show) {

        Debug.Log("show merchant items to sale");

        if(show) {
            merchantUIAnimator.SetTrigger("Show");
        } else {
            merchantUIAnimator.SetTrigger("Hide");
        }

        UpdateSelectedItemUI();
    }

}
