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
    [SerializeField] protected bool itemsBoughtAreLocked = true;

    public event EventHandler OnNewItemHovered;
    public event EventHandler OnDescriptionPanelOpened;
    public event EventHandler OnPlayerBoughtMinorItem;
    public event EventHandler OnPlayerBoughtMajorItem;

    protected MerchantItem selectedMerchantItem;
    protected int selectedItemIndex;
    protected int previousSelectedItemIndex;


    private List<MerchantItemUI> smallMerchantItemUIList = new List<MerchantItemUI>();
    private List<MerchantItemUI> bigMerchantItemUIList = new List<MerchantItemUI>();

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
        payCurrencyUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
        ShowItemsToSale(false);
    }

    protected void Merchant_OnPlayerStartedInteractedWithMerchant(object sender, System.EventArgs e) {
        ShowItemsToSale(true);
        SelectFirstAvailableItem();
        UpdateSelectedItemUI();
        UpdateDescriptionPanelVisuals();
    }

    protected void Merchant_OnStructurePrimaryFunctionUsed(object sender, System.EventArgs e) {
        RefreshMerchantItemsUI();
    }

    protected void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!merchant.GetShopOpen()) return;
        NavigateUIItems();
    }

    protected void NavigateUIItems() {
        float selectDir = GameInput.Instance.GetMovementFloatNormalized();

        // Compteur pour limiter les essais et éviter les boucles infinies
        int attempts = 0;

        if (selectDir != 0) {
            // Naviguer à travers tous les items
            List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();
            int itemCount = allItems.Count;

            do {
                if (selectDir > 0) {
                    selectedItemIndex = (selectedItemIndex + 1) % itemCount;
                }
                else {
                    selectedItemIndex = (selectedItemIndex - 1 + itemCount) % itemCount;
                }

                attempts++;

                if (attempts >= itemCount) {
                    Debug.LogWarning("All items are purchased! Navigation aborted.");
                    HideItemUI();
                    return;
                }

            } while (allItems[selectedItemIndex].isPurchased && allItems[selectedItemIndex].buyingLocksPurchasesUntilRefresh);

            OnNewItemHovered?.Invoke(this, EventArgs.Empty);

            UpdateSelectedItemUI();
            UpdateDescriptionPanelVisuals();
            previousSelectedItemIndex = selectedItemIndex;
        }
    }

    private void SelectNextAvailableItem() {
        List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();
        int itemCount = allItems.Count;

        int attempts = 0;

        do {
            selectedItemIndex = (selectedItemIndex + 1) % itemCount;

            if (!allItems[selectedItemIndex].isPurchased) {
                UpdateSelectedItemUI(); // Met à jour l'UI avec le prochain item valide
                UpdateDescriptionPanelVisuals();
                previousSelectedItemIndex = selectedItemIndex;
                return;
            }

            attempts++;
        } while (attempts < itemCount);

        // Si tous les items sont achetés
        Debug.LogWarning("All items are purchased! No valid item to select.");
        HideItemUI();

    }

    protected void SelectFirstAvailableItem() {
        List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();
        int itemCount = allItems.Count;

        // Commence toujours à l'index 0 pour chercher le premier item valide
        int attempts = 0;
        selectedItemIndex = 0;

        do {
            if (!allItems[selectedItemIndex].isPurchased) {
                UpdateSelectedItemUI(); // Met à jour l'UI avec le premier item valide
                UpdateDescriptionPanelVisuals();
                previousSelectedItemIndex = selectedItemIndex;
                return;
            }

            selectedItemIndex = (selectedItemIndex + 1) % itemCount; // Passe à l'item suivant
            attempts++;
        } while (attempts < itemCount);

        // Si tous les items sont achetés
        Debug.LogWarning("All items are purchased! No valid item to select.");
        HideItemUI();
    }

    protected void UpdateSelectedItemUI() {
        // Ignore template dans les listes
        int majorItemsCount = bigMerchantItemUIList.Count;
        int minorItemsCount = smallMerchantItemUIList.Count;

        // Initialisation de l'item sélectionné
        selectedMerchantItem = null;

        // Parcourir les items majeurs
        for (int i = 0; i < majorItemsCount; i++) {
            MerchantItemUI merchantItemUI = bigMerchantItemUIList[i];

            if (i == selectedItemIndex) {
                merchantItemUI.HighlightItem(true);
                payCurrencyUI.SetOrbTemplateUIList(merchantItemUI.GetPayCurrencyTemplateWorldUIList());
                selectedMerchantItem = merchantItemUI.GetMerchantItemLinked();
            }
            else {
                merchantItemUI.HighlightItem(false);
            }
        }

        // Parcourir les items mineurs
        for (int i = 0; i < minorItemsCount; i++) {
            MerchantItemUI merchantItemUI = smallMerchantItemUIList[i];

            // L'index global commence après les items majeurs
            if (i + majorItemsCount == selectedItemIndex) {
                merchantItemUI.HighlightItem(true);
                payCurrencyUI.SetOrbTemplateUIList(merchantItemUI.GetPayCurrencyTemplateWorldUIList());
                selectedMerchantItem = merchantItemUI.GetMerchantItemLinked();
            }
            else {
                merchantItemUI.HighlightItem(false);
            }
        }

        // Vérifie si un item valide est trouvé
        if (selectedMerchantItem != null) {
            merchant.SetCurrentHoveredItem(selectedMerchantItem);
            merchant.SetCurrentSelectedItemPurchased(selectedMerchantItem.isPurchased);
        }
        else {
            Debug.LogWarning("UpdateSelectedItemUI: No valid item found for selection!");
        }
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

    private void HideItemUI() {
        merchantDescriptionPanelUI.ClosePanel();
        int majorItemsCount = bigMerchantItemUIContainer.childCount - 1;
        int minorItemsCount = smallMerchantItemUIContainer.childCount - 1;

        // Parcourir les items majeurs
        for (int i = 1; i <= majorItemsCount; i++) {
            MerchantItemUI merchantItemUI = bigMerchantItemUIContainer.GetChild(i).GetComponent<MerchantItemUI>();
            merchantItemUI.HighlightItem(false);
        }

        // Parcourir les items mineurs
        for (int i = 1; i <= minorItemsCount; i++) {
            MerchantItemUI merchantItemUI = smallMerchantItemUIContainer.GetChild(i).GetComponent<MerchantItemUI>();
            merchantItemUI.HighlightItem(false);
        }
    }

    private void Merchant_OnPlayerBoughtItem1(object sender, Merchant.OnPlayerBoughtItemEventArgs e) {

        if(e.boughtItem.itemType == MerchantItem.MerchantItemType.ActiveSkill || e.boughtItem.itemType == MerchantItem.MerchantItemType.Trap) {
            OnPlayerBoughtMajorItem?.Invoke(this, EventArgs.Empty);
        }

        if (e.boughtItem.itemType == MerchantItem.MerchantItemType.PassiveSkill || e.boughtItem.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
            OnPlayerBoughtMinorItem?.Invoke(this, EventArgs.Empty);
        }

        MerchantItemUI merchantItemUI = FindMerchantItemUI(e.boughtItem);
        merchantItemUI.PurchaseItem();
        merchant.SetItemSold(merchantItemUI.GetMerchantItemLinked());

        if(e.boughtItem.itemType != MerchantItem.MerchantItemType.Trap) {
            SelectNextAvailableItem();
        }
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
        bigMerchantItemUIList.Clear();
        smallMerchantItemUIList.Clear();

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
                MerchantItemUI majorMerchantItemUI = Instantiate(bigMerchantItemUITemplate, bigMerchantItemUIContainer).GetComponent<MerchantItemUI>();
                majorMerchantItemUI.SetLinkedItem(merchantItem);
                bigMerchantItemUIList.Add(majorMerchantItemUI);
            }
            bigMerchantItemUITemplate.gameObject.SetActive(false);


            List<MerchantItem> minorItemList = merchant.GetMinorItemListForSale();
            smallMerchantItemUITemplate.gameObject.SetActive(true);
            foreach (MerchantItem merchantItem in minorItemList) {
                MerchantItemUI minorMerchantItemUI = Instantiate(smallMerchantItemUITemplate, smallMerchantItemUIContainer).GetComponent<MerchantItemUI>();
                minorMerchantItemUI.SetLinkedItem(merchantItem);
                smallMerchantItemUIList.Add(minorMerchantItemUI);
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
    }


}
