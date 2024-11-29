using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StructureVisual : MonoBehaviour {

    [SerializeField] protected List<GameObject> structureLights;
    [SerializeField] protected SpriteRenderer structureSpriteRenderer;
    [SerializeField] protected SpriteRenderer structureFunctionIconSpriteRenderer;
    [SerializeField] protected Color greyedStructionIconColor;
    [SerializeField] protected Material unhoveredMaterial;
    [SerializeField] protected Material hoveredMaterial;
    [SerializeField] protected bool structureHasFunctionIcon;

    protected Structure structure;

    protected virtual void Awake() {
        structure = GetComponentInParent<Structure>();
        SetXAxisScale();
    }

    private void SetXAxisScale() {
        if (structure.transform.position.x < 0) {
            Vector3 localScale = new Vector3(-1, 1, 1);
            transform.localScale = localScale;
        }
    }

    protected virtual void Start() {
        structure.OnStructureUpgraded += Structure_OnStructureUpgraded;
        structure.OnStructureInteractionsUpdated += Structure_OnStructureInteractionsUpdated;
        structure.OnPlayerTriggeredIn += Structure_OnPlayerTriggeredIn;
        structure.OnPlayerTriggeredOut += Structure_OnPlayerTriggeredOut;

        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        foreach (GameObject gameObject in structureLights) {
            gameObject.SetActive(true);
        }
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        foreach(GameObject gameObject in structureLights) {
            gameObject.SetActive(false);
        }
    }

    private void Structure_OnStructureInteractionsUpdated(object sender, System.EventArgs e) {
        if(structure.GetActiveStructureInteractionTypeList().Contains(Structure.StructureInteractionType.primaryFunction)) {
            HighlightStructureFunctionIcon(true);
        }
        else {
            HighlightStructureFunctionIcon(false);
        }
    }

    protected virtual void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        int structureLevel = structure.GetStructureLevel();
        structureSpriteRenderer.sprite = structure.GetStructureSO().buildingUpgradeSpriteList[structureLevel - 1];
    }

    protected virtual void HighlightStructureFunctionIcon(bool highlight) {
        if (!structureHasFunctionIcon) return;
        if(highlight) {
            structureFunctionIconSpriteRenderer.color = Color.white;
        } else {
            structureFunctionIconSpriteRenderer.color = greyedStructionIconColor;
        }
    }

    protected virtual void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        structureSpriteRenderer.material = unhoveredMaterial;
    }

    protected virtual void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        structureSpriteRenderer.material = hoveredMaterial;
    }

}
