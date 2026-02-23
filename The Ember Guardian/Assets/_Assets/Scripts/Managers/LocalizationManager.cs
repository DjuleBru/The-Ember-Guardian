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
    [SerializeField] private TMP_FontAsset koreanFont;

    [SerializeField] private Material standardMaterial;
    [SerializeField] private Material standardMaterial_JP;
    [SerializeField] private Material standardMaterial_CN;
    [SerializeField] private Material standardMaterial_KR;
    [SerializeField] private Material blueGlowMaterial;
    [SerializeField] private Material blueGlowMaterial_JP;
    [SerializeField] private Material blueGlowMaterial_CN;
    [SerializeField] private Material blueGlowMaterial_KR;
    [SerializeField] private Material greenGlowMaterial;
    [SerializeField] private Material greenGlowMaterial_JP;
    [SerializeField] private Material greenGlowMaterial_CN;
    [SerializeField] private Material greenGlowMaterial_KR;
    [SerializeField] private Material redGlowMaterial;
    [SerializeField] private Material redGlowMaterial_JP;
    [SerializeField] private Material redGlowMaterial_CN;
    [SerializeField] private Material redGlowMaterial_KR;

    public enum Language {
        English,
        French,
        German,
        Spanish,
        Japanese,
        Chinese,
        Korean,
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
        public string Korean;
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
        Debug.Log("key " + key  + " "+ translatedText);
        TMP_FontAsset fontToUse = null;

        if (currentLanguage == Language.Japanese) {
            fontToUse = japaneseFont;
        }
        else if (currentLanguage == Language.Chinese) {
            fontToUse = chineseFont;
        }
        else if (currentLanguage == Language.Korean) {
            fontToUse = koreanFont;
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
                    case "Korean": translatedText = entry.Korean; break;
                    default: translatedText = entry.English; break;
                }
                break;
            }
        }

        if (string.IsNullOrEmpty(translatedText)) {
            if (localizationEntry == null) {
                Debug.LogWarning("No localization entry for key " + key);
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
        if (currentLanguage == Language.Korean) return koreanFont;
        return standardFont;
    }

    public Material GetBlueGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return blueGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return blueGlowMaterial_CN;
        if (currentLanguage == Language.Korean) return blueGlowMaterial_KR;
        return blueGlowMaterial;
    }
    public Material GetGreenGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return greenGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return greenGlowMaterial_CN;
        if (currentLanguage == Language.Korean) return greenGlowMaterial_KR;
        return greenGlowMaterial;
    }
    public Material GetRedGlowMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return redGlowMaterial_JP;
        if (currentLanguage == Language.Chinese) return redGlowMaterial_CN;
        if (currentLanguage == Language.Korean) return redGlowMaterial_KR;
        return redGlowMaterial;
    }

    public Material GetStandardMaterial() {
        Language currentLanguage = SettingsManager.Instance.GetLanguage();

        if (currentLanguage == Language.Japanese) return standardMaterial_JP;
        if (currentLanguage == Language.Chinese) return standardMaterial_CN;
        if (currentLanguage == Language.Korean) return standardMaterial_KR;
        return standardMaterial;
    }

}
