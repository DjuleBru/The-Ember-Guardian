using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltraWideLayoutGroupResizer : MonoBehaviour
{
    [SerializeField] private float ultraWideSpacing;

    private VerticalLayoutGroup layoutGroup;

    void Start() {
        layoutGroup = GetComponent<VerticalLayoutGroup>();

        float aspectRatio = (float)Screen.width / Screen.height;

        if (aspectRatio >= 2.33f) {
            layoutGroup.spacing = ultraWideSpacing;
        }
    }
}
