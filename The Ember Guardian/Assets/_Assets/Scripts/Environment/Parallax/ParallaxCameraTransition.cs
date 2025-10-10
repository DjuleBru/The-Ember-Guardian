using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class ParallaxCameraTransition : MonoBehaviour
{

    [SerializeField] private bool hasCameraTransition;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera transitionCamera;

    [SerializeField] private float transitionRadius;
    [SerializeField] private float cameraTransitionRadius;

    private float lastCameraX;

    private float leftCameraTransitionPosition;
    private float rightCameraTransitionPosition;

    private bool transitionStarted;

    private void Awake() {
        leftCameraTransitionPosition = transform.position.x - cameraTransitionRadius;
        rightCameraTransitionPosition = transform.position.x + cameraTransitionRadius;

        lastCameraX = Camera.main.transform.position.x;

        transitionCamera.enabled = true;
        mainCamera.enabled = true;

        transitionCamera.Priority = 10;
        mainCamera.Priority = 20;
    }

    private void Update() {
        float cameraX = Camera.main.transform.position.x;

        // GESTION DE LA CAMERA
        if (cameraX < rightCameraTransitionPosition && cameraX > leftCameraTransitionPosition) {

            if (!transitionStarted && hasCameraTransition) {
                transitionCamera.Priority = 20;
                mainCamera.Priority = 10;
                transitionStarted = true;
                CameraManager.Instance.SetCameraLockedByTransition(true);
            }

        }
        else {

            if (transitionStarted && hasCameraTransition) {
                transitionCamera.Priority = 10;
                mainCamera.Priority = 20;
                transitionStarted = false;
                CameraManager.Instance.SetCameraLockedByTransition(false);
            }

        }
    }

}
