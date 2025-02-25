using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MerchantItemUI_TrapUpgrade : MerchantItemUI
{

    [SerializeField] protected Image parentMerchantItemImage;

    public override void SetLinkedItem(MerchantItem item) {
        base.SetLinkedItem(item);
        TrapItem trapItem = item as TrapItem;

        parentMerchantItemImage.sprite = trapItem.GetTrapSO().linkedTrapSO.Icon;
    }
}
