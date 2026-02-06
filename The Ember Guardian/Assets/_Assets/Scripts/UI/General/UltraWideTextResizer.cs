using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UltraWideTextResizer : MonoBehaviour
{

    [SerializeField] private float newFontSize;

    void Start() {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();

        float aspectRatio = (float)Screen.width / Screen.height;
        if (aspectRatio == 1.6f) return; // STEAM DECK

        if (aspectRatio >= 2.33f) {
            text.fontSize = newFontSize;
        }

        if (aspectRatio <= 1.6f) {
            text.fontSize = newFontSize;
        }
    }
}
