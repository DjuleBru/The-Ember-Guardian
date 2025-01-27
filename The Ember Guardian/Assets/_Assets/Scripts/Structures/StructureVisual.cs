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
    [SerializeField] protected Animator structureSpriteMaterialAnimator;

    [SerializeField] protected bool animateSpriteMaterialOnBuild;
    [SerializeField] protected bool structureHasFunctionIcon;

    protected Structure structure;
    protected bool built;

    protected virtual void Awake() {
        structure = GetComponentInParent<Structure>();
        SetXAxisScale();
    }

    protected void SetXAxisScale() {
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
        structure.OnInitialCampStructureBuilt += Structure_OnInitialCampStructureBuilt;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            PlayerCampVisual.Instance.OnCampBackgroundBuilt += PlayerCampVisual_OnCampBackgroundBuilt;
        }

        HandleInitialBuildAnimation();
    }

    private void Structure_OnInitialCampStructureBuilt(object sender, System.EventArgs e) {
        HandleInitialBuildAnimation();
    }

    private void HandleInitialBuildAnimation() {

        if (!animateSpriteMaterialOnBuild) {

            built = true;
            Debug.Log("BuiltAtStart");
            structureSpriteMaterialAnimator.SetTrigger("BuiltAtStart");

        }
        else {

            Debug.Log("Build");
            structureSpriteMaterialAnimator.SetTrigger("Build");

        }
    }

    protected virtual void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        foreach (GameObject gameObject in structureLights) {
            gameObject.SetActive(true);
        }
    }

    protected virtual void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        foreach(GameObject gameObject in structureLights) {
            gameObject.SetActive(false);
        }
    }

    protected void PlayerCampVisual_OnCampBackgroundBuilt(object sender, System.EventArgs e) {
        if (!animateSpriteMaterialOnBuild) return;
        if (built) return;

        built = true;
        structureSpriteMaterialAnimator.SetTrigger("Build");
    }

    protected void Structure_OnStructureInteractionsUpdated(object sender, System.EventArgs e) {
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
