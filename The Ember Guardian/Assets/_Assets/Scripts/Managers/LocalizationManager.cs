using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static LocalizationManager;
public class LocalizationData {
    public Dictionary<string, Dictionary<string, string>> languages;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    private Dictionary<string, Dictionary<string, string>> localizedTexts;
    public enum Language {
        English,
        Français,
        Deutsch,
        Español,
        Italiano
    }
    [Serializable]
    public class LocalizationEntry {
        public string key;
        public string English;
        public string French;
        public string German;
        public string Spanish;
        public string Italian;
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
            Debug.Log("JSON Content loaded.");

            try {
                // Désérialisation du JSON avec JsonUtility
                localizationData = JsonUtility.FromJson<LocalizationData>(jsonContent);

                if (localizationData != null && localizationData.entries != null) {
                    Debug.Log($"Found {localizationData.entries.Count} entries.");
                    foreach (var entry in localizationData.entries) {
                        Debug.Log($"Key: {entry.key}, French: {entry.French}, English: {entry.English}");
                    }
                }
                else {
                    Debug.LogError("Error: localizationData or entries is null!");
                }
            }
            catch (System.Exception ex) {
                Debug.LogError($"Deserialization error: {ex.Message}");
            }
        }
        else {
            Debug.LogError("Localization file not found in StreamingAssets!");
        }
    }


    public string GetLocalizedText(string key) {
        string translatedText = "";

        foreach (var entry in localizationData.entries) {
            if (entry.key == key) {
                switch (currentLanguage.ToString()) {
                    case "French": translatedText = entry.French;
                        break;
                    case "English": translatedText = entry.English;
                        break;
                    case "German": translatedText = entry.German;
                        break;
                    case "Spanish": translatedText = entry.Spanish;
                        break;
                    case "Italian": translatedText = entry.Italian;
                        break;
                    default: translatedText = entry.English;  // Default language
                        break;
                }
            }
        }
        return translatedText;  // Return the key if no translation is found
    }
}
