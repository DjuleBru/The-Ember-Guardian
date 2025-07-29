using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxTransition : MonoBehaviour
{

    [SerializeField] private bool hasCameraTransition;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera transitionCamera;

    [SerializeField] private ParallaxEnvironment leftParallax;
    [SerializeField] private ParallaxEnvironment rightParallax;

    [SerializeField] private float transitionRadius;
    [SerializeField] private float cameraTransitionRadius;

    private float leftTransitionPosition;
    private float rightTransitionPosition;

    private float leftCameraTransitionPosition;
    private float rightCameraTransitionPosition;

    private float transitionRange;

    private bool playerIsInTransitionParallax;
    private bool transitionStarted;

    private void Awake() {
        transitionRange = transitionRadius*2;
        leftTransitionPosition = transform.position.x - transitionRadius;
        rightTransitionPosition = transform.position.x + transitionRadius;

        leftCameraTransitionPosition = transform.position.x - cameraTransitionRadius;
        rightCameraTransitionPosition = transform.position.x + cameraTransitionRadius;
    }

    private void Update() {

        float cameraX = Camera.main.transform.position.x;

        // GESTION DES TRANSPARENCES
        if (cameraX < rightTransitionPosition && cameraX > leftTransitionPosition) {
            float rightParallaxAmount = (cameraX - leftTransitionPosition) / transitionRange;
            float leftParallaxAmount = 1f - rightParallaxAmount;

            leftParallax.SetParallaxTransparency(leftParallaxAmount);
            rightParallax.SetParallaxTransparency(rightParallaxAmount);
            playerIsInTransitionParallax = true;
        }
        else {
            // Sortie à gauche
            if (cameraX <= leftTransitionPosition && playerIsInTransitionParallax) {
                leftParallax.SetParallaxTransparency(1f);
                rightParallax.SetParallaxTransparency(0f);
                playerIsInTransitionParallax = false;
            }
            // Sortie à droite
            else if (cameraX >= rightTransitionPosition && playerIsInTransitionParallax) {
                leftParallax.SetParallaxTransparency(0f);
                rightParallax.SetParallaxTransparency(1f);
                playerIsInTransitionParallax = false;
            }
        }

        if (cameraX < rightCameraTransitionPosition && cameraX > leftCameraTransitionPosition) {

            if (!transitionStarted && hasCameraTransition) {
                transitionCamera.enabled = true;
                mainCamera.enabled = false;
                transitionStarted = true;
            }

        }
        else {

            if (transitionStarted && hasCameraTransition) {
                transitionCamera.enabled = false;
                mainCamera.enabled = true;
                transitionStarted = false;
            }

        }
    }
}
