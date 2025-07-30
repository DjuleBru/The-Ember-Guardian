using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxTransitionZone : MonoBehaviour {

    [SerializeField] private Transform leftLimitPosition;
    [SerializeField] private Transform leftFadeEndPosition;
    [SerializeField] private Transform rightFadeStartPosition;
    [SerializeField] private Transform rightLimitPosition;

    [SerializeField] private ParallaxEnvironment interiorParallax;
    [SerializeField] private ParallaxEnvironment exteriorParallax;

    private float leftLimitX;
    private float leftFadeEndX;
    private float rightFadeStartX;
    private float rightLimitX;

    private void Awake() {
        leftLimitX = leftLimitPosition.position.x;
        leftFadeEndX = leftFadeEndPosition.position.x;
        rightFadeStartX = rightFadeStartPosition.position.x;
        rightLimitX = rightLimitPosition.position.x;
    }

    private void Update() {
        float cameraX = Camera.main.transform.position.x;
        float interiorAlpha = 0f;

        if (cameraX <= leftLimitX || cameraX >= rightLimitX) {
            // Zone extérieure
            interiorAlpha = 0f;
        }
        else if (cameraX > leftLimitX && cameraX < leftFadeEndX) {
            // Fade-In gauche
            float t = (cameraX - leftLimitX) / (leftFadeEndX - leftLimitX);
            interiorAlpha = Mathf.Clamp01(t);
        }
        else if (cameraX >= leftFadeEndX && cameraX <= rightFadeStartX) {
            // Intérieur complet
            interiorAlpha = 1f;
        }
        else if (cameraX > rightFadeStartX && cameraX < rightLimitX) {
            // Fade-Out droite
            float t = 1f - (cameraX - rightFadeStartX) / (rightLimitX - rightFadeStartX);
            interiorAlpha = Mathf.Clamp01(t);
        }

        // Appliquer les transparences
        interiorParallax.SetParallaxTransparency(interiorAlpha);
        exteriorParallax.SetParallaxTransparency(1f - interiorAlpha);
    }
}
