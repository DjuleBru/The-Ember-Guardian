using Sirenix.OdinInspector;
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

    [SerializeField] private Transform leftLevelEndCollider;
    [SerializeField] private Transform rightLevelEndCollider;
    [SerializeField] private List<Obstacle> blockingObstacles;
    [SerializeField] private StructureLocation conditionalLockedStructureLocation;
    [SerializeField] private bool setEndLevelPositionRelativeToPlayer = true;
    private float minLevelLimit;
    private float maxLevelLimit;


    private bool levelRegionUnlocked;
    private int levelHubMerchantInteractionIndex;

    public event EventHandler OnNewLocationShown;
    public event EventHandler OnLevelSuccess;
    public event EventHandler OnLevelFailed;
    public event EventHandler OnLevelLimitsChanged;

    private void Awake() {
        Instance = this;
        Obstacle.OnAnyObstacleInitialized += Obstacle_OnAnyObstacleInitialized;
    }


    private void Start() {
        if(conditionalLockedStructureLocation != null) {
            conditionalLockedStructureLocation.gameObject.SetActive(false);
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindMoreCompanions) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        }

        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArmorer) {
            levelHubMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelHubMerchant_OnPlayerStoppedInteractingWithHubMerchant;
        }

        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
            levelHubMerchant.SetHasTalkLinesToShowAfterDelay(false,false, 1f);
            levelHubMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelHubMerchant_OnPlayerStoppedInteractingWithHubMerchant;
            LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;
        }

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        ES3.Save("lastLevelEnvironment", levelSO.environmentType);

        RefreshLevelLimits();
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        levelHubMerchant.SetHasTalkLinesToShow(true);
    }

    private void Obstacle_OnAnyObstacleInitialized(object sender, EventArgs e) {
        Obstacle obstacle = (Obstacle)sender;
        blockingObstacles.Add(obstacle);
        obstacle.OnObstacleBuilt += Obstacle_OnObstacleBuilt;
    }

    private void Obstacle_OnObstacleBuilt(object sender, EventArgs e) {
        Obstacle obstacle = (Obstacle)sender;
        blockingObstacles.Remove(obstacle);
        RefreshLevelLimits();
    }

    private void RefreshLevelLimits() {
        float maxLevelLimitTemp = rightLevelEndCollider.transform.position.x;
        float minLevelLimitTemp = leftLevelEndCollider.transform.position.x;

        foreach(Obstacle obstacle in blockingObstacles) {

            if(obstacle.transform.position.x < 0 && obstacle.transform.position.x > minLevelLimitTemp) {
                minLevelLimitTemp = obstacle.transform.position.x;
            }

            if (obstacle.transform.position.x > 0 && obstacle.transform.position.x < maxLevelLimitTemp) {
                maxLevelLimitTemp = obstacle.transform.position.x;
            }

        }

        minLevelLimit = minLevelLimitTemp;
        maxLevelLimit = maxLevelLimitTemp;

        OnLevelLimitsChanged?.Invoke(this, EventArgs.Empty);    
    }

    public float GetMaxLevelLimit() {
        return maxLevelLimit;
    }

    public float GetMinLevelLimit() {
        return minLevelLimit;
    }

    public float GetMaxLevelLimitAbsolute() {
        return rightLevelEndCollider.transform.position.x;
    }

    public float GetMinLevelLimitAbsolute() {
        return leftLevelEndCollider.transform.position.x;
    }

    private void LevelHubMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, EventArgs e) {
        levelHubMerchantInteractionIndex++;
        if(levelHubMerchantInteractionIndex == 2) {
            StartCoroutine(EnableEndLevelPortal(2f));
        }

        if(levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
            if(conditionalLockedStructureLocation != null) {
                conditionalLockedStructureLocation.gameObject.SetActive(true);
                conditionalLockedStructureLocation.UnlockStructureLocation();
            }
        }
    }


    private void LevelUI_OnObjectiveCompleted(object sender, EventArgs e) {
        LevelSuccess();
    }

    [Button]
    private void LevelSuccess() {
        Vector3 endLevelPortalPosition = endLevelPortal.transform.position; 

        if (setEndLevelPositionRelativeToPlayer) {
            endLevelPortalPosition = new Vector3(Player.Instance.transform.position.x + 10f, 0, 0);
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
            if (DemoMainLevelManager.Instance != null) {

                if (!DemoMainLevelManager.Instance.GetIsMainDemoLevel()) return;

                // Demo level
                endLevelPortal.transform.position = endLevelPortalPosition;
                StartCoroutine(EnableEndLevelPortal(2f));

            }
            else {
                endLevelPortalPosition = new Vector3(levelHubMerchant.transform.position.x + 10f, 0, 0);
                StartCoroutine(EnableEndLevelPortal(2f));
            }

        } else {
            StartCoroutine(EnableEndLevelPortal(2f));
        }

        OnLevelSuccess?.Invoke(this, EventArgs.Empty);
        endLevelPortal.transform.position = endLevelPortalPosition;
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArmorer) return;

        StartCoroutine(EnableEndLevelPortal(4f));
    }

    public void SaveLevelCompletedProgression() {
        MetaProgressionManager.Instance.SetLevelCompleted(GetLevelSO());
        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber();
        SaveMerchantsAndTalkLines();
    }

    public void ShowNewLocationUI() {
        //if (!levelRegionUnlocked) {
        //    MetaProgressionManager.Instance.SetLevelRegionUnlocked(levelSO.environmentType);
        //    LevelUI_Locations.Instance.ShowLocationText(levelSO.GetLevelEnvironmentTypeString());
        //    OnNewLocationShown?.Invoke(this, EventArgs.Empty);
        //}

        LevelUI_Locations.Instance.ShowLocationText(levelSO.GetLevelEnvironmentTypeString());
        OnNewLocationShown?.Invoke(this, EventArgs.Empty);
    }

    public LevelSO GetLevelSO() {
        return levelSO;
    }

    public void LooseLevel() {
        OnLevelFailed?.Invoke(this, EventArgs.Empty);
        StartCoroutine(LooseLevelCoroutine());

        float defeatGemsProportionsRewarded = .33f;
        if (DemoMainLevelManager.Instance != null) {
            defeatGemsProportionsRewarded = 1f;
        }

        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber(defeatGemsProportionsRewarded);
        MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(true);
    }

    private IEnumerator LooseLevelCoroutine() {
        yield return new WaitForSeconds(3f);
        SceneLoader.Instance.LoadHub(3f);
    }

    private IEnumerator EnableEndLevelPortal(float delayToEnable) {
        Debug.Log("enableEndLevelPortal");
        yield return new WaitForSeconds(delayToEnable);
        endLevelPortal.gameObject.SetActive(true);
    }

    [Button]
    public void LooseLevelManual() {
        LooseLevel();
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
        Obstacle.OnAnyObstacleInitialized -= Obstacle_OnAnyObstacleInitialized;

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
        }
    }
}
