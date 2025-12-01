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

    private int baseDifficulty = 50;

    private List<CreatureSO> dayCreatures;

    public static DayCreatureSpawnerManager_HordeMode Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        dayCreatures = GetEnvironmentCreatureSOList(HordeModeCustomizationManager.Instance.GetSelectedEnvironment());

        baseDifficulty *= Mathf.RoundToInt(GetEnvironmentDifficultyMultiplier(HordeModeCustomizationManager.Instance.GetSelectedEnvironment()));
    }

    public void GenerateSpawnersForBlock(HordeModeBlock block) {
        if (block == null) return;

        // Déterminer la difficulté du bloc
        float difficultyMultiplier = GetDifficultyMultiplier(block);
        int blockDifficulty = Mathf.CeilToInt(difficultyMultiplier * baseDifficulty); // Ajustable selon ton système

        BlockSize size = block.GetBlockSize();
        // Déterminer combien de types de créatures sur ce bloc
        int numCreatureTypes = Random.Range(GetMinCreatureAmountPerBlockSize(size), GetMaxCreatureAmountPerBlockSize(size)+1);

        // Mélanger la liste des créatures
        List<CreatureSO> availableCreatures = new List<CreatureSO>(dayCreatures);
        Shuffle(availableCreatures);

        List<CreatureSO> selectedCreatures = availableCreatures.GetRange(0, numCreatureTypes);

        // Répartir le nombre de créatures entre les types
        int remainingDifficulty = blockDifficulty;
        List<int> spawnAmounts = new List<int>();
        List<int> eliteAmounts = new List<int>();

        for (int i = 0; i < selectedCreatures.Count; i++) {
            int amount = Random.Range(1, remainingDifficulty + 1);
            if (i == selectedCreatures.Count - 1)
                amount = remainingDifficulty; // tout ce qui reste sur le dernier

            remainingDifficulty -= amount;

            int spawnAmount = Mathf.Max(1, Mathf.FloorToInt(amount / selectedCreatures[i].difficulty));

            bool hasElite = UnityEngine.Random.value > .1f;
            int eliteAmount = 0;
            if (hasElite) {
                eliteAmount = Mathf.FloorToInt(spawnAmount * 0.2f); // 10/20% élites si elite sélectionné (hasElite 10%)
            }

            spawnAmounts.Add(spawnAmount);
            eliteAmounts.Add(eliteAmount);
        }

        Debug.Log(block + " baseDifficulty " + blockDifficulty);

        // Générer les spawners dynamiquement
        block.GenerateCreatureSpawners(selectedCreatures, spawnAmounts, eliteAmounts);
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

    public float GetEnvironmentDifficultyMultiplier(LevelSO.LevelEnvironment env) {
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            return 1.33f;
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return 1.66f;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return 2f;
        }
        return 1;
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
