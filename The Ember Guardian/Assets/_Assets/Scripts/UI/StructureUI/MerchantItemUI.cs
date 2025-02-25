using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class MerchantItemUI : MonoBehaviour
{
    [SerializeField] protected Animator itemVisualAnimator;
    [SerializeField] protected Animator itemCurrencyContainerUIAnimator;

    [SerializeField] protected Image merchantItemImage;
    [SerializeField] protected GameObject merchantItemTextGameObject;
    [SerializeField] protected TextMeshProUGUI merchantItemLevelText;

    [SerializeField] protected RectTransform currencyUIParent;
    [SerializeField] protected RectTransform currencyUITemplate;
    [SerializeField] protected List<PayCurrencyTemplateWorldUI> payCurrencyUIList = new List<PayCurrencyTemplateWorldUI>();

    protected MerchantItem linkedItem;
    protected bool itemPurchased;

    protected void Awake() {
        InitializeVisuals();
    }

    public virtual void SetLinkedItem(MerchantItem item) {
        Debug.Log("SetLinkedItem " + item.itemName + " " + gameObject.GetInstanceID());
        merchantItemImage.material.SetFloat("_GreyscaleBlend", 0f);
        currencyUIParent.gameObject.SetActive(true);

        linkedItem = item;
        itemPurchased = item.isPurchased;

        merchantItemImage.sprite = item.icon;
        if(item.currentLevel != 1) {
            merchantItemLevelText.text = item.currentLevel.ToString();
            merchantItemTextGameObject.SetActive(true);
        }

        RefreshItemPriceCurrencyUI();
        HighlightItem(false);
    }

    protected void InitializeVisuals() {
        merchantItemImage.material = new Material(merchantItemImage.material);
        merchantItemTextGameObject.SetActive(false);
    }

    public void PurchaseItem() {
        linkedItem.Purchase();
        itemPurchased = true;

        if(linkedItem.buyingLocksPurchasesUntilRefresh) {
            currencyUIParent.gameObject.SetActive(false);
            merchantItemImage.material.SetFloat("_GreyscaleBlend", 1f);
        } else {
            itemCurrencyContainerUIAnimator.SetTrigger("Unhover");
            itemCurrencyContainerUIAnimator.SetTrigger("Hover");
        }
    }

    protected void RefreshItemPriceCurrencyUI() {
        payCurrencyUIList.Clear();
        currencyUITemplate.gameObject.SetActive(true);

        foreach (RectTransform child in currencyUIParent) {
            if (child == currencyUITemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < linkedItem.price; i++) {
            PayCurrencyTemplateWorldUI payCurrencyUITemplate = Instantiate(currencyUITemplate, currencyUIParent).GetComponent<PayCurrencyTemplateWorldUI>();
            payCurrencyUIList.Add(payCurrencyUITemplate);
        }
        currencyUITemplate.gameObject.SetActive(false);
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

    public MerchantItem GetMerchantItemLinked() {
        return linkedItem;
    }
}
