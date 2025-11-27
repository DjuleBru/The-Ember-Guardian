using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BlockSize {
    Starting,
    Tiny,
    Small,
    Medium,
    Big
}

[System.Serializable]
public class BlockDefinition {
    public BlockSize size;
    public GameObject prefab;
    public float baseWeight = 1f;
}

public class HordeModeMapGenerationManager : MonoBehaviour {

    public static HordeModeMapGenerationManager Instance;

    [Header("Blocks")]
    public List<BlockDefinition> blockDefinitions;

    [Header("Generation Settings")]
    public int blocksPerSide = 5;
    public float spacingFromCenter = 4f;

    private Dictionary<BlockSize, float> sizeWeights = new();
    private List<BlockSize> sizeHistory = new();
    private const int historyLimit = 3;

    private List<HordeModeBlock> leftBlocks = new List<HordeModeBlock>();
    private List<HordeModeBlock> rightBlocks = new List<HordeModeBlock>();

    private int distanceToForceSmallBlocks = 2;
    private int rightDistanceIndex = 0;
    private int leftDistanceIndex = 0;

    private int gunChestBlocksSpawned;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        InitializeWeights();

        GenerateSide(+1, rightBlocks);
        GenerateSide(-1, leftBlocks);

        // Maintenant qu’on a les blocs initiaux, on assigne pour chaque distance
        for (int dist = 1; dist <= blocksPerSide; dist++) {
            AssignDistanceBlockTypesDynamic(dist);
        }
    }

    void InitializeWeights() {
        foreach (var b in blockDefinitions) {
            if(b.size == BlockSize.Medium) {
                sizeWeights[b.size] = .8f;
                continue;
            }
            if (b.size == BlockSize.Big) {
                sizeWeights[b.size] = .5f;
                continue;
            }
            sizeWeights[b.size] = b.baseWeight;
        }
    }

    void GenerateSide(int direction, List<HordeModeBlock> storeList) {
        Vector3 pos = Vector3.right * spacingFromCenter * direction;

        for (int i = 0; i < blocksPerSide; i++) {
            BlockDefinition chosen = PickNextSize(i+1);
            if (chosen == null)
                return;

            GameObject blockObj = Instantiate(chosen.prefab, pos, Quaternion.identity, transform);
            HordeModeBlock block = blockObj.GetComponent<HordeModeBlock>();

            block.SetDir(direction);

            storeList.Add(block);

            float spacing = block != null ? block.GetTotalSpacing() : spacingFromCenter;
            pos += Vector3.right * spacing * direction;

            RegisterSizeHistory(chosen.size);
        }
    }

    public void PlayerEnteredBlock(HordeModeBlock block) {
        // RIGHT SIDE
        if (rightBlocks.Count > 0 && block == rightBlocks[rightBlocks.Count - 2]) {
            GenerateNextRightBlock(rightBlocks.Count);
        }

        // LEFT SIDE
        if (leftBlocks.Count > 0 && block == leftBlocks[leftBlocks.Count - 2]) {
            GenerateNextLeftBlock(leftBlocks.Count);
        }
    }
    void GenerateNextRightBlock(int dist) {
        GenerateNextBlock(+1, rightBlocks, dist);
    }

    void GenerateNextLeftBlock(int dist) {
        GenerateNextBlock(-1, leftBlocks, dist);
    }

    void GenerateNextBlock(int direction, List<HordeModeBlock> storeList, int distance) {
        BlockDefinition chosen = PickNextSize(distance);
        if (chosen == null)
            return;

        HordeModeBlock last = storeList[^1];
        Vector3 pos = last.GetRightExitPosition();

        GameObject blockObj = Instantiate(chosen.prefab, pos, Quaternion.identity, transform);
        HordeModeBlock newBlock = blockObj.GetComponent<HordeModeBlock>();
        newBlock.SetDir(direction);
        storeList.Add(newBlock);

        RegisterSizeHistory(chosen.size);

        // distance côté correspondant
        int dist = ((direction == +1) ? rightBlocks.Count : leftBlocks.Count);

        AssignDistanceBlockTypesDynamic(dist);
    }

    void AssignDistanceBlockTypesDynamic(int dist) {
        Debug.Log("AssignDistanceBlockTypesDynamic " + dist);
        HordeModeBlock left = (leftBlocks.Count >= dist) ? leftBlocks[dist - 1] : null;
        HordeModeBlock right = (rightBlocks.Count >= dist) ? rightBlocks[dist - 1] : null;

        // shops déjà débloqués
        var unlocked = ComputeUnlockedShops();
        Dictionary<int, ShopType> shopsPerDistance = new();
        for (int i = 0; i < unlocked.Count && i < 4; i++)
            shopsPerDistance[i + 1] = unlocked[i];

        bool placeShopHere = shopsPerDistance.ContainsKey(dist);
        ShopType shop = placeShopHere ? shopsPerDistance[dist] : default;

        // --- assignation shop si les deux côtés existent ---
        HordeModeBlock shopBlock = null;
        HordeModeBlock otherBlock = null;

        if (placeShopHere && left != null && right != null) {
            if (Random.value < 0.5f) {
                shopBlock = left;
                otherBlock = right;
            }
            else {
                shopBlock = right;
                otherBlock = left;
            }
            shopBlock.SetShopType(shop);
            shopBlock.SetBlockType(BlockType.Shop);
        }

        // --- DISTANCE 1 ---
        if (dist == 1) {
            // si un bloc est manquant : assigner ce bloc comme aléatoire simple
            if (left != null && left != shopBlock) AssignSimpleResource(left);
            if (right != null && right != shopBlock) AssignSimpleResource(right);
            return;
        }

        // --- DISTANCE 2-4 ---
        if (dist == 2 || dist == 3 || dist == 4) {
            // si les deux existent
            if (left != null && right != null) {
                HordeModeBlock forcedSimple = (Random.value < 0.5f ? left : right);
                if (forcedSimple != shopBlock) AssignSimpleResource(forcedSimple);
                HordeModeBlock other = (forcedSimple == left ? right : left);
                if (other != shopBlock) AssignMidRangeBlockType(other, placeShopHere);
            }
            else {
                // un seul bloc présent : choix aléatoire s'il sera le forcedSimple ou non
                HordeModeBlock single = left ?? right;
                if (single != shopBlock) {
                    if (Random.value < 0.5f) AssignSimpleResource(single);
                    else AssignMidRangeBlockType(single, placeShopHere);
                }
            }
            return;
        }

        // --- DISTANCE > 5 ---
        if (left != null && right != null) {
            AssignFarDistanceTypes(left, right);
        }
        else if (left != null || right != null) {
            HordeModeBlock single = left ?? right;

            // Meme logique que DISTANCE 2-3 : choix aléatoire entre simple ou “far”
            if (Random.value < 0.5f) {
                AssignSimpleResource(single);
            }
            else {
                // On utilise la logique far mais sur un seul bloc
                BlockType[] farAll = {
                    BlockType.Animals,
                    BlockType.Scavengables,
                    BlockType.ResourceChest,
                    BlockType.WeaponChest,
                    BlockType.TrapChest,
                    BlockType.Mine,
                    BlockType.CombatOnly,
                    BlockType.None
                };
                single.SetBlockType(farAll[Random.Range(0, farAll.Length)]);
            }
        }
    }

    public List<ShopType> ComputeUnlockedShops() {
        // --- Récupération des états de déblocage ---
        bool isWeaponUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Armorer);
        bool isHeroUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Trainer);
        bool isDogUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Tamer);
        bool isWorkerUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Watcher);
        bool isStructuresUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Architect);

        // --- Debug override ---
        bool unlockAll = DebugManager.Instance.GetHordeModeAllUnlockedDebug();
        if (unlockAll) {
            isWeaponUnlocked = true;
            isHeroUnlocked = true;
            isDogUnlocked = true;
            isWorkerUnlocked = true;
            isStructuresUnlocked = true;
        }

        // --- Construction de la liste de shops disponibles ---
        List<ShopType> unlocked = new();

        if (isWeaponUnlocked) unlocked.Add(ShopType.Weapons);
        if (isHeroUnlocked) unlocked.Add(ShopType.Hero);
        if (isDogUnlocked) unlocked.Add(ShopType.Dog);
        if (isWorkerUnlocked) unlocked.Add(ShopType.Worker);
        if (isStructuresUnlocked) unlocked.Add(ShopType.Structures);

        return unlocked;
    }

    void AssignSimpleResource(HordeModeBlock block) {
        List<BlockType> simpleResources = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest };

        if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
            simpleResources.Add(BlockType.Scavengables);
        }

        SetRandomBlockType(block, simpleResources);
    }

    void AssignMidRangeBlockType(HordeModeBlock block, bool shopIsOnThisDistance) {
        List<BlockType> midRangeBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest, BlockType.TrapChest };

        if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
            midRangeBlocs.Add(BlockType.Scavengables);
        }

        if (HordeModeProgressionManager.Instance.GetWeaponAmountUnlocked() > gunChestBlocksSpawned) {
            midRangeBlocs.Add(BlockType.WeaponChest);
        }

        SetRandomBlockType(block, midRangeBlocs);
    }

    void AssignFarDistanceTypes(HordeModeBlock left, HordeModeBlock right) {
        List<BlockType> farRangeSafeBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest};
        List<BlockType> farRangeAllBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest, BlockType.TrapChest, BlockType.CombatOnly, BlockType.None };

        if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
            farRangeAllBlocs.Add(BlockType.Mine);
            farRangeAllBlocs.Add(BlockType.Scavengables);
            farRangeSafeBlocs.Add(BlockType.Scavengables);
        }

        if (HordeModeProgressionManager.Instance.GetWeaponAmountUnlocked() > gunChestBlocksSpawned) {
            farRangeAllBlocs.Add(BlockType.WeaponChest);
        }


        // 1) Choisir qui est le bloc safe (50/50)
        bool leftIsSafe = Random.value < 0.5f;

        HordeModeBlock safeBlock = leftIsSafe ? left : right;
        HordeModeBlock otherBlock = leftIsSafe ? right : left;

        // 2) Assigner safeBlock
        SetRandomBlockType(safeBlock, farRangeSafeBlocs);

        // 3) Assigner l’autre
        SetRandomBlockType(otherBlock, farRangeAllBlocs);
    }


    private void SetRandomBlockType(HordeModeBlock block, List<BlockType> list) {
        BlockType selectedType = list[Random.Range(0, list.Count)];
        if (selectedType == BlockType.WeaponChest) {
            gunChestBlocksSpawned++;
        }

        block.SetBlockType(selectedType);
    }

    BlockDefinition PickNextSize(int dist) {
        if(dist >= distanceToForceSmallBlocks) {
            ApplySizeBalancing();
        } else {
            // Close blocks = small only
            return blockDefinitions[1];
        }


        float total = sizeWeights.Sum(kvp => kvp.Value);
        if (total <= 0f)
            return null;

        float r = Random.value * total;
        float cum = 0;

        foreach (var b in blockDefinitions) {
            cum += sizeWeights[b.size];
            if (r <= cum)
                return b;
        }

        return blockDefinitions.Last();
    }

    void RegisterSizeHistory(BlockSize size) {
        sizeHistory.Add(size);
        if (sizeHistory.Count > historyLimit)
            sizeHistory.RemoveAt(0);
    }

    void ApplySizeBalancing() {
        // Reset to base
        foreach (var b in blockDefinitions)
            sizeWeights[b.size] = b.baseWeight;

        if (sizeHistory.Count == 0)
            return;

        // Smoothing small streaks
        int smallStreak = sizeHistory.Count(s => s == BlockSize.Tiny || s == BlockSize.Small);

        if (smallStreak >= 2) {
            sizeWeights[BlockSize.Tiny] *= 0.3f;
            sizeWeights[BlockSize.Small] *= 0.5f;
            sizeWeights[BlockSize.Medium] *= 2f;
            sizeWeights[BlockSize.Big] *= 1.5f;
        }

        // Avoid Big -> Big
        if (sizeHistory.Last() == BlockSize.Big)
            sizeWeights[BlockSize.Big] *= 0.2f;

        // Stabilize medium
        sizeWeights[BlockSize.Medium] *= 1.1f;
    }
}
