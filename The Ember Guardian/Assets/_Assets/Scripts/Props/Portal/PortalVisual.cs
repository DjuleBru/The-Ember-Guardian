using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalVisual : MonoBehaviour
{
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private GameObject portalBodyGameObject;
    [SerializeField] private GameObject portalFloorFrontGameObject;
    [SerializeField] private GameObject portalFloorBackGameObject;
    [SerializeField] private GameObject portalMarkingsGameObject;
    [SerializeField] private Light2D portalBeamLight;
    [SerializeField] private List<GameObject> portalSideLightGameObjects;

    private Portal portal;

    private void Awake() {
        portal = GetComponentInParent<Portal>();
        portalFloorFrontGameObject.SetActive(false);
        portalMarkingsGameObject.SetActive(false);

        portal.OnPortalUnlocked += Portal_OnPortalUnlocked;
        portal.OnPortalAppeared += Portal_OnPortalAppeared;
        portal.OnPortalDisappeared += Portal_OnPortalDisappeared;
        portal.OnPlayerEnteredTriggerArea += Portal_OnPlayerEnteredTriggerArea;
        portal.OnPlayerExitedTriggerArea += Portal_OnPlayerExitedTriggerArea;

        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;
        portal.OnTeleporterActivated += Portal_OnTeleporterActivated;
        portal.OnTeleporterActivatedOut += Portal_OnTeleporterActivatedOut;
        portal.OnPortalSetToTeleportPlayer += Portal_OnPortalSetToTeleportPlayer;

        TurnOnBeamLight(false);
        TurnOnSideLights(false);
    }
    private void Start() {
        portalFloorFrontGameObject.SetActive(false);

        if (portal.GetIsHUBTeleporter()) {
            if (!portal.GetPortalUnlocked()) {
                portalFloorBackGameObject.SetActive(false);
            }
            else {
                TurnOnSideLights(true);
            }
        }

        if (portal.GetIsStartLevelTeleporter()) {
            portalFloorBackGameObject.SetActive(false);
            portalMarkingsGameObject.SetActive(false);
            portalBodyGameObject.SetActive(false);
            portalBeamLight.enabled = false;
        }
    }

    private void Portal_OnPortalUnlocked(object sender, System.EventArgs e) {
        TurnOnSideLights(true);
    }

    private void Portal_OnPortalAppeared(object sender, System.EventArgs e) {
        portalFloorBackGameObject.SetActive(true);
        portalFloorFrontGameObject.SetActive(true);
        portalMarkingsGameObject.SetActive(true);
        portalBodyGameObject.SetActive(true);

        portalAnimator.SetTrigger("Appear");
    }

    private void Portal_OnPortalSetToTeleportPlayer(object sender, System.EventArgs e) {
        TurnOnBeamLight(false);
    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        portalFloorFrontGameObject.SetActive(true);
        portalMarkingsGameObject.SetActive(true);
    }

    private void Portal_OnTeleporterActivated(object sender, System.EventArgs e) {
        portalAnimator.SetTrigger("Teleport");
    }

    private void Portal_OnTeleporterActivatedOut(object sender, System.EventArgs e) {
        portalFloorBackGameObject.SetActive(true);
        portalFloorFrontGameObject.SetActive(true);
        portalMarkingsGameObject.SetActive(true);
        portalBodyGameObject.SetActive(true);

        TurnOnBeamLight(true);
        portalAnimator.SetBool("Teleport_Out", true);
        portalAnimator.SetTrigger("Teleport_OutTrigger");
    }

    private void Portal_OnPortalDisappeared(object sender, System.EventArgs e) {
        portalAnimator.SetTrigger("Disappear");
    }
    private void Portal_OnPlayerExitedTriggerArea(object sender, System.EventArgs e) {
        TurnOnBeamLight(false);
        portalFloorFrontGameObject.SetActive(false);
    }

    private void Portal_OnPlayerEnteredTriggerArea(object sender, System.EventArgs e) {
        TurnOnBeamLight(true);
    }

    private void TurnOnBeamLight(bool enabled) {
        portalBeamLight.enabled = enabled;
    }

    private void TurnOnSideLights(bool enabled) {
        foreach (GameObject light in portalSideLightGameObjects) {
            light.SetActive(enabled);
        }
    }

}
