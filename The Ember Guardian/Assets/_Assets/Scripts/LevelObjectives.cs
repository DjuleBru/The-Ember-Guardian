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
    private bool darklingNestCleared;
    private int NPCInteractionsIndex;

    private int nightsSurvived = -1;
    private int nightsToSurvive;

    public event EventHandler OnNightSurvived;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;

        if (levelMerchantList.Count != 0) {
            foreach (HubMerchant levelMerchant in levelMerchantList) {
                levelMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant;
            }
        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.DestroyNest) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
            PlayerCurrencies.Instance.OnEmberDropped += PlayerCurrencies_OnEmberDropped;

            if (EndLevelArea.Instance != null) {
                EndLevelArea.Instance.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
            }
        }

        if (LevelManager.Instance.GetLevelSO().endLevelType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights) {

            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
            nightsToSurvive = LevelManager.Instance.GetLevelSO().nightsToSurviveAmount;
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

            LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(.5f);

            yield return new WaitForSeconds(2f);

            if (NPCInteractionsIndex == 1) {
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
        }
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
        StartCoroutine(EndLevelCoroutine());
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
    }

    private void EndLevelArea_OnEndLevelAreaCleared(object sender, System.EventArgs e) {
        darklingNestCleared = true;
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.ClearNest, LevelUI_ObjectiveUI.SubObjectiveType.LightFire);
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

    private IEnumerator EndLevelCoroutine() {
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

}
