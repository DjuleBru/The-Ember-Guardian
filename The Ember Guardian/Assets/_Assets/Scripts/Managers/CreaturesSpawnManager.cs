using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CreaturesSpawnManager : MonoBehaviour
{
    public static CreaturesSpawnManager Instance;

    [SerializeField] private AnimationCurve subWaveDifficultyCurve;
    [SerializeField] private int startWaveToSpawnFromBothSides;

    public class SpawnedCreatureInfo {
        public CreatureSO creature; // Type de créature à spawner
        public SpawnSide spawnSide; // Position de spawn (gauche ou droite)

        public SpawnedCreatureInfo(CreatureSO creature, SpawnSide spawnSide) {
            this.creature = creature;
            this.spawnSide = spawnSide;
        }
    }

    private Dictionary<int, List<SpawnedCreatureInfo>> waveCreaturesDictionary = new Dictionary<int, List<SpawnedCreatureInfo>>();

    public enum SpawnSide { Left, Right };
    private float spawnDistanceToPlayerOrCamp = 30f;

    public List<CreatureSO> creatureTypes;

    public int baseDifficulty;
    public float growthFactor;
    public float minWaveDuration;
    public float maxWaveDuration;
    public float waveIntensityFactor;
    public float delayBetweenSubWaves;

    private float waveDifficulty;
    private float waveDifficultyLeftProportion;
    private float waveDifficultyRightProportion;
    private float waveDuration;

    private int currentWaveNumber;
    private int subWaveNumber;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            currentWaveNumber++;
            Debug.Log(currentWaveNumber);
            SetWaveParameters(currentWaveNumber);
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("SpawnWave");
            StartCoroutine(SpawnWave());
        }
    }

    private void Start() {
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
    }
 
    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        currentWaveNumber++;
        SetWaveParameters(currentWaveNumber);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        StartCoroutine(SpawnWave());
    }

    public void SetTutorialWave() {
        currentWaveNumber++;
        SetWaveParameters(currentWaveNumber);
    }

    public void SetWaveParameters(int waveNumber) {
        waveDifficulty = baseDifficulty * Mathf.Pow(growthFactor, waveNumber);
        waveDuration = Mathf.Lerp(minWaveDuration, maxWaveDuration, (waveNumber-1) / 10f);
        subWaveNumber = (int)(waveDuration / delayBetweenSubWaves);

        SetWaveSidesProportion(waveNumber);

        Debug.Log("waveNumber " + waveNumber);
        Debug.Log("WaveDuration " + waveDuration);
        Debug.Log("subWaveNumber " + subWaveNumber);
        Debug.Log("WaveDifficulty " + waveDifficulty);
        Debug.Log("waveDifficultyLeftProportion " + waveDifficultyLeftProportion);
        Debug.Log("waveDifficultyRightProportion " + waveDifficultyRightProportion);

        DayNightManager.Instance.SetNightDuration(waveDuration + waveDuration / 3);

        // Préparer la liste de toutes les créatures à spawner pour cette vague
        waveCreaturesDictionary.Clear();

       for(int i=0 ; i < subWaveNumber; i++) {
            float subWaveDifficulty = subWaveDifficultyCurve.Evaluate((float)i / subWaveNumber) * waveDifficulty;

            List<SpawnedCreatureInfo> subWaveCreatures = PrepareSubWaveCreatures(subWaveDifficulty, waveDifficultyLeftProportion, i);
            waveCreaturesDictionary.Add(i, subWaveCreatures);
            CountCreatureOccurrences(subWaveCreatures);
        }
    }

    private void SetWaveSidesProportion(int waveNumber) {

        waveDifficultyLeftProportion = Random.Range(0f, 1f);
        float allFromOneSideTreshold = .2f;

        bool initialWaves = waveNumber < startWaveToSpawnFromBothSides;
        if (initialWaves) {
            float leftOrRightSide = Random.Range(0f, 1f);
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
        int subWaveIndex = 0;
        int spawnedCount = 0;

        while (subWaveIndex < subWaveNumber) {
            // Détermine combien de créatures spawn à chaque intervalle

            foreach(SpawnedCreatureInfo creatureInfo in waveCreaturesDictionary[subWaveIndex]) {
                SpawnCreatureAtSide(creatureInfo.creature, creatureInfo.spawnSide);
                spawnedCount++;
            }

            yield return new WaitForSeconds(delayBetweenSubWaves); // Délai entre les sous-vagues
            subWaveIndex++;
        }
    }

    private void SpawnCreatureAtSide(CreatureSO creatureToSpawn, SpawnSide spawnSide) {

        Creature creature = Instantiate(creatureToSpawn.creaturePrefab, GetSpawnPosition(spawnSide), Quaternion.identity).GetComponent<Creature>();
        creature.SetAsDayCreature(false);
        CreaturesManager.Instance.AddCreatureToNightWave(creature);
    }

    private List<CreatureSO> GetCreatureSOListToSpawn(float difficultyBudget) {
        List<CreatureSO> creaturesToSpawn = new List<CreatureSO>();

        List<CreatureSO> creaturesTypesToSpawn = SelectCreatureSOTypes();

        if (creaturesTypesToSpawn.Count == 0) {
            Debug.LogWarning("No monsters selected due to insufficient budget. Exiting loop.");
        }

        while (difficultyBudget > 0) {

            foreach (var creatureType in creaturesTypesToSpawn) {

                int maxSpawnCount = Mathf.FloorToInt(difficultyBudget / creatureType.difficulty);
                int spawnCount = Random.Range(1, maxSpawnCount + 1);


                for (int i = 0; i < spawnCount; i++) {

                    creaturesToSpawn.Add(creatureType);
                    difficultyBudget -= creatureType.difficulty;

                    if (difficultyBudget < creatureType.difficulty)
                        break;

                }

                if (difficultyBudget <= 0) break;
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

    private List<SpawnedCreatureInfo> PrepareSubWaveCreatures(float subWaveDifficulty, float leftProportion, int subWaveIndex) {

        Debug.Log("Subwave " + subWaveIndex + " Difficulty " + subWaveDifficulty);

        List<SpawnedCreatureInfo> waveCreatures = new List<SpawnedCreatureInfo>();

        List<CreatureSO> creaturesToSpawn = GetCreatureSOListToSpawn(subWaveDifficulty);
        int totalCreaturesToSpawn = creaturesToSpawn.Count;

        // Décide d'un spawn à gauche, à droite, ou des deux côtés
        float spawnDecision = Random.Range(0f, 1f);

        if(leftProportion == 0) {
            spawnDecision = 1;
        } 
        if(leftProportion == 1) {
            spawnDecision = 0;
        }

        if (spawnDecision < 0.2f) // 20% de chance que les monstres viennent seulement de gauche
        {
            int leftMonstersCount = totalCreaturesToSpawn;

            for (int i = 0; i < leftMonstersCount; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Left)); // Tous à gauche
            }
        }
        else if (spawnDecision > 0.8f) // 20% de chance que les monstres viennent seulement de droite
        {
            int rightMonstersCount = totalCreaturesToSpawn;

            for (int i = 0; i < rightMonstersCount; i++) {
                CreatureSO creatureToSpawn = creaturesToSpawn[i];
                waveCreatures.Add(new SpawnedCreatureInfo(creatureToSpawn, SpawnSide.Right)); // Tous à droite
            }
        }
        else // 60% de chance de répartir entre gauche et droite
        {
            int leftMonstersCount = Mathf.FloorToInt(totalCreaturesToSpawn * leftProportion);

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

    Vector3 GetSpawnPosition(SpawnSide spawnSide) {
        // Logique pour choisir une position de spawn, par exemple autour d'une zone spécifique
        float xSpawnPosition = 0;
        if(spawnSide == SpawnSide.Left) {
            if(Player.Instance.transform.position.x < CampZoneManager.Instance.GetCampCenterMinLimit()) {
                xSpawnPosition = Player.Instance.transform.position.x - spawnDistanceToPlayerOrCamp;
            } else {
                xSpawnPosition = CampZoneManager.Instance.GetCampCenterMinLimit() - spawnDistanceToPlayerOrCamp;
            }
        } else {
            if (Player.Instance.transform.position.x > CampZoneManager.Instance.GetCampCenterMinLimit()) {
                xSpawnPosition = Player.Instance.transform.position.x + spawnDistanceToPlayerOrCamp;
            }
            else {
                xSpawnPosition = CampZoneManager.Instance.GetCampCenterMaxLimit() + spawnDistanceToPlayerOrCamp;
            }
        }

        Debug.Log(xSpawnPosition);
        return new Vector3(xSpawnPosition + Random.Range(-2, 2), 1, 0);

    }

    public void CountCreatureOccurrences(List<SpawnedCreatureInfo> spawnedCreatures) {
        // Créer un dictionnaire pour stocker les occurrences
        Dictionary<(CreatureSO creatureType, string position), int> occurrences = new Dictionary<(CreatureSO, string), int>();

        // Parcourir la liste des créatures spawnées
        foreach (var spawnedCreature in spawnedCreatures) {
            if (spawnedCreature.creature != null) // Vérifie que la créature n'est pas nulle
            {
                // Déterminer la position (left ou right)
                string spawnPosition = spawnedCreature.spawnSide < 0 ? "left" : "right";

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


}
