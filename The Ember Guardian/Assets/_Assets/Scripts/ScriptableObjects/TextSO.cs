using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextSO : ScriptableObject
{

    public string englishText;
    public string frenchText;


    public string GetTextInLanguage(SettingsManager.Language language) {
        if(language == SettingsManager.Language.English) {
            return englishText;
        }
        if (language == SettingsManager.Language.French) {
            return frenchText;
        }

        return englishText;
    }
}
