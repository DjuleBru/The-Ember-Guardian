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
        FindNest,
        DestroyNest,
        HUB_HeadToFire,
        HUB_HeadToNewLevel,
        FindAndDestroyNest,
        FindArmorer,
        FindMoreCompanions,
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
        KeepFireLit,
        TalkToArmorer,
        MeetTrainer,
        MeetTamer,
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

    private void Awake() {
        Instance = this;
        objectiveGameObject.SetActive(false); 
        subObjectiveTemplate.gameObject.SetActive(false);
        GetComponent<Animator>().enabled = false;
    }

    public void ShowObjectiveUI(ObjectiveType objectiveType) {
        Debug.Log("ShowObjectiveUI " + objectiveType);
        objectiveText.text = GetObjectiveTextFromType(objectiveType);
        GetComponent<Animator>().enabled = true;
        objectiveGameObject.SetActive(true);
        OnObjectiveUIShown?.Invoke(this, EventArgs.Empty);

        currentObjectiveType = objectiveType;
    }

    public void SetNewObjectiveUI(ObjectiveType objectiveType) {
        Debug.Log("SetNewObjectiveUI " + objectiveType);
        objectiveText.text = GetObjectiveTextFromType(objectiveType);
        objectiveGameObject.SetActive(true);
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
        yield return new WaitForSeconds(delay);

        foreach (SubObjectiveType subObjective in subObjectiveTypeList) {
            SubObjectiveUI subObjectiveText = Instantiate(subObjectiveTemplate, subObjectiveContainer).GetComponent<SubObjectiveUI>();
            subObjectiveText.SetSubObjective(subObjective);
            subObjectiveText.gameObject.SetActive(true);
            yield return new WaitForSeconds(.3f);
        }
    }

    public void SetSubObjectiveCompleted(SubObjectiveType subObjectiveType, List<SubObjectiveType> subObjectiveTypeUnlockedList = null) {
        bool subObjectiveIsListed = false;

        foreach (SubObjectiveUI subObjectiveUI in subObjectiveContainer.GetComponentsInChildren<SubObjectiveUI>(true)) {
            Debug.Log("SetSubObjectiveCompleted " + subObjectiveType + " Checking subObjective " + subObjectiveUI.GetSubObjectiveType());
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

        if(currentObjectiveType == ObjectiveType.SetupCamp) {
            StartCoroutine(EndCampSetupObjective());
        }
    }

    private IEnumerator EndCampSetupObjective() {
        Debug.Log("EndCampSetupObjective");
        SetObjectiveCompletedCoroutine(0f);

        yield return new WaitForSeconds(5f);

        DayNightManager.Instance.ChangeState(DayNightManager.State.Dusk);

        yield return new WaitForSeconds(2f);

        List<SubObjectiveType> subObjectivesUnlocked = new List<SubObjectiveType> {
                SubObjectiveType.Build2Barricades,
                SubObjectiveType.Build2Towers,
                SubObjectiveType.FuelFire,
        };

        SetNewObjectiveUI(ObjectiveType.PrepareForNight);
        SetSubObjectivesUI(subObjectivesUnlocked);
        Fire.Instance.ManualSetFireCurrentMaxFuelTreshold(Fire.State.mild);
        Fire.Instance.SetStructurePrimaryFunctionUnlocked(true);
        Tutorial.Instance.UnlockDefensiveStructureLocations();
    }

    public string GetSubObjectiveTextFromType(SubObjectiveType subObjectiveType) {

        if(subObjectiveType == SubObjectiveType.Keep2WorkersAlive) {
            return "Keep at least 2 workers alive";
        }
        if (subObjectiveType == SubObjectiveType.BuildHunterShrine) {
            return "Build trapper shrine";
        }
        if (subObjectiveType == SubObjectiveType.LightMainFire) {
            return "Light main fire";
        }
        if (subObjectiveType == SubObjectiveType.Build2Barricades) {
            return "Build 2 barricades";
        }
        if (subObjectiveType == SubObjectiveType.Build2Towers) {
            return "Build 2 towers";
        }
        if (subObjectiveType == SubObjectiveType.BuildAmmoCrafter) {
            return "Build ammo crafter";
        }
        if (subObjectiveType == SubObjectiveType.TurnOnAmmoCrafter) {
            return "Turn on ammo crafter";
        }
        if (subObjectiveType == SubObjectiveType.CollectCrafterAmmo) {
            return "Collect ammo from crafter";
        }
        if (subObjectiveType == SubObjectiveType.WaitCraftingAmmo) {
            return "Wait for ammo to be crafted";
        }
        if (subObjectiveType == SubObjectiveType.Recruit2Hunters) {
            return "Recruit at least 2 Hunters";
        }
        if (subObjectiveType == SubObjectiveType.RecruitMoreEmberlings) {
            return "Explore to recruit more emberlings";
        }
        if (subObjectiveType == SubObjectiveType.WaitForHunt) {
            return "Wait for Hunters to hunt animals";
        }
        if (subObjectiveType == SubObjectiveType.CollectOrbsFromHunters) {
            return "Collect orbs from the Hunters";
        }
        if (subObjectiveType == SubObjectiveType.FuelFire) {
            return "Add fuel to the fire";
        }
        if (subObjectiveType == SubObjectiveType.ClearNest) {
            return "Clear the nest from the darklings";
        }
        if (subObjectiveType == SubObjectiveType.LightFire) {
            return "Light the fire to destroy the nest";
        }
        if (subObjectiveType == SubObjectiveType.ExtractEmberTutorial) {
            return "Extract an ember from the main fire (the fire must be fully fuelled)";
        }
        if (subObjectiveType == SubObjectiveType.ExtractEmber) {
            return "Extract an ember from the main fire";
        }
        if (subObjectiveType == SubObjectiveType.FindNest) {
            return "Find the darklings nest";
        }
        if (subObjectiveType == SubObjectiveType.HUB_TalkToTrader) {
            return "Talk to the Gem Trader";
        }
        if (subObjectiveType == SubObjectiveType.HUB_ExtractEmber) {
            return "Extract an ember from the eternal fire";
        }
        if (subObjectiveType == SubObjectiveType.HUB_HeadToTeleporter) {
            return "Head to the teleporter";
        }
        if (subObjectiveType == SubObjectiveType.KeepFireLit) {
            return "Do not let the fire die";
        }
        if (subObjectiveType == SubObjectiveType.MeetTamer) {
            return "Meet the Tamer";
        }
        if (subObjectiveType == SubObjectiveType.MeetTrainer) {
            return "Meet the Trainer";
        }
        return "";
    }

    private string GetObjectiveTextFromType(ObjectiveType objectiveType) {

        if (objectiveType == ObjectiveType.KeepWorkersAlive) {
            return "Protect your emberlings";
        }
        if (objectiveType == ObjectiveType.SetupCamp) {
            return "Setup camp";
        }
        if (objectiveType == ObjectiveType.Explore) {
            return "Explore to the right of camp";
        }
        if (objectiveType == ObjectiveType.PrepareForNight) {
            return "Prepare for the night";
        }
        if (objectiveType == ObjectiveType.Survive) {
            return "Survive the night";
        }
        if (objectiveType == ObjectiveType.FindNest) {
            return "Find the darklings nest";
        }
        if (objectiveType == ObjectiveType.DestroyNest) {
            return "Destroy the darklings nest";
        }
        if (objectiveType == ObjectiveType.HUB_HeadToFire) {
            return "Head back to the main fire";
        }
        if (objectiveType == ObjectiveType.HUB_HeadToNewLevel) {
            return "Explore new areas";
        }
        if (objectiveType == ObjectiveType.FindAndDestroyNest) {
            return "Find and destroy the darkling's nest";
        }
        if (objectiveType == ObjectiveType.FindArmorer) {
            return "Find the Armorer's Workshop";
        }
        if (objectiveType == ObjectiveType.FindMoreCompanions) {
            return "Find more companions";
        }
        return "";
    }

}
