using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextSO : ScriptableObject
{
    public enum Language {
        English,
        French,
    }

    public string englishText;
    public string frenchText;


    public string GetTextInLanguage(Language language) {
        if(language == Language.English) {
            return englishText;
        }
        if (language == Language.French) {
            return frenchText;
        }

        return englishText;
    }
}
