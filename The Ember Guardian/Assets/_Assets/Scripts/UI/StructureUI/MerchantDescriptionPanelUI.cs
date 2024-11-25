using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantDescriptionPanelUI : MonoBehaviour
{

    [SerializeField] protected RectTransform descriptionPanelRectTransform;
    [SerializeField] protected Animator descriptionPanelAnimator;
    [SerializeField] protected Image descriptionPanelItemIcon;
    [SerializeField] protected TextMeshProUGUI descriptionPanelItemName;
    [SerializeField] protected TextMeshProUGUI descriptionPanelItemStatChanges;
    [SerializeField] protected TextMeshProUGUI descriptionPanelItemDescription;

    public void SetPanelPosition(RectTransform rectTransform) {
        //Check if we switched from big item to small item
        descriptionPanelRectTransform.position = rectTransform.position;
    }

    public void OpenPanel() {
        descriptionPanelAnimator.SetTrigger("Open");
    }

    public void UpdateDescriptionPanelVisuals(MerchantItem merchantItem) {
        descriptionPanelItemIcon.sprite = merchantItem.icon;
        descriptionPanelItemName.text = merchantItem.itemName;
        descriptionPanelItemStatChanges.text = merchantItem.itemStatChanges;
        descriptionPanelItemDescription.text = merchantItem.itemName;
    }

}
