using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArchitectTable_MainMenu : ArchitectTable
{
    [SerializeField] private GameObject customizeCampPanelGO;
    [SerializeField] private GameObject firstSelectedGO;

    private bool panelOpen;
    protected override void Awake() {
        Instance = this;

        AddStructureBlueprint[] allStructureBlueprints = allStructureBlueprintsParent.GetComponentsInChildren<AddStructureBlueprint>();
        foreach (AddStructureBlueprint blueprint in allStructureBlueprints) {
            blueprint.SubscribeToNewItemsEvents();
        }
        customizeCampPanelGO.SetActive(false);
    }

    public void OpenCloseCustomizeCampPanel() {
        Debug.Log("OpenCloseCustomizeCampPanel " + panelOpen) ;
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
}
