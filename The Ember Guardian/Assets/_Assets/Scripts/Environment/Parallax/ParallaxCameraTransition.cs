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
    }

    private void Update() {

        float cameraX = Camera.main.transform.position.x;
        // GESTION DE LA CAMERA
        if (cameraX < rightCameraTransitionPosition && cameraX > leftCameraTransitionPosition) {

            if (!transitionStarted && hasCameraTransition) {
                transitionCamera.enabled = true;
                mainCamera.enabled = false;
                transitionStarted = true;
                CameraManager.Instance.SetCameraLockedByTransition(true);
            }

        }
        else {

            if (transitionStarted && hasCameraTransition) {
                transitionCamera.enabled = false;
                mainCamera.enabled = true;
                transitionStarted = false;
                CameraManager.Instance.SetCameraLockedByTransition(false);
            }

        }
    }

}
