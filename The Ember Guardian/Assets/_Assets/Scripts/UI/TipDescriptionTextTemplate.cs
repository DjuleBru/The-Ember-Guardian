using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TipDescriptionTextTemplate : MonoBehaviour
{
    [SerializeField] private Animator templateAnimator;
    [SerializeField] private TextMeshProUGUI templateText;
    [SerializeField] private Image templateIcon;

    private void Awake() {
        templateAnimator.enabled = false;
    }

    public void SetTipDescription(string text) {
        templateText.text = text;
    }

    public void SetTipDescriptionAdvanced(string text) {

        string[] parts = text.Split(new string[] { "[icon:" }, System.StringSplitOptions.None);
        Dictionary<string, Sprite> iconDictionary = GameIcons.Instance.GetIconDictionary();

        Debug.Log("parts length " + parts.Length);
        foreach (Transform child in transform) {
            if (child == templateIcon.transform) continue;
            if (child == templateText.transform) continue;
            Destroy(child.gameObject);
        }

        foreach (string part in parts) {
            Debug.Log("part " + part);

            if (part.Contains("]")) {
                string[] split = part.Split(']');
                string iconKey = split[0];
                string remainingText = split.Length > 1 ? split[1] : "";

                if (iconDictionary.TryGetValue(iconKey, out Sprite iconSprite)) {
                    Image newIcon = Instantiate(templateIcon, transform);
                    newIcon.sprite = iconSprite;
                    newIcon.gameObject.SetActive(true);
                }

                if (!string.IsNullOrEmpty(remainingText)) {
                    TextMeshProUGUI newText = Instantiate(templateText, transform);
                    newText.text = remainingText;
                    newText.gameObject.SetActive(true);
                }
            }
            else {
                TextMeshProUGUI newText = Instantiate(templateText, transform);
                newText.text = part;
                newText.gameObject.SetActive(true);
            }
        }

        templateText.gameObject.SetActive(false);
        templateIcon.gameObject.SetActive(false);
    }

    public void ShowTipText() {
        gameObject.SetActive(true);
        templateAnimator.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void HideTipText() {
        gameObject.SetActive(false);
        templateAnimator.enabled = false;
    }
}
