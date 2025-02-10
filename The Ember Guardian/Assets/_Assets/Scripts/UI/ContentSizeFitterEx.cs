using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterEx : ContentSizeFitter {
    public Vector2 sizeMin = new Vector2(0f, 0f);
    public Vector2 sizeMax = new Vector2(1920f, 1080f);

    public override void SetLayoutHorizontal() {
        base.SetLayoutHorizontal();
        var rectTransform = transform as RectTransform;
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = Mathf.Clamp(sizeDelta.x, sizeMin.x, sizeMax.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
    }

    public override void SetLayoutVertical() {
        base.SetLayoutVertical();
        var rectTransform = transform as RectTransform;
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = Mathf.Clamp(sizeDelta.y, sizeMin.y, sizeMax.y);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ContentSizeFitterEx))]
public class ContentSizeFitterExEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}
#endif
