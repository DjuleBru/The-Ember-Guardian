using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation_StartLevelFire : StructureLocation {

    private bool lightFireTooltipShown;
    private bool lightFireTooltipBeingShown;

    protected override void Awake() {
        base.Awake();
        lightFireTooltipShown = ES3.Load("lightFireTooltipShown", false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        if(!lightFireTooltipShown && !lightFireTooltipBeingShown) {
            lightFireTooltipBeingShown = true;
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_lightFire"), InputControlIcons.Control.Interact, 999f);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject.GetComponent<Player>() == null) return;


        if(!lightFireTooltipShown && lightFireTooltipBeingShown) {
            PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            lightFireTooltipBeingShown = false;
        }
    }

    public override Structure BuildStructure() {
        InvokeOnAnyStructureBuilt(Fire.Instance);
        Fire.Instance.gameObject.SetActive(true);
        Fire.Instance.ActivateInitialFire();

        if (!lightFireTooltipShown) {
            if(lightFireTooltipBeingShown) {
                lightFireTooltipBeingShown = false;
                PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            }
            ES3.Save("lightFireTooltipShown", true);
        }

        StartCoroutine(DestroyGameObjectAfterFrame());
        return Fire.Instance;
    }
}
