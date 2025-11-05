using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using static LocalizationManager;
public class LocalizationData {
    public Dictionary<string, Dictionary<string, string>> languages;
}

public class LocalizedResult {
    public string text;
    public TMP_FontAsset font;

    public LocalizedResult(string text, TMP_FontAsset font) {
        this.text = text;
        this.font = font;
    }
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    private Dictionary<string, Dictionary<string, string>> localizedTexts;
    [SerializeField] private TMP_FontAsset standardFont;
    [SerializeField] private TMP_FontAsset japaneseFont;
    [SerializeField] private TMP_FontAsset chineseFont;

    [SerializeField] private Material standardMaterial;
    [SerializeField] private Material standardMaterial_JP;
    [SerializeField] private Material standardMaterial_CN;
    [SerializeField] private Material blueGlowMaterial;
    [SerializeField] private Material blueGlowMaterial_JP;
    [SerializeField] private Material blueGlowMaterial_CN;
    [SerializeField] private Material greenGlowMaterial;
    [SerializeField] private Material greenGlowMaterial_JP;
    [SerializeField] private Material greenGlowMaterial_CN;
    [SerializeField] private Material redGlowMaterial;
    [SerializeField] private Material redGlowMaterial_JP;
    [SerializeField] private Material redGlowMaterial_CN;

    public enum Language {
        English,
        French,
        German,
        Spanish,
        Japanese,
        Chinese,
    }
    [Serializable]
    public class LocalizationEntry {
        public string key;
        public string English;
        public string French;
        public string German;
        public string Spanish;
        public string Japanese;
        public string Chinese;
    }
    public class LocalizationData {
        public List<LocalizationEntry> entries;
    }
    private LocalizationData localizationData;
    private Language currentLanguage;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            currentLanguage = SettingsManager.Instance.GetLanguage();
            LoadLocalization();
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(Language language) {
        currentLanguage = language;
    }

    void LoadLocalization() {
        string filePath = Path.Combine(Application.streamingAssetsPath, "TEG_Localization.json");

        if (File.Exists(filePath)) {
            string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
            localizationData = JsonUtility.FromJson<LocalizationData>(jsonContent);
        }
        else {
            Debug.LogError("Localization file not found in StreamingAssets!");
        }
    }

    public LocalizedResult GetLocalized(string key) {
        string translatedText = GetLocalizedText(key); // ton code existant
        TMP_FontAsset fontToUse = null;

        if (currentLanguage == Language.Japanese) {
            fontToUse = japaneseFont;
        }
        else if (currentLanguage == Language.Chinese) {
            fontToUse = chineseFont;
        }
        else  {
            fontToUse = standardFont;
        }
        // tu peux rajouter d'autres polices plus tard si besoin

        return new LocalizedResult(translatedText, fontToUse);
    }

    public string GetLocalizedText(string key, params object[] args) {
        string translatedText = "";
        LocalizationEntry localizationEntry = null;

        foreach (var entry in localizationData.entries) {
            if (entry.key == key) {
                localizationEntry = entry;
                switch (currentLanguage.ToString()) {
                    case "French": translatedText = entry.French; break;
                    case "English": translatedText = entry.English; break;
                    case "German": translatedText = entry.German; break;
                    case "Spanish": translatedText = entry.Spanish; break;
                    case "Japanese": translatedText = entry.Japanese; break;
                    case "Chinese": translatedText = entry.Chinese; break;
                    default: translatedText = entry.English; break;
                }
                break;
            }
        }

        if (string.IsNullOrEmpty(translatedText)) {
            if (localizationEntry == null) {
                Debug.LogError("No localization entry for key " + key);
                translatedText = key;
            }
            else {
                translatedText = localizationEntry.English;
            }
        }

        // Injection des arguments (si présents)
        if (args != null && args.Length > 0) {
            translatedText = string.Format(translatedText, args);
        }

        return translatedText;
    }

    public TMP_FontAsset GetCurrentFont() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return japaneseFont;
        if (currentLanguage == Language.Chinese) return chineseFont;
        return standardFont;
    }

    public Material GetBlueGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return blueGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return blueGlowMaterial_CN;
        return blueGlowMaterial;
    }
    public Material GetGreenGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return greenGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return greenGlowMaterial_CN;
        return greenGlowMaterial;
    }
    public Material GetRedGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return redGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return redGlowMaterial_CN;
        return redGlowMaterial;
    }

    public Material GetStandardMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return standardMaterial_JP;
        if (currentLanguage == Language.Chinese) return standardMaterial_CN;
        return standardMaterial;
    }

}
