using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureVisual_Trap : StructureVisual
{
    private Structure_Trap trap;
    [SerializeField] private bool showTrapFullSpriteOnHover;
    [SerializeField] private SpriteRenderer trapSpriteRenderer;
    [SerializeField] private SpriteRenderer fullTrapSpriteRenderer;
    [SerializeField] private Sprite trapDepletedSprite;
    [SerializeField] private SpriteRenderer depletedUsesGlowSpriteRenderer;
    [SerializeField] private Animator trapAnimator;
    [SerializeField] private Animator fullTrapAnimator;

    protected override void Start() {
        base.Start();
        trap = GetComponentInParent<Structure_Trap>();

        if (showTrapFullSpriteOnHover) {
            fullTrapSpriteRenderer.sprite = structure.GetStructureSO().structureSprite;
        }

        trap.OnTrapTriggered += Trap_OnTrapTriggered;
        trap.OnTrapActiveEnded += Trap_OnTrapActiveEnded;
        trap.OnTrapTriggeredEnded += Trap_OnTrapTriggeredEnded;
        trap.OnTrapDepletedUses += Trap_OnTrapDepletedUses;
        trap.OnTrapRearmed += Trap_OnTrapRearmed;
        trap.OnTrapBroken += Trap_OnTrapBroken;

        SetActiveVisuals();
    }

    private void Trap_OnTrapBroken(object sender, EventArgs e) {
        trapAnimator.SetTrigger("Break");
    }

    private void Trap_OnTrapRearmed(object sender, EventArgs e) {
        SetActiveVisuals();
        trapAnimator.SetTrigger("Rearm");
    }

    private void Trap_OnTrapDepletedUses(object sender, EventArgs e) {
        SetDepletedVisuals();
    }

    private void Trap_OnTrapTriggeredEnded(object sender, EventArgs e) {
        if (trap.GetHasUsesLeft()) {
            trapAnimator.SetTrigger("TriggeredEnd");
        }
    }

    private void Trap_OnTrapActiveEnded(object sender, System.EventArgs e) {

    }

    private void SetDepletedVisuals() {
        Debug.Log("SetDepletedVisuals");
        trapAnimator.enabled = false;
        trapSpriteRenderer.sprite = trapDepletedSprite;
        depletedUsesGlowSpriteRenderer.enabled = true;
    }

    private void SetActiveVisuals() {
        trapAnimator.enabled = true;
        depletedUsesGlowSpriteRenderer.enabled = false;
    }

    private void Trap_OnTrapTriggered(object sender, System.EventArgs e) {
        trapAnimator.SetTrigger("Triggered");
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredOut (sender, e);

        if(showTrapFullSpriteOnHover) {
            fullTrapAnimator.SetTrigger("Hide");
            fullTrapAnimator.ResetTrigger("Show");
        }
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredIn (sender, e);

        if (showTrapFullSpriteOnHover) {
            fullTrapAnimator.SetTrigger("Show");
            fullTrapAnimator.ResetTrigger("Hide");
        }
    }
}
