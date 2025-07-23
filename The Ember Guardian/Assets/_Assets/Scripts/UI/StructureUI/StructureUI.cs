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
    protected bool playerInTriggerArea;
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

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, System.EventArgs e) {
        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();
        if (activeTypes.Count <= 1) return;

        int currentIndex = activeTypes.IndexOf(structure.GetCurrentStructureInteractionType());
        int newIndex = (currentIndex - 1 + activeTypes.Count) % activeTypes.Count;

        SwitchToUIType(activeTypes[newIndex]);
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, System.EventArgs e) {
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
        if (!playerInTriggerArea) return;

        playerInTriggerArea = false;

        if(returnToPrimaryFunctionUIOnTriggerExit) {
            SwitchToUIType(Structure.StructureInteractionType.primaryFunction);
        }

        SetUIActive(false);
    }

    protected virtual void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (playerInTriggerArea) return;

        playerInTriggerArea = true;
        SetUIActive(true);
        RefreshShownUI();
    }

    private void Structure_OnWorkerStartedRefilling(object sender, EventArgs e) {
        Debug.Log("Structure_OnWorkerStartedRefilling ");
        SetUIActive(true);
        StartCoroutine(SetUIActiveAfterDelay(1f, false));
    }

    protected IEnumerator SetUIActiveAfterDelay(float delay, bool active) {
        yield return new WaitForSeconds(delay);
        SetUIActive(active);
    }

    protected virtual void SetUIActive(bool active) {
        if (uiActive == active) return;
        
        //Debug.Log("SetUIActive " + active);
        UIGameObject.SetActive(active);

        foreach (GameObject go in otherUIGameObjectList) {
            go.SetActive(active);
        }
        uiActive = active;
    }

    protected void UpdateArrowsVisibility() {
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

        int currentIndex = activeTypes.IndexOf(structure.GetCurrentStructureInteractionType());

        // Afficher ou cacher les flèches
        leftArrow.gameObject.SetActive(currentIndex > 0); // Flèche gauche active si ce n'est pas le premier élément
        rightArrow.gameObject.SetActive(currentIndex < activeTypes.Count - 1); // Flèche droite active si ce n'est pas le dernier élément
    }

    protected void ShowStructurePrimaryFunctionUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.primaryFunction);

        functionUIGameObject.SetActive(true);
        secondaryFunctionUIGameObject.SetActive(false);
        upgradeGameObject.SetActive(false);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
    }

    protected void ShowStructureSecondaryFunctionUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.secondaryFunction);

        secondaryFunctionUIGameObject.SetActive(true);
        functionUIGameObject.SetActive(false);
        upgradeGameObject.SetActive(false);

        if(secondaryFunctionPayOrbsUIList != null) {
            payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(secondaryFunctionPayOrbsUIList));
        }
    }

    protected void ShowStructureUpgradeUI() {
        structure.SetCurrentStructureInteractionType(Structure.StructureInteractionType.upgrade);

        functionUIGameObject.SetActive(false);
        secondaryFunctionUIGameObject.SetActive(false);
        upgradeGameObject.SetActive(true);

        int structureLevel = structure.GetStructureLevel();
        foreach(GameObject gameObject in upgradeToNextLevelUIGameObjectList) {
            gameObject.SetActive(false);
        }

        if (structureLevel > upgradeToNextLevelUIGameObjectList.Count) return;
        upgradeToNextLevelUIGameObjectList[structureLevel-1].SetActive(true);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(upgradeToNextLevelPayOrbsUIList[structureLevel - 1]));
    }

    protected void RefreshShownUI() {
        List<Structure.StructureInteractionType> activeTypes = structure.GetActiveStructureInteractionTypeList();

        // Désactiver toutes les UI par défaut
        functionUIGameObject.SetActive(false);
        secondaryFunctionUIGameObject.SetActive(false);
        upgradeGameObject.SetActive(false);

        // Afficher le type courant
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

        //UpdateSwitchUIGameObjectActivation();
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

}
