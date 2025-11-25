using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private LevelSO levelSO;
    [SerializeField] private bool isHordeMode;
    [SerializeField] private Portal endLevelPortal;
    [SerializeField] private HubMerchant levelHubMerchant;

    [SerializeField] private Transform leftLevelEndCollider;
    [SerializeField] private Transform rightLevelEndCollider;
    [SerializeField] private StructureLocation conditionalLockedStructureLocation;
    [SerializeField] private bool setEndLevelPositionRelativeToPlayer = true;

    [SerializeField] private List<int> maxCurrencyStorageList;
    [SerializeField] private List<float> difficultyReductionFactorsList;

    private LevelSO.LevelEnvironment currentLevelEnvironment;
    private List<Obstacle> allObstacles = new List<Obstacle>();
    private List<Obstacle> blockingObstacles = new List<Obstacle>();
    private List<Chest> allChests = new List<Chest>();
    private List<TrialArea> allTrialAreas = new List<TrialArea>();
    private List<Collectible> collectiblesInWorld = new List<Collectible>();
    private float minLevelLimit;
    private float maxLevelLimit;

    private bool levelSucceeded;
    private bool conditionalLockedStructureLocationUnlocked;
    private bool conditionalLockedStructureLocationBuilt;
    private int levelHubMerchantInteractionIndex;

    public event EventHandler OnNewLocationShown;
    public event EventHandler OnLevelSuccess;
    public event EventHandler OnLevelFailed;
    public event EventHandler OnLevelLimitsChanged;
    public event EventHandler<OnEndLevelPortalEnabledEventArgs> OnEndLevelPortalEnabled;
    public class OnEndLevelPortalEnabledEventArgs : EventArgs {
        public Vector3 endLevelPortalPosition;
    }

    private void Awake() {
        Instance = this;
        Obstacle.OnAnyObstacleInitialized += Obstacle_OnAnyObstacleInitialized;

    }

    private void Start() {
        if(!isHordeMode) {
            if (conditionalLockedStructureLocation != null && !conditionalLockedStructureLocationUnlocked) {
                conditionalLockedStructureLocation.gameObject.SetActive(false);
                conditionalLockedStructureLocation.OnStructureBuilt += ConditionalLockedStructureLocation_OnStructureBuilt;
            }

            if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
            }

            if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindAndDestroyTwoNests) {
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
                EndLevelArea.Instance_Left.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
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
                levelHubMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelHubMerchant_OnPlayerStoppedInteractingWithHubMerchant;
                LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted += LevelUI_OnObjectiveCompleted;

                if (!SavingManager_Level.Instance.GetLoadingSavedLevel()) {
                    levelHubMerchant.SetHasTalkLinesToShowAfterDelay(false, false, 1f);
                }

            }
        }

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        LoadEnvironmentType();
        RefreshLevelLimits();
    }

    private void LoadEnvironmentType() {

        if (isHordeMode) {
            currentLevelEnvironment = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();
        }
        else {
            currentLevelEnvironment = levelSO.environmentType;
        }

        ES3.Save("lastLevelEnvironment", currentLevelEnvironment);

    }

    public LevelSO.LevelEnvironment GetCurrentLevelEnvironment() {
        return currentLevelEnvironment;
    }

    private void ConditionalLockedStructureLocation_OnStructureBuilt(object sender, EventArgs e) {
        conditionalLockedStructureLocationBuilt = true;
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
                delayToShowReturnToHubObj = 7f;
            }

            LevelObjectives.Instance.ShowReturnToHubObj(delayToShowReturnToHubObj);
            LevelSuccess(delayToShowReturnToHubObj + 1f);
        }

        if(levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
            if (conditionalLockedStructureLocation != null) {
                conditionalLockedStructureLocation.gameObject.SetActive(true);
                conditionalLockedStructureLocation.UnlockStructureLocation();
                conditionalLockedStructureLocationUnlocked = true;
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
                StartCoroutine(EnableEndLevelPortalCoroutine(8f));

            }
            else {
                if(levelHubMerchant != null && !setEndLevelPositionRelativeToPlayer) {
                    endLevelPortalPosition = new Vector3(levelHubMerchant.transform.position.x + 10f, 0, 0);
                }
                StartCoroutine(EnableEndLevelPortalCoroutine(12f));
            }

        } else {
            StartCoroutine(EnableEndLevelPortalCoroutine(delayToReturnToHub));
        }

        OnLevelSuccess?.Invoke(this, EventArgs.Empty);
        endLevelPortal.transform.position = endLevelPortalPosition;
        levelSucceeded = true;
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, EventArgs e) {
        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArmorer) return;

        if (levelSO.levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.ExploreCorruptedCity) {
            if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest && LevelObjectives.Instance.GetNPCInteractionsIndex() == 1) {
                LevelSuccess(4f);
            };
        };

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindAndDestroyTwoNests) {
            if(LevelObjectives.Instance.GetLeftDarklingNestDestroyed() && LevelObjectives.Instance.GetRightDarklingNestDestroyed()) {
                LevelSuccess(4f);
            }
        };
    }

    public void SaveLevelCompletedProgression() {
        MetaProgressionManager.Instance.SetLevelCompleted(GetLevelSO());
        MetaProgressionManager.Instance.SaveLevelGemsAndHoldingEmber();
        SaveMerchantsAndTalkLines();
    }

    public void ShowNewLocationUI() {
        MetaProgressionManager.Instance.SetLevelRegionUnlocked(levelSO.environmentType);

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

        if(isHordeMode) {
            string key = "hordeMode_maxNightsSurvived_" + currentLevelEnvironment.ToString();
            ES3.Save(key, DayNightManager.Instance.GetCurrentDay());

            int currentDay = DayNightManager.Instance.GetCurrentDay();
            int xpReward = CalculateHordeModeXPReward(currentDay - 1);
            HordeModeProgressionManager.Instance.AddRunXP(xpReward);

            ES3.Save("backFromHordeModeAfterDefeat", true);
            ES3.Save("lastHordeModeNightsSurvived", currentDay-1);
        }
    }

    public int CalculateHordeModeXPReward(int nightCount) {
        int baseXP = 30;        // XP pour la première nuit
        int extraXP = 10;       // XP ajouté par nuit supplémentaire

        if (nightCount <= 0)
            return 0;

        // Nuit 1 donne baseXP
        // Nuit 2 donne baseXP + extraXP
        // Nuit 3 donne baseXP + (extraXP * 2)
        // etc.

        int reward = baseXP + (extraXP * (nightCount - 1));
        return reward;
    }

    private IEnumerator LooseLevelCoroutine() {
        yield return new WaitForSeconds(3f);

        if(isHordeMode) {
            SceneLoader.Instance.LoadMainMenu(3f);
            yield break;
        }
        SceneLoader.Instance.LoadHub(3f);
    }

    public void EnableEndLevelPortal(float delayToEnable) {
        StartCoroutine(EnableEndLevelPortalCoroutine(delayToEnable));
    }

    private IEnumerator EnableEndLevelPortalCoroutine(float delayToEnable) {
        yield return new WaitForSeconds(delayToEnable);
        endLevelPortal.gameObject.SetActive(true);
        OnEndLevelPortalEnabled?.Invoke(this, new OnEndLevelPortalEnabledEventArgs {
            endLevelPortalPosition = endLevelPortal.transform.position
        });
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
        if (levelHubMerchant == null) return false;

        return levelHubMerchant.GetMerchantHasNewTalkLinkes();
    }
    public void SetLevelHubMerchantHasTalkLinesToShow(bool hasTalkLines) {
        if (levelHubMerchant == null) return;
        levelHubMerchant.SetHasTalkLinesToShow(hasTalkLines, true, true);
    }


    public void SetLevelHubMerchantInteractionIndex(int levelHubMerchantInteractionIndex) {
        this.levelHubMerchantInteractionIndex = levelHubMerchantInteractionIndex;
    }

    public bool GetConditionalLockedStructureLocationUnlocked() {
        return conditionalLockedStructureLocationUnlocked;
    }
    public bool GetConditionalLockedStructureLocationBuilt() {
        return conditionalLockedStructureLocationBuilt;
    }

    public bool IsHordeMode() {
        return isHordeMode;
    }

    public void SetConditionalLockedStructureLocationState(bool conditionalLockedStructureLocationUnlocked, bool conditionalLockedStructureLocationBuilt) {
        this.conditionalLockedStructureLocationUnlocked = conditionalLockedStructureLocationUnlocked;
        this.conditionalLockedStructureLocationBuilt = conditionalLockedStructureLocationBuilt;

        if(conditionalLockedStructureLocationUnlocked && conditionalLockedStructureLocation != null) {
            if (conditionalLockedStructureLocationBuilt) return;

            conditionalLockedStructureLocation.gameObject.SetActive(true);
            conditionalLockedStructureLocation.UnlockStructureLocation();
        }
    }

    public List<int> GetMaxCurrencyStorageList() {
        return maxCurrencyStorageList;
    }
    public List<float> GetDifficultyReductionFactorsList() {
        return difficultyReductionFactorsList;
    }

    private void OnDestroy() {
        LevelUI_ObjectiveUI.Instance.OnObjectiveCompleted -= LevelUI_OnObjectiveCompleted;
        Obstacle.OnAnyObstacleInitialized -= Obstacle_OnAnyObstacleInitialized;

        if (levelSO.endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            EndLevelArea.Instance.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
        }
    }


}
