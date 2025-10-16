using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI : MonoBehaviour
{
    [SerializeField] protected GameObject UIGameObject;
    [SerializeField] protected List<GameObject> otherUIGameObjectList;

    [SerializeField] protected GameObject functionUIGameObject;
    [SerializeField] protected GameObject secondaryFunctionUIGameObject;
    [SerializeField] protected GameObject upgradeGameObject;
    [SerializeField] protected GameObject switchUIGameObjectList;

    [SerializeField] protected GameObject functionPayOrbsUIList;
    [SerializeField] protected GameObject secondaryFunctionPayOrbsUIList;

    [SerializeField] protected List<GameObject> upgradeToNextLevelPayOrbsUIList;
    [SerializeField] protected List<GameObject> upgradeToNextLevelUIGameObjectList;

    [SerializeField] protected List<GameObject> levelSlotVisualContainerList;
    [SerializeField] protected bool returnToPrimaryFunctionUIOnTriggerExit = true;

    protected bool uiActive = true;
    protected PayCurrencyUI payOrbsUI;
    protected Structure structure;
    public event EventHandler OnStructureDisplayedFunctionChanged;

    protected virtual void Awake() {
        structure = GetComponentInParent<Structure>();
        payOrbsUI = structure.GetComponent<PayCurrencyUI>();

        SetUIActive(false);
        SetUIXAxisScale();
    }

    protected virtual void Start() {
        structure.OnPlayerTriggeredIn += Structure_OnPlayerTriggeredIn;
        structure.OnPlayerTriggeredOut += Structure_OnPlayerTriggeredOut;
        structure.OnStructureInteractionsUpdated += Structure_OnStructureInteractionsUpdated;
        structure.OnStructureUpgraded += Structure_OnStructureUpgraded;
        structure.OnWorkerStartedRefilling += Structure_OnWorkerStartedRefilling;
        structure.OnForceUpdateInteractionTypeUI += Structure_OnForceUpdateInteractionTypeUI;

        GameInput.Instance.OnPlayerRightSwitchPerformed += GameInput_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerLeftSwitchPerformed += GameInput_OnPlayerLeftSwitchPerformed;
    }


    protected void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        foreach(GameObject go in levelSlotVisualContainerList) {
            go.SetActive(false);
        }

        levelSlotVisualContainerList[structure.GetStructureLevel() -1].SetActive(true);
    }

    protected void SetUIXAxisScale() {
        if (structure.transform.position.x < 0) {
            Vector3 localScale = new Vector3(-1, 1, 1);
            transform.localScale = localScale;
        }
    }

    protected void Structure_OnStructureInteractionsUpdated(object sender, System.EventArgs e) {
        RefreshShownUI();
    }

    private void Structure_OnForceUpdateInteractionTypeUI(object sender, EventArgs e) {
        if(structure.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.primaryFunction) {
            ShowStructurePrimaryFunctionUI();
        }
        if (structure.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.secondaryFunction) {
            ShowStructureSecondaryFunctionUI();
        }
        if (structure.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.upgrade) {
            ShowStructureUpgradeUI();
        }
        UpdateArrowsVisibility();
        OnStructureDisplayedFunctionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, System.EventArgs e) {
        if (!structure.GetPlayerInTriggerArea()) return;

        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();
        if (activeTypes.Count <= 1) return;

        int currentIndex = activeTypes.IndexOf(structure.GetCurrentStructureInteractionType());
        int newIndex = (currentIndex - 1 + activeTypes.Count) % activeTypes.Count;

        SwitchToUIType(activeTypes[newIndex]);
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, System.EventArgs e) {
        if (!structure.GetPlayerInTriggerArea()) return;

        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();
        if (activeTypes.Count <= 1) return;

        int currentIndex = activeTypes.IndexOf(structure.GetCurrentStructureInteractionType());
        int newIndex = (currentIndex + 1) % activeTypes.Count;

        SwitchToUIType(activeTypes[newIndex]);
    }

    private void SwitchToUIType(Structure.StructureInteractionType interactionType) {

        switch (interactionType) {
            case Structure.StructureInteractionType.primaryFunction:
                ShowStructurePrimaryFunctionUI();
                break;
            case Structure.StructureInteractionType.secondaryFunction:
                ShowStructureSecondaryFunctionUI();
                break;
            case Structure.StructureInteractionType.upgrade:
                ShowStructureUpgradeUI();
                break;
        }

        UpdateArrowsVisibility();
        OnStructureDisplayedFunctionChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (structure.GetPlayerInTriggerArea() && structure.GetPayCurrencyUI().GetPlayerInteracting()) return;

        HideStructureUI();
    }

    protected void HideStructureUI() {
        if (returnToPrimaryFunctionUIOnTriggerExit) {
            SwitchToUIType(Structure.StructureInteractionType.primaryFunction);
        }

        SetUIActive(false);
    }

    protected virtual void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (structure.GetPlayerInTriggerArea()) return;

        SetUIActive(true);
        RefreshShownUI();
    }

    private void Structure_OnWorkerStartedRefilling(object sender, EventArgs e) {
        SetUIActive(true);
        StartCoroutine(SetUIActiveAfterDelay(2.5f, false));
    }

    protected IEnumerator SetUIActiveAfterDelay(float delay, bool active) {
        yield return new WaitForSeconds(delay);
        SetUIActive(active);
    }

    protected virtual void SetUIActive(bool active) {
        if (uiActive == active) return;

        UIGameObject.SetActive(active);

        foreach (GameObject go in otherUIGameObjectList) {
            go.SetActive(active);
        }
        uiActive = active;
    }

    protected void UpdateArrowsVisibility() {
        if (switchUIGameObjectList == null) return;

        Transform leftArrow = switchUIGameObjectList.transform.Find("LeftArrow");
        Transform rightArrow = switchUIGameObjectList.transform.Find("RightArrow");

        if (leftArrow == null || rightArrow == null) {
            Debug.LogWarning("LeftArrow or RightArrow GameObject not found as children of SwitchUIGameObject.");
            return;
        }

        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();
        if (activeTypes.Count <= 1) {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
        } else {
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
        }
        return;
    }

    protected void ShowStructurePrimaryFunctionUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.primaryFunction);

        if(functionUIGameObject != null) {
            functionUIGameObject.SetActive(true);
        }
        if(secondaryFunctionPayOrbsUIList != null) {
            secondaryFunctionUIGameObject.SetActive(false);
        }
        if (secondaryFunctionUIGameObject != null) {
            secondaryFunctionUIGameObject.SetActive(false);
        }
        if (upgradeGameObject != null) {
            upgradeGameObject.SetActive(false);
        }

        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
    }

    protected void ShowStructureSecondaryFunctionUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.secondaryFunction);

        if (functionUIGameObject != null) {
            functionUIGameObject.SetActive(false);
        }
        if (secondaryFunctionUIGameObject != null) {
            secondaryFunctionUIGameObject.SetActive(true);
        }
        if (upgradeGameObject != null) {
            upgradeGameObject.SetActive(false);
        }

        if(secondaryFunctionPayOrbsUIList != null) {
            payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(secondaryFunctionPayOrbsUIList));
        }
    }

    protected void ShowStructureUpgradeUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.upgrade);

        if (functionUIGameObject != null) {
            functionUIGameObject.SetActive(false);
        }
        if (secondaryFunctionUIGameObject != null) {
            secondaryFunctionUIGameObject.SetActive(false);
        }
        if (upgradeGameObject != null) {
            upgradeGameObject.SetActive(true);
        }

        int structureLevel = structure.GetStructureLevel();
        foreach(GameObject gameObject in upgradeToNextLevelUIGameObjectList) {
            gameObject.SetActive(false);
        }

        if (structureLevel > upgradeToNextLevelUIGameObjectList.Count) return;
        upgradeToNextLevelUIGameObjectList[structureLevel-1].SetActive(true);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(upgradeToNextLevelPayOrbsUIList[structureLevel - 1]));
    }

    protected virtual void RefreshShownUI() {
        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();

        // Désactiver toutes les UI par défaut

        if (functionUIGameObject != null) {
            functionUIGameObject.SetActive(false);
        }
        if (secondaryFunctionUIGameObject != null) {
            secondaryFunctionUIGameObject.SetActive(false);
        }
        if (upgradeGameObject != null) {
            upgradeGameObject.SetActive(false);
        }

        // Afficher le type courantx
        if (activeTypes.Contains(structure.GetCurrentStructureInteractionType())) {
            switch (structure.GetCurrentStructureInteractionType()) {
                case Structure.StructureInteractionType.primaryFunction:
                    ShowStructurePrimaryFunctionUI();
                    break;
                case Structure.StructureInteractionType.secondaryFunction:
                    ShowStructureSecondaryFunctionUI();
                    break;
                case Structure.StructureInteractionType.upgrade:
                    ShowStructureUpgradeUI();
                    break;
            }
        }
        else if (activeTypes.Count > 0) {
            // Si le type courant est invalide, afficher le premier disponible
            structure.SetCurrentStructureInteractionType(activeTypes[0]);
            RefreshShownUI();
        }

        UpdateArrowsVisibility();
    }

    protected List<PayCurrencyTemplateWorldUI> RecomposePayOrbsUIList(GameObject upgradeToNextLevelPayOrbsUIGameObject) {
        List<PayCurrencyTemplateWorldUI> payOrbsUIList = new List<PayCurrencyTemplateWorldUI>();
        PayCurrencyTemplateWorldUI[] orbTemplateWorldUIs = upgradeToNextLevelPayOrbsUIGameObject.GetComponentsInChildren<PayCurrencyTemplateWorldUI>();


        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplateWorldUIs) {
            payOrbsUIList.Add(orbTemplateWorldUI);
        }

        return payOrbsUIList;
    }

    public int GetCurrentPayCurrencyAmount() {
        return payOrbsUI.GetCurrencyAmountToPay();
    }

    protected void OnDestroy() {
        GameInput.Instance.OnPlayerRightSwitchPerformed -= GameInput_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerLeftSwitchPerformed -= GameInput_OnPlayerLeftSwitchPerformed;
    }

}
