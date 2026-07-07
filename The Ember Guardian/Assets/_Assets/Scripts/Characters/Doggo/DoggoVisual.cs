using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggoVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private ParticleSystem runDustPS;
    [SerializeField] private DogAI_Retreiver retreiverAI;
    [SerializeField] private GameObject hasCurrenciesGO;
    [SerializeField] private DogCreatureDetectionCollider detectionCollider;
    [SerializeField] private GameObject detectedAmbushGO;
    [SerializeField] private GameObject speedupLineRenderer;
    private DogAnimatorManager dogAnimatorManager;


    private Dog dog;
    private DogAI dogAI;

    private void Awake() {
        dog = GetComponentInParent<Dog>();
        dogAI = GetComponentInParent<DogAI>();
        dogAnimatorManager = GetComponent<DogAnimatorManager>();
        detectionCollider.OnAmbushDetected += DetectionCollider_OnAmbushDetected;
        detectionCollider.OnNoAmbushDetected += DetectionCollider_OnNoAmbushDetected;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;

        dogAnimatorManager.OnFootstepTriggered += DogAnimatorManager_OnFootstepTriggered;
        retreiverAI.OnDogCollectedCurrency += RetreiverAI_OnDogCollectedCurrency;
        retreiverAI.OnDogDroppedAllCurrencies += RetreiverAI_OnDogDroppedAllCurrencies;

        hasCurrenciesGO.SetActive(false);
        detectedAmbushGO.SetActive(false);
        speedupLineRenderer.SetActive(false);
    }

    private void DetectionCollider_OnNoAmbushDetected(object sender, System.EventArgs e) {
        if (Dog.Instance.GetDogType() != Dog.DogType.GermanShepherd) return;
        detectedAmbushGO.SetActive(false);
    }

    private void DetectionCollider_OnAmbushDetected(object sender, System.EventArgs e) {
        if (Dog.Instance.GetDogType() != Dog.DogType.GermanShepherd) return;
        detectedAmbushGO.SetActive(true);
    }

    private void RetreiverAI_OnDogDroppedAllCurrencies(object sender, System.EventArgs e) {
        hasCurrenciesGO.SetActive(false);
    }

    private void RetreiverAI_OnDogCollectedCurrency(object sender, System.EventArgs e) {
        hasCurrenciesGO.SetActive(true);
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
