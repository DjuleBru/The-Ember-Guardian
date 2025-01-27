using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CreaturesSpawnManager : MonoBehaviour
{
    public static CreaturesSpawnManager Instance;

    [SerializeField] private List<AnimationCurve> subWaveDifficultyCurveList;

    public class SpawnedCreatureInfo {
        public CreatureSO creature; // Type de créature à spawner
        public SpawnSide spawnSide; // Position de spawn (gauche ou droite)

        public SpawnedCreatureInfo(CreatureSO creature, SpawnSide spawnSide) {
            this.creature = creature;
            this.spawnSide = spawnSide;
        }
    }

    public event EventHandler<OnRemainingNightCreaturesChangedEventArgs> OnRemainingNightCreaturesChanged;
    public class OnRemainingNightCreaturesChangedEventArgs : EventArgs {
        public float remainingNightCreaturesNormalized;
    }

    private Dictionary<int, List<SpawnedCreatureInfo>> waveCreaturesDictionary = new Dictionary<int, List<SpawnedCreatureInfo>>();

    public enum SpawnSide { Left, Right };
    private float spawnDistanceToPlayerOrCamp = 30f;

    private List<CreatureSO> creatureTypes;

    private int totalNightCreatures;
    private int remainingNightCreatures;
    private int totalNightCreatureHP;
    private int remainingNightCreaturesHP;

    private int startWaveToSpawnFromBothSides;
    private int baseDifficulty;
    private float growthFactor;
    private float minWaveDuration;
    private float maxWaveDuration;
    private float waveIntensityFactor;
    private float delayBetweenSubWaves;

    private bool canSpawnElite;
    private float eliteSpawnProbability = .05f;

    private float waveDifficulty;
    private float minSubwaveDifficulty;
    private float maxSubwaveDifficulty;
    private float waveDifficultyLeftProportion;
    private float waveDifficultyRightProportion;
    private float waveDuration;

    private int currentWaveNumber;
    private int subWaveNumber;
    private int subWaveIndex;

    private void Awake() {
        Instance = this;

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
        
        creatureTypes = LevelManager.Instance.GetLevelSO().nightCreatureTypes;
        baseDifficulty = LevelManager.Instance.GetLevelSO().baseDifficulty;
        minSubwaveDifficulty = LevelManager.Instance.GetLevelSO().minSubwaveDifficulty;
        maxSubwaveDifficulty = LevelManager.Instance.GetLevelSO().maxSubwaveDifficulty;
        growthFactor = LevelManager.Instance.GetLevelSO().growthFactor;
        minWaveDuration = LevelManager.Instance.GetLevelSO().minWaveDuration;
        maxWaveDuration = LevelManager.Instance.GetLevelSO().maxWaveDuration;
        waveIntensityFactor = LevelManager.Instance.GetLevelSO().waveIntensityFactor;
        delayBetweenSubWaves = LevelManager.Instance.GetLevelSO().delayBetweenSubWaves;
        startWaveToSpawnFromBothSides = LevelManager.Instance.GetLevelSO().startWaveToSpawnFromBothSides;

        canSpawnElite = LevelManager.Instance.GetLevelSO().canSpawnElite;
    }

    private void Start() {
        CreaturesManager.Instance.OnCreatureAtNightKilled += CreaturesManager_OnCreatureAtNightKilled;
        CreaturesManager.Instance.OnCreatureAtNightSpawned += CreaturesManager_OnCreatureAtNightSpawned;
    }

    private void CreaturesManager_OnCreatureAtNightSpawned(object sender, CreaturesManager.OnCreatureAtNightKilledEventArgs e) {
        remainingNightCreaturesHP += e.creature.GetCreatureSO().maxHealth;
        totalNightCreatureHP += e.creature.GetCreatureSO().maxHealth;
        totalNightCreatures++;
        remainingNightCreatures++;

        float remainingNightCreaturesHealthNormalized = (float)remainingNightCreaturesHP / (float)totalNightCreatureHP;
        float remainingNightCreaturesNormalized = (float)remainingNightCreatures / (float)totalNightCreatures;

        OnRemainingNightCreaturesChanged?.Invoke(this, new OnRemainingNightCreaturesChangedEventArgs {
            remainingNightCreaturesNormalized = remainingNightCreaturesNormalized
        });
    }

    private void CreaturesManager_OnCreatureAtNightKilled(object sender, CreaturesManager.OnCreatureAtNightKilledEventArgs e) {
        remainingNightCreaturesHP -= e.creature.GetCreatureSO().maxHealth;
        remainingNightCreatures--;

        float remainingNightCreaturesHealthNormalized = (float)remainingNightCreaturesHP / (float)totalNightCreatureHP;
        float remainingNightCreaturesNormalized = (float)remainingNightCreatures / (float)totalNightCreatures;

        OnRemainingNightCreaturesChanged?.Invoke(this, new OnRemainingNightCreaturesChangedEventArgs {
            remainingNightCreaturesNormalized = remainingNightCreaturesNormalized
        });
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            currentWaveNumber++;
            SetWaveParameters(currentWaveNumber, true, true);
            //SetTutorialWave();
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("SpawnWave");
            StartCoroutine(SpawnWave());
        }
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        currentWaveNumber++;
        SetWaveParameters(currentWaveNumber, true, true);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        StartCoroutine(SpawnWave());
    }

    public void SetTutorialWave() {
        currentWaveNumber++;
        waveDifficultyLeftProportion = .5f;
        waveDifficultyRightProportion = .5f;

        SetWaveParameters(currentWaveNumber, false, false);

    }

    public void SetWaveParameters(int waveNumber, bool wavesRandomSideProportion, bool subWaveRandomSideProportion) {

        waveDifficulty = baseDifficulty * Mathf.Pow(growthFactor, waveNumber);
        waveDuration = Mathf.Lerp(minWaveDuration, maxWaveDuration, (waveNumber-1) / 10f);
        subWaveNumber = (int)(waveDifficulty / maxSubwaveDifficulty)+1;
        AnimationCurve subWaveDifficultyCurve = subWaveDifficultyCurveList[UnityEngine.Random.Range(0, subWaveDifficultyCurveList.Count)];

        if(wavesRandomSideProportion) {
            SetWaveSidesProportion(waveNumber);
        }

        Debug.Log("waveNumber " + waveNumber);
        Debug.Log("WaveDuration " + waveDuration);
        Debug.Log("Total subwaves " + subWaveNumber);
        Debug.Log("WaveDifficulty " + waveDifficulty);
        Debug.Log("waveLeftProportion " + waveDifficultyLeftProportion);
        Debug.Log("waveRightProportion " + waveDifficultyRightProportion);

        DayNightManager.Instance.SetNightDuration(waveDuration*2);

        // Préparer la liste de toutes les créatures à spawner pour cette vague
        waveCreaturesDictionary.Clear();

        for (int i=1 ; i <= subWaveNumber+1; i++) {
            float subWaveDifficultyXNormalized = (float)i / (subWaveNumber + 1);
            Debug.Log("subWaveDifficultyXNormalized " + subWaveDifficultyXNormalized);
            float subWaveDifficulty = subWaveDifficultyCurve.Evaluate(subWaveDifficultyXNormalized) * (waveDifficulty);

            if(subWaveDifficulty < minSubwaveDifficulty) {
                subWaveDifficulty = minSubwaveDifficulty;
            }

            List<SpawnedCreatureInfo> subWaveCreatures = PrepareSubWaveCreatures(subWaveDifficulty, waveDifficultyLeftProportion, i, subWaveRandomSideProportion);
            waveCreaturesDictionary.Add(i, subWaveCreatures);
            CountCreatureOccurrences(subWaveCreatures);
        }


        totalNightCreatureHP = 0;
        totalNightCreatures = 0;
        remainingNightCreaturesHP = 0;
        remainingNightCreatures = 0;
    }

    private void SetWaveSidesProportion(int waveNumber) {

        waveDifficultyLeftProportion = UnityEngine.Random.Range(0f, 1f);
        float allFromOneSideTreshold = .2f;

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

    private IEnumerator SpawnWave() {
        subWaveIndex = 0;
        int spawnedCount = 0;

        while (subWaveIndex != subWaveNumber + 1) {
            // Détermine combien de créatures spawn à chaque intervalle
            subWaveIndex++;

            Debug.Log("subWaveIndex " + subWaveIndex);
            Debug.Log("subWaveNumber" + (subWaveNumber));

            foreach(SpawnedCreatureInfo creatureInfo in waveCreaturesDictionary[subWaveIndex]) {
                SpawnCreatureAtSide(creatureInfo.creature, creatureInfo.spawnSide);
                spawnedCount++;
            }

            yield return new WaitForSeconds(delayBetweenSubWaves); // Délai entre les sous-vagues
        }
    }

    private void SpawnCreatureAtSide(CreatureSO creatureToSpawn, SpawnSide spawnSide) {
        Creature creature = Instantiate(creatureToSpawn.creaturePrefab, GetSpawnPosition(spawnSide, creatureToSpawn), Quaternion.identity).GetComponent<Creature>();
        creature.SetAsDayCreature(false);
        CreaturesManager.Instance.AddCreatureToNightWave(creature);

        if (!canSpawnElite) return;
        float eliteRandomFloat = UnityEngine.Random.Range(0f, 1f);
        if(eliteRandomFloat < eliteSpawnProbability) {
            creature.SetAsEliteCreature();
        }
    }

    private List<CreatureSO> GetCreatureSOListToSpawn(float difficultyBudget) {
        List<CreatureSO> creaturesToSpawn = new List<CreatureSO>();
        List<CreatureSO> creaturesTypesToSpawn = SelectCreatureSOTypes();

        if (creaturesTypesToSpawn.Count == 0) {
            Debug.LogWarning("No monsters selected due to insufficient budget. Exiting loop.");
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

        foreach(CreatureSO creatureSO in creatureTypes) {
            if(creatureSO.initialWaveSpawn <= currentWaveNumber) {
                creaturesThisWave.Add(creatureSO);
            }
        }

        return creaturesThisWave;
    }

    private List<SpawnedCreatureInfo> PrepareSubWaveCreatures(float subWaveDifficulty, float leftProportion, int subWaveIndex, bool subWaveRandomSideProportion) {

        Debug.Log("Subwave " + subWaveIndex + " Difficulty " + subWaveDifficulty);

        List<SpawnedCreatureInfo> waveCreatures = new List<SpawnedCreatureInfo>();

        List<CreatureSO> creaturesToSpawn = GetCreatureSOListToSpawn(subWaveDifficulty);
        int totalCreaturesToSpawn = creaturesToSpawn.Count;

        // Décide d'un spawn à gauche, à droite, ou des deux côtés
        float spawnDecision = .5f;

        if (subWaveRandomSideProportion) {
            spawnDecision = UnityEngine.Random.Range(0f, 1f);
        }

        if(leftProportion == 0) {
            spawnDecision = 1;
        } 
        if(leftProportion == 1) {
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

    Vector3 GetSpawnPosition(SpawnSide spawnSide, CreatureSO creatureToSpawn) {
        // Logique pour choisir une position de spawn, par exemple autour d'une zone spécifique
        float xSpawnPosition = 0;

        if(spawnSide == SpawnSide.Left) {

            if(Player.Instance.transform.position.x < CampZoneManager.Instance.GetCampCenterMinLimit()) {
                xSpawnPosition = Player.Instance.transform.position.x - spawnDistanceToPlayerOrCamp;
            } else {
                xSpawnPosition = CampZoneManager.Instance.GetCampCenterMinLimit() - spawnDistanceToPlayerOrCamp;
            }

        } else {
            if (Player.Instance.transform.position.x > CampZoneManager.Instance.GetCampCenterMaxLimit()) {
                xSpawnPosition = Player.Instance.transform.position.x + spawnDistanceToPlayerOrCamp;
            }
            else {
                xSpawnPosition = CampZoneManager.Instance.GetCampCenterMaxLimit() + spawnDistanceToPlayerOrCamp;
            }
        }

        float yPosition = 1f;
        if (creatureToSpawn.flying) {
            yPosition = UnityEngine.Random.Range(creatureToSpawn.flightMinAltitude, creatureToSpawn.flightMaxAltitude);
        }

        return new Vector3(xSpawnPosition + UnityEngine.Random.Range(-2, 2), yPosition, 0);

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
            Debug.Log($"Creature Type: {creatureType.name}, Spawn Position: {position}, Count: {kvp.Value}");
        }
    }

    public bool GetAllNightCreaturesKilled() {
        return remainingNightCreatures == 0;
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

    public int GetTotalPlannedCreaturesHPForNight(List<SpawnedCreatureInfo> spawnedCreatures) {
        int totalCreatureHP = 0;

        // Parcourir la liste des créatures spawnées
        foreach (var spawnedCreature in spawnedCreatures) {
            if (spawnedCreature.creature != null) { // Vérifie que la créature n'est pas nulle
                totalCreatureHP += spawnedCreature.creature.maxHealth;
            }
            else {
                Debug.LogWarning("SpawnedCreatureInfo contains a null creature.");
            }
        }

        return totalCreatureHP;
    }

}
