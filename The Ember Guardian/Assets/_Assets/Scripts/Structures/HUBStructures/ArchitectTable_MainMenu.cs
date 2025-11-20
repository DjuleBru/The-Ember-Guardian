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

        AddStructureBlueprint[] allStructureBlueprints = allStructureBlueprintsParent.GetComponentsInChildren<AddStructureBlueprint>();
        foreach (AddStructureBlueprint blueprint in allStructureBlueprints) {
            blueprint.SubscribeToNewItemsEvents();
        }
        customizeCampPanelGO.SetActive(false);

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
}
