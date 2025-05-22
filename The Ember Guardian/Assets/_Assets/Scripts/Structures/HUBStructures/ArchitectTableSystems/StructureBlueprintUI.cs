using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureBlueprintUI : MonoBehaviour
{
    [SerializeField] private GameObject moveBlueprintGO;
    [SerializeField] private GameObject removeBlueprintGO;

    [SerializeField] private Image moveBlueprintIcon;
    [SerializeField] private Image removeBlueprintIcon;
    [SerializeField] private Sprite pickUpBlueprintSprite;
    [SerializeField] private Sprite dropBlueprintSprite;
    [SerializeField] private Sprite resetMovementSprite;
    [SerializeField] private Sprite removeBlueprintSprite;

    private Animator animator;
    private StructureBlueprint structureBlueprint;

    private void Awake() {
        structureBlueprint = GetComponentInParent<StructureBlueprint>();
        animator = GetComponent<Animator>();

        structureBlueprint.OnStructureStartedMoving += StructureBlueprint_OnStructureStartedMoving;
        structureBlueprint.OnStructureStoppedMoving += StructureBlueprint_OnStructureStoppedMoving;
        structureBlueprint.OnBlueprintHovered += StructureBlueprint_OnBlueprintHovered;
        structureBlueprint.OnBlueprintUnhovered += StructureBlueprint_OnBlueprintUnhovered;
    }

    private void StructureBlueprint_OnBlueprintUnhovered(object sender, System.EventArgs e) {
        animator.ResetTrigger("Show");
        animator.SetTrigger("Hide");
    }

    private void StructureBlueprint_OnBlueprintHovered(object sender, System.EventArgs e) {
        animator.ResetTrigger("Hide");
        animator.SetTrigger("Show");
    }

    private void StructureBlueprint_OnStructureStoppedMoving(object sender, System.EventArgs e) {
        moveBlueprintIcon.sprite = pickUpBlueprintSprite;
        removeBlueprintIcon.sprite = removeBlueprintSprite;
    }

    private void StructureBlueprint_OnStructureStartedMoving(object sender, System.EventArgs e) {
        moveBlueprintIcon.sprite = dropBlueprintSprite;
        removeBlueprintIcon.sprite = resetMovementSprite;
    }

    private void Start() {
        if(!structureBlueprint.GetLinkedStructureSO().structurePositionMovable) {
            moveBlueprintGO.SetActive(false);
        }

        if (!structureBlueprint.GetLinkedStructureSO().structurePositionRemovable) {
            removeBlueprintGO.SetActive(false);
        }
    }
}
