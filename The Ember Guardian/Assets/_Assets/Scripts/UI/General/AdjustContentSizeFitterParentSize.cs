using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustContentSizeFitterParentSize : MonoBehaviour
{
    [SerializeField] private RectTransform contentSizeFitterChild;
    [SerializeField] private RectTransform rectTransform;

    private void Start() {
        UpdateHeight();
    }

    public void UpdateHeight() {
        if (contentSizeFitterChild != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentSizeFitterChild);
            float newHeight = contentSizeFitterChild.rect.height;
            if (newHeight == 0) return;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
        }
    }
}
