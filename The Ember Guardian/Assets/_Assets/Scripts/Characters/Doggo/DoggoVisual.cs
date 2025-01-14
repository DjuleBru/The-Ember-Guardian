using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggoVisual : MonoBehaviour
{
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;


    private Dog dog;

    private void Awake() {
        dog = GetComponentInParent<Dog>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;
    }

    private void Start() {
        dog.OnPlayerTriggeredIn += Dog_OnPlayerTriggeredIn;
        dog.OnPlayerTriggeredOut += Dog_OnPlayerTriggeredOut;
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
    private void Dog_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        //bodySpriteRenderer.material = cleanMaterial;
    }

    private void Dog_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        //bodySpriteRenderer.material = hoveredMaterial;
    }

    private void OnDestroy() {
        dog.OnPlayerTriggeredIn -= Dog_OnPlayerTriggeredIn;
        dog.OnPlayerTriggeredOut -= Dog_OnPlayerTriggeredOut;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer -= Portal_OnAnyPortalSetToTeleportPlayer;
    }
}
