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

        if(Player.Instance.transform.position.x < rightTransitionPosition && Player.Instance.transform.position.x > leftTransitionPosition) {

            float rightParallaxAmount = (Player.Instance.transform.position.x - leftTransitionPosition) / transitionRange;
            float leftParallaxAmount = 1- rightParallaxAmount;

            leftParallax.SetParallaxTransparency(leftParallaxAmount);
            rightParallax.SetParallaxTransparency(rightParallaxAmount);

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
