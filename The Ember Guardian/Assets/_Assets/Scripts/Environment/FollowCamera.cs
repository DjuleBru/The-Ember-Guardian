using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private Transform cameraTransform;
    private void Start() {
        cameraTransform = Camera.main.transform;
    }

    private void Update() {
        cameraTransform = Camera.main.transform;
    }


    private void FixedUpdate() {
        transform.position = new Vector3(cameraTransform.position.x, transform.position.y);
    }
}
