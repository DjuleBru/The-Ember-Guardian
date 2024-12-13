using BehaviorDesigner.Runtime.Tasks;
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

    [SerializeField] private Image iconImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private ItemButtonUI_Visual itemButtonUI_Visual;
    [SerializeField] private Color boughtOutlineColor;
    [SerializeField] private Color boughtBackgroundColor;
    [SerializeField] private Sprite outlineImageBoughtSprite;
    [SerializeField] private List<Image> outputLinkUnlockedImageList;
    [SerializeField] private GameObject itemLevelBackgroundGameObject;
    [SerializeField] private TextMeshProUGUI itemLevelText;

    [SerializeField] private bool isBoughtAtStart;
    [SerializeField] private bool showItemLevel;
    [SerializeField] private bool itemUpgradeable;
    [SerializeField] private int itemLevel;
    [SerializeField] private int itemMaxLevel = 1;

    private HubMerchantItem hubMerchantItem;
    private Button button;

    private bool itemSelected;
    private bool itemHovered;
    private bool itemBought;
    private bool itemBuyable;
    private bool itemUnlocked;

    private bool buttonSelected;

    public static event EventHandler OnAnyOutputLinkUnlocked;
    public static event EventHandler OnAnyButtonSelected;
    public static event EventHandler OnAnyButtonHovered;
    public static event EventHandler OnAnyLockedButtonTryPress;

    private void Awake() {
        button = GetComponent<Button>();
        hubMerchantItem = GetComponent<HubMerchantItem>();

        iconImage.material = new Material(iconImage.material);
        backgroundImage.color = Color.black;
        InitializeDescriptionCard();

        if (!showItemLevel) {
            itemLevelBackgroundGameObject.SetActive(false);
        }

        OnAnyOutputLinkUnlocked += ItemButtonUI_OnAnyItemButtonUIBought;
        OnAnyButtonSelected += ItemButtonUI_OnAnyButtonSelected;
        OnAnyButtonHovered += ItemButtonUI_OnAnyButtonHovered;
    }

    private void Start() {

        if(!isBoughtAtStart) {

            LoadItemStatus();

        } else {

            itemBought = true;
            itemBuyable = false;
            SetItemUnlocked();
            SetItemBoughtVisuals();
            SetOutputLinksBought();

        }
    }

    private void InitializeDescriptionCard() {
        string itemName = hubMerchantItem.GetItemName();
        string itemDescription = hubMerchantItem.GetDescription();
        string itemStatDescription = hubMerchantItem.GetStatDescription();
        int greenGemCost = hubMerchantItem.GetGreenGemCost();
        int redGemCost = hubMerchantItem.GetRedGemCost();
        descriptionCard.SetDescriptionCardText(itemName, itemStatDescription, itemDescription, greenGemCost, redGemCost);
        descriptionCard.gameObject.SetActive(false);
    }

    private void ItemButtonUI_OnAnyItemButtonUIBought(object sender, EventArgs e) {
        ItemButtonUI itemButtonUI = (ItemButtonUI)sender;

        if (lockingItemButtonUIList.Contains(itemButtonUI)) {
            SetLockingItemBought(itemButtonUI);
        }
    }

    public void BuyItem() {
        if (!itemUnlocked) {
            OnAnyLockedButtonTryPress?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (!itemBuyable) return;
        if (!hubMerchantItem.CanBuyItem()) return;

        itemLevel++;

        if (!itemBought) {

            itemBought = true;
            hubMerchantItem.BuyItem();
            StartBuyItemVisuals();

            if (itemUpgradeable) {
                if (itemLevel < itemMaxLevel) {
                    itemLevelBackgroundGameObject.SetActive(true);
                    UpgradeItemUI();
                }
            } else {
                itemBuyable = false;
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

            if (!itemUpgradeable) return;
            // Upgrade
            UpgradeItemUI();

        }

    }

    private void LoadItemStatus() {
        itemBought = MetaProgressionManager.Instance.GetMerchantItemBought(hubMerchantItem.GetItemType());
        itemUnlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(hubMerchantItem.GetItemType());

        if (itemUnlocked) {
            SetItemUnlocked();
        }

        if (itemBought) {
            SetItemBoughtVisuals();
            SetOutputLinksBought();
        }
    }

    private void StartBuyItemVisuals() {
        outlineImage.sprite = outlineImageBoughtSprite;
        outlineImage.color = boughtOutlineColor;
        backgroundImage.color = boughtBackgroundColor;

        itemButtonUI_Visual.StartBuyAnimation(hubMerchantItem.GetRedGemCost(), hubMerchantItem.GetGreenGemCost());
    }

    private void SetItemBoughtVisuals() {
        outlineImage.sprite = outlineImageBoughtSprite;
        outlineImage.color = boughtOutlineColor;
        backgroundImage.color = boughtBackgroundColor;

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

    public void UpgradeItemUI() {
        itemLevelText.text = itemLevel.ToString();
        if (itemLevel == itemMaxLevel) {
            itemBuyable = false;
        }
    }

    public void SetLockingItemBought(ItemButtonUI itemButtonUI) {
        lockingItemButtonUIList.Remove(itemButtonUI);

        if(lockingItemButtonUIList.Count == 0) {
            SetItemUnlocked();
        }
    }

    public void SetItemUnlocked() {
        if (itemBought) return;

        hubMerchantItem.UnlockItem();
        itemUnlocked = true;
        itemBuyable = true;

        outlineImage.color = Color.white;
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
            transform.SetAsLastSibling(); // Amène la carte au-dessus
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
}
