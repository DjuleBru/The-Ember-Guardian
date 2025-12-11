using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreaturesSpawnManager_HordeMode : CreaturesSpawnManager
{
    [SerializeField] private List<CreatureSO> nightCreatures_VG;
    [SerializeField] private List<CreatureSO> nightCreatures_CC;
    [SerializeField] private List<CreatureSO> nightCreatures_LH;
    [SerializeField] private List<CreatureSO> nightCreatures_FD;

    [SerializeField] private CreatureSO crawlerCreature_VG;
    [SerializeField] private CreatureSO crawlerCreature_CC;
    [SerializeField] private CreatureSO crawlerCreature_LH;
    [SerializeField] private CreatureSO crawlerCreature_FD; 

    [SerializeField] private CreatureSO ghoulCreature_VG;
    [SerializeField] private CreatureSO ghoulCreature_LH;
    [SerializeField] private CreatureSO ghoulCreature_FD;

    [SerializeField] private CreatureSO bossCreature_VG;
    [SerializeField] private CreatureSO bossCreature_CC;
    [SerializeField] private CreatureSO bossCreature_LH;
    [SerializeField] private CreatureSO bossCreature_FD;

    public enum HordeWaveType {
        Normal,
        Peaceful,
        Extreme
    }
    protected HordeWaveType currentHordeWaveType = HordeWaveType.Normal;

    [SerializeField] protected int minExtremeInterval = 3;
    [SerializeField] protected int maxExtremeInterval = 5;

    [SerializeField] protected int minPeacefulInterval = 2;
    [SerializeField] protected int maxPeacefulInterval = 4;

    public event EventHandler OnExtremeWavePrepared;
    public event EventHandler OnPeacefulWavePrepared;
    public event EventHandler OnBossWavePrepared;

    protected float extremeWaveDifficultyMultiplier = 2f;
    protected float extremeSubWaveDifficultyMultiplier = 1.5f;
    protected float peacefulWaveDifficultyMultiplier = .25f;
    protected float peacefulSubWaveDifficultyMultiplier = .5f;

    protected int nightsSinceLastExtreme;
    protected int nightsSinceLastPeaceful;

    protected int nextExtremeAt = 0;
    protected int nextPeacefulAt = 0;

    protected override void Awake() {
        base.Awake();

        if (!SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            InitHordeIntervals();
        }
    }

    protected override void Start() {
        base.Start();
        SetNightCreaturesPool();
    }

    private void InitHordeIntervals() {
        nightsSinceLastExtreme = 0;
        nightsSinceLastPeaceful = 0;

        nextExtremeAt = UnityEngine.Random.Range(minExtremeInterval, maxExtremeInterval + 1);
        nextPeacefulAt = UnityEngine.Random.Range(minPeacefulInterval, maxPeacefulInterval + 1);
    }

    private void SetNightCreaturesPool() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();
        creatureTypes = nightCreatures_VG;
        crawlerCreature = crawlerCreature_VG;
        ghoulCreature = ghoulCreature_VG;
        bossCreatureType = bossCreature_VG;

        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            creatureTypes = nightCreatures_LH;
            crawlerCreature = crawlerCreature_LH;
            ghoulCreature = ghoulCreature_LH;
            bossCreatureType = bossCreature_LH;
        }
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            creatureTypes = nightCreatures_CC;
            crawlerCreature = crawlerCreature_CC;
            bossCreatureType = bossCreature_CC;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            creatureTypes = nightCreatures_FD;
            crawlerCreature = crawlerCreature_FD;
            ghoulCreature = ghoulCreature_FD;
            bossCreatureType = bossCreature_FD;
        }
    }

    public override void SetWaveParameters(int waveNumber, bool wavesRandomSideProportion, bool subWaveRandomSideProportion) {
        DetermineHordeWaveType();

        totalNightCreatures = 0;
        bossSpawnsThisNight = false;
        if (hasBoss) {
            bossSpawnsThisNight = bossNightsSpawns.Contains(currentWaveNumber);
        }

        currentSpecialWaveType = SpecialWaveType.none;
        if (hasSpecialWaveTypes && !bossSpawnsThisNight && currentSpecialWaveAmount < maxSpecialWaveAmount) {
            float roll = UnityEngine.Random.Range(0f, 1f);
            if (roll < specialWaveProbability) {
                currentSpecialWaveType = specialWaveTypesInLevel[UnityEngine.Random.Range(0, specialWaveTypesInLevel.Count)];
                currentSpecialWaveAmount++;
            }
        }

        if (setDifficultyAnimationCurve) {

            float waveNumberNormalized = (float)(waveNumber) / (float)maxWavesInAnimationCurve;
            if (waveNumberNormalized > 1) {
                waveNumberNormalized = 1;
            }
            waveDifficulty = difficultyAtMaxWave * difficultyAnimationCurve.Evaluate(waveNumberNormalized);

        }
        else {

            waveDifficulty = baseDifficulty * Mathf.Pow(waveNumber, growthFactor);

        }

        minSubwaveDifficulty = Mathf.Min(minSubwaveDifficulty_min * Mathf.Pow(minMaxSubwaveDifficultyGrowthFactor, waveNumber), minSubwaveDifficulty_max);
        maxSubwaveDifficulty = Mathf.Min(maxSubwaveDifficulty_min * Mathf.Pow(minMaxSubwaveDifficultyGrowthFactor, waveNumber), maxSubwaveDifficulty_max);

        // Appliquer le multiplicateur cumulatif
        waveDifficulty *= cumulativeDifficultyMultiplier;
        minSubwaveDifficulty *= cumulativeDifficultyMultiplier;
        maxSubwaveDifficulty *= cumulativeDifficultyMultiplier;

        // Appliquer le multiplicateur de difficullté
        waveDifficulty *= currentNightWaveDifficultyMultiplier;
        minSubwaveDifficulty *= currentNightWaveDifficultyMultiplier;
        maxSubwaveDifficulty *= currentNightWaveDifficultyMultiplier;

        // APPLIQUER LE MULTIPLICATEUR DE TYPE DE VAGUE
        if (currentHordeWaveType == HordeWaveType.Extreme) {
            Debug.Log("EXTREME WAVE BEFORE" + waveDifficulty);
            waveDifficulty *= extremeWaveDifficultyMultiplier;
            minSubwaveDifficulty *= extremeSubWaveDifficultyMultiplier;
            maxSubwaveDifficulty *= extremeSubWaveDifficultyMultiplier;
            Debug.Log("EXTREME WAVE AFTER" + waveDifficulty);
        }
        if (currentHordeWaveType == HordeWaveType.Peaceful) {
            Debug.Log("PEACEFUL WAVE BEFORE" + waveDifficulty);
            waveDifficulty *= peacefulWaveDifficultyMultiplier;
            minSubwaveDifficulty *= peacefulSubWaveDifficultyMultiplier;
            maxSubwaveDifficulty *= peacefulSubWaveDifficultyMultiplier;
            Debug.Log("PEACEFUL WAVE AFTER" + waveDifficulty);
        }

        // Add boss
        if (hasBoss && bossSpawnsThisNight) {
            Debug.Log("WaveDifficultyBeforeAddingBoss " + waveDifficulty);

            waveDifficulty *= bossNightWaveDifficultyMultiplier;
            minSubwaveDifficulty *= bossNightWaveDifficultyMultiplier;
            maxSubwaveDifficulty *= bossNightWaveDifficultyMultiplier;
        }

        subWaveNumber = (int)(waveDifficulty / maxSubwaveDifficulty) + 1;
        AnimationCurve subWaveDifficultyCurve = subWaveDifficultyCurveList[UnityEngine.Random.Range(0, subWaveDifficultyCurveList.Count)];

        if (wavesRandomSideProportion) {
            SetWaveSidesProportion(waveNumber);

            if (waveDifficultyLeftProportion == 0 || waveDifficultyRightProportion == 0) {
                // All creatures from ONE side : reduce difficulty
                //waveDifficulty = waveDifficulty / 1.25f;
            }
        }

        if (DebugManager.Instance.GetLogNightWavesData()) {
            Debug.Log("waveNumber " + waveNumber);
            Debug.Log("Total subwaves " + subWaveNumber);
            Debug.Log("WaveDifficulty " + waveDifficulty);
            Debug.Log("minSubwaveDifficulty " + minSubwaveDifficulty);
            Debug.Log("maxSubwaveDifficulty " + maxSubwaveDifficulty);
            Debug.Log("waveLeftProportion " + waveDifficultyLeftProportion);
            Debug.Log("waveRightProportion " + waveDifficultyRightProportion);
        }

        // Préparer la liste de toutes les créatures à spawner pour cette vague
        waveCreaturesDictionary.Clear();
        List<float> subWaveDifficultiesRelative = new List<float>();

        for (int i = 0; i < subWaveNumber; i++) {
            float subWaveDifficultyXNormalized = (float)i / (subWaveNumber + 1);
            float subWaveDifficulty = subWaveDifficultyCurve.Evaluate(subWaveDifficultyXNormalized);
            subWaveDifficultiesRelative.Add(subWaveDifficulty);
        }

        float totalSubWaveDifficultiesAbsolute = subWaveDifficultiesRelative.Sum();
        for (int i = 0; i < subWaveNumber; i++) {

            float subWaveDifficultyAbsolute = (waveDifficulty / totalSubWaveDifficultiesAbsolute) * subWaveDifficultiesRelative[i];
            if (subWaveDifficultyAbsolute < minSubwaveDifficulty) {
                subWaveDifficultyAbsolute = minSubwaveDifficulty;
            }
            if (subWaveDifficultyAbsolute > maxSubwaveDifficulty) {
                subWaveDifficultyAbsolute = maxSubwaveDifficulty;
            }

            List<SpawnedCreatureInfo> subWaveCreatures = PrepareSubWaveCreatures(subWaveDifficultyAbsolute, waveDifficultyLeftProportion, i, subWaveRandomSideProportion);
            waveCreaturesDictionary.Add(i, subWaveCreatures);
            CountCreatureOccurrences(subWaveCreatures);

            totalNightCreatures += GetTotalPlannedCreaturesForNight(subWaveCreatures);
        }


        totalNightCreatureHP = 0;
        remainingNightCreaturesHP = 0;

        remainingNightCreatures = totalNightCreatures;

        InvokeOnNightWaveDifficultyChange();
    }

    protected void DetermineHordeWaveType() {
        bool bossWave = false;

        // Boss night : override
        if (hasBoss && bossNightsSpawns.Contains(currentWaveNumber)) {
            currentHordeWaveType = HordeWaveType.Normal;
            nightsSinceLastExtreme--;
            OnBossWavePrepared?.Invoke(this, EventArgs.Empty);

            bossWave = true;
            StartCoroutine(IncrementIndexesAfterDelay(bossWave, false, false));
            return;
        }

        // impossible d'être peaceful et extreme le même jour
        bool hitExtreme = nightsSinceLastExtreme >= nextExtremeAt;
        bool hitPeaceful = nightsSinceLastPeaceful >= nextPeacefulAt;

        //Debug.Log("hitExtreme " + hitExtreme);
        //Debug.Log("hitPeaceful " + hitPeaceful);

        if (hitExtreme) {
            currentHordeWaveType = HordeWaveType.Extreme;
            OnExtremeWavePrepared?.Invoke(this, EventArgs.Empty);
            StartCoroutine(IncrementIndexesAfterDelay(bossWave, hitExtreme, hitPeaceful));
            return;
        }

        if (hitPeaceful) {
            currentHordeWaveType = HordeWaveType.Peaceful;
            OnPeacefulWavePrepared?.Invoke(this, EventArgs.Empty);
            StartCoroutine(IncrementIndexesAfterDelay(bossWave, hitExtreme, hitPeaceful));
            return;
        }

        // Sinon : Normal
        currentHordeWaveType = HordeWaveType.Normal;

        //Debug.Log("currentHordeWaveType " + currentHordeWaveType);
    }

    private IEnumerator IncrementIndexesAfterDelay(bool bossWave, bool hitExtreme, bool hitPeaceful) {
        yield return new WaitForSeconds(2f);

        if (bossWave) {
            nightsSinceLastExtreme--;
            yield break;
        }

        if (hitExtreme) {
            nightsSinceLastExtreme = 0;
            nextExtremeAt = UnityEngine.Random.Range(minExtremeInterval, maxExtremeInterval + 1);

        } else {
            nightsSinceLastExtreme++;
        }

        if(hitPeaceful) {
            nightsSinceLastPeaceful = 0;
            nextPeacefulAt = UnityEngine.Random.Range(minPeacefulInterval, maxPeacefulInterval + 1);

        } else {
            nightsSinceLastPeaceful++;

        }
    }

    public void LoadSaveData(HordeMapSaveData saveData) {
        nightsSinceLastExtreme = saveData.nightsSinceLastExtreme;
        nightsSinceLastPeaceful = saveData.nightsSinceLastPeaceful;
        nextPeacefulAt = saveData.nextPeacefulAt;
        nextExtremeAt = saveData.nextExtremeAt;
    }

    public int GetNightsSinceLastExtreme() {
        return nightsSinceLastExtreme;
    }
    public int GetNightsSinceLastPeaceful() {
        return nightsSinceLastPeaceful;
    }
    public int GetNextExtremeAt() {
        return nextExtremeAt;
    }
    public int GetNextPeacefulAt() {
        return nextPeacefulAt;
    }

}
