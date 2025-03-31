using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class LocalizationData {
    public Dictionary<string, Dictionary<string, string>> languages;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    private Dictionary<string, Dictionary<string, string>> localizedTexts;
    public enum Language {
        english,
        french,
        german,
        spanish,
        italian
    }

    private Language currentLanguage;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLanguage = SettingsManager.Instance.GetLanguage();
            LoadLocalization();
        }
        else {
            Destroy(gameObject);
        }
    }


    void LoadLocalization() {
        string filePath = Path.Combine(Application.streamingAssetsPath, "TEG_Localization.json");

        if (File.Exists(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>("{\"languages\":" + dataAsJson + "}");
            localizedTexts = loadedData.languages;
        }
        else {
            Debug.LogError("Fichier de localisation introuvable : " + filePath);
        }
    }

    public string GetLocalizedText(string key) {
        if (localizedTexts != null && localizedTexts.ContainsKey(currentLanguage.ToString()) && localizedTexts[currentLanguage.ToString()].ContainsKey(key)) {
            return localizedTexts[currentLanguage.ToString()][key];
        }
        return "MISSING_TEXT";
    }
}
