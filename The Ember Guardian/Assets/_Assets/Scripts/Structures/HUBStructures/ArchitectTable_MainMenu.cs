using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArchitectTable_MainMenu : ArchitectTable
{
    [SerializeField] private GameObject customizeCampPanelGO;
    [SerializeField] private GameObject firstSelectedGO;
    [SerializeField] private TextMeshProUGUI remainingBudgetText;
    [SerializeField] private int initialBudget;

    private bool panelOpen;
    protected override void Awake() {
        Instance = this;

        AddStructureBlueprint[] allStructureBlueprints = allStructureBlueprintsParent.GetComponentsInChildren<AddStructureBlueprint>(true);
        foreach (AddStructureBlueprint blueprint in allStructureBlueprints) {
            blueprint.SubscribeToNewItemsEvents();
        }
        customizeCampPanelGO.SetActive(false);

    }

    protected void Start() {
        bool moreGems1Unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.MoreCampCustomizationBudget1);
        bool moreGems2Unlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.MoreCampCustomizationBudget2);

        if(moreGems1Unlocked) {
            initialBudget += 10;
        }
        if (moreGems2Unlocked) {
            initialBudget += 10;
        }

        remainingBudget = initialBudget;
        remainingBudgetText.text = initialBudget.ToString();
    }

    public void OpenCloseCustomizeCampPanel() {
        if (panelOpen) {

            panelOpen = false;
            customizeCampPanelGO.SetActive(false);

        }
        else {

            panelOpen = true;
            customizeCampPanelGO.SetActive(true);

            if (GameInput.Instance.IsUsingGamepad()) {
                EventSystem.current.SetSelectedGameObject(firstSelectedGO);
            }

        }
    }
    public override void AddToBudget(int addition) {
        remainingBudget += addition;
        remainingBudgetText.text = remainingBudget.ToString();
    }
    public override void RemoveFromBudget(int removal) {
        remainingBudget -= removal;
        remainingBudgetText.text = remainingBudget.ToString();
    }

    private void OnDestroy() {
        AddStructureBlueprint[] allStructureBlueprints = allStructureBlueprintsParent.GetComponentsInChildren<AddStructureBlueprint>(true);

        foreach (AddStructureBlueprint blueprint in allStructureBlueprints) {
            blueprint.UnsubscribeToNewItemsEvents();
        }
    }
}
