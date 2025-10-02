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
    [SerializeField] private StructureLocation conditionalLockedStructureLocation;
    [SerializeField] private bool setEndLevelPositionRelativeToPlayer = true;

    private List<Obstacle> allObstacles = new List<Obstacle>();
    private List<Obstacle> blockingObstacles = new List<Obstacle>();
    private List<Chest> allChests = new List<Chest>();
    private List<TrialArea> allTrialAreas = new List<TrialArea>();
    private List<Collectible> collectiblesInWorld = new List<Collectible>();
    private float minLevelLimit;
    private float maxLevelLimit;

    private bool levelSucceeded;
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
        if(SavingManager_Level.Instance != null) {
            if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;
        }


        if(levelHubMerchant != null) {
            if (levelHubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) return;
            levelHubMerchant.SetHasTalkLinesToShow(true);
        }
    }

    private void Obstacle_OnAnyObstacleInitialized(object sender, EventArgs e) {
        Obstacle obstacle = (Obstacle)sender;
        blockingObstacles.Add(obstacle);
        allObstacles.Add(obstacle);
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
            float delayToShowReturnToHubObj = 2f;
            if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
                delayToShowReturnToHubObj = 4f;
            }

            LevelObjectives.Instance.ShowReturnToHubObj(delayToShowReturnToHubObj);
            LevelSuccess(delayToShowReturnToHubObj + 1f);
            //StartCoroutine(EnableEndLevelPortal(delayToShowReturnToHubObj+1f));
        }

        if(levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
            if (conditionalLockedStructureLocation != null) {
                conditionalLockedStructureLocation.gameObject.SetActive(true);
                conditionalLockedStructureLocation.UnlockStructureLocation();
                Debug.Log("LevelHubMerchant_OnPlayerStoppedInteractingWithHubMerchant");
            }
        }
    }

    private void LevelUI_OnObjectiveCompleted(object sender, EventArgs e) {
        if(DemoMainLevelManager.Instance != null) {
            if (!DemoMainLevelManager.Instance.GetDemoFirstLevelCompleted()) return;
        }

        LevelSuccess();

        float delayToShowReturnToHubObj = 2.5f;
        if(levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
            delayToShowReturnToHubObj = 4f;
        }
        LevelObjectives.Instance.ShowReturnToHubObj(delayToShowReturnToHubObj);
    }

    [Button]
    public void LevelSuccess(float delayToReturnToHub = 2f) {
        Debug.Log("LevelSuccess");
        Debug.Log("levelSucceeded " + levelSucceeded);

        if (levelSucceeded) return;
        Vector3 endLevelPortalPosition = endLevelPortal.transform.position; 

        if (setEndLevelPositionRelativeToPlayer) {
            endLevelPortalPosition = new Vector3(Player.Instance.transform.position.x + 10f, 0, 0);
        }

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {
            if (DemoMainLevelManager.Instance != null) {

                if (!DemoMainLevelManager.Instance.GetIsMainDemoLevel()) return;

                // Demo level
                endLevelPortal.transform.position = endLevelPortalPosition;
                StartCoroutine(EnableEndLevelPortal(delayToReturnToHub));

            }
            else {
                if(levelHubMerchant != null && !setEndLevelPositionRelativeToPlayer) {
                    endLevelPortalPosition = new Vector3(levelHubMerchant.transform.position.x + 10f, 0, 0);
                }
                StartCoroutine(EnableEndLevelPortal(delayToReturnToHub));
            }

        } else {
            StartCoroutine(EnableEndLevelPortal(delayToReturnToHub));
        }

        OnLevelSuccess?.Invoke(this, EventArgs.Empty);
        endLevelPortal.transform.position = endLevelPortalPosition;
        levelSucceeded = true;
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArmorer) return;

        LevelSuccess(4f);
        //StartCoroutine(EnableEndLevelPortal(4f));
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

        float defeatGemsProportionsRewarded = 1f;
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

    public int GetLevelHubMerchantInteractionIndex() {
        return levelHubMerchantInteractionIndex;
    }

    public bool GetLevelSucceeded() {
        return levelSucceeded;
    }

    public List<Obstacle> GetAllObstacles() {
        return allObstacles;
    }

    public void AddChest(Chest chest) {
        allChests.Add(chest);
    }

    public List<Chest> GetAllChests() {
        return allChests;
    }
    public void AddTrialArea(TrialArea trialArea) {
        allTrialAreas.Add(trialArea);
    }

    public List<TrialArea> GetAllTrialAreas() {
        return allTrialAreas;
    }

    public List<Collectible> GetAllCollectibles() {
        return collectiblesInWorld;
    }

    public void AddCollectible(Collectible collectible) {
        collectiblesInWorld.Add(collectible);
    }

    public void RemoveCollectible(Collectible collectible) {
        collectiblesInWorld.Remove(collectible);
    }

    public bool GetLevelHubMerchantHasTalkLinesToShow() {
        return levelHubMerchant.GetMerchantHasNewTalkLinkes();
    }
    public void SetLevelHubMerchantHasTalkLinesToShow(bool hasTalkLines) {
        levelHubMerchant.SetHasTalkLinesToShow(hasTalkLines);
    }


    public void SetLevelHubMerchantInteractionIndex(int levelHubMerchantInteractionIndex) {
        this.levelHubMerchantInteractionIndex = levelHubMerchantInteractionIndex;
    }

    private void OnDestroy() {
        LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted -= LevelUI_OnObjectiveCompleted;
        Obstacle.OnAnyObstacleInitialized -= Obstacle_OnAnyObstacleInitialized;

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
        }
    }


}
