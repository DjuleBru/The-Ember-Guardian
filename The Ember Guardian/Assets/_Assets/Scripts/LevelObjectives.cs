using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectives : MonoBehaviour
{

    [SerializeField] private HubMerchant levelMerchant;
    [SerializeField] private HubMerchantTalkUI levelMerchantTalkUI;
    [SerializeField] private MerchantTextLinesSO finalMerchantTextLines;
    private EndLevelArea endLevelArea;

    private bool emberExtracted;
    private bool initialFireLit;
    private bool darklingNestCleared;
    private bool playerStoppedInteractingWithMerchant;
    private int NPCInteractionsIndex;

    private void Start() {
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        PlayerCurrencies.Instance.OnEmberDropped += PlayerCurrencies_OnEmberDropped;
        EndLevelArea.Instance.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
        EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;

        if(levelMerchant != null) {
            levelMerchant.OnPlayerStoppedInteractingWithHubMerchant += LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant;
        }
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        initialFireLit = true;
    }

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
        MetaProgressionManager.Instance.SetMerchantUnlocked(HubMerchant.HubMerchantType.GunMerchant);
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

    private void LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        NPCInteractionsIndex++;

        Debug.Log("LevelMerchant_OnPlayerStoppedInteractingWithHubMerchant " + NPCInteractionsIndex);
        StartCoroutine(SetNextNPCObjective());
    }


    private IEnumerator SetNextNPCObjective() {
        LevelUI_ObjectiveUI.Instance.SetObjectiveCompleted(.5f);

        yield return new WaitForSeconds(2f);

        if (levelMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {
            if(NPCInteractionsIndex == 1) {
                LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.DestroyNest);

                List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectives = new List<LevelUI_ObjectiveUI.SubObjectiveType>();
                if (!PlayerCurrencies.Instance.GetCarryingEmber()) {
                    subObjectives.Add(LevelUI_ObjectiveUI.SubObjectiveType.ExtractEmber);
                }

                subObjectives.Add(LevelUI_ObjectiveUI.SubObjectiveType.ClearNest);

                LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectives);
            }
        }
    }

    private IEnumerator EndLevelCoroutine() {
        yield return new WaitForSeconds(1f);
        levelMerchantTalkUI.SetTalkingWithMerchant(finalMerchantTextLines, false);
    }
}
