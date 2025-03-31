using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextSO : ScriptableObject
{

    public string englishText;
    public string frenchText;


    public string GetTextInLanguage(LocalizationManager.Language language) {
        if(language == LocalizationManager.Language.english) {
            return englishText;
        }
        if (language == LocalizationManager.Language.french) {
            return frenchText;
        }

        return englishText;
    }
}
