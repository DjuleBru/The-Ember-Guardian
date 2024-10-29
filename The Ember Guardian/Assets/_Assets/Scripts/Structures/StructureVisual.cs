using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureVisual : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer structureSpriteRenderer;
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
    }

    protected void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        int structureLevel = structure.GetStructureLevel();
        structureSpriteRenderer.sprite = structure.GetStructureSO().buildingUpgradeSpriteList[structureLevel - 1];
    }
}
