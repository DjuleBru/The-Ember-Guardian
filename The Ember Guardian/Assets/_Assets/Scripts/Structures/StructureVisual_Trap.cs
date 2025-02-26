using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureVisual_Trap : StructureVisual
{
    private Structure_Trap trap;
    [SerializeField] private bool showTrapFullSpriteOnHover;
    [SerializeField] private SpriteRenderer fullTrapSpriteRenderer;
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
    }

    private void Trap_OnTrapActiveEnded(object sender, System.EventArgs e) {
        if (!trap.GetHasUsesLeft()) {

        }
    }

    private void Trap_OnTrapTriggered(object sender, System.EventArgs e) {
        trapAnimator.SetTrigger("Triggered");
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredOut (sender, e);

        if(showTrapFullSpriteOnHover) {
            fullTrapAnimator.SetTrigger("Hide");
        }
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredIn (sender, e);

        if (showTrapFullSpriteOnHover) {
            fullTrapAnimator.SetTrigger("Show");
        }
    }
}
