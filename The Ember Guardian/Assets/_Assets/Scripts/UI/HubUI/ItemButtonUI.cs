using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using Plugins.Animate_UI_Materials;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
{
    [SerializeField] private List<ItemButtonUI> lockingItemButtonUIList;
    [SerializeField] private ItemDescriptionCardUI descriptionCard;

    [SerializeField] private Color outlineUnlockedBuyableColor;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private ItemButtonUI_Visual itemButtonUI_Visual;
    [SerializeField] private Color boughtOutlineColor;
    [SerializeField] private Color boughtBackgroundColor;
    [SerializeField] private Sprite outlineImageBoughtSprite;
    [SerializeField] private List<Image> outputLinkUnlockedImageList;
    [SerializeField] private GameObject itemLevelBackgroundGameObject;
    [SerializeField] private Sprite defaultOutlineSprite;
    [SerializeField] private Sprite itemEquippedOutlineSprite;
    [SerializeField] private Sprite itemMaxedOutlineSprite;
    [SerializeField] private TextMeshProUGUI itemLevelText;

    [SerializeField] private bool showItemLevel;

    private HubMerchantItem hubMerchantItem;
    private Button button;

    private bool itemSelected;
    private bool itemHovered;

    private bool buttonSelected;

    public static event EventHandler OnAnyOutputLinkUnlocked;
    public static event EventHandler OnAnyButtonSelected;
    public static event EventHandler OnAnyButtonHovered;
    public static event EventHandler OnAnyLockedButtonTryPress;
    public static event EventHandler OnAnyHubMerchantItemFailedBuy;
    public static event EventHandler OnAnyHubMerchantItemTryBuyMaxedItem;

    private void Awake() {
        button = GetComponent<Button>();
        hubMerchantItem = GetComponent<HubMerchantItem>();

        iconImage.material = new Material(iconImage.material);
        outlineImage.material = new Material(outlineImage.material);

        backgroundImage.color = Color.black;

        if (!showItemLevel) {
            itemLevelBackgroundGameObject.SetActive(false);
        }

        OnAnyOutputLinkUnlocked += ItemButtonUI_OnAnyOutputLinkUnlocked;
        OnAnyButtonSelected += ItemButtonUI_OnAnyButtonSelected;
        OnAnyButtonHovered += ItemButtonUI_OnAnyButtonHovered;
        hubMerchantItem.OnHubMerchantItemBought += HubMerchantItem_OnHubMerchantItemBought;
        hubMerchantItem.OnHubMerchantItemLoaded += HubMerchantItem_OnHubMerchantItemLoaded;
        hubMerchantItem.OnHubMerchantItemUpgraded += HubMerchantItem_OnHubMerchantItemUpgraded;
        hubMerchantItem.OnItemMustRefreshDescriptionCard += HubMerchantItem_OnItemMustRefreshDescriptionCard;
        hubMerchantItem.OnHubMerchantItemEquipped += HubMerchantItem_OnHubMerchantItemEquipped;
        hubMerchantItem.OnHubMerchantItemUnequipped += HubMerchantItem_OnHubMerchantItemUnequipped;
    }


    private void HubMerchantItem_OnHubMerchantItemUnequipped(object sender, EventArgs e) {
        RefreshItemEquippedUI();
    }

    private void HubMerchantItem_OnHubMerchantItemEquipped(object sender, EventArgs e) {
        RefreshItemEquippedUI();
    }

    private void HubMerchantItem_OnItemMustRefreshDescriptionCard(object sender, EventArgs e) {
        RefreshDescriptionCard();
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
        }

        if (hubMerchantItem.GetItemBought()) {
            SetOutputLinksBought();
        }

        RefreshItemStatusVisuals();
        RefreshItemLevelUI();
        RefreshDescriptionCard();
        RefreshItemEquippedUI();
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

        descriptionCard.SetDescriptionCardText(itemName, constantUnlockDescription, itemStatDescription, itemDescription, itemStatValues, itemStatModifierValues);
        descriptionCard.SetDescriptionCardCost(greenGemCost, redGemCost, blueGemCost, yellowGemCost, purpleGemCost);

        if(!hubMerchantItem.GetItemUpgradeable()) {
            
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

    private void ItemButtonUI_OnAnyOutputLinkUnlocked(object sender, EventArgs e) {
        ItemButtonUI itemButtonUI = (ItemButtonUI)sender;

        if (lockingItemButtonUIList.Contains(itemButtonUI)) {
            SetLockingItemBought(itemButtonUI);
        }

        RefreshItemStatusVisuals();
    }

    public void BuyItem() {

        if (!hubMerchantItem.GetItemUnlocked()) {
            Debug.Log("OnAnyLockedButtonTryPress");
            OnAnyLockedButtonTryPress?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (!hubMerchantItem.GetitemEquipable() && hubMerchantItem.GetItemMaxed()) {
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
        itemButtonUI_Visual.StartBuyAnimation(hubMerchantItem.GetRedGemCost(), hubMerchantItem.GetGreenGemCost());
    }

    private void SetItemBoughtVisuals() {
        if(hubMerchantItem.GetItemUpgradeable() && hubMerchantItem.GetItemMaxed()) {
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
            itemLevelText.text = "MAX";
        } else {
            itemLevelText.text = hubMerchantItem.GetItemLevel().ToString();
        }
    }

    public void RefreshItemEquippedUI() {
        if (hubMerchantItem.GetItemEquipped() && hubMerchantItem.GetitemEquipable()) {

            outlineImage.sprite = itemEquippedOutlineSprite;
            outlineImage.color = Color.white;

        }

        else {
            outlineImage.sprite = defaultOutlineSprite;
            RefreshItemStatusVisuals();
        }
    }
    public void StartUpgradeItemAnimation() {
        itemButtonUI_Visual.StartBuyAnimation(hubMerchantItem.GetRedGemCost(), hubMerchantItem.GetGreenGemCost());
    }

    public void SetLockingItemBought(ItemButtonUI itemButtonUI) {
        lockingItemButtonUIList.Remove(itemButtonUI);

        if (lockingItemButtonUIList.Count == 0) {
            SetItemUnlocked();
        }
    }

    public void SetItemUnlocked() {
        if (hubMerchantItem.GetItemBought()) return;

        hubMerchantItem.UnlockItem();
        RefreshItemStatusVisuals();
    }

    private void RefreshItemStatusVisuals() {
        if(!hubMerchantItem.GetItemUnlocked()) {
            outlineImage.color = Color.grey;
            return;
        }

        if (hubMerchantItem.GetItemBought()) {
            SetItemBoughtVisuals();

            if (hubMerchantItem.GetItemEquipped()) {

                outlineImage.sprite = itemEquippedOutlineSprite;
                outlineImage.color = Color.white;

            }
            return;
        }

        if (!hubMerchantItem.CanBuyItem()) {
            outlineImage.color = Color.white;
            return;
        }
        else {
            outlineImage.color = outlineUnlockedBuyableColor;
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
    private void ItemButtonUI_OnAnyButtonHovered(object sender, EventArgs e) {
        //if(GameInput.Instance.IsUsingGamepad()) return;
        ItemButtonUI itemButtonUI = sender as ItemButtonUI;

        if (this == itemButtonUI) {
            itemHovered = true;
            descriptionCard.gameObject.SetActive(true);
            descriptionCard.transform.SetParent(transform.parent);
            descriptionCard.transform.SetAsLastSibling(); // Amène la carte au-dessus
        }

        if (this != itemButtonUI && itemHovered) {
            itemHovered = false;
            descriptionCard.gameObject.SetActive(false);
        }
    }

    private void ItemButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ItemButtonUI itemButtonUI = sender as ItemButtonUI;

        if (this == itemButtonUI) {
            itemSelected = true;
            descriptionCard.gameObject.SetActive(true);
            transform.SetAsLastSibling(); // Amène la carte au-dessus
        }

        if (this != itemButtonUI && itemSelected) {
            itemSelected = false;
            descriptionCard.gameObject.SetActive(false);
        }
    }

    public void OnSelect(BaseEventData eventData) {
        buttonSelected = true;
        OnAnyButtonSelected?.Invoke(this, EventArgs.Empty);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnAnyButtonHovered?.Invoke(this, EventArgs.Empty);
    }

    public void OnPointerExit(PointerEventData eventData) {
        itemHovered = false;
        descriptionCard.gameObject.SetActive(false);
    }

    public void OnDeselect(BaseEventData eventData) {
        buttonSelected = false;
    }
    #endregion

    private void OnDestroy() {
        OnAnyOutputLinkUnlocked -= ItemButtonUI_OnAnyOutputLinkUnlocked;
        OnAnyButtonSelected -= ItemButtonUI_OnAnyButtonSelected;
        OnAnyButtonHovered -= ItemButtonUI_OnAnyButtonHovered;
    }
}
