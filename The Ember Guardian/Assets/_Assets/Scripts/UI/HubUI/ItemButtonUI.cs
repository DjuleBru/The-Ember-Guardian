using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour
{
    [SerializeField] private List<ItemButtonUI> lockingItemButtonUIList;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Color boughtOutlineColor;
    [SerializeField] private Sprite outlineImageBoughtSprite;
    [SerializeField] private List<Image> outputLinkUnlockedImageList;
    [SerializeField] private GameObject itemLevelBackgroundGameObject;
    [SerializeField] private TextMeshProUGUI itemLevelText;

    [SerializeField] private bool showItemLevel;
    [SerializeField] private bool itemUpgradeable;
    [SerializeField] private int itemLevel;
    [SerializeField] private int itemMaxLevel = 1;

    private Button button;

    private bool itemBought;
    private bool itemBuyable;
    private bool itemUnlocked;

    public static event EventHandler OnAnyOutputLinkUnlocked;

    private void Awake() {
        button = GetComponent<Button>();

        iconImage.material = new Material(iconImage.material);

        if(!showItemLevel) {
            itemLevelBackgroundGameObject.SetActive(false);
        }
    }

    private void Start() {
        ItemButtonUI.OnAnyOutputLinkUnlocked += ItemButtonUI_OnAnyItemButtonUIBought;

        if (lockingItemButtonUIList.Count == 0) {
            SetItemUnlocked();
        }
    }

    private void ItemButtonUI_OnAnyItemButtonUIBought(object sender, EventArgs e) {
        ItemButtonUI itemButtonUI = (ItemButtonUI)sender;
        if(lockingItemButtonUIList.Contains(itemButtonUI)) {
            SetLockingItemBought(itemButtonUI);
        }
    }

    public void BuyItem() {
        if (!itemUnlocked) return;
        if (!itemBuyable) return;

        itemLevel++;

        if (!itemBought) {

            itemBought = true;
            outlineImage.sprite = outlineImageBoughtSprite;
            outlineImage.color = boughtOutlineColor;
            iconImage.material.SetFloat("_GreyscaleBlend", 0f);

            if(itemUpgradeable) {
                if (itemLevel < itemMaxLevel) {
                    itemLevelBackgroundGameObject.SetActive(true);
                    UpgradeItem();
                }
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
            UpgradeItem();

        }

    }

    public void UpgradeItem() {
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
        Debug.Log("set item unlocked");
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
}
