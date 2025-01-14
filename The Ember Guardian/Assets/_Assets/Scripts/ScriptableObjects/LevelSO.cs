using Sirenix.OdinInspector;
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
    public bool isNewEnvironmentDiscoveryLevel;
    [ShowIf("isNewEnvironmentDiscoveryLevel")]
    public AudioClip newEnvironmentDiscoveryAudioClip;

    public string linkedSceneName;
    public bool hasFog;
    [ShowIf("hasFog")]
    public float fogFrontAlpha;
    [ShowIf("hasFog")]
    public float fogBackAlpha;

    public int baseDifficulty;
    public int maxSubwaveDifficulty;
    public float growthFactor;
    public float minWaveDuration;
    public float maxWaveDuration;
    public float waveIntensityFactor;
    public float delayBetweenSubWaves;
    public float startWaveToSpawnFromBothSides;

    public List<AudioClip> levelAudioClips;

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
