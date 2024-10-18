using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour {

    [SerializeField] private Vector2 parallaxEffectMultiplier;

    private Transform cameraTransform;
    private Vector3 previousCamPos;
    private float textureUnitSizeX;

    private float distanceX;
    private float distanceY;

    private float smoothingX = 1f;
    private float smoothingY = 1f;

    private void Start() {
        cameraTransform = Camera.main.transform;
        previousCamPos = cameraTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;

    }

    private void Update() {
        cameraTransform = Camera.main.transform;
    }


    private void FixedUpdate() {

        HandleParallaxOriginal();
    }

    private void HandleParallaxOriginal() {
        Vector3 deltaMovement = cameraTransform.position - previousCamPos;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y, 0);

        previousCamPos = cameraTransform.position;

        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX) {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }
    }
}
