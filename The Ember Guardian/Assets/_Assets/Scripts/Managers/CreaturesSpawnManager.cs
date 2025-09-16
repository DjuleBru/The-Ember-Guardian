using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class CreaturesSpawnManager : MonoBehaviour {
    public static CreaturesSpawnManager Instance;

    public class SpawnedCreatureInfo {
        public CreatureSO creature; // Type de créature à spawner
        public SpawnSide spawnSide; // Position de spawn (gauche ou droite)

        public SpawnedCreatureInfo(CreatureSO creature, SpawnSide spawnSide) {
            this.creature = creature;
            this.spawnSide = spawnSide;
        }
    }

    private List<SpecialWaveType> specialWaveTypesInLevel;
    private float specialWaveProbability;
    private int maxSpecialWaveAmount;
    private int currentSpecialWaveAmount;
    private bool hasSpecialWaveTypes;
    public enum SpecialWaveType {
        none,
        crawlers,
        flying,
        ghouls,
    }
    [SerializeField] private Transform nightCreaturesTransformParent;
    [SerializeField] private CreatureSO crawlerCreature;
    [SerializeField] private CreatureSO ghoulCreature;
    private SpecialWaveType currentSpecialWaveType = SpecialWaveType.none;

    public event EventHandler<OnRemainingNightCreaturesChangedEventArgs> OnRemainingNightCreaturesChanged;
    public class OnRemainingNightCreaturesChangedEventArgs : EventArgs {
        public float remainingNightCreaturesNormalized;
    }

    private Dictionary<int, List<SpawnedCreatureInfo>> waveCreaturesDictionary = new Dictionary<int, List<SpawnedCreatureInfo>>();
    private Dictionary<CreatureSO, Queue<Creature>> creaturePools = new Dictionary<CreatureSO, Queue<Creature>>();

    public enum SpawnSide { Left, Right };
    private float spawnDistanceToPlayerOrCamp = 40f;

    private List<CreatureSO> creatureTypes;

    [SerializeField] private List<AnimationCurve> subWaveDifficultyCurveList;

    [SerializeField] private AnimationCurve difficultyAnimationCurve;
    private bool setDifficultyAnimationCurve;
    private int difficultyAtMaxWave;
    private int minDifficultyAtMaxWave;
    private int maxDifficultyAtMaxWave;
    private int maxWavesInAnimationCurve;
    private float cumulativeDifficultyMultiplier = 1f;

    private bool hasBoss;
    private bool bossSpawnsThisNight;
    private CreatureSO bossCreatureType;
    private List<int> bossNightsSpawns;

    private int totalNightCreatures;
    private int remainingNightCreatures;
    private int totalNightCreatureHP;
    private int remainingNightCreaturesHP;
    private int remainingSubWaveCreatures;
    private int maxRemainingSubWaveCreaturesForNextSubwave = 2;

    private int startWaveToSpawnFromBothSides;
    private int baseDifficulty;
    private float growthFactor;
    private float minMaxSubwaveDifficultyGrowthFactor;

    private bool spawnEquallyFromBothSides;
    private bool canSpawnElite;
    private float eliteSpawnProbability = .05f;
    private float bossNightWaveDifficultyMultiplier;

    [SerializeField] private float referenceWaveInitialDifficulty;
    [SerializeField] private float referenceWaveGrowthFactor;
    private float referenceWaveDifficulty;
    private float waveDifficulty;
    private float minSubwaveDifficulty_min;
    private float minSubwaveDifficulty_max;
    private float minSubwaveDifficulty;
    private float maxSubwaveDifficulty_min;
    private float maxSubwaveDifficulty_max;
    private float maxSubwaveDifficulty;
    private float waveDifficultyLeftProportion;
    private float waveDifficultyRightProportion;

    private int typicalLevelDaysToComplete;
    private float maxWaveDifficulty;

    private float minLevelXPosition;
    private float maxLevelXPosition;

    [SerializeField] private int debugInitialWaveNumber;
    private int currentWaveNumber;
    private int subWaveNumber;
    private int subWaveIndex;

    private bool debugInputs;
    private bool debugDontSpawnAtNight;

    private void Awake() {
        Instance = this;

        if (debugInitialWaveNumber != 0) {
            currentWaveNumber = debugInitialWaveNumber;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        LevelSO levelSO = LevelManager.Instance.GetLevelSO();

        creatureTypes = levelSO.nightCreatureTypes;
        baseDifficulty = levelSO.baseDifficulty;
        typicalLevelDaysToComplete = levelSO.typicalLevelDaysToComplete;

        hasBoss = levelSO.hasBoss;
        bossCreatureType = levelSO.bossCreatureType;
        bossNightsSpawns = levelSO.bossNightSpawns;
        bossNightWaveDifficultyMultiplier = levelSO.bossNightWaveDifficultyMultiplier;

        minSubwaveDifficulty_max = levelSO.minSubwaveDifficulty;
        minSubwaveDifficulty_min = levelSO.intialMinSubwaveDifficulty;
        maxSubwaveDifficulty_max = levelSO.maxSubwaveDifficulty;
        maxSubwaveDifficulty_min = levelSO.intialMaxSubwaveDifficulty;

        growthFactor = levelSO.growthFactor;
        minMaxSubwaveDifficultyGrowthFactor = levelSO.minMaxSubwaveDifficultyGrowthFactor;
        startWaveToSpawnFromBothSides = levelSO.startWaveToSpawnFromBothSides;

        setDifficultyAnimationCurve = levelSO.setDifficultyAnimationCurve;
        difficultyAtMaxWave = levelSO.difficultyAtMaxWave;
        difficultyAnimationCurve = levelSO.difficultyAnimationCurve;
        maxWavesInAnimationCurve = levelSO.maxWaveInAnimationCurve;

        hasSpecialWaveTypes = levelSO.hasSpecialWaveTypes;
        specialWaveProbability = levelSO.specialWaveProbability;
        specialWaveTypesInLevel = levelSO.specialWaveTypesInLevel;
        maxSpecialWaveAmount = levelSO.maxSpecialWaveAmount;

        canSpawnElite = levelSO.canSpawnElite;
    }

    private void Start() {
        minLevelXPosition = LevelManager.Instance.GetMinLevelLimit();
        maxLevelXPosition = LevelManager.Instance.GetMaxLevelLimit();

        CreaturesManager.Instance.OnCreatureAtNightKilled += CreaturesManager_OnCreatureAtNightKilled;
        CreaturesManager.Instance.OnCreatureAtNightSpawned += CreaturesManager_OnCreatureAtNightSpawned;
        CreaturesManager.Instance.OnAdditionalCreatureAtNightSpawned += CreaturesManager_OnAdditionalCreatureAtNightSpawned;
        LevelManager.Instance.OnLevelLimitsChanged += LevelManager_OnLevelLimitsChanged;

        debugInputs = DebugManager.Instance.GetAllowDebugInputs_CreaturesSpawnManager();
        debugDontSpawnAtNight = DebugManager.Instance.GetDebugDontSpawnAtNight();

        maxWaveDifficulty = baseDifficulty * Mathf.Pow(typicalLevelDaysToComplete, growthFactor);
        Debug.Log("maxWaveDifficulty " + maxWaveDifficulty);
    }

    private void LevelManager_OnLevelLimitsChanged(object sender, EventArgs e) {
        minLevelXPosition = LevelManager.Instance.GetMinLevelLimit();
        maxLevelXPosition = LevelManager.Instance.GetMaxLevelLimit();
    }

    private void CreaturesManager_OnAdditionalCreatureAtNightSpawned(object sender, CreaturesManager.OnCreatureAtNightKilledEventArgs e) {
        // Coming from creature spawners like Summoner
        remainingNightCreatures++;
        totalNightCreatures++;

        float remainingNightCreaturesNormalized = (float)remainingNightCreatures / (float)totalNightCreatures;

        OnRemainingNightCreaturesChanged?.Invoke(this, new OnRemainingNightCreaturesChangedEventArgs {
            remainingNightCreaturesNormalized = remainingNightCreaturesNormalized
        });
    }

    private void CreaturesManager_OnCreatureAtNightSpawned(object sender, CreaturesManager.OnCreatureAtNightKilledEventArgs e) {
        float remainingNightCreaturesHealthNormalized = (float)remainingNightCreaturesHP / (float)totalNightCreatureHP;
        float remainingNightCreaturesNormalized = (float)remainingNightCreatures / (float)totalNightCreatures;

        OnRemainingNightCreaturesChanged?.Invoke(this, new OnRemainingNightCreaturesChangedEventArgs {
            remainingNightCreaturesNormalized = remainingNightCreaturesNormalized
        });
    }

    private void CreaturesManager_OnCreatureAtNightKilled(object sender, CreaturesManager.OnCreatureAtNightKilledEventArgs e) {
        remainingNightCreaturesHP -= e.creature.GetCreatureSO().maxHealth;
        remainingNightCreatures--;
        //Debug.Log("creature killed : remainingNightCreatures " + remainingNightCreatures);

        float remainingNightCreaturesHealthNormalized = (float)remainingNightCreaturesHP / (float)totalNightCreatureHP;
        float remainingNightCreaturesNormalized = (float)remainingNightCreatures / (float)totalNightCreatures;

        if (remainingSubWaveCreatures > 0) {
            remainingSubWaveCreatures--;
        }

        OnRemainingNightCreaturesChanged?.Invoke(this, new OnRemainingNightCreaturesChangedEventArgs {
            remainingNightCreaturesNormalized = remainingNightCreaturesNormalized
        });
    }

    private void Update() {
        if (!debugInputs) return;
        HandleDebugInputs();
    }

    private void HandleDebugInputs() {
        if (Input.GetKeyDown(KeyCode.U)) {
            currentWaveNumber++;
            SetWaveParameters(currentWaveNumber, true, true);
            //SetTutorialWave();
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("SpawnWave");
            StartCoroutine(SpawnWave(currentWaveNumber));
        }
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        currentWaveNumber++;

        if (spawnEquallyFromBothSides) {

            waveDifficultyLeftProportion = .5f;
            waveDifficultyRightProportion = .5f;
            SetWaveParameters(currentWaveNumber, false, false);

        }
        else {
            SetWaveParameters(currentWaveNumber, true, true);
        }
        SetReferenceWaveParameters(currentWaveNumber);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        if (debugDontSpawnAtNight) return;
        StartCoroutine(SpawnWave(currentWaveNumber));
    }

    public void SetTutorialWave() {
        currentWaveNumber++;
        waveDifficultyLeftProportion = .5f;
        waveDifficultyRightProportion = .5f;

        SetWaveParameters(currentWaveNumber, false, false);
    }

    public void SetDemoWave() {
        currentWaveNumber = debugInitialWaveNumber;

        SetWaveParameters(currentWaveNumber, true, true);
    }

    public void SetWaveParameters(int waveNumber, bool wavesRandomSideProportion, bool subWaveRandomSideProportion) {
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
                Debug.Log("Special wave type selected: " + currentSpecialWaveType);
            }
        }

        if (setDifficultyAnimationCurve) {

            float waveNumberNormalized = (float)(waveNumber) / (float)maxWavesInAnimationCurve;
            if (waveNumberNormalized > 1) {
                waveNumberNormalized = 1;
            }
            waveDifficulty = difficultyAtMaxWave * difficultyAnimationCurve.Evaluate(waveNumberNormalized);

        } else {

            waveDifficulty = baseDifficulty * Mathf.Pow(waveNumber, growthFactor);

        }

        minSubwaveDifficulty = Mathf.Min(minSubwaveDifficulty_min * Mathf.Pow(minMaxSubwaveDifficultyGrowthFactor, waveNumber), minSubwaveDifficulty_max);
        maxSubwaveDifficulty = Mathf.Min(maxSubwaveDifficulty_min * Mathf.Pow(minMaxSubwaveDifficultyGrowthFactor, waveNumber), maxSubwaveDifficulty_max);

        // Appliquer le multiplicateur cumulatif
        waveDifficulty *= cumulativeDifficultyMultiplier;
        minSubwaveDifficulty *= cumulativeDifficultyMultiplier;
        maxSubwaveDifficulty *= cumulativeDifficultyMultiplier;

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

        Debug.Log("waveNumber " + waveNumber);
        Debug.Log("Total subwaves " + subWaveNumber);
        Debug.Log("WaveDifficulty " + waveDifficulty);
        Debug.Log("minSubwaveDifficulty " + minSubwaveDifficulty);
        Debug.Log("maxSubwaveDifficulty " + maxSubwaveDifficulty);
        Debug.Log("waveLeftProportion " + waveDifficultyLeftProportion);
        Debug.Log("waveRightProportion " + waveDifficultyRightProportion);

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

        //Debug.Log("totalNightCreatures " + totalNightCreatures);
        remainingNightCreatures = totalNightCreatures;
    }

    private void SetReferenceWaveParameters(int waveNumber) {
        referenceWaveDifficulty = referenceWaveInitialDifficulty * Mathf.Pow(waveNumber, referenceWaveGrowthFactor);
    }

    private List<SpawnedCreatureInfo> PrepareSubWaveCreatures(float subWaveDifficulty, float leftProportion, int subWaveIndex, bool subWaveRandomSideProportion) {

        Debug.Log("Subwave " + subWaveIndex + " Difficulty " + subWaveDifficulty);

        List<SpawnedCreatureInfo> waveCreatures = new List<SpawnedCreatureInfo>();

        List<CreatureSO> creaturesToSpawn = new List<CreatureSO>();

        if (currentSpecialWaveType != SpecialWaveType.none) {
            creaturesToSpawn = GetSpecialWaveCreatures(currentSpecialWaveType, subWaveDifficulty);
        }
        else {
            creaturesToSpawn = GetCreatureSOListToSpawn(subWaveDifficulty, subWaveIndex);
        }

        int totalCreaturesToSpawn = creaturesToSpawn.Count;

        // Décide d'un spawn à gauche, à droite, ou des deux côtés
        float spawnDecision = .5f;

        if (subWaveRandomSideProportion) {
            spawnDecision = UnityEngine.Random.Range(0f, 1f);
        }

        if (leftProportion == 0) {
            spawnDecision = 1;
        }
        if (leftProportion == 1) {
            spawnDecision = 0;
        }
        int leftMonstersCount = 0;
        int rightMonstersCount = 0;

        if (spawnDecision < 0.2f) // 20% de chance que les monstres viennent seulement de gauche
        {
            leftMonstersCount = totalCreaturesToSpawn;

            for (int i = 0; i < leftMonstersCount; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Left)); // Tous à gauche
            }
        }
        else if (spawnDecision > 0.8f) // 20% de chance que les monstres viennent seulement de droite
        {
            rightMonstersCount = totalCreaturesToSpawn;

            for (int i = 0; i < rightMonstersCount; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Right)); // Tous à droite
            }
        }
        else // 60% de chance de répartir entre gauche et droite
        {
            leftMonstersCount = Mathf.FloorToInt(totalCreaturesToSpawn * leftProportion);

            // Spawns des monstres à gauche
            for (int i = 0; i < leftMonstersCount; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Left));
            }

            // Spawns des monstres à droite
            for (int i = leftMonstersCount; i < totalCreaturesToSpawn; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Right));
            }
        }
        return waveCreatures;
    }

    private List<CreatureSO> GetSpecialWaveCreatures(SpecialWaveType waveType, float difficultyBudget) {
        List<CreatureSO> creaturesToSpawn = new List<CreatureSO>();
        CreatureSO forcedCreature = null;

        switch (waveType) {
            case SpecialWaveType.crawlers:
                forcedCreature = crawlerCreature;
                break;
            case SpecialWaveType.ghouls:
                forcedCreature = ghoulCreature;
                break;
            default:
                Debug.LogWarning("Special wave type not implemented: " + waveType);
                return creaturesToSpawn;

            case SpecialWaveType.flying:
                List<CreatureSO> flyingCreatures = new List<CreatureSO>();
                foreach (CreatureSO creatureSO in creatureTypes) {
                    if(creatureSO.flying) {
                        flyingCreatures.Add(creatureSO);
                    }
                }
                float totalProbability = flyingCreatures.Sum(c => c.spawnProbability);

                while (difficultyBudget > 0) {
                    float randomValue = UnityEngine.Random.Range(0f, totalProbability);
                    float cumulative = 0f;
                    CreatureSO selected = null;

                    foreach (var c in flyingCreatures) {
                        cumulative += c.spawnProbability;
                        if (randomValue <= cumulative) {
                            selected = c;
                            break;
                        }
                    }

                    if (selected != null && difficultyBudget >= selected.difficulty) {
                        creaturesToSpawn.Add(selected);
                        difficultyBudget -= selected.difficulty;
                    }
                    else {
                        break;
                    }
                }

                 break;
        }

        if (forcedCreature == null) return creaturesToSpawn;

        // On dépense tout le budget uniquement en cette créature
        while (difficultyBudget >= forcedCreature.difficulty) {
            creaturesToSpawn.Add(forcedCreature);
            difficultyBudget -= forcedCreature.difficulty;
        }

        return creaturesToSpawn;
    }

    private void SetWaveSidesProportion(int waveNumber) {

        waveDifficultyLeftProportion = UnityEngine.Random.Range(0f, 1f);
        float allFromOneSideTreshold = .1f;

        bool initialWaves = waveNumber < startWaveToSpawnFromBothSides;
        if (initialWaves) {
            float leftOrRightSide = UnityEngine.Random.Range(0f, 1f);
            if (leftOrRightSide > .5f) {
                waveDifficultyLeftProportion = 0;
            }
            else {
                waveDifficultyLeftProportion = 1;
            }
        }
        else {

            if (waveDifficultyLeftProportion < allFromOneSideTreshold) {
                waveDifficultyLeftProportion = 0;
            }
            if (waveDifficultyLeftProportion > (1 - allFromOneSideTreshold)) {
                waveDifficultyLeftProportion = 1;
            }
        }

        waveDifficultyRightProportion = 1 - waveDifficultyLeftProportion;

    }

    private IEnumerator SpawnWave(int waveNumber) {
        Debug.Log("Spawn wave " + waveNumber);
        subWaveIndex = 0;

        while (subWaveIndex < subWaveNumber && waveNumber == currentWaveNumber) {
            Debug.Log("Spawning subwave " + subWaveIndex);
            remainingSubWaveCreatures = waveCreaturesDictionary[subWaveIndex].Count;
            Debug.Log("TotalSubWaveCreatures " + remainingSubWaveCreatures);

            // Grouper les créatures par type
            var groupedCreatures = waveCreaturesDictionary[subWaveIndex]
                .GroupBy(c => c.creature)
                .Select(group => group.ToList()) // Convertit chaque groupe en liste homogène
                .ToList();

            // Mélanger les groupes pour éviter qu'ils ne soient toujours spawnés dans le même ordre
            groupedCreatures = groupedCreatures.OrderBy(g => UnityEngine.Random.value).ToList();

            // File d'attente des groupes pour alterner leur apparition
            Queue<List<SpawnedCreatureInfo>> groupQueue = new Queue<List<SpawnedCreatureInfo>>(groupedCreatures);

            while (groupQueue.Count > 0) {
                List<SpawnedCreatureInfo> currentGroup = groupQueue.Dequeue(); // Prendre un groupe

                // Vérifier la taille max du paquet
                int maxPacketSize = currentGroup[0].creature.maxCreaturesPerPacket;

                if (currentSpecialWaveType == SpecialWaveType.ghouls || currentSpecialWaveType == SpecialWaveType.flying) {
                    maxPacketSize *= 2;
                }
                if (currentSpecialWaveType == SpecialWaveType.crawlers) {
                    maxPacketSize *= 3;
                }

                int chunkSize = currentGroup.Count > maxPacketSize ? maxPacketSize : currentGroup.Count;

                for (int i = 0; i < chunkSize; i++) {
                    SpawnedCreatureInfo creatureInfo = currentGroup[0];
                    currentGroup.RemoveAt(0);

                    SpawnCreatureAtSide(creatureInfo.creature, creatureInfo.spawnSide);
                    yield return new WaitForSeconds(0.2f); // Délai entre les créatures d'un même paquet
                }

                // Si le groupe n'est pas totalement vidé, on le remet dans la file pour plus tard
                if (currentGroup.Count > 0) {
                    groupQueue.Enqueue(currentGroup);
                }

                // Pause plus longue avant de spawn le prochain paquet
                yield return new WaitForSeconds(2f);
            }

            // Attendre que toutes les créatures de cette subwave soient éliminées
            Debug.Log("remainingSubWaveCreatures " + (remainingSubWaveCreatures));
            Debug.Log("maxRemainingSubWaveCreaturesForNextSubwave " + maxRemainingSubWaveCreaturesForNextSubwave);
            yield return new WaitUntil(() => remainingSubWaveCreatures <= maxRemainingSubWaveCreaturesForNextSubwave);

            subWaveIndex++;
        }
    }


    private void SpawnCreatureAtSide(CreatureSO creatureToSpawn, SpawnSide spawnSide) {
        //Creature creature = GetCreatureFromPool(
        //    creatureToSpawn,
        //    GetSpawnPosition(spawnSide, creatureToSpawn),
        //    Quaternion.identity
        //);

        Creature creature = Instantiate(creatureToSpawn.creaturePrefab, GetSpawnPosition(spawnSide, creatureToSpawn), Quaternion.identity, nightCreaturesTransformParent).GetComponent<Creature>();
        creature.SetAsDayCreature(false);
        CreaturesManager.Instance.AddCreatureToNightWave(creature);

        if (!canSpawnElite || creatureToSpawn.isBoss) return;
        float eliteRandomFloat = UnityEngine.Random.Range(0f, 1f);
        if (eliteRandomFloat < eliteSpawnProbability) {
            creature.SetAsEliteCreature();
        }
    }
    private List<CreatureSO> GetCreatureSOListToSpawn(float difficultyBudget, int subWaveIndex) {
        List<CreatureSO> creaturesToSpawn = new List<CreatureSO>();
        List<CreatureSO> creaturesTypesToSpawn = SelectCreatureSOTypes();

        if (creaturesTypesToSpawn.Count == 0) {
            Debug.LogWarning("No monsters selected due to insufficient budget. Exiting loop.");
        }

        // Add boss
        if (hasBoss && subWaveIndex == 0) {
            if (bossNightsSpawns.Contains(currentWaveNumber)) {
                creaturesToSpawn.Add(bossCreatureType);
                difficultyBudget -= bossCreatureType.difficulty;

            };
        }

        // Normaliser les probabilités de spawn
        float totalProbability = creaturesTypesToSpawn.Sum(creature => creature.spawnProbability);

        while (difficultyBudget > 0) {

            // Sélectionner un type de créature aléatoirement selon la probabilité
            float randomValue = UnityEngine.Random.Range(0f, totalProbability);
            CreatureSO selectedCreature = null;

            float cumulativeProbability = 0;
            foreach (var creatureType in creaturesTypesToSpawn) {
                cumulativeProbability += creatureType.spawnProbability;
                if (randomValue <= cumulativeProbability) {
                    selectedCreature = creatureType;
                    break;
                }
            }

            if (selectedCreature != null) {
                // Vérifie si le budget permet de spawner cette créature
                if (difficultyBudget >= selectedCreature.difficulty) {
                    creaturesToSpawn.Add(selectedCreature);
                    difficultyBudget -= selectedCreature.difficulty;
                }
                else {
                    break; // Budget insuffisant pour ajouter d'autres créatures
                }
            }
        }
        return creaturesToSpawn;
    }

    List<CreatureSO> SelectCreatureSOTypes() {
        List<CreatureSO> creaturesThisWave = new List<CreatureSO>();

        foreach (CreatureSO creatureSO in creatureTypes) {
            if (creatureSO.initialWaveSpawn <= currentWaveNumber) {
                creaturesThisWave.Add(creatureSO);
            }
        }

        return creaturesThisWave;
    }

    Vector3 GetSpawnPosition(SpawnSide spawnSide, CreatureSO creatureToSpawn) {
        // Logique pour choisir une position de spawn, par exemple autour d'une zone spécifique
        float xSpawnPosition = 0;
        float spawnDistance = spawnDistanceToPlayerOrCamp;

        if (creatureToSpawn.isBoss) {
            spawnDistance /= 1.5f;
        }


        if (spawnSide == SpawnSide.Left) {
            xSpawnPosition = CampZoneManager.Instance.GetMinZoneLimit() - spawnDistance;

        }
        else {
            xSpawnPosition = CampZoneManager.Instance.GetMaxZoneLimit() + spawnDistance;
        }

        float yPosition = 2f;
        if (creatureToSpawn.flying) {
            yPosition = UnityEngine.Random.Range(creatureToSpawn.flightMinAltitude, creatureToSpawn.flightMaxAltitude);
        }

        if (xSpawnPosition > maxLevelXPosition) {
            xSpawnPosition = maxLevelXPosition - 10f;
        }
        if (xSpawnPosition < minLevelXPosition) {
            xSpawnPosition = minLevelXPosition + 10f;
        }

        return new Vector3(xSpawnPosition + UnityEngine.Random.Range(-4f, 4f), yPosition, 0);

    }

    public void CountCreatureOccurrences(List<SpawnedCreatureInfo> spawnedCreatures) {
        // Créer un dictionnaire pour stocker les occurrences
        Dictionary<(CreatureSO creatureType, string position), int> occurrences = new Dictionary<(CreatureSO, string), int>();

        // Parcourir la liste des créatures spawnées
        foreach (var spawnedCreature in spawnedCreatures) {
            if (spawnedCreature.creature != null) // Vérifie que la créature n'est pas nulle
            {
                // Déterminer la position (left ou right)
                string spawnPosition = spawnedCreature.spawnSide == SpawnSide.Left ? "Left" : "Right";
                // Créer une clé pour le dictionnaire
                var key = (spawnedCreature.creature, spawnPosition);

                // Incrémenter le compteur pour cette créature et cette position
                if (occurrences.ContainsKey(key)) {
                    occurrences[key]++;
                }
                else {
                    occurrences[key] = 1; // Initialiser à 1
                }
            }
            else {
                Debug.LogWarning("SpawnedCreatureInfo contains a null creature.");
            }
        }

        // Afficher les résultats dans le format souhaité
        foreach (var kvp in occurrences) {
            var (creatureType, position) = kvp.Key;
            //Debug.Log($"Creature Type: {creatureType.name}, Spawn Position: {position}, Count: {kvp.Value}");
        }
    }

    public void ApplyPermanentShockwaveEffect(float additionalReductionMultiplier) {
        cumulativeDifficultyMultiplier *= additionalReductionMultiplier;
        Debug.Log("cumulativeDifficultyMultiplier " + cumulativeDifficultyMultiplier);
        SetWaveParameters(currentWaveNumber, true, true);
    }

    public bool GetAllNightCreaturesKilled() {
        return remainingNightCreatures == 0;
    }

    public int GetRemainingSubWavesCreatures() {
        return remainingSubWaveCreatures;
    }

    public int GetTotalPlannedCreaturesForNight(List<SpawnedCreatureInfo> spawnedCreatures) {
        int totalCreatures = 0;

        // Parcourir la liste des créatures spawnées
        foreach (var spawnedCreature in spawnedCreatures) {
            if (spawnedCreature.creature != null) { // Vérifie que la créature n'est pas nulle
                totalCreatures++;
            }
            else {
                Debug.LogWarning("SpawnedCreatureInfo contains a null creature.");
            }
        }

        return totalCreatures;
    }

    public void SetBaseDifficulty(int baseDifficulty) {
        this.baseDifficulty = baseDifficulty;
    }

    public void SetGrowthFactor(float growthFactor) {
        this.growthFactor = growthFactor;
    }
    public void SetMinMaxDifficultyGrowthFactor(float growthFactor) {
        this.minMaxSubwaveDifficultyGrowthFactor = growthFactor;
    }

    public void SetCanSpawnElite(bool canSpawnElite) {
        this.canSpawnElite = canSpawnElite;
    }

    public void SetSpawnEquallyFromBothSides(bool spawnEquallyFromBothSides) {
        this.spawnEquallyFromBothSides = spawnEquallyFromBothSides;
    }
    public void SetMaxRemainingSubwaveCreaturesForNextSubwave(int maxCreatures) {
        maxRemainingSubWaveCreaturesForNextSubwave = maxCreatures;
    }

    public int GetCurrentWaveNumber() {
        return currentWaveNumber;
    }

    public float GetRawCurrentWaveDifficulty() {
        float rawWaveDifficulty = waveDifficulty;
        if (bossSpawnsThisNight) {
            rawWaveDifficulty /= bossNightWaveDifficultyMultiplier;
        }

        Debug.Log("bossSpawnsThisNight " + bossSpawnsThisNight + " rawWaveDifficulty " + rawWaveDifficulty);
        return rawWaveDifficulty;
    }
    public float GetMaxWaveDifficulty() {
        return maxWaveDifficulty;
    }
    public void SetSetDifficultyAnimationCurve() {
        setDifficultyAnimationCurve = true;
    }

    public float GetReferenceWaveDifficulty() {
        return referenceWaveDifficulty;
    }

    public bool GetBossSpawnsThisNight() {
        return bossSpawnsThisNight;
    }

    public Dictionary<CreatureSO, int> GetNextWaveCreaturesBySide(SpawnSide side) {
        Dictionary<CreatureSO, int> creaturesCount = new Dictionary<CreatureSO, int>();

        foreach (var subWave in waveCreaturesDictionary.Values) {
            foreach (var creatureInfo in subWave) {
                if (creatureInfo.spawnSide == side) {
                    if (!creaturesCount.ContainsKey(creatureInfo.creature)) {
                        creaturesCount[creatureInfo.creature] = 1;
                    }
                    else {
                        creaturesCount[creatureInfo.creature]++;
                    }
                }
            }
        }

        return creaturesCount;
    }

    private Creature GetCreatureFromPool(CreatureSO creatureSO, Vector3 position, Quaternion rotation) {
        if (!creaturePools.ContainsKey(creatureSO)) {
            creaturePools[creatureSO] = new Queue<Creature>();
        }

        Queue<Creature> pool = creaturePools[creatureSO];

        Creature creature;
        if (pool.Count > 0 && !pool.Peek().gameObject.activeSelf && !creatureSO.isBoss) {
            creature = pool.Dequeue();
            creature.transform.position = position;
            creature.transform.rotation = rotation;
            creature.gameObject.SetActive(true);
        }
        else {
            creature = Instantiate(creatureSO.creaturePrefab, position, rotation, nightCreaturesTransformParent).GetComponent<Creature>();
        }

        // On remet dans la file pour être réutilisé plus tard
        pool.Enqueue(creature);

        return creature;
    }

}
