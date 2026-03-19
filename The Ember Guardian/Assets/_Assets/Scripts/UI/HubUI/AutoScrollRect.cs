using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollRect : MonoBehaviour {

    [SerializeField] protected bool autoScrollWithMouse = false;
    [SerializeField] protected bool doNotLerp = false;
    [SerializeField] protected bool dropdownMenuAutoScroll = false;

    protected ScrollRect scrollRect;
    protected RectTransform hoveredButtonUI;
    protected RectTransform previousSelectedButtonUI;
    protected float smoothSpeed = 2f;

    protected EventSystem eventSystem;

    protected RectTransform manualTarget;
    protected bool manualScrollRequest = false;

    void Start() {
        eventSystem = EventSystem.current;
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.enabled = true;
    }

    void Update() {
        if (manualScrollRequest && manualTarget != null) {
            previousSelectedButtonUI = manualTarget;
            AdjustScrollPosition();

            if (ReachedTargetPosition()) {
                manualScrollRequest = false;
                manualTarget = null;
            }
            return;
        }

        if (!autoScrollWithMouse) {
            if (!GameInput.Instance.IsUsingGamepad()) return;
        }

        GameObject selected = eventSystem.currentSelectedGameObject;

        // Button is not in scroll rect
        if (selected == null) return;

        if (!IsChildOfScrollContent(selected))  return;

        if (selected != null && (selected.GetComponent<Button>() != null || selected.GetComponent<ButtonUI>() != null || selected.GetComponent<Toggle>() != null)) {
            hoveredButtonUI = selected.GetComponent<RectTransform>();
            previousSelectedButtonUI = hoveredButtonUI;
        }

        if (previousSelectedButtonUI != null) {
            AdjustScrollPosition();
        }
    }

    protected virtual void AdjustScrollPosition() {
        float contentWidth = scrollRect.content.rect.width;
        float contentHeight = scrollRect.content.rect.height;
        float viewportWidth = scrollRect.viewport.rect.width;
        float viewportHeight = scrollRect.viewport.rect.height;

        Vector2 buttonLocalPosition = previousSelectedButtonUI.localPosition;
        ItemButtonUI itemButtonUI = previousSelectedButtonUI.GetComponent<ItemButtonUI>();
        if (itemButtonUI != null) {
            buttonLocalPosition.x = itemButtonUI.GetLocalPosition().x;
        }
        float centeredPositionX = buttonLocalPosition.x - viewportWidth / 2f;
        float centeredPositionY = buttonLocalPosition.y - viewportHeight / 2f;

        float normalizedPositionX = Mathf.Clamp01((centeredPositionX + contentWidth / 2f) / (contentWidth - viewportWidth));
        float normalizedPositionY = Mathf.Clamp01((centeredPositionY + contentHeight / 2f) / (contentHeight - viewportHeight));

        if(dropdownMenuAutoScroll) {
            // position Y réelle dans le content (top = 0, down = positif)
            float itemY = -buttonLocalPosition.y;

            // centre de l’item
            float centeredY = itemY - viewportHeight / 2f;

            // normalisation (0 bas : 1 haut)
            normalizedPositionY = Mathf.Clamp01(
                1f - (centeredY / (contentHeight - viewportHeight))
            );
        }

        if (doNotLerp) {
            scrollRect.verticalNormalizedPosition = normalizedPositionY;
        } else {
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

    protected float CalculateNormalizedX(RectTransform target) {
        float contentWidth = scrollRect.content.rect.width;
        float viewportWidth = scrollRect.viewport.rect.width;

        Vector2 localPos = target.localPosition;
        ItemButtonUI itemButtonUI = target.GetComponent<ItemButtonUI>();
        if (itemButtonUI != null) {
            localPos.x = itemButtonUI.GetLocalPosition().x;
        }

        float centeredX = localPos.x - viewportWidth / 2f;
        return Mathf.Clamp01((centeredX + contentWidth / 2f) / (contentWidth - viewportWidth));
    }

    protected bool ReachedTargetPosition() {
        float targetX = CalculateNormalizedX(previousSelectedButtonUI);
        float delta = Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetX);
        return delta < 0.01f;
    }

    public void CenterOn(RectTransform target) {
        manualTarget = target;
        manualScrollRequest = true;
    }
    protected bool IsChildOfScrollContent(GameObject obj) {
        return obj.transform.IsChildOf(transform);
    }
}
