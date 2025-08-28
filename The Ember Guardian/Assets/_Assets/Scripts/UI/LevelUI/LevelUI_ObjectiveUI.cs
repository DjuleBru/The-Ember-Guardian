using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUI_ObjectiveUI : MonoBehaviour
{

    public enum ObjectiveType {
        KeepWorkersAlive,
        SetupCamp,
        PrepareForNight,
        Explore,
        Survive,
        TrySurvive,
        FindNest,
        DestroyNest,
        HUB_HeadToFire,
        HUB_HeadToNewLevel,
        HUBDemo_PrepareToReturn,
        FindAndDestroyNest,
        FindArmorer,
        FindMoreCompanions,
        FindWatcher,
        FindArchitect,
        SurviveNights,
        FindStockpiles,
        ReturnToHub,
        ExploreCorruptedCity,
        FindArchitectTable,
        CollectOrbs,
        EscortConvoy,
        DefendFlame,
        FindAndDestroyTwoNests,
    }

    public enum SubObjectiveType {
        None,
        Keep2WorkersAlive,
        LightMainFire,
        Build2Barricades,
        Build2Towers,
        BuildAmmoCrafter,
        BuildHunterShrine,
        Recruit2Hunters,
        RecruitEmberlings,
        RecruitMoreEmberlings,
        CollectCrafterAmmo,
        WaitCraftingAmmo,
        TurnOnAmmoCrafter,
        WaitForHunt,
        CollectOrbsFromHunters,
        FuelFire,
        ExtractEmberTutorial,
        ExtractEmber,
        FindNest,
        ClearNest,
        LightFire,
        HUB_TalkToTrader,
        HUB_ExtractEmber,
        HUB_HeadToTeleporter,
        HUBDemo_DropGems,
        HUBDemo_BuyUpgrade,
        KeepFireLit,
        MeetTrainer,
        MeetTamer,
        SurviveNights,
        TalkToWatcher,
        LoadBelt,
        ReloadGun,
        HeadBackToCamp,
        OpenChest,
        FindArmorerStockpile,
        FindTrainerStockpile,
        TeleportBackToHub,
        FindArchitect,
        ProgressWithScavengers,
        TalkToArchitect,
        BuildWatcherArtifact,
        CollectOrbs,
        TalkToMushroomMerchant,
    }

    public static LevelUI_ObjectiveUI Instance;

    [SerializeField] private GameObject objectiveGameObject;
    [SerializeField] private Animator objectiveAnimator;
    [SerializeField] private Transform subObjectiveContainer;
    [SerializeField] private Transform subObjectiveTemplate;
    [SerializeField] private TextMeshProUGUI objectiveText;
    private ObjectiveType currentObjectiveType;

    public event EventHandler OnObjectiveUIShown;
    public event EventHandler OnObjectiveCompleted;
    public event EventHandler OnSubObjectiveUICompleted;
    public event EventHandler OnSubObjectiveUIProgressed;

    private void Awake() {
        Instance = this;
        objectiveGameObject.SetActive(false); 
        subObjectiveTemplate.gameObject.SetActive(false);
        GetComponent<Animator>().enabled = false;
    }

    private void Start() {
        if(LevelObjectives.Instance != null) {
            LevelObjectives.Instance.OnNightSurvived += LevelObjectives_OnNightSurvived;
            LevelObjectives.Instance.OnObstacleRemoved += LevelObjectives_OnObstacleRemoved;
            LevelObjectives.Instance.OnWatcherArtifactFilled += LevelObjectives_OnWatcherArtifactFilled;
        }
    }


    public void ShowObjectiveUI(ObjectiveType objectiveType) {
        objectiveText.text = GetObjectiveTextFromType(objectiveType);
        GetComponent<Animator>().enabled = true;
        objectiveGameObject.SetActive(true);
        OnObjectiveUIShown?.Invoke(this, EventArgs.Empty);

        currentObjectiveType = objectiveType;
    }

    public void SetNewObjectiveUI(ObjectiveType objectiveType) {
        objectiveText.text = GetObjectiveTextFromType(objectiveType);
        objectiveGameObject.SetActive(true);
        objectiveAnimator.ResetTrigger("Completed");
        objectiveAnimator.SetTrigger("NewObjective");
        OnObjectiveUIShown?.Invoke(this, EventArgs.Empty);

        currentObjectiveType = objectiveType;
    }

    public void SetSubObjectivesUI(List<SubObjectiveType> subObjectiveTypeList) {
        StartCoroutine(InstantiateSubObjectivesUICoroutine(subObjectiveTypeList, 4f));
    }

    private IEnumerator SetSubObjectivesUICoroutine(List<SubObjectiveUI> subObjectiveUIList, float delay) {
        yield return new WaitForSeconds(delay);

        foreach (SubObjectiveUI subObjective in subObjectiveUIList) {
            subObjective.gameObject.SetActive(true);
            yield return new WaitForSeconds(.3f);
        }
    }

    private IEnumerator InstantiateSubObjectivesUICoroutine(List<SubObjectiveType> subObjectiveTypeList, float delay) {
        List<SubObjectiveUI> subObjectivesUIList = new List<SubObjectiveUI>();
        foreach (SubObjectiveType subObjective in subObjectiveTypeList) {
            SubObjectiveUI subObjectiveText = Instantiate(subObjectiveTemplate, subObjectiveContainer).GetComponent<SubObjectiveUI>();
            subObjectiveText.SetSubObjective(subObjective);
            subObjectivesUIList.Add(subObjectiveText);
        }

        yield return new WaitForSeconds(delay);

        foreach (SubObjectiveUI subObjectiveUI in subObjectivesUIList) {
            if(!subObjectiveUI.GetCompleted()) {
                subObjectiveUI.gameObject.SetActive(true);
                yield return new WaitForSeconds(.3f);
            }
        }
    }

    public void SetSubObjectiveCompleted(SubObjectiveType subObjectiveType, List<SubObjectiveType> subObjectiveTypeUnlockedList = null) {
        bool subObjectiveIsListed = false;

        foreach (SubObjectiveUI subObjectiveUI in subObjectiveContainer.GetComponentsInChildren<SubObjectiveUI>(true)) {
            if (subObjectiveUI.GetSubObjectiveType() == subObjectiveType) {
                subObjectiveIsListed = true;    
            }
        }

        if (!subObjectiveIsListed) return;
        StartCoroutine(SetSubObjectiveCompleteCoroutine(subObjectiveType, subObjectiveTypeUnlockedList));
    }

    public void SetNextSubObjective(SubObjectiveType subObjectiveType, SubObjectiveType nextSubObjective) {
        foreach (SubObjectiveUI subObjectiveUI in subObjectiveContainer.GetComponentsInChildren<SubObjectiveUI>(true)) {

            if (subObjectiveUI.GetSubObjectiveType() == subObjectiveType) {
                subObjectiveUI.SetNext(nextSubObjective);
                OnSubObjectiveUIProgressed?.Invoke(this, EventArgs.Empty);
            }

        }
    }

    public IEnumerator SetSubObjectiveCompleteCoroutine(SubObjectiveType subObjectiveTypeCompleted, List<SubObjectiveType> subObjectiveTypeUnlockedList = null) {
        bool allSubObjectivesCompleted = true;

        foreach (SubObjectiveUI subObjectiveUI in subObjectiveContainer.GetComponentsInChildren<SubObjectiveUI>(true)) {
            if (subObjectiveUI.GetSubObjectiveType() == SubObjectiveType.None) continue;

            if (subObjectiveUI.GetSubObjectiveType() == subObjectiveTypeCompleted) {
                subObjectiveUI.SetCompleted();
                yield return new WaitForSeconds(.6f);
                OnSubObjectiveUICompleted?.Invoke(this, EventArgs.Empty);
            }

            if (!subObjectiveUI.GetCompleted()) {
                allSubObjectivesCompleted = false;
            }
        }

        if(subObjectiveTypeUnlockedList != null) {

            List<SubObjectiveUI> subObjectivesUIUnlocked = new List<SubObjectiveUI> ();
            foreach (SubObjectiveType subObjectiveType in subObjectiveTypeUnlockedList) {
                SubObjectiveUI subObjectiveText = Instantiate(subObjectiveTemplate, subObjectiveContainer).GetComponent<SubObjectiveUI>();
                subObjectiveText.SetSubObjective(subObjectiveType);
                subObjectivesUIUnlocked.Add(subObjectiveText);
            }

            yield return new WaitForSeconds(1f);

            StartCoroutine(SetSubObjectivesUICoroutine(subObjectivesUIUnlocked, 1f));
            yield return null;
        } else {

            if (allSubObjectivesCompleted) {
                StartCoroutine(SetObjectiveCompletedCoroutine(1f));
            }
        }
    }

    public void SetObjectiveCompleted(float delay) {
        StartCoroutine(SetObjectiveCompletedCoroutine(delay));
    }

    public IEnumerator SetObjectiveCompletedCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        objectiveAnimator.SetTrigger("Completed");
        yield return new WaitForSeconds(.5f);
        OnObjectiveCompleted?.Invoke(this, EventArgs.Empty);
    }


    private void LevelObjectives_OnNightSurvived(object sender, EventArgs e) {
        RefreshCountableSubObjective(SubObjectiveType.SurviveNights);
    }

    private void LevelObjectives_OnObstacleRemoved(object sender, EventArgs e) {
        RefreshCountableSubObjective(SubObjectiveType.ProgressWithScavengers);
    }

    private void LevelObjectives_OnWatcherArtifactFilled(object sender, EventArgs e) {
        RefreshCountableSubObjective(SubObjectiveType.CollectOrbs);
    }

    private void RefreshCountableSubObjective(SubObjectiveType subObjectiveType) {
        foreach (SubObjectiveUI subObjectiveUI in subObjectiveContainer.GetComponentsInChildren<SubObjectiveUI>(true)) {
            if (subObjectiveUI.GetSubObjectiveType() == SubObjectiveType.None) continue;

            if (subObjectiveUI.GetSubObjectiveType() == subObjectiveType) {
                subObjectiveUI.SetSubObjective(subObjectiveType);
            }

        }
    }

    public string GetSubObjectiveTextFromType(SubObjectiveType subObjectiveType) {
        string subObjectiveKey = "SubObj_" + subObjectiveType.ToString();

        if (subObjectiveType == SubObjectiveType.SurviveNights) {
            return LocalizationManager.Instance.GetLocalizedText("SubObj_Survive") + " " + LevelObjectives.Instance.GetNightsToSurvive() + " " + LocalizationManager.Instance.GetLocalizedText("SubObj_Nights") + " " + "(" + LevelObjectives.Instance.GetNightsSurvived() + "/" + LevelObjectives.Instance.GetNightsToSurvive() + ")";
        }
        if (subObjectiveType == SubObjectiveType.ProgressWithScavengers) {
            return LocalizationManager.Instance.GetLocalizedText("SubObj_ProgressWithScavengers") + " " + LevelObjectives.Instance.GetObstaclesToRemove() + " " + LocalizationManager.Instance.GetLocalizedText("SubObj_Obstacles") + " " + "(" + LevelObjectives.Instance.GetObstaclesRemoved() + "/" + LevelObjectives.Instance.GetObstaclesToRemove() + ")";
        }
        if (subObjectiveType == SubObjectiveType.CollectOrbs) {
            return LocalizationManager.Instance.GetLocalizedText("SubObj_CollectOrbs") + "(" + LevelObjectives.Instance.GetWatcherArtifactFillAmount() + "/" + LevelObjectives.Instance.GetWatcherArtifactTotalFillAmount() + ")";
        }
        return LocalizationManager.Instance.GetLocalizedText(subObjectiveKey);
        
    }

    private string GetObjectiveTextFromType(ObjectiveType objectiveType) {
        string objectiveKey = "Obj_" + objectiveType.ToString();
        return LocalizationManager.Instance.GetLocalizedText(objectiveKey);
    }

    public ObjectiveType GetCurrentObjectiveType() {
        return currentObjectiveType;
    }

    private void OnDestroy() {
        if (LevelObjectives.Instance != null) {
            LevelObjectives.Instance.OnNightSurvived -= LevelObjectives_OnNightSurvived;
        }
    }

}
