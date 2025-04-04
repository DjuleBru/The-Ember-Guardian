using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class AlignAutoSizedTextSizes : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> textToAlignList;

    private void Start() {
        RefreshTextSizes();
    }

    public void RefreshTextSizes() {
        StartCoroutine(RefreshTextSizesCoroutine());
    }

    private IEnumerator RefreshTextSizesCoroutine() {
        foreach (TextMeshProUGUI text in textToAlignList) {
            text.enableAutoSizing = true;
        }

        yield return new WaitForEndOfFrame();

        float minTextSize = Mathf.Infinity;
        foreach (TextMeshProUGUI text in textToAlignList) {
            text.enableAutoSizing = true;
            if (text.fontSize < minTextSize) {
                minTextSize = text.fontSize;
            }
            
        }
        yield return new WaitForEndOfFrame();

        foreach (TextMeshProUGUI text in textToAlignList) {
            text.enableAutoSizing = false;
            text.fontSize = minTextSize;
        }
    }
}
