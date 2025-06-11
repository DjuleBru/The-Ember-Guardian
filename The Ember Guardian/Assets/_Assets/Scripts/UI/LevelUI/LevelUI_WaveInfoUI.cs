using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUI_WaveInfoUI : MonoBehaviour
{
    public static LevelUI_WaveInfoUI Instance;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private WaveInfoUI_OneSide leftSideInfo;
    [SerializeField] private WaveInfoUI_OneSide rightSideInfo;

    private int maxDifficulty = 10;
    private bool waveInfoShown;
    private bool fullWaveInfoShown;

    private bool enemyTypesDetectionUnlocked;
    private bool enemyAmountDetectionUnlocked;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        enemyTypesDetectionUnlocked = StructureStats.Instance.GetObservationTowerEnemyTypesDetectionUnlocked();
        enemyAmountDetectionUnlocked = StructureStats.Instance.GetObservationTowerEnemyAmountDetectionUnlocked();
    }

    public void RefreshWaveInfo() {
        Dictionary<CreatureSO, int> leftSideCreatures = CreaturesSpawnManager.Instance.GetNextWaveCreaturesBySide(CreaturesSpawnManager.SpawnSide.Left);
        Dictionary<CreatureSO, int> rightSideCreatures = CreaturesSpawnManager.Instance.GetNextWaveCreaturesBySide(CreaturesSpawnManager.SpawnSide.Right);

        float waveDifficulty = CreaturesSpawnManager.Instance.GetCurrentWaveDifficulty();
        float maxWaveDifficulty = CreaturesSpawnManager.Instance.GetMaxWaveDifficulty();

        var (leftScore, rightScore) = GetSideDifficultiesScaledByGlobalThreat(leftSideCreatures, rightSideCreatures, waveDifficulty, maxWaveDifficulty);
        leftSideInfo.RefreshDifficulty(leftScore);
        rightSideInfo.RefreshDifficulty(rightScore);

        leftSideInfo.RefreshCreatures(leftSideCreatures, enemyTypesDetectionUnlocked, enemyAmountDetectionUnlocked);
        rightSideInfo.RefreshCreatures(rightSideCreatures, enemyTypesDetectionUnlocked, enemyAmountDetectionUnlocked);
    }

    public (int leftScore, int rightScore) GetSideDifficultiesScaledByGlobalThreat( Dictionary<CreatureSO, int> leftCreatures, Dictionary<CreatureSO, int> rightCreatures, float currentWaveDifficulty, float maxWaveDifficulty ) {
        int leftRaw = leftCreatures.Sum(entry => entry.Key.difficulty * entry.Value);
        int rightRaw = rightCreatures.Sum(entry => entry.Key.difficulty * entry.Value);
        float totalRaw = leftRaw + rightRaw;

        if (totalRaw == 0 || currentWaveDifficulty <= 0 || maxWaveDifficulty <= 0)
            return (0, 0);

        float globalThreatRatio = currentWaveDifficulty / maxWaveDifficulty;

        float leftRatio = (float)leftRaw / currentWaveDifficulty;
        float rightRatio = (float)rightRaw / currentWaveDifficulty;

        int leftScore = Mathf.Clamp(Mathf.RoundToInt(leftRatio * globalThreatRatio * maxDifficulty * 1.5f), 0, maxDifficulty);
        int rightScore = Mathf.Clamp(Mathf.RoundToInt(rightRatio * globalThreatRatio * maxDifficulty * 1.5f), 0, maxDifficulty);

        return (leftScore, rightScore);
    }

    public void ShowFullWaveInfoUI() {
        if (fullWaveInfoShown) return;
        if (!enemyTypesDetectionUnlocked && enemyAmountDetectionUnlocked) return;

        leftSideInfo.ShowFullWaveInfoUI();
        rightSideInfo.ShowFullWaveInfoUI();

        fullWaveInfoShown = true;
    }

    public void HideFullWaveInfoUI() {
        if (!fullWaveInfoShown) return;
        if (!enemyTypesDetectionUnlocked && enemyAmountDetectionUnlocked) return;

        leftSideInfo.HideFullWaveInfoUI();
        rightSideInfo.HideFullWaveInfoUI();

        fullWaveInfoShown = false;
    }

    public void ShowWaveInfoUI() {
        if (waveInfoShown) return;

        uiAnimator.ResetTrigger("Hide");
        uiAnimator.SetTrigger("Show");

        waveInfoShown = true;
    }

    public void HideWaveInfoUI() {
        if (!waveInfoShown) return;

        uiAnimator.ResetTrigger("Show");
        uiAnimator.SetTrigger("Hide");

        waveInfoShown = false;
    }
}
