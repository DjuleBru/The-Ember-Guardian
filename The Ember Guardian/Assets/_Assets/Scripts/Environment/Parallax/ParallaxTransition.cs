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

    private bool transitionStarted;

    private void Awake() {
        transitionRange = transitionRadius*2;
        leftTransitionPosition = transform.position.x - transitionRadius;
        rightTransitionPosition = transform.position.x + transitionRadius;

        leftCameraTransitionPosition = transform.position.x - cameraTransitionRadius;
        rightCameraTransitionPosition = transform.position.x + cameraTransitionRadius;
    }

    private void Update() {

        float playerX = Player.Instance.transform.position.x;

        // GESTION DES TRANSPARENCES
        if (playerX < rightTransitionPosition && playerX > leftTransitionPosition) {
            float rightParallaxAmount = (playerX - leftTransitionPosition) / transitionRange;
            float leftParallaxAmount = 1f - rightParallaxAmount;

            leftParallax.SetParallaxTransparency(leftParallaxAmount);
            rightParallax.SetParallaxTransparency(rightParallaxAmount);
        }
        else {
            // Sortie à gauche
            if (playerX <= leftTransitionPosition) {
                leftParallax.SetParallaxTransparency(1f);
                rightParallax.SetParallaxTransparency(0f);
            }
            // Sortie à droite
            else if (playerX >= rightTransitionPosition) {
                leftParallax.SetParallaxTransparency(0f);
                rightParallax.SetParallaxTransparency(1f);
            }
        }

        if (Player.Instance.transform.position.x < rightCameraTransitionPosition && Player.Instance.transform.position.x > leftCameraTransitionPosition) {

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
