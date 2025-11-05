using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {
    [SerializeField] private string localizationKey;
    private TextMeshProUGUI textComponent;

    void Start() {
        textComponent = GetComponent<TextMeshProUGUI>();
        SettingsManager.Instance.OnLanguageChanged += SettingsManager_OnLanguageChanged;
        UpdateText();
    }

    private void SettingsManager_OnLanguageChanged(object sender, System.EventArgs e) {
        UpdateText();
    }

    public void UpdateText() {
        if (LocalizationManager.Instance != null) {
            var result = LocalizationManager.Instance.GetLocalized(localizationKey);

            textComponent.text = result.text;

            if (result.font != null) {
                textComponent.font = result.font;
            }
        }
    }
}

