using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScrollRect_GunMerchantHordeMode : AutoScrollRect
{
    protected override void AdjustScrollPosition() {
        float contentWidth = scrollRect.content.rect.width;
        float contentHeight = scrollRect.content.rect.height;
        float viewportWidth = scrollRect.viewport.rect.width;
        float viewportHeight = scrollRect.viewport.rect.height;

        Vector2 buttonLocalPosition = previousSelectedButtonUI.transform.parent.localPosition;
        ItemButtonUI itemButtonUI = previousSelectedButtonUI.GetComponent<ItemButtonUI>();

        if(itemButtonUI.GetIsTreeChild()) {
            buttonLocalPosition = (previousSelectedButtonUI.transform.parent).transform.parent.localPosition;
        }

        float centeredPositionX = buttonLocalPosition.x - viewportWidth / 2f;
        float centeredPositionY = buttonLocalPosition.y - viewportHeight / 2f;

        float normalizedPositionX = Mathf.Clamp01((centeredPositionX + contentWidth / 2f) / (contentWidth - viewportWidth));
        float normalizedPositionY = Mathf.Clamp01((centeredPositionY + contentHeight / 2f) / (contentHeight - viewportHeight));

        if (doNotLerp) {
            scrollRect.verticalNormalizedPosition = normalizedPositionY;
        }
        else {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                 scrollRect.horizontalNormalizedPosition,
                 normalizedPositionX,
                 Time.deltaTime * smoothSpeed);

            scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                scrollRect.verticalNormalizedPosition,
                normalizedPositionY,
                Time.deltaTime * smoothSpeed);
        }

    }
}
