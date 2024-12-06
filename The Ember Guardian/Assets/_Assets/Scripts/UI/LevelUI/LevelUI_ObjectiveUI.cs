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
        ExtractEmber,
        FindNest,
        ClearNest,
        LightFire,
    }

    public static LevelUI_ObjectiveUI Instance;

    [SerializeField] private GameObject objectiveGameObject;
    [SerializeField] private Animator objectiveAnimator;
    [SerializeField] private Transform subObjectiveContainer;
    [SerializeField] private Transform subObjectiveTemplate;
    [SerializeField] private TextMeshProUGUI objectiveText;
    private ObjectiveType currentObjectiveType;

    public event EventHandler OnObjectiveUIShown;
    public event EventHandler OnObjectiveUICompleted;
    public event EventHandler OnSubObjectiveUICompleted;

    private void Awake() {
        Instance = this;
        objectiveGameObject.SetActive(false); 
        subObjectiveTemplate.gameObject.SetActive(false);
        GetComponent<Animator>().enabled = false;
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
        objectiveAnimator.SetTrigger("NewObjective");
        OnObjectiveUIShown?.Invoke(this, EventArgs.Empty);

        currentObjectiveType = objectiveType;
        Debug.Log("SetNewObjectiveUI " + currentObjectiveType);
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
            Debug.Log("current subObjectives " + subObjectiveUI.GetSubObjectiveType().ToString());

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

        Debug.Log("allSubObjectivesCompleted " + allSubObjectivesCompleted);
        Debug.Log("subObjectiveTypeUnlockedList == null" + subObjectiveTypeUnlockedList == null);
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
        Debug.Log("SetObjectiveCompleted " + delay);
        StartCoroutine(SetObjectiveCompletedCoroutine(delay));
    }

    public IEnumerator SetObjectiveCompletedCoroutine(float delay) {
        Debug.Log("SetObjectiveCompletedCoroutine " + delay);
        yield return new WaitForSeconds(delay);
        objectiveAnimator.SetTrigger("Completed");
        yield return new WaitForSeconds(.5f);
        OnObjectiveUICompleted?.Invoke(this, EventArgs.Empty);

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
            return "Recruit at least 2 trappers";
        }
        if (subObjectiveType == SubObjectiveType.RecruitMoreEmberlings) {
            return "Explore to recruit more emberlings";
        }
        if (subObjectiveType == SubObjectiveType.WaitForHunt) {
            return "Wait for trappers to hunt animals";
        }
        if (subObjectiveType == SubObjectiveType.CollectOrbsFromHunters) {
            return "Collect orbs from the trappers";
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
        if (subObjectiveType == SubObjectiveType.ExtractEmber) {
            return "Extract an ember from the main fire";
        }
        if (subObjectiveType == SubObjectiveType.FindNest) {
            return "Find the darklings nest";
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
        return "";
    }

}
