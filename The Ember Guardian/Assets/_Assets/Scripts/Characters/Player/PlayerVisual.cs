using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem exhaustedPS;
    [SerializeField] private ParticleSystem recoveringStaminaPS;
    [SerializeField] private Transform aimReticleGamepad;
    [SerializeField] private GameObject gunGameObject;

    private void Awake() {
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
        PlayerMovement.Instance.OnPlayerRecoverStaminaFastStarted += PlayerMovement_OnPlayerRecoverStaminaFastStarted;
        PlayerMovement.Instance.OnPlayerRecoverStaminaFastStopped += PlayerMovement_OnPlayerRecoverStaminaFastStopped;
        PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerEndedPettingDog += PetDog_OnPlayerEndedPettingDog;

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
    }

    private void PlayerMovement_OnPlayerRecoverStaminaFastStopped(object sender, System.EventArgs e) {
        recoveringStaminaPS.Stop();
    }

    private void PlayerMovement_OnPlayerRecoverStaminaFastStarted(object sender, System.EventArgs e) {
        recoveringStaminaPS.Play();
    }

    private void PetDog_OnPlayerEndedPettingDog(object sender, System.EventArgs e) {
        gunGameObject.SetActive(true);

    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        gunGameObject.SetActive(false);
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        //RefreshGamepadReticle();
    }

    private void RefreshGamepadReticle() {
        if (GameInput.Instance.IsUsingGamepad()) {
            aimReticleGamepad.gameObject.SetActive(true);
        }
        else {
            aimReticleGamepad.gameObject.SetActive(false);
        }
    }

    private void Portal_OnAnyPortalSetToTeleportPlayer(object sender, System.EventArgs e) {
        ShowVisuals(false);
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, System.EventArgs e) {
        ShowVisuals(true);
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, System.EventArgs e) {
        exhaustedPS.Stop();
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, System.EventArgs e) {
        exhaustedPS.Play();
    }

    private void ShowVisuals(bool show) {
        gameObject.SetActive(show);
    }

    private void OnDestroy() {
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer -= Portal_OnAnyPortalSetToTeleportPlayer;
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
