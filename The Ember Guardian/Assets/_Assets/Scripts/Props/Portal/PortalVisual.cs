using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalVisual : MonoBehaviour
{
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private GameObject portalFloorFrontGameObject;
    [SerializeField] private GameObject portalMarkingsGameObject;
    [SerializeField] private List<Light2D> portalLights;

    private Portal portal;

    private void Awake() {
        portal = GetComponentInParent<Portal>();
        portalFloorFrontGameObject.SetActive(false);
        portalMarkingsGameObject.SetActive(false);
        portal.OnPlayerEnteredTriggerArea += Portal_OnPlayerEnteredTriggerArea;
        portal.OnPlayerExitedTriggerArea += Portal_OnPlayerExitedTriggerArea;

        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;
        portal.OnTeleporterActivated += Portal_OnTeleporterActivated;

        TurnOnLights(false);

    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        portalFloorFrontGameObject.SetActive(true);
        portalMarkingsGameObject.SetActive(true);
    }

    private void Portal_OnTeleporterActivated(object sender, System.EventArgs e) {
        portalAnimator.SetTrigger("Teleport");
    }

    private void Portal_OnPlayerExitedTriggerArea(object sender, System.EventArgs e) {
        TurnOnLights(false);
    }

    private void Portal_OnPlayerEnteredTriggerArea(object sender, System.EventArgs e) {
        TurnOnLights(true);
    }

    private void TurnOnLights(bool enabled) {
        foreach(Light2D light in  portalLights) {
            light.enabled = enabled;
        }
    }

}
