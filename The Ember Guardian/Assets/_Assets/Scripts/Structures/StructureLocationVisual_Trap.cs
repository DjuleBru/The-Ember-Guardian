using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocationVisual_Trap : StructureLocationVisual
{
    private StructureLocation_Trap structureLocation_Trap;
    [SerializeField] protected Animator leftSwitchArrowsAnimator;
    [SerializeField] protected Animator rightSwitchArrowsAnimator;

    protected bool arrowsDisplayed;

    protected override void Awake() {
        base.Awake();
        structureLocation_Trap = GetComponentInParent<StructureLocation_Trap>();
        structureLocation_Trap.OnStructureSOToBuildChanged += StructureLocation_Trap_OnStructureSOToBuildChanged;
        structureLocation_Trap.OnTrapTypesAmountInInventoryChanged += StructureLocation_Trap_OnTrapTypesAmountInInventoryChanged;
    }

    private void StructureLocation_Trap_OnTrapTypesAmountInInventoryChanged(object sender, EventArgs e) {
        RefreshShowSlotVisual();
    }

    protected override void StructureLocation_OnStructureLocationUnlocked(object sender, System.EventArgs e) {
        base.StructureLocation_OnStructureLocationUnlocked(sender, e);
        RefreshShowSlotVisual();
    }

    private void RefreshShowSlotVisual() {
        if (!structureLocation_Trap.GetStructureLocationUnlocked()) return;
        if (structureLocation_Trap.GetTrapTypeAmountInInventory() == 0) {
            slotVisual.SetActive(false);
        }
        else {
            slotVisual.SetActive(true);
        }
    }

    private void StructureLocation_Trap_OnStructureSOToBuildChanged(object sender, EventArgs e) {
        structureVisual_Build.sprite = structureLocation_Trap.GetStructureSOToBuild().structureSprite;
    }

    protected override void StructureLocation_OnStructureLocationLoaded_Locked(object sender, EventArgs e) {

    }

    protected override void HideVisuals() {
        base.HideVisuals();

        if(structureLocation_Trap.GetTrapTypeAmountInInventory() > 1) {
            HideArrows();
        }
    }

    protected override void ShowAllVisuals() {
        base.ShowAllVisuals();

        if (structureLocation_Trap.GetTrapTypeAmountInInventory() > 1) {
            ShowArrows();
        }
    }

    private void ShowArrows() {
        leftSwitchArrowsAnimator.GetComponent<Animator>().ResetTrigger("Hide");
        leftSwitchArrowsAnimator.GetComponent<Animator>().SetTrigger("Show");
        rightSwitchArrowsAnimator.GetComponent<Animator>().ResetTrigger("Hide");
        rightSwitchArrowsAnimator.GetComponent<Animator>().SetTrigger("Show");
    }

    private void HideArrows() {
        leftSwitchArrowsAnimator.GetComponent<Animator>().ResetTrigger("Show");
        leftSwitchArrowsAnimator.GetComponent<Animator>().SetTrigger("Hide");
        rightSwitchArrowsAnimator.GetComponent<Animator>().ResetTrigger("Show");
        rightSwitchArrowsAnimator.GetComponent<Animator>().SetTrigger("Hide");
    }
}
