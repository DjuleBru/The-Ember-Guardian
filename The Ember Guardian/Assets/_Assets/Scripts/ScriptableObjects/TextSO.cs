using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextSO : ScriptableObject
{

    public string englishText;
    public string frenchText;


    public string GetTextInLanguage(LocalizationManager.Language language) {
        if(language == LocalizationManager.Language.English) {
            return englishText;
        }
        if (language == LocalizationManager.Language.Français) {
            return frenchText;
        }

        return englishText;
    }
}
