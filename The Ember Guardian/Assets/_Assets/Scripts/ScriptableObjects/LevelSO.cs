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
        SacredTemple,
        City,
        CorruptedCity,
    }

    public List<LevelSO> levelsUnlockedByLevel;
    public List<HubMerchant.HubMerchantType> merchantsUnlockedInLevel;
    public MerchantTextLinesSO gemMerchantTextLinesAfterLevel;
    public List<MerchantTextLinesSO> newMerchantTextLinesAfterLevel;

    public LevelEnvironment environmentType;
    public LevelUI_ObjectiveUI.ObjectiveType levelObjectiveType;
    public LevelUI_ObjectiveUI.ObjectiveType endLevelType;
    public bool talkToNpcAFterObjective;
    public int nightsToSurviveAmount = 0;
    public List<CreatureSO> nightCreatureTypes;
    public bool hasBoss;
    [ShowIf("hasBoss")]
    public CreatureSO bossCreatureType;
    [ShowIf("hasBoss")]
    public List<int> bossNightSpawns;

    public bool isReplayableLevel;
    [ShowIf("isReplayableLevel")]
    public List<CreatureSO> dayCreatureTypes;
    [ShowIf("isReplayableLevel")]
    public int totalDayCreatureDifficulty;

    public bool isNewEnvironmentDiscoveryLevel;
    [ShowIf("isNewEnvironmentDiscoveryLevel")]
    public AudioClip newEnvironmentDiscoveryAudioClip;

    public string linkedSceneName;
    public bool hasFog;
    [ShowIf("hasFog")]
    public float fogFrontAlpha;
    [ShowIf("hasFog")]
    public float fogBackAlpha;
    public bool hasWind;
    [ShowIf("hasWind")]
    public WindManager.WindStrength initialWindStrength;
    [ShowIf("hasWind")]
    public List<WindManager.WindStrength> windStrengthsAllowedInLevel;
    public bool hasRain;
    [ShowIf("hasRain")]
    public RainManager.RainIntensity initialRainIntensity;
    [ShowIf("hasRain")]
    public List<RainManager.RainIntensity> rainIntensitiesAllowedInLevel;

    public int baseDifficulty;
    public int intialMinSubwaveDifficulty;
    public int minSubwaveDifficulty;
    public int intialMaxSubwaveDifficulty;
    public int maxSubwaveDifficulty;
    public float growthFactor;
    public bool setDifficultyAnimationCurve;
    [ShowIf("setDifficultyAnimationCurve")]
    public AnimationCurve difficultyAnimationCurve;
    [ShowIf("setDifficultyAnimationCurve")]
    public int maxWaveInAnimationCurve;
    [ShowIf("setDifficultyAnimationCurve")]
    public int difficultyAtMaxWave;
    [ShowIf("setDifficultyAnimationCurve")]
    public int minDifficultyAtMaxWave;
    [ShowIf("setDifficultyAnimationCurve")]
    public int maxDifficultyAtMaxWave;
    public float minMaxSubwaveDifficultyGrowthFactor;
    public int startWaveToSpawnFromBothSides;
    public bool canSpawnElite;

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
