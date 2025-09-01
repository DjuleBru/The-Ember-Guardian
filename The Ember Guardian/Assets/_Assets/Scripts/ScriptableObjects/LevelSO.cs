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
        TheLumenHollow,
        City,
        CorruptedCity,
        TheFracturedDistrict,
    }



    public string linkedSceneName;
    public bool isBranchingLevel;
    [ShowIf("isBranchingLevel")]
    public LevelSO requiredLevelSO1;
    [ShowIf("isBranchingLevel")]
    public LevelSO requiredLevelSO2;
    [ShowIf("isBranchingLevel")]
    public LevelSO parallelLevelSO;

    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    public Sprite levelImage;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    public string levelNameLocalizationKey;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    public string levelDescriptionLocalizationKey;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    [Range(0, 10)]
    public int faunaAmount;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    [Range(0, 10)]
    public int scrapAmount;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    [Range(0, 10)]
    public int minesAmount;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    [Range(0, 10)]
    public int chestAmount;
    [BoxGroup("LevelDisplay")]
    [LabelWidth(300)]
    [Range(0, 10)]
    public int wildEmberlingsAmount;

    [BoxGroup("Objective")]
    [LabelWidth(300)]
    public LevelUI_ObjectiveUI.ObjectiveType levelObjectiveType;
    [BoxGroup("Objective")]
    [LabelWidth(300)]
    public LevelUI_ObjectiveUI.ObjectiveType endLevelType;
    [BoxGroup("Objective")]
    [LabelWidth(300)]
    public bool talkToNpcAFterObjective;
    [BoxGroup("Objective")]
    [LabelWidth(300)]
    public int nightsToSurviveAmount = 0;
    [BoxGroup("Objective")]
    [LabelWidth(300)]
    public bool isNewEnvironmentDiscoveryLevel;
    [BoxGroup("Objective")]
    [LabelWidth(300)]
    [ShowIf("isNewEnvironmentDiscoveryLevel")]
    public AudioClip newEnvironmentDiscoveryAudioClip;

    [BoxGroup("Progression")]
    [LabelWidth(300)]
    public List<LevelSO> levelsUnlockedByLevel;
    [BoxGroup("Progression")]
    [LabelWidth(300)]
    public List<HubMerchant.HubMerchantType> merchantsUnlockedInLevel;
    [BoxGroup("Progression")]
    [LabelWidth(300)]
    public MerchantTextLinesSO gemMerchantTextLinesAfterLevel;
    [BoxGroup("Progression")]
    [LabelWidth(300)]
    public List<MerchantTextLinesSO> newMerchantTextLinesAfterLevel;
    [BoxGroup("Progression")]
    [LabelWidth(300)]
    public bool unlocksNewGemType;
    [BoxGroup("Progression")]
    [LabelWidth(300)]
    [ShowIf("unlocksNewGemType")]
    public List<PlayerCurrencies.CurrencyType> newGemTypeUnlockedByLevelList;

    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    public bool isReplayableLevel;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    [ShowIf("isReplayableLevel")]
    public int totalDayCreatureDifficulty;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    public List<CreatureSO> dayCreatureTypes;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    public List<CreatureSO> nightCreatureTypes;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    public bool hasBoss;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    [ShowIf("hasBoss")]
    public CreatureSO bossCreatureType;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    [ShowIf("hasBoss")]
    public List<int> bossNightSpawns;
    [BoxGroup("Creatures")]
    [LabelWidth(300)]
    [ShowIf("hasBoss")]
    public float bossNightWaveDifficultyMultiplier = .33f;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int startWaveToSpawnFromBothSides;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int baseDifficulty;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int intialMinSubwaveDifficulty;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int minSubwaveDifficulty;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int intialMaxSubwaveDifficulty;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int maxSubwaveDifficulty;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public float growthFactor;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int typicalLevelDaysToComplete;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public float minMaxSubwaveDifficultyGrowthFactor;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public bool setDifficultyAnimationCurve;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    [ShowIf("setDifficultyAnimationCurve")]
    public int difficultyAtMaxWave;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    [ShowIf("setDifficultyAnimationCurve")]
    public AnimationCurve difficultyAnimationCurve;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    [ShowIf("setDifficultyAnimationCurve")]
    public int maxWaveInAnimationCurve;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public bool canSpawnElite;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public bool hasSpecialWaveTypes;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public List<CreaturesSpawnManager.SpecialWaveType> specialWaveTypesInLevel;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public float specialWaveProbability;
    [BoxGroup("Creatures/NightWaves")]
    [LabelWidth(300)]
    public int maxSpecialWaveAmount;

    [BoxGroup("Environment")]
    [LabelWidth(300)]
    public LevelEnvironment environmentType;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    public bool hasFog;
    [ShowIf("hasFog")]
    public float fogFrontAlpha;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    [ShowIf("hasFog")]
    public float fogBackAlpha;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    public bool hasWind;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    [ShowIf("hasWind")]
    public WindManager.WindStrength initialWindStrength;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    [ShowIf("hasWind")]
    public List<WindManager.WindStrength> windStrengthsAllowedInLevel;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    public bool hasRain;
    [ShowIf("hasRain")]
    public RainManager.RainIntensity initialRainIntensity;
    [BoxGroup("Environment")]
    [LabelWidth(300)]
    [ShowIf("hasRain")]
    public List<RainManager.RainIntensity> rainIntensitiesAllowedInLevel;

    [BoxGroup("Music")]
    [LabelWidth(300)]
    public List<AudioClip> levelRandomBackgroundTracks;
    [BoxGroup("Music")]
    [LabelWidth(300)]
    public List<AudioClip> levelExplorationTracks;
    [BoxGroup("Music")]
    [LabelWidth(300)]
    public List<AudioClip> levelExplorationTracksStreamerMode;

    public string GetLevelEnvironmentTypeString() {
        return LocalizationManager.Instance.GetLocalizedText(environmentType.ToString());
    }
}
