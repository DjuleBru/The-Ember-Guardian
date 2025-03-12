using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggoVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;


    private Dog dog;

    private void Awake() {
        dog = GetComponentInParent<Dog>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;
    }

    private void Portal_OnAnyPortalSetToTeleportPlayer(object sender, System.EventArgs e) {
        ShowVisuals(false);
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, System.EventArgs e) {
        ShowVisuals(true);
    }

    private void ShowVisuals(bool show) {
        gameObject.SetActive(show);
    }

    private void OnDestroy() {
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer -= Portal_OnAnyPortalSetToTeleportPlayer;
    }
}
