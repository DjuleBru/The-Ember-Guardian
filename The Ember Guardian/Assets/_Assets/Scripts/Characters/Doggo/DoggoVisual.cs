using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggoVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private ParticleSystem runDustPS;
    private DogAnimatorManager dogAnimatorManager;


    private Dog dog;
    private DogAI dogAI;

    private void Awake() {
        dog = GetComponentInParent<Dog>();
        dogAI = GetComponentInParent<DogAI>();
        dogAnimatorManager = GetComponent<DogAnimatorManager>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;

        dogAnimatorManager.OnFootstepTriggered += DogAnimatorManager_OnFootstepTriggered;
    }

    private void DogAnimatorManager_OnFootstepTriggered(object sender, System.EventArgs e) {
        if(dogAI.GetState() == DogAI.State.runToCamp || dogAI.GetState() == DogAI.State.runWithPlayer) {
            int randomParticles = UnityEngine.Random.Range(0, 5);
            runDustPS.Emit(randomParticles);
        }
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
