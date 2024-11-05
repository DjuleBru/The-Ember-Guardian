using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI : MonoBehaviour
{
    [SerializeField] protected GameObject UIGameObject;

    [SerializeField] protected GameObject functionUIGameObject;
    [SerializeField] protected GameObject upgradeGameObject;
    [SerializeField] protected GameObject switchUIGameObjectList;

    [SerializeField] protected GameObject functionPayOrbsUIList;

    [SerializeField] protected List<GameObject> upgradeToNextLevelPayOrbsUIList;
    [SerializeField] protected List<GameObject> upgradeToNextLevelUIGameObjectList;

    [SerializeField] protected List<GameObject> levelSlotVisualContainerList;

    protected PayOrbsUI payOrbsUI;
    protected Structure structure;

    protected void Awake() {
        structure = GetComponentInParent<Structure>();
        payOrbsUI = structure.GetComponent<PayOrbsUI>();

        SetUIActive(false);
        SetUIXAxisScale();
    }

    protected void Start() {
        structure.OnPlayerTriggeredIn += Structure_OnPlayerTriggeredIn;
        structure.OnPlayerTriggeredOut += Structure_OnPlayerTriggeredOut;
        structure.OnStructureInteractionsUpdated += Structure_OnStructureInteractionsUpdated;

        GameInput.Instance.OnPlayerLeftRightSwitchPerformed += GameInput_OnPlayerLeftRightSwitchPerformed;
    }


    private void SetUIXAxisScale() {
        if (structure.transform.position.x < 0) {
            Vector3 localScale = new Vector3(-1, 1, 1);
            transform.localScale = localScale;
        }
    }

    private void Structure_OnStructureInteractionsUpdated(object sender, System.EventArgs e) {
        RefreshShownUI();
    }

    private void GameInput_OnPlayerLeftRightSwitchPerformed(object sender, System.EventArgs e) {
        if (structure.GetActiveStructureInteractionTypeList().Count <= 1) return;

        if(structure.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.function) {
            ShowStructureUpgradeUI();
        } else {
            ShowStructureFunctionUI();
        }
    }

    protected void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        ShowStructureFunctionUI();
        SetUIActive(false);
    }

    protected void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        SetUIActive(true);
        RefreshShownUI();
    }

    private void Structure_OnStructureFunctionUnlocked(object sender, System.EventArgs e) {
        RefreshShownUI();
    }

    private void Structure_OnStructureFunctionLocked(object sender, System.EventArgs e) {
        RefreshShownUI();
    }

    protected void SetUIActive(bool active) {
        UIGameObject.SetActive(active);
        UpdateSwitchUIGameObjectActivation();
    }

    protected void UpdateSwitchUIGameObjectActivation() {

        if(structure.GetActiveStructureInteractionTypeList().Count > 1) {
            switchUIGameObjectList.SetActive(true);
        } else {
            switchUIGameObjectList.SetActive(false);
        }
    }

    protected void ShowStructureFunctionUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.function);

        functionUIGameObject.SetActive(true);
        upgradeGameObject.SetActive(false);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
    }

    protected void ShowStructureUpgradeUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.upgrade);

        functionUIGameObject.SetActive(false);
        upgradeGameObject.SetActive(true);

        int structureLevel = structure.GetStructureLevel();
        foreach(GameObject gameObject in upgradeToNextLevelUIGameObjectList) {
            gameObject.SetActive(false);
        }

        upgradeToNextLevelUIGameObjectList[structureLevel-1].SetActive(true);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(upgradeToNextLevelPayOrbsUIList[structureLevel - 1]));
    }

    protected void ActivateUpgradeUI(bool active) {
        RefreshShownUI();
        UpdateSwitchUIGameObjectActivation();
    }

    protected void RefreshShownUI() {
        // Lower Upgrade types are higher priority

        // Upgrade
        if (structure.GetActiveStructureInteractionTypeList().Contains(Structure.StructureInteractionType.upgrade)) {
            ShowStructureUpgradeUI();
        } else {
           upgradeGameObject.SetActive(false);
        }

        // Function
        if (structure.GetActiveStructureInteractionTypeList().Contains(Structure.StructureInteractionType.function)) {
            ShowStructureFunctionUI();
        }
        else {
            functionUIGameObject.SetActive(false);
        }

        UpdateSwitchUIGameObjectActivation();
    }

    private List<OrbTemplateWorldUI> RecomposePayOrbsUIList(GameObject upgradeToNextLevelPayOrbsUIGameObject) {
        List<OrbTemplateWorldUI> payOrbsUIList = new List<OrbTemplateWorldUI>();
        OrbTemplateWorldUI[] orbTemplateWorldUIs = upgradeToNextLevelPayOrbsUIGameObject.GetComponentsInChildren<OrbTemplateWorldUI>();

        foreach (OrbTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIs) {
            payOrbsUIList.Add(orbTemplateWorldUI);
        }

        return payOrbsUIList;
    }

}
