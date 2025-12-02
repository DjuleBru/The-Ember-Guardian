using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCreatureSpawnerManager_HordeMode : MonoBehaviour {
    [SerializeField] private AnimationCurve distanceDifficultyCurve; // Courbe pour ajuster la difficulté selon la distance au centre
    [SerializeField] private int minCreatureTypesPerBlock = 1;
    [SerializeField] private int maxCreatureTypesPerBlock = 3;

    [SerializeField] private List<CreatureSO> creaturesSOList_VG;
    [SerializeField] private List<CreatureSO> creaturesSOList_CC;
    [SerializeField] private List<CreatureSO> creaturesSOList_LH;
    [SerializeField] private List<CreatureSO> creaturesSOList_FD;

    [SerializeField] private List<CreatureSO> fallbackCreaturesSOList;
    [SerializeField] private List<CreatureSO> fallbackCreaturesSOList_VG;
    [SerializeField] private List<CreatureSO> fallbackCreaturesSOList_CC;
    [SerializeField] private List<CreatureSO> fallbackCreaturesSOList_LH;
    [SerializeField] private List<CreatureSO> fallbackCreaturesSOList_FD;

    private int baseDifficulty = 50;

    private List<CreatureSO> dayCreatures;

    public static DayCreatureSpawnerManager_HordeMode Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        dayCreatures = GetEnvironmentCreatureSOList(HordeModeCustomizationManager.Instance.GetSelectedEnvironment());
        fallbackCreaturesSOList = GetEnvironmentFallbackCreatureSOList(HordeModeCustomizationManager.Instance.GetSelectedEnvironment());
    }

    private void GenerateSpawnersForBlock(HordeModeBlock block) {
        if (block == null) return;

        float difficultyMultiplier = GetDifficultyMultiplier(block);
        int blockDifficulty = Mathf.CeilToInt(difficultyMultiplier * baseDifficulty);

        BlockSize size = block.GetBlockSize();
        int numCreatureTypes = Random.Range(GetMinCreatureAmountPerBlockSize(size), GetMaxCreatureAmountPerBlockSize(size) + 1);

        // Sélection des types
        List<CreatureSO> selected = new List<CreatureSO>(dayCreatures);
        Shuffle(selected);
        if (numCreatureTypes > selected.Count) numCreatureTypes = selected.Count;
        selected = selected.GetRange(0, numCreatureTypes);

        List<CreatureSO> finalCreatureList = new List<CreatureSO>();
        List<int> finalSpawnAmounts = new List<int>();
        List<int> finalEliteAmounts = new List<int>();

        int remainingDifficulty = blockDifficulty;

        foreach (CreatureSO type in selected) {
            if (remainingDifficulty <= 0) break;

            int maxPacket = Mathf.Max(1, type.maxCreaturesPerPacket_Day);
            int spawnAmount = Mathf.FloorToInt(remainingDifficulty / type.difficulty);

            if (spawnAmount > maxPacket)
                spawnAmount = maxPacket;

            if (spawnAmount > 0) {
                finalCreatureList.Add(type);
                finalSpawnAmounts.Add(spawnAmount);
                finalEliteAmounts.Add(Mathf.FloorToInt(spawnAmount * 0.2f));
                remainingDifficulty -= spawnAmount * type.difficulty;
            }
        }

        // ---- COMPENSATION AVEC FALLBACK ----
        if (remainingDifficulty > 0) {
            Shuffle(fallbackCreaturesSOList);

            foreach (CreatureSO fb in fallbackCreaturesSOList) {
                if (remainingDifficulty <= 0) break;

                int spawnAmount = Mathf.FloorToInt(remainingDifficulty / fb.difficulty);

                if (spawnAmount <= 0)
                    continue;

                finalCreatureList.Add(fb);
                finalSpawnAmounts.Add(spawnAmount);
                finalEliteAmounts.Add(Mathf.FloorToInt(spawnAmount * 0.2f));

                remainingDifficulty -= spawnAmount * fb.difficulty;
            }
        }

        block.GenerateCreatureSpawners(finalCreatureList, finalSpawnAmounts, finalEliteAmounts);
    }

    public void StartGeneratingSpawners(HordeModeBlock block) {
        StartCoroutine(GenerateSpawnersForBlockAfterFrame(block));
    }

    private IEnumerator GenerateSpawnersForBlockAfterFrame(HordeModeBlock block) {
        yield return new WaitForEndOfFrame();
        GenerateSpawnersForBlock(block);
    }

    public float GetDifficultyMultiplier(HordeModeBlock block) {
        float sizeMult = GetSizeMultiplier(block.GetBlockSize());
        float typeMult = 1f;

        if (block.HasBlockType(BlockType.CombatOnly)) typeMult = 1f;
        else if (block.HasBlockType(BlockType.None)) typeMult = 0f;
        else if (block.HasBlockType(BlockType.ResourceChest)) typeMult = .5f;
        else if (block.HasBlockType(BlockType.Scavengables)) typeMult = .5f;
        else if (block.HasBlockType(BlockType.Animals)) typeMult = .5f;

        float distance = Mathf.Abs(block.transform.position.x);
        float maxDistance = 300f;
        if(distance > maxDistance) distance = maxDistance;

        float distanceMult = distanceDifficultyCurve.Evaluate(distance/maxDistance);

        return sizeMult * typeMult * distanceMult;
    }

    public float GetSizeMultiplier(BlockSize size) {
        switch (size) {
            case BlockSize.Tiny: return 0.5f;
            case BlockSize.Small: return 0.7f;
            case BlockSize.Medium: return 1f;
            case BlockSize.Big: return 1.3f;
            default: return 1f;
        }
    }

    private void Shuffle<T>(List<T> list) {
        for (int i = 0; i < list.Count; i++) {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public List<CreatureSO> GetEnvironmentCreatureSOList(LevelSO.LevelEnvironment env) {
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            return creaturesSOList_CC;
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return creaturesSOList_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return creaturesSOList_FD;
        }
        return creaturesSOList_VG;
    }

    public List<CreatureSO> GetEnvironmentFallbackCreatureSOList(LevelSO.LevelEnvironment env) {
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            return fallbackCreaturesSOList_CC;
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return fallbackCreaturesSOList_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return fallbackCreaturesSOList_FD;
        }
        return fallbackCreaturesSOList_VG;
    }

    public int GetMinCreatureAmountPerBlockSize(BlockSize size) {
        switch (size) {
            case BlockSize.Tiny: return 1;
            case BlockSize.Small: return 1;
            case BlockSize.Medium: return 2;
            case BlockSize.Big: return 3;
            default: return 1;
        }
    }

    public int GetMaxCreatureAmountPerBlockSize(BlockSize size) {
        switch (size) {
            case BlockSize.Tiny: return 1;
            case BlockSize.Small: return 2;
            case BlockSize.Medium: return 4;
            case BlockSize.Big: return 5;
            default: return 1;
        }
    }
}
