using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalFeedbacks : MonoBehaviour
{
    private Portal portal;

    [SerializeField] private MMF_Player playerTeleportationFeedbacks;

    private void Awake() {
        portal = GetComponentInParent<Portal>();
    }

    private void Start() {
        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;
    }

    private void Portal_OnPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        playerTeleportationFeedbacks.PlayFeedbacks();
    }
}
