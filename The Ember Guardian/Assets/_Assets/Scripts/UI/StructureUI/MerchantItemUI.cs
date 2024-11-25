using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantItemUI : MonoBehaviour
{
    [SerializeField] private Animator itemVisualAnimator;
    [SerializeField] private Animator itemCurrencyContainerUIAnimator;

    [SerializeField] private Image merchantItemImage;
    [SerializeField] private RectTransform currencyUIParent;
    [SerializeField] private RectTransform currencyUITemplate;
    [SerializeField] private List<PayCurrencyTemplateWorldUI> payCurrencyUIList = new List<PayCurrencyTemplateWorldUI>();

    private MerchantItem linkedItem;
    private bool itemPurchased;

    private void Awake() {
        merchantItemImage.material = new Material(merchantItemImage.material);
    }

    public void SetLinkedItem(MerchantItem item) {
        linkedItem = item;

        Debug.Log(item.itemName);
        //itemNameText.text = item.itemName;
        merchantItemImage.sprite = item.icon;

        RefreshItemPriceCurrencyUI();
    }

    public void PurchaseItem() {
        linkedItem.Purchase();
        itemPurchased = true;

        currencyUIParent.gameObject.SetActive(false);
        merchantItemImage.material.SetFloat("_GreyscaleBlend", 1f);
    }

    private void RefreshItemPriceCurrencyUI() {
        payCurrencyUIList.Clear();

        payCurrencyUIList.Add(currencyUITemplate.GetComponent<PayCurrencyTemplateWorldUI>());

        for (int i = 0; i < linkedItem.price; i++) {
            PayCurrencyTemplateWorldUI payCurrencyUITemplate = Instantiate(currencyUITemplate, currencyUIParent).GetComponent<PayCurrencyTemplateWorldUI>();
            payCurrencyUIList.Add(payCurrencyUITemplate);
        }

    }

    public void HighlightItem(bool highlighted) {
        if(highlighted) {

            itemVisualAnimator.SetTrigger("Hover");
            itemVisualAnimator.ResetTrigger("Unhover");
            itemCurrencyContainerUIAnimator.SetTrigger("Hover");
            itemCurrencyContainerUIAnimator.ResetTrigger("Unhover");

        } else {

            itemVisualAnimator.SetTrigger("Unhover");
            itemVisualAnimator.ResetTrigger("Hover");
            itemCurrencyContainerUIAnimator.SetTrigger("Unhover");
            itemCurrencyContainerUIAnimator.ResetTrigger("Hover");

        }

        foreach(PayCurrencyTemplateWorldUI payCurrencyTemplate in payCurrencyUIList) {
            payCurrencyTemplate.SetHovered(highlighted);
        }
    }

    public List<PayCurrencyTemplateWorldUI> GetPayCurrencyTemplateWorldUIList() {
        return payCurrencyUIList;
    }

    public bool GetItemPurchased() {
        return itemPurchased;
    }

    public MerchantItem GetMerchantItemLinked() {
        return linkedItem;
    }
}
