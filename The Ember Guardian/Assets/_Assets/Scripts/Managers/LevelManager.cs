using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private LevelSO levelSO;

    public event EventHandler OnNewLocationShown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        MetaProgressionManager.Instance.SetLevelCompleted(GetLevelSO());

        for (int i = 0; i < levelSO.merchantsUnlockedInLevel.Count; i++) {

            MetaProgressionManager.Instance.SetMerchantUnlocked(levelSO.merchantsUnlockedInLevel[i]);
            MetaProgressionManager.Instance.SetNextMerchantTalkLines(levelSO.merchantsUnlockedInLevel[i], levelSO.newMerchantTextLinesAfterLevel[i]);

        }

        MetaProgressionManager.Instance.SetPreviousLevelsUnlocked(levelSO.levelsUnlockedByLevel);

        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType.GemMerchant, true);
        MetaProgressionManager.Instance.SetNextMerchantTalkLines(HubMerchant.HubMerchantType.GemMerchant, levelSO.gemMerchantTextLinesAfterLevel);
    }

    public void ShowNewLocationUI() {
        bool levelRegionUnlocked = MetaProgressionManager.Instance.GetLevelRegionUnlocked(levelSO.environmentType);
        if (!levelRegionUnlocked) {
            MetaProgressionManager.Instance.SetLevelRegionUnlocked(levelSO.environmentType);
            LevelUI_Locations.Instance.ShowLocationText(levelSO.GetLevelEnvironmentTypeString());
            OnNewLocationShown?.Invoke(this, EventArgs.Empty);
        }
    }

    public LevelSO GetLevelSO() {
        return levelSO;
    }

    public void LooseLevel() {
        StartCoroutine(LooseLevelCoroutine());
    }

    private IEnumerator LooseLevelCoroutine() {
        yield return new WaitForSeconds(2f);
        SceneLoader.Instance.LoadHub(3f);
    }
}
