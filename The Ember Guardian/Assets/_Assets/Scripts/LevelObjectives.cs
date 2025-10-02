using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class LevelObjectives : MonoBehaviour
{
    public static LevelObjectives Instance;

    [SerializeField] private List<HubMerchant> levelMerchantList;
    [SerializeField] private HubMerchantTalkUI levelMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO finalMerchantTextLines;

    private bool emberExtracted;
    private bool initialFireLit;
    private bool darklingNestFound;
    private bool darklingNestCleared;
    private bool returnToHubObjectiveShown;
    private int NPCInteractionsIndex;

    private int nightsSurvived = -1;
    private int nightsToSurvive;
    [SerializeField] private int obstaclesToRemove;
    private int obstaclesRemoved;
    [SerializeField] private int watcherArtifactFillUpTotalAmount;
    private int watcherArtifactFillUpAmount;

    public event EventHandler OnNightSurvived;
    public event EventHandler OnObstacleRemoved;
    public event EventHandler OnWatcherArtifactFilled;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        ScavengableObstacle.OnAnyObstacleBuilt += ScavengableObstacle_OnAnyObstacleBuilt;

        if (levelMerchantList.Count != 0) {
            foreach (HubMerchant levelMerchant in levelMerchantList) {
                levelMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant;
            }
        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
            PlayerCurrencies.Instance.OnEmberDropped += PlayerCurrencies_OnEmberDropped;

            if (EndLevelArea.Instance != null) {
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
                EndLevelArea.Instance.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
                EndLevelAreaCollider.OnPlayerTriggeredInAnyEndLevelArea += EndLevelAreaCollider_OnPlayerTriggeredInAnyEndLevelArea;
            }
        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {

            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
            nightsToSurvive = LevelManager.Instance.GetLevelSO().nightsToSurviveAmount;
        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
            CurrencyStorage_Objective.OnAnyMaxCurrencyAmountReached += CurrencyStorage_Objective_OnAnyMaxCurrencyAmountReached;
        }

    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        nightsSurvived++;
        OnNightSurvived?.Invoke(this, EventArgs.Empty);

        if (nightsSurvived == nightsToSurvive) {

            if(LevelManager.Instance.GetLevelSO().talkToNpcAFterObjective) {

                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.SurviveNights, LevelUI_ObjectiveUI.SubObjectiveType.TalkToWatcher);
                levelMerchantTalkUI.SetTextLinesSO(finalMerchantTextLines);

            } else {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.SurviveNights);
                ShowReturnToHubObj(4f);
            }

        }
    }

    private void CurrencyStorage_Objective_OnAnyMaxCurrencyAmountReached(object sender, EventArgs e) {
        watcherArtifactFillUpAmount++;
        OnWatcherArtifactFilled?.Invoke(this, EventArgs.Empty);

        if (watcherArtifactFillUpAmount == watcherArtifactFillUpTotalAmount) {

            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbs);
            ShowReturnToHubObj(4f);
        }
    }

    private void ScavengableObstacle_OnAnyObstacleBuilt(object sender, EventArgs e) {
        ScavengableObstacle obstacle = sender as ScavengableObstacle;
        if (obstacle != null) {
            obstaclesRemoved++;
            OnObstacleRemoved?.Invoke(this, EventArgs.Empty);

            if (obstaclesRemoved == obstaclesToRemove) {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.ProgressWithScavengers);
                ShowReturnToHubObj(4f);
            }
        }
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, StructureLocation.OnAnyStructureBuiltEventArgs e) {
        StructureLocation location = sender as StructureLocation;

        if(location.GetStructureSOToBuild().structureType == StructureSO.StructureType.currencyStorage_Objective) {
            if(LevelManager.Instance.GetLevelSO().levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {

                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveList = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.CollectOrbs };
                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveList);

                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.BuildWatcherArtifact);

            }
        }

    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        initialFireLit = true;

        if (Tutorial.Instance != null) return;

        if(DemoMainLevelManager.Instance != null) {
            // Demo level
            if (!DemoMainLevelManager.Instance.GetDemoMainLevelTutorialCompleted() || !DemoMainLevelManager.Instance.GetDemoFirstLevelCompleted()) return;
        }

        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;

        StartCoroutine(ShowLevelObjective());
    }

    private IEnumerator ShowLevelObjective() {
        yield return new WaitForSeconds(3f);
        LevelUI_ObjectiveUI.ObjectiveType objectiveTypeToShow = LevelManager.Instance.GetLevelSO().levelObjectiveType;

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(objectiveTypeToShow);

        if(LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindMoreCompanions) {

            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
                LevelUI_ObjectiveUI.SubObjectiveType.MeetTamer,
                LevelUI_ObjectiveUI.SubObjectiveType.MeetTrainer};

            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);

        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {

            LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.SurviveNights);
            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.SurviveNights
                };

            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);

        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindArchitectTable) {

            LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.FindArchitectTable);
            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.TalkToArchitect
                };

            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);

        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {

            LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs);
            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.TalkToMushroomMerchant
                };

            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);

        }


        if (objectiveTypeToShow == LevelUI_ObjectiveUI.ObjectiveType.ExploreCorruptedCity) {

            LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.ExploreCorruptedCity);
            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>() {
                    LevelUI_ObjectiveUI.SubObjectiveType.FindArchitect,
                    LevelUI_ObjectiveUI.SubObjectiveType.FindNest,
                };

            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);

        }

    }

    private void LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        NPCInteractionsIndex++;

        HubMerchant levelMerchant = (HubMerchant)sender;
        StartCoroutine(SetNextNPCObjective(levelMerchant));
    }

    private IEnumerator SetNextNPCObjective(HubMerchant levelMerchant) {

        if(levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.HeroMerchant) {

            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.MeetTrainer);

            yield return new WaitForSeconds(1f);

            if (NPCInteractionsIndex == 2) {
                LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(.5f);
            }

        }

        if (levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.DogTamer) {
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.MeetTamer);

            yield return new WaitForSeconds(1f);

            if (NPCInteractionsIndex == 2) {
                LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(.5f);
            }
        }

        if (levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {


            if (NPCInteractionsIndex == 1) {

                LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(.5f);

                yield return new WaitForSeconds(2f);
                LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.DestroyNest);

                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>();
                if (!PlayerCurrencies.Instance.GetCarryingEmber()) {
                    subObjectives.Add(LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber);
                }

                subObjectives.Add(LevelUI_ObjectiveUI.SubObjectiveType.ClearNest);

                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
            }
        }

        if(levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) {

            yield return new WaitForSeconds(1f);

            if(NPCInteractionsIndex == 2) {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TalkToWatcher);
            }

            if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveList = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.BuildWatcherArtifact };
                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveList);

                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TalkToWatcher);
            }
        }

        if (levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.StructuresMerchant) {

            yield return new WaitForSeconds(1f);

            if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.FindArchitectTable) {
                LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.TalkToArchitect, LevelUI_ObjectiveUI.SubObjectiveType.ProgressWithScavengers);
            }

            if((LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest)) {
                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FindArchitect);
            }
                
        }

        if (levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.MushroomMerchant) {

            yield return new WaitForSeconds(1f);

            if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.CollectOrbs) {
                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveList = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.BuildWatcherArtifact };
                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveList);

                LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TalkToMushroomMerchant);
            }

        }
    }
    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub);
    }

    public void ShowReturnToHubObj(float delay) {
        if (returnToHubObjectiveShown) return;

        returnToHubObjectiveShown = true;

        StartCoroutine(ShowReturnToHubObjective(delay));
    }
 
    private IEnumerator ShowReturnToHubObjective(float delayBeforeShowing) {
        yield return new WaitForSeconds(delayBeforeShowing);
        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.ReturnToHub);
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.TeleportBackToHub });
    }

    #region DESTROY NEST LEVEL

    private void PlayerCurrencies_OnEmberDropped(object sender, System.EventArgs e) {
        if (!initialFireLit) return;
        if (darklingNestCleared) return;

        if(emberExtracted) {
            List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber};
 
            LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
            emberExtracted = false;
        }
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, System.EventArgs e) {
        if(LevelManager.Instance.GetLevelSO().levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArmorer) {
            StartCoroutine(StartFinalMerchantDialog());
        } else {
            ShowReturnToHubObj(3f);
        }

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
    }

    private void EndLevelArea_OnEndLevelAreaCleared(object sender, System.EventArgs e) {
        darklingNestCleared = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.ClearNest, LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
    }

    private void EndLevelAreaCollider_OnPlayerTriggeredInAnyEndLevelArea(object sender, EventArgs e) {
        if (darklingNestFound) return;
        darklingNestFound = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.FindNest, LevelUI_ObjectiveUI.SubObjectiveType.ClearNest);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!initialFireLit) return;
        if (emberExtracted) return;

        if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {

            emberExtracted = true;
            LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber);
        }
    }

    #endregion

    private IEnumerator StartFinalMerchantDialog() {
        yield return new WaitForSeconds(3f);
        levelMerchantTalkUI.SetTalkingWithMerchant(finalMerchantTextLines);
    }

    public int GetNightsSurvived() {
        return nightsSurvived;
    }

    public int GetNightsToSurvive() {
        return nightsToSurvive;
    }

    public void SetNightsToSurvive(int nightsToSurvive) {
        Debug.Log("SetNightsToSurvive " + nightsToSurvive);
        this.nightsToSurvive = nightsToSurvive;
    }
    public void SetNightsSurvived(int nightsSurvived) {
        this.nightsSurvived = nightsSurvived;
    }
    public void SetObstaclesRemoved(int obstaclesRemoved) {
        this.obstaclesRemoved = obstaclesRemoved;
    }
    public int GetObstaclesToRemove() {
        return obstaclesToRemove;
    }
    public int GetObstaclesRemoved() {
        return obstaclesRemoved;
    }

    public bool GetEmberExtracted() {
        return emberExtracted;
    }
    public void SetEmberExtracted(bool emberExtracted) {
        this.emberExtracted = emberExtracted;
    }
    public bool GetInitialFireLit() {
        return initialFireLit;
    }
    public void SetInitialFireLit(bool initialFireLit) {
        this.initialFireLit = initialFireLit;
    }
    public bool GetDarklingNestCleared() {
        return darklingNestCleared;
    }
    public void SetDarklingNestCleared(bool darklingNestCleared) {
        this.darklingNestCleared = darklingNestCleared;
    }
    public bool GetReturnToHubObjectiveShown() {
        return returnToHubObjectiveShown;
    }
    public void SetReturnToHubObjectiveShown(bool returnToHubObjectiveShown) {
        this.returnToHubObjectiveShown = returnToHubObjectiveShown;
    }
    public bool GetDarklingNestFound() {
        return darklingNestFound;
    }
    public void SetDarklingNestFound(bool darklingNestFound) {
        this.darklingNestFound = darklingNestFound;
    }
    public int GetNPCInteractionsIndex() {
        return NPCInteractionsIndex;
    }

    public void SetNPCInteractionsIndex(int NPCInteractionsIndex) {
        this.NPCInteractionsIndex = NPCInteractionsIndex;
    }
    public int GetWatcherArtifactTotalFillAmount() {
        return watcherArtifactFillUpTotalAmount;
    }

    public int GetWatcherArtifactFillAmount() {
        return watcherArtifactFillUpAmount;
    }
    public void SetWatcherArtifactFillAmount(int watcherArtifactFillUpAmount) {
        this.watcherArtifactFillUpAmount = watcherArtifactFillUpAmount;
    }



    private void OnDestroy() {
        EndLevelAreaCollider.OnPlayerTriggeredInAnyEndLevelArea -= EndLevelAreaCollider_OnPlayerTriggeredInAnyEndLevelArea;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        ScavengableObstacle.OnAnyObstacleBuilt -= ScavengableObstacle_OnAnyObstacleBuilt;
        StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
        CurrencyStorage_Objective.OnAnyMaxCurrencyAmountReached -= CurrencyStorage_Objective_OnAnyMaxCurrencyAmountReached;
    }

}
