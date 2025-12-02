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

    private int distanceToForceSmallBlocks = 3;

    int distSinceLastWeaponChest = 0;
    int distSinceLastTrapChest = 0;
    int minDistBetweenSpecialChests = 6;
    int maxDistBetweenSpecialChests = 9;

    int nextWeaponChestThreshold;
    int nextTrapChestThreshold;

    int distSinceLastWorkerSpawner = 0;
    int nextWorkerSpawnerThreshold = 4;

    private float lastFastTravelRightX = 0f;
    private float lastFastTravelLeftX = 0f;
    [SerializeField] private float fastTravelThreshold = 175f;

    private void Awake() {
        Instance = this;

        nextWeaponChestThreshold = Random.Range(minDistBetweenSpecialChests, maxDistBetweenSpecialChests);
        nextTrapChestThreshold = Random.Range(minDistBetweenSpecialChests, maxDistBetweenSpecialChests);

        distSinceLastWeaponChest = nextWeaponChestThreshold/2;
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
            shopBlock.AddBlockType(BlockType.Shop);
        }


        // --- DISTANCE 1 ---
        if (dist == 1) {
            // si un bloc est manquant : assigner ce bloc comme aléatoire simple
            if (left != null && left != shopBlock) AssignSimpleResource(left);
            if (right != null && right != shopBlock) AssignSimpleResource(right);
            TryAssignWorkerSpawner(dist, left, right);
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
            TryAssignWorkerSpawner(dist, left, right);
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

                List<BlockType> farAll = new List<BlockType>() {
                    BlockType.Animals,
                    BlockType.ResourceChest,
                    BlockType.CombatOnly,
                    BlockType.None
                };

                if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
                    farAll.Add(BlockType.Scavengables);
                    farAll.Add(BlockType.Mine);
                }

                SetRandomBlockType(single, farAll);
            }
        }

        if(left != null) {
            TryAddChests(left);
            TryAssignFastTravel(left);
        }
        if (right != null) {
            TryAddChests(right);
            TryAssignFastTravel(right);
        }
        TryAssignWorkerSpawner(dist, left, right);
    }
    private void TryAssignWorkerSpawner(int dist, HordeModeBlock left, HordeModeBlock right) {

        // Pas de blocs = rien à faire
        if (left == null && right == null)
            return;

        // --- Distance 1 : WorkerSpawner à gauche OU droite (50/50)
        if (dist == 1) {
            if (left != null && right != null) {
                HordeModeBlock chosen = (Random.value < 0.5f) ? left : right;
                chosen.AddBlockType(BlockType.WorkerSpawner);
            }
            else {
                // Un seul bloc dans des générations dynamiques
                (left ?? right)?.AddBlockType(BlockType.WorkerSpawner);
            }
            return;
        }

        // --- Distance 2 : WorkerSpawner sur l'autre côté par rapport à dist=1
        if (dist == 2) {
            if (left != null && right != null) {
                // Inversion du côté du dist=1
                HordeModeBlock chosen = (Random.value < 0.5f) ? right : left;
                chosen.AddBlockType(BlockType.WorkerSpawner);
            }
            else {
                (left ?? right)?.AddBlockType(BlockType.WorkerSpawner);
            }
            return;
        }

        // --- Distance > 2 : WorkerSpawner tous les 4 "de distance cumulée"
        // On utilise la sizeValue du bloc choisi
        // Et on place sur un côté aléatoire
        HordeModeBlock candidate = null;
        if (left != null && right != null) {
            candidate = (Random.value < 0.5f) ? left : right;
        }
        else {
            candidate = left ?? right;
        }

        if (candidate.HasBlockType(BlockType.FastTravelTP))
            return;

        int sizeValue = candidate.GetBlockSize() switch {
            BlockSize.Tiny => 1,
            BlockSize.Small => 2,
            BlockSize.Medium => 3,
            BlockSize.Big => 4,
            _ => 2
        };

        distSinceLastWorkerSpawner += sizeValue;

        if (distSinceLastWorkerSpawner >= nextWorkerSpawnerThreshold) {
            candidate.AddBlockType(BlockType.WorkerSpawner);
            distSinceLastWorkerSpawner = 0;
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
        TryAddChests(block);
        TryAssignFastTravel(block);
    }

    void AssignMidRangeBlockType(HordeModeBlock block, bool shopIsOnThisDistance) {
        List<BlockType> midRangeBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest};

        if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
            midRangeBlocs.Add(BlockType.Scavengables);
        }

        SetRandomBlockType(block, midRangeBlocs);
        TryAddChests(block);
        TryAssignFastTravel(block);
    }

    void AssignFarDistanceTypes(HordeModeBlock left, HordeModeBlock right) {
        List<BlockType> farRangeSafeBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest};
        List<BlockType> farRangeAllBlocs = new List<BlockType> { BlockType.Animals, BlockType.ResourceChest,BlockType.CombatOnly, BlockType.None };

        if (HordeModeProgressionManager.Instance.GetStructureUnlocked(StructureSO.StructureType.minerShrine)) {
            farRangeAllBlocs.Add(BlockType.Mine);
            farRangeAllBlocs.Add(BlockType.Scavengables);
            farRangeSafeBlocs.Add(BlockType.Scavengables);
        }


        // 1) Choisir qui est le bloc safe (50/50)
        bool leftIsSafe = Random.value < 0.5f;

        HordeModeBlock safeBlock = leftIsSafe ? left : right;
        HordeModeBlock otherBlock = leftIsSafe ? right : left;

        // 2) Assigner safeBlock
        SetRandomBlockType(safeBlock, farRangeSafeBlocs);

        // 3) Assigner l’autre
        SetRandomBlockType(otherBlock, farRangeAllBlocs);
        TryAddChests(left);
        TryAddChests(right);
        TryAssignFastTravel(left);
        TryAssignFastTravel(right);
    }


    private void SetRandomBlockType(HordeModeBlock block, List<BlockType> list) {
        BlockType selectedType = list[Random.Range(0, list.Count)];

        block.AddBlockType(selectedType);
    }

    void TryAddChests(HordeModeBlock block) {
        BlockSize size = block.GetBlockSize();
        int sizeValue = size switch {
            BlockSize.Tiny => 1,
            BlockSize.Small => 2,
            BlockSize.Medium => 3,
            BlockSize.Big => 4,
            _ => 2
        };

        // --- WEAPON CHEST ---
        distSinceLastWeaponChest += sizeValue;

        if (distSinceLastWeaponChest >= nextWeaponChestThreshold &&
            !block.HasBlockType(BlockType.TrapChest)) {
            block.AddBlockType(BlockType.WeaponChest);
            distSinceLastWeaponChest = 0;
            nextWeaponChestThreshold = Random.Range(minDistBetweenSpecialChests, maxDistBetweenSpecialChests);
        }

        // --- TRAP CHEST ---
        distSinceLastTrapChest += sizeValue;

        if (distSinceLastTrapChest >= nextTrapChestThreshold &&
            !block.HasBlockType(BlockType.WeaponChest)) {
            block.AddBlockType(BlockType.TrapChest);
            distSinceLastTrapChest = 0;
            nextTrapChestThreshold = Random.Range(minDistBetweenSpecialChests, maxDistBetweenSpecialChests);
        }
    }

    private void TryAssignFastTravel(HordeModeBlock block) {
        float currentX = block.transform.position.x;
        bool isRightDirection = currentX > 0;

        // --- DIRECTION RIGHT ---
        if (isRightDirection) {
            float dist = Mathf.Abs(currentX - lastFastTravelRightX);

            if (dist >= fastTravelThreshold) {
                block.AddBlockType(BlockType.FastTravelTP);
                lastFastTravelRightX = currentX;
            }
        }

        // --- DIRECTION LEFT ---
        else {
            float dist = Mathf.Abs(currentX - lastFastTravelLeftX);

            if (dist >= fastTravelThreshold) {
                block.AddBlockType(BlockType.FastTravelTP);
                lastFastTravelLeftX = currentX;
            }
        }
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
