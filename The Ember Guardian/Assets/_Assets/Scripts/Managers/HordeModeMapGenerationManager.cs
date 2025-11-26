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

    private void Awake() {
        Instance = this;
    }

    void Start() {
        InitializeWeights();
        GenerateSide(+1, rightBlocks);
        GenerateSide(-1, leftBlocks);
    }

    void InitializeWeights() {
        foreach (var b in blockDefinitions)
            sizeWeights[b.size] = b.baseWeight;
    }

    void GenerateSide(int direction, List<HordeModeBlock> storeList) {
        Vector3 pos = Vector3.right * spacingFromCenter * direction;

        for (int i = 0; i < blocksPerSide; i++) {
            BlockDefinition chosen = PickNextSize();
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
        if (rightBlocks.Count > 0 && block == rightBlocks[rightBlocks.Count - 1]) {
            GenerateNextRightBlock();
        }

        // LEFT SIDE
        if (leftBlocks.Count > 0 && block == leftBlocks[leftBlocks.Count - 1]) {
            GenerateNextLeftBlock();
        }
    }
    void GenerateNextRightBlock() {
        GenerateNextBlock(+1, rightBlocks);
    }

    void GenerateNextLeftBlock() {
        GenerateNextBlock(-1, leftBlocks);
    }

    void GenerateNextBlock(int direction, List<HordeModeBlock> storeList) {
        BlockDefinition chosen = PickNextSize();
        if (chosen == null)
            return;

        HordeModeBlock last = storeList[storeList.Count - 1];
        Vector3 pos = last.GetRightExitPosition() * direction;

        // Correction : exit position est déjà dans l'espace monde,
        // donc on n’utilise PAS '* direction'.
        pos = last.GetRightExitPosition();

        GameObject blockObj = Instantiate(chosen.prefab, pos, Quaternion.identity, transform);
        HordeModeBlock block = blockObj.GetComponent<HordeModeBlock>();

        block.SetDir(direction);
        storeList.Add(block);

        RegisterSizeHistory(chosen.size);
    }

    BlockDefinition PickNextSize() {
        ApplySizeBalancing();

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
