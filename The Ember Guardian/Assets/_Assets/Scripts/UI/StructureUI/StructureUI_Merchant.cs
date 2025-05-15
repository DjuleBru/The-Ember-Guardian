using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

    private List<MerchantItem> columnLeft = new();
    private List<MerchantItem> columnRight = new();
    private int selectedCol = 0; // 0 = gauche, 1 = droite
    private int selectedRow = 0;

    protected MerchantItem selectedMerchantItem;
    protected int selectedItemIndex;
    protected Vector2Int selectedGridPos;
    protected int previousSelectedItemIndex;
    private int columns = 2;

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
        GameInput.Instance.OnPlayerNavigateUIPerformed += GameInput_OnPlayerNavigateUIPerformed;
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

    private void GameInput_OnPlayerNavigateUIPerformed(object sender, EventArgs e) {
        if (!merchant.GetShopOpen()) return;
        NavigateUIItems();
    }
    protected void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!merchant.GetShopOpen()) return;
        NavigateUIItems();
    }

    void NavigateUIItems() {
        Vector2 input = GameInput.Instance.GetUINavigationVector();
        Debug.Log(input);
        if (input == Vector2.zero) return;

        int horizontal = Mathf.RoundToInt(input.x);
        int vertical = -Mathf.RoundToInt(input.y); // haut = -1, bas = +1

        if (horizontal != 0) {
            selectedCol = Mathf.Clamp(selectedCol + horizontal, 0, 1);
            ClampSelection();
        }

        if (vertical != 0) {
            int rowCount = GetCurrentColumn().Count;
            int attempts = 0;

            do {
                selectedRow += vertical;

                // wrap autour si on sort des limites
                if (selectedRow < 0) selectedRow = rowCount - 1;
                if (selectedRow >= rowCount) selectedRow = 0;

                attempts++;
                if (attempts > rowCount) break; // sécurité anti-boucle infinie
            }
            while (!IsCurrentItemValid());
        }

        ClampSelection(); // pour s'assurer qu'on reste dans la colonne correcte
        UpdateSelectedItemUI();
    }

    private void SelectNextAvailableItem() {
        List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();
        int itemCount = allItems.Count;

        int attempts = 0;

        do {
            selectedItemIndex = (selectedItemIndex + 1) % itemCount;

            Debug.Log(selectedItemIndex + " puchased " + allItems[selectedItemIndex].isPurchased);
            Debug.Log(selectedItemIndex + " buyingLocksPurchasesUntilRefresh " + allItems[selectedItemIndex].buyingLocksPurchasesUntilRefresh);

            if (!allItems[selectedItemIndex].isPurchased && allItems[selectedItemIndex].buyingLocksPurchasesUntilRefresh) {
                Debug.Log("selectedItemIndex " + selectedItemIndex);
                selectedGridPos = GetGridPosFromIndex(selectedItemIndex);
                Debug.Log("selectedGridPos " + selectedGridPos);
                UpdateSelectedItemUI();
                UpdateDescriptionPanelVisuals();
                previousSelectedItemIndex = selectedItemIndex;
                return;
            }

            attempts++;
        } while (attempts < itemCount);

        Debug.LogWarning("All items are purchased! No valid item to select.");
        HideItemUI();
    }

    protected void SelectFirstAvailableItem() {
        List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();

        for (int i = 0; i < allItems.Count; i++) {
            if (!allItems[i].isPurchased && allItems[i].buyingLocksPurchasesUntilRefresh) {
                selectedItemIndex = i;
                selectedGridPos = GetGridPosFromIndex(i);
                UpdateSelectedItemUI();
                UpdateDescriptionPanelVisuals();
                previousSelectedItemIndex = selectedItemIndex;
                return;
            }
        }

        Debug.LogWarning("All items are purchased! No valid item to select.");
        HideItemUI();
    }

    void UpdateSelectedItemUI() {
        MerchantItem selected = GetCurrentItem();
        if (selected == null) {
            Debug.LogWarning("Aucun item sélectionnable");
            return;
        }

        selectedItemIndex = merchant.GetAllCurrentItemsForSale().IndexOf(selected);
        selectedMerchantItem = selected;

        merchant.SetCurrentHoveredItem(selectedMerchantItem);
        merchant.SetCurrentSelectedItemCanBeBought(
            selectedMerchantItem.isPurchased && selectedMerchantItem.buyingLocksPurchasesUntilRefresh
        );

        // Refresh visuels
        for (int i = 0; i < bigMerchantItemUIList.Count; i++) {
            bool isSelected = (i == selectedItemIndex);
            bigMerchantItemUIList[i].HighlightItem(i == selectedItemIndex);

            if (isSelected) {
                payCurrencyUI.SetOrbTemplateUIList(bigMerchantItemUIList[i].GetPayCurrencyTemplateWorldUIList());
            }
        }


        // Mise à jour des items mineurs (décalage d'index)
        for (int i = 0; i < smallMerchantItemUIList.Count; i++) {
            int index = i + bigMerchantItemUIList.Count;
            bool isSelected = (index == selectedItemIndex);
            smallMerchantItemUIList[i].HighlightItem(isSelected);

            if (isSelected) {
                payCurrencyUI.SetOrbTemplateUIList(smallMerchantItemUIList[i].GetPayCurrencyTemplateWorldUIList());
            }
        }

        OnNewItemHovered?.Invoke(this, EventArgs.Empty);
        UpdateDescriptionPanelVisuals();
        previousSelectedItemIndex = selectedItemIndex;

    }

    protected void UpdateDescriptionPanelVisuals() {
        //Check if we switched from big item to small item
        if(useMajorMinorDistinction) {

            if(selectedItemIndex < bigMerchantItemUIList.Count) {
                // First time the player opens the panel
                merchantDescriptionPanelUI.SetPanelPosition(descriptionPanelLeftPosition);
            } else {
                merchantDescriptionPanelUI.SetPanelPosition(descriptionPanelRightPosition);
            }

            merchantDescriptionPanelUI.OpenPanel();
            OnDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);

        }

        merchantDescriptionPanelUI.UpdateDescriptionPanelVisuals(selectedMerchantItem);

    }

    private void HideItemUI() {
        merchantDescriptionPanelUI.ClosePanel();

        Debug.Log(bigMerchantItemUIList.Count);
        foreach (MerchantItemUI merchantItemUI in bigMerchantItemUIList) {

            merchantItemUI.HighlightItem(false);
        }

        foreach (MerchantItemUI merchantItemUI in smallMerchantItemUIList) {
            merchantItemUI.HighlightItem(false);
        }
    }

    private void Merchant_OnPlayerBoughtItem1(object sender, Merchant.OnPlayerBoughtItemEventArgs e) {
        Debug.Log(e.boughtItem.itemName);
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

        SplitItemsIntoColumns();
    }

    void SplitItemsIntoColumns() {
        columnLeft.Clear();
        columnRight.Clear();

        foreach(MerchantItem item in merchant.GetMajorItemListForSale()) {
            columnLeft.Add(item);
        }
        foreach (MerchantItem item in merchant.GetMinorItemListForSale()) {
            columnRight.Add(item);
        }

        selectedCol = 0;
        selectedRow = 0;
        ClampSelection();
    }
    protected void ShowItemsToSale(bool show) {

        if(show) {
            merchantUIAnimator.SetTrigger("Show");
        } else {
            merchantUIAnimator.SetTrigger("Hide");
        }
    }

    void ClampSelection() {
        int maxRow = GetCurrentColumn().Count - 1;
        selectedRow = Mathf.Clamp(selectedRow, 0, maxRow);

        // skip les items invalides
        while (!IsCurrentItemValid() && selectedRow > 0) {
            selectedRow--;
        }
    }

    List<MerchantItem> GetCurrentColumn() {
        return selectedCol == 0 ? columnLeft : columnRight;
    }

    MerchantItem GetCurrentItem() {
        var column = GetCurrentColumn();
        if (selectedRow >= 0 && selectedRow < column.Count)
            return column[selectedRow];

        return null;
    }

    bool IsCurrentItemValid() {
        var item = GetCurrentItem();
        return item != null && !item.isPurchased || (item.isPurchased && !item.buyingLocksPurchasesUntilRefresh);
    }

    int GetIndexFromGridPos(Vector2Int gridPos, int columns) {
        return gridPos.y * columns + gridPos.x;
    }

    Vector2Int GetGridPosFromIndex(int index) {
        List<MerchantItem> allItems = merchant.GetAllCurrentItemsForSale();

        // Reconstruire les deux colonnes
        List<MerchantItem> col0 = new List<MerchantItem>();
        List<MerchantItem> col1 = new List<MerchantItem>();

        for (int i = 0; i < allItems.Count; i++) {
            if (i < merchant.GetMajorItemListForSale().Count) col0.Add(allItems[i]);  // Colonne 0
            else col1.Add(allItems[i]);  // Colonne 1
        }

        // Calcul de l'index de la ligne dans la colonne
        int currentIndex = 0;

        for (int col = 0; col < 2; col++) {
            List<MerchantItem> currentCol = (col == 0) ? col0 : col1;

            if (index < currentIndex + currentCol.Count) {
                int row = index - currentIndex;  // Calcul de la ligne

                // Mettre à jour les valeurs de selectedCol et selectedRow
                selectedCol = col;
                selectedRow = row;

                // Retourner la position sous forme de Vector2Int
                return new Vector2Int(selectedCol, selectedRow);
            }

            currentIndex += currentCol.Count;
        }

        Debug.LogWarning("Index hors des limites dans GetGridPosFromIndex");
        return new Vector2Int(0, 0);  // Si l'index est invalide
    }

}
