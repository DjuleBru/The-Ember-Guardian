using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private LevelSO levelSO;
    [SerializeField] private Portal endLevelPortal;
    [SerializeField] private HubMerchant levelHubMerchant;

    private bool levelRegionUnlocked;

    public event EventHandler OnNewLocationShown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
        }
        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindMoreCompanions) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        }

        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        }

        levelRegionUnlocked = MetaProgressionManager.Instance.GetLevelRegionUnlocked(levelSO.environmentType);
    }


    private void LevelUI_OnObjectiveCompleted(object sender, EventArgs e) {
        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindMoreCompanions) {

            Vector3 endLevelPortalPosition = new Vector3(Player.Instance.transform.position.x + 10f, 0, 0);
            endLevelPortal.transform.position = endLevelPortalPosition;

            StartCoroutine(EnableEndLevelPortal(2f));
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {

            Vector3 endLevelPortalPosition = new Vector3(levelHubMerchant.transform.position.x + 10f, 0, 0);
            endLevelPortal.transform.position = endLevelPortalPosition;

            StartCoroutine(EnableEndLevelPortal(2f));
        }

    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        StartCoroutine(EnableEndLevelPortal(4f));
    }

    public void SaveLevelCompletedProgression() {
        MetaProgressionManager.Instance.SetLevelCompleted(GetLevelSO());
        MetaProgressionManager.Instance.SaveLevelGems();
        SaveMerchantsAndTalkLines();
    }

    public void ShowNewLocationUI() {
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
        float defeatGemsProportionsRewarded = .33f;
        MetaProgressionManager.Instance.SaveLevelGems(defeatGemsProportionsRewarded);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(true);
    }

    private IEnumerator LooseLevelCoroutine() {
        yield return new WaitForSeconds(2f);
        SceneLoader.Instance.LoadHub(3f);
    }

    private IEnumerator EnableEndLevelPortal(float delayToEnable) {
        yield return new WaitForSeconds(delayToEnable);
        endLevelPortal.gameObject.SetActive(true);
    }

    private void SaveMerchantsAndTalkLines() {

        for (int i = 0; i < levelSO.merchantsUnlockedInLevel.Count; i++) {

            MetaProgressionManager.Instance.SetMerchantUnlocked(levelSO.merchantsUnlockedInLevel[i]);
            MetaProgressionManager.Instance.SetNextMerchantTalkLines(levelSO.merchantsUnlockedInLevel[i], levelSO.newMerchantTextLinesAfterLevel[i]);

        }

        MetaProgressionManager.Instance.SetPreviousLevelsUnlocked(levelSO.levelsUnlockedByLevel);

        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(HubMerchant.HubMerchantType.GemMerchant, true);
        MetaProgressionManager.Instance.SetNextMerchantTalkLines(HubMerchant.HubMerchantType.GemMerchant, levelSO.gemMerchantTextLinesAfterLevel);
    }

    private void OnDestroy() {
        LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted -= LevelUI_OnObjectiveCompleted;

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
        }
    }
}
