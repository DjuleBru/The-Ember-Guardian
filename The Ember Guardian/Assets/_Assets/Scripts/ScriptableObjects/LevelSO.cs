using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public enum LevelEnvironment {
        TheLostGreens,
        TheVerdantGraveyard,
        Gloomspire,
        EmberheartSanctum,
    }

    public LevelEnvironment environmentType;
    public LevelUI_ObjectiveUI.ObjectiveType levelObjectiveType;
    public List<CreatureSO> nightCreatureTypes;
    public string linkedSceneName;
    public float fogFrontAlpha;
    public float fogBackAlpha;

    public string GetLevelEnvironmentTypeString() {
        if(environmentType == LevelEnvironment.TheLostGreens) {
            return "The Lost Greens";
        };
        if (environmentType == LevelEnvironment.TheVerdantGraveyard) {
            return "The Verdant Graveyard";
        };
        return "";
    }
}
