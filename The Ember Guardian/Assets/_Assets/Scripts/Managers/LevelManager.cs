using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private LevelSO levelSO;
    [SerializeField] private Portal endLevelPortal;

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
    }

    private void LevelUI_OnObjectiveCompleted(object sender, EventArgs e) {
        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindMoreCompanions) {

            Vector3 endLevelPortalPosition = new Vector3(Player.Instance.transform.position.x + 10f, 0, 0);
            endLevelPortal.transform.position = endLevelPortalPosition;

            StartCoroutine(EnableEndLevelPortal(2f));
        }

    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        StartCoroutine(EnableEndLevelPortal(4f));
    }

    public void SaveLevelCompletedProgression() {
        MetaProgressionManager.Instance.SetLevelCompleted(GetLevelSO());
        SaveMerchantsAndTalkLines();
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
        MetaProgressionManager.Instance.SaveLevelDefeatGems();
        MetaProgressionManager.Instance.SetGemsRewarded(false);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(true);
        Debug.Log("SetGemsRewarded");
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
}
